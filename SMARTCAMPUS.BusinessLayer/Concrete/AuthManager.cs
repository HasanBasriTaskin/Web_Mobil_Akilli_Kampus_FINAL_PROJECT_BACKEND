using AutoMapper;
using Microsoft.Extensions.Configuration; // Add this
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore; // Added for FirstOrDefaultAsync
using SMARTCAMPUS.BusinessLayer.Abstract;
using SMARTCAMPUS.BusinessLayer.Common;
using SMARTCAMPUS.BusinessLayer.Tools;
using SMARTCAMPUS.DataAccessLayer.Abstract;
using SMARTCAMPUS.EntityLayer.DTOs.Auth;
using SMARTCAMPUS.EntityLayer.Models;

using Microsoft.AspNetCore.Http;
using SMARTCAMPUS.EntityLayer.DTOs; // Add this namespace

namespace SMARTCAMPUS.BusinessLayer.Concrete
{
    public class AuthManager : IAuthService
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly JwtTokenGenerator _tokenGenerator;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly INotificationService _notificationService;
        private readonly IConfiguration _configuration;

        public AuthManager(UserManager<User> userManager, 
                           SignInManager<User> signInManager, 
                           JwtTokenGenerator tokenGenerator, 
                           IMapper mapper, 
                           IUnitOfWork unitOfWork,
                           IHttpContextAccessor httpContextAccessor,
                           INotificationService notificationService,
                           IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenGenerator = tokenGenerator;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _httpContextAccessor = httpContextAccessor;
            _notificationService = notificationService;
            _configuration = configuration;
        }

        public async Task<Response<LoginResponseDto>> LoginAsync(LoginDto loginDto)
        {
            var user = await _userManager.FindByEmailAsync(loginDto.Email);
            if (user == null)
            {
                return Response<LoginResponseDto>.Fail("Geçersiz e-posta veya şifre", 400); 
            }

            if (!user.IsActive)
            {
                return Response<LoginResponseDto>.Fail("Hesap aktif değil. Lütfen e-postanızı doğrulayın.", 400);
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);
            if (!result.Succeeded)
            {
                 return Response<LoginResponseDto>.Fail("Geçersiz e-posta veya şifre", 400); // Güvenlik için aynı mesaj
            }

            // Get Roles
            var roles = await _userManager.GetRolesAsync(user);
            var primaryRole = roles.FirstOrDefault() ?? "User";

            // Generate Token
            var tokenDto = _tokenGenerator.GenerateToken(user, roles);

            // Save Refresh Token
            var refreshTokenEntity = new RefreshToken
            {
                UserId = user.Id,
                Token = tokenDto.RefreshToken,
                Expires = tokenDto.RefreshTokenExpiration,
                CreatedDate = DateTime.UtcNow, 
                CreatedByIp = GetIpAddress()
            };

            await _unitOfWork.RefreshTokens.AddAsync(refreshTokenEntity);
            await _unitOfWork.CommitAsync();

            // Build user info based on role
            var loginUserDto = new LoginUserDto
            {
                Id = user.Id,
                Email = user.Email!,
                FullName = user.FullName,
                UserType = primaryRole,
                Role = primaryRole,
                IsEmailVerified = user.EmailConfirmed,
                IsActive = user.IsActive,
                ProfilePictureUrl = user.ProfilePictureUrl,
                CreatedAt = user.CreatedDate
            };

            // Fetch Student or Faculty info based on role
            if (primaryRole == "Student")
            {
                var student = await _unitOfWork.Students.Where(s => s.UserId == user.Id).FirstOrDefaultAsync();
                if (student != null)
                {
                    loginUserDto.Student = new StudentInfoDto
                    {
                        StudentNumber = student.StudentNumber,
                        DepartmentId = student.DepartmentId,
                        EnrollmentDate = student.CreatedDate
                    };
                }
            }
            else if (primaryRole == "Faculty")
            {
                var faculty = await _unitOfWork.Faculties.Where(f => f.UserId == user.Id).FirstOrDefaultAsync();
                if (faculty != null)
                {
                    loginUserDto.Faculty = new FacultyInfoDto
                    {
                        EmployeeNumber = faculty.EmployeeNumber,
                        Title = faculty.Title,
                        DepartmentId = faculty.DepartmentId,
                        OfficeLocation = faculty.OfficeLocation
                    };
                }
            }

            var loginResponse = new LoginResponseDto
            {
                AccessToken = tokenDto.AccessToken,
                RefreshToken = tokenDto.RefreshToken,
                AccessTokenExpiration = tokenDto.AccessTokenExpiration,
                RefreshTokenExpiration = tokenDto.RefreshTokenExpiration,
                User = loginUserDto
            };

            return Response<LoginResponseDto>.Success(loginResponse, 200);
        }

        public async Task<Response<TokenDto>> RegisterAsync(RegisterUserDto registerDto)
        {
            if (registerDto.UserType == "Student")
            {
                return await RegisterStudentAsync(registerDto);
            }
            else if (registerDto.UserType == "Faculty")
            {
                return await RegisterFacultyAsync(registerDto);
            }
            else
            {
                return Response<TokenDto>.Fail("Geçersiz kullanıcı tipi", 400);
            }
        }

        private async Task<Response<TokenDto>> RegisterStudentAsync(RegisterUserDto registerDto)
        {
             // 1. Check if user exists
            var userExists = await _userManager.FindByEmailAsync(registerDto.Email);
            if (userExists != null)
            {
                return Response<TokenDto>.Fail("Bu e-posta adresiyle kayıtlı kullanıcı zaten var", 400);
            }

            if (string.IsNullOrEmpty(registerDto.StudentNumber))
            {
                 return Response<TokenDto>.Fail("Öğrenci numarası zorunludur", 400);
            }

            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try 
            {
                // Create Identity User
                var user = _mapper.Map<User>(registerDto);
                user.UserName = registerDto.Email; 
                user.CreatedDate = DateTime.UtcNow;
                user.IsActive = false; 

                var result = await _userManager.CreateAsync(user, registerDto.Password);
                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(e => e.Description).ToList();
                    return Response<TokenDto>.Fail(errors, 400);
                }

                await _userManager.AddToRoleAsync(user, "Student");

                // Create Student Entity
                var student = new Student
                {
                    UserId = user.Id,
                    StudentNumber = registerDto.StudentNumber,
                    DepartmentId = registerDto.DepartmentId
                };

                await _unitOfWork.Students.AddAsync(student);
                await _unitOfWork.CommitAsync();

                await transaction.CommitAsync();

                // Generate Token & Send Email
                return await ProcessPostRegistrationAsync(user, new List<string> { "Student" });
            }
            catch (Exception ex)
            {
               await transaction.RollbackAsync();
               return Response<TokenDto>.Fail($"Registration failed: {ex.Message}", 500);
            }
        }

        private async Task<Response<TokenDto>> RegisterFacultyAsync(RegisterUserDto registerDto)
        {
             // 1. Check if user exists
            var userExists = await _userManager.FindByEmailAsync(registerDto.Email);
            if (userExists != null)
            {
                return Response<TokenDto>.Fail("Bu e-posta adresiyle kayıtlı kullanıcı zaten var", 400);
            }

            if (string.IsNullOrEmpty(registerDto.EmployeeNumber) || string.IsNullOrEmpty(registerDto.Title))
            {
                 return Response<TokenDto>.Fail("Sicil numarası ve Ünvan zorunludur", 400);
            }

            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try 
            {
                // Create Identity User
                var user = _mapper.Map<User>(registerDto);
                user.UserName = registerDto.Email; 
                user.CreatedDate = DateTime.UtcNow;
                user.IsActive = false; 

                var result = await _userManager.CreateAsync(user, registerDto.Password);
                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(e => e.Description).ToList();
                    return Response<TokenDto>.Fail(errors, 400);
                }

                await _userManager.AddToRoleAsync(user, "Faculty");

                // Create Faculty Entity
                var faculty = new Faculty
                {
                    UserId = user.Id,
                    EmployeeNumber = registerDto.EmployeeNumber,
                    Title = registerDto.Title,
                    OfficeLocation = registerDto.OfficeLocation,
                    DepartmentId = registerDto.DepartmentId
                };

                await _unitOfWork.Faculties.AddAsync(faculty);
                await _unitOfWork.CommitAsync();

                await transaction.CommitAsync();

                // Generate Token & Send Email
                return await ProcessPostRegistrationAsync(user, new List<string> { "Faculty" });
            }
            catch (Exception ex)
            {
               await transaction.RollbackAsync();
               return Response<TokenDto>.Fail($"Registration failed: {ex.Message}", 500);
            }
        }

        private async Task<Response<TokenDto>> ProcessPostRegistrationAsync(User user, List<string> roles)
        {
             // Generate Token
            var tokenDto = _tokenGenerator.GenerateToken(user, roles);

            // Save Refresh Token
            var refreshTokenEntity = new RefreshToken
            {
                UserId = user.Id,
                Token = tokenDto.RefreshToken,
                Expires = tokenDto.RefreshTokenExpiration,
                CreatedDate = DateTime.UtcNow,
                CreatedByIp = GetIpAddress() 
            };

            await _unitOfWork.RefreshTokens.AddAsync(refreshTokenEntity);
            await _unitOfWork.CommitAsync();

            // Send Verification Email
            var emailToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            
            var verifTokenEntity = new EmailVerificationToken
            {
                UserId = user.Id,
                Token = emailToken,
                ExpiresAt = DateTime.UtcNow.AddHours(24),
                CreatedDate = DateTime.UtcNow
            };
            await _unitOfWork.EmailVerificationTokens.AddAsync(verifTokenEntity);
            await _unitOfWork.CommitAsync();

            var clientUrl = _configuration["ClientSettings:Url"] ?? "http://localhost:3000";
            var verifyLink = $"{clientUrl}/verify-email?userId={user.Id}&token={Uri.EscapeDataString(emailToken)}";
            
            await _notificationService.SendEmailVerificationAsync(user.Email!, verifyLink);

            return Response<TokenDto>.Success(tokenDto, 201);
        }

        public async Task<Response<NoDataDto>> VerifyEmailAsync(string userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return Response<NoDataDto>.Fail("Kullanıcı bulunamadı", 404);

            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (!result.Succeeded)
            {
                return Response<NoDataDto>.Fail("E-posta doğrulama başarısız", 400);
            }

            user.IsActive = true;
            await _userManager.UpdateAsync(user);

            return Response<NoDataDto>.Success(200);
        }

        public async Task<Response<TokenDto>> CreateTokenByRefreshTokenAsync(string refreshToken)
        {
            var existingToken = await _unitOfWork.RefreshTokens.Where(x => x.Token == refreshToken).FirstOrDefaultAsync();

            if (existingToken == null)
            {
                return Response<TokenDto>.Fail("Token bulunamadı", 404);
            }

            if (!existingToken.IsValid) // Revoked or Expired
            {
               return Response<TokenDto>.Fail("Token aktif değil", 400); 
            }

            var user = await _userManager.FindByIdAsync(existingToken.UserId);
            if (user == null)
            {
                 return Response<TokenDto>.Fail("Kullanıcı bulunamadı", 404);
            }
            
            existingToken.Revoked = DateTime.UtcNow;
            existingToken.RevokedByIp = GetIpAddress();
            existingToken.ReasonRevoked = "Replaced by new token";
            
            _unitOfWork.RefreshTokens.Update(existingToken);

            var roles = await _userManager.GetRolesAsync(user);
            var newTokenDto = _tokenGenerator.GenerateToken(user, roles);

            var newRefreshTokenEntity = new RefreshToken
            {
                UserId = user.Id,
                Token = newTokenDto.RefreshToken,
                Expires = newTokenDto.RefreshTokenExpiration,
                CreatedDate = DateTime.UtcNow,
                CreatedByIp = GetIpAddress()
            };

            await _unitOfWork.RefreshTokens.AddAsync(newRefreshTokenEntity);
            await _unitOfWork.CommitAsync();

            return Response<TokenDto>.Success(newTokenDto, 200);
        }

        public async Task<Response<NoDataDto>> RevokeRefreshTokenAsync(string refreshToken)
        {
            var existingToken = await _unitOfWork.RefreshTokens.Where(x => x.Token == refreshToken).FirstOrDefaultAsync();

            if (existingToken == null)
            {
                return Response<NoDataDto>.Fail("Token not found", 404);
            }

            if (existingToken.IsValid)
            {
                existingToken.Revoked = DateTime.UtcNow;
                existingToken.RevokedByIp = GetIpAddress();
                existingToken.ReasonRevoked = "Revoked by user";
                
                _unitOfWork.RefreshTokens.Update(existingToken);
                await _unitOfWork.CommitAsync();
            }

            return Response<NoDataDto>.Success(200);
        }

        public async Task<Response<NoDataDto>> ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto)
        {
            var user = await _userManager.FindByEmailAsync(forgotPasswordDto.Email);
            if (user == null)
            {
                // To prevent email enumeration, we should return success even if user not found.
                // But for development/debugging, distinct messages might be helpful.
                // Standard security practice: "If the email is registered, a reset link has been sent."
                return Response<NoDataDto>.Success(200); 
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            // Save token metadata to our custom table
            var passwordResetToken = new PasswordResetToken
            {
                UserId = user.Id,
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddHours(24), // 24 saat geçerlilik süresi
                CreatedDate = DateTime.UtcNow
            };

            await _unitOfWork.PasswordResetTokens.AddAsync(passwordResetToken);
            await _unitOfWork.CommitAsync();

            // Send Email
            var clientUrl = _configuration["ClientSettings:Url"] ?? "http://localhost:3000";
            var resetLink = $"{clientUrl}/reset-password?email={user.Email}&token={Uri.EscapeDataString(token)}";
            
            await _notificationService.SendPasswordResetEmailAsync(user.Email!, resetLink);
            
            return Response<NoDataDto>.Success(200);
        }

        public async Task<Response<NoDataDto>> ResetPasswordAsync(ResetPasswordDto resetPasswordDto)
        {
            var user = await _userManager.FindByEmailAsync(resetPasswordDto.Email);
            if (user == null)
            {
                 return Response<NoDataDto>.Fail("Invalid request", 400);
            }

            var result = await _userManager.ResetPasswordAsync(user, resetPasswordDto.Token, resetPasswordDto.NewPassword);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                return Response<NoDataDto>.Fail(errors, 400);
            }

            return Response<NoDataDto>.Success(200);
        }

        public async Task<Response<NoDataDto>> ChangePasswordAsync(ChangePasswordDto changePasswordDto)
        {
            var user = await _userManager.FindByIdAsync(changePasswordDto.UserId);
            if (user == null) return Response<NoDataDto>.Fail("Kullanıcı bulunamadı", 404);

            if (changePasswordDto.NewPassword != changePasswordDto.ConfirmNewPassword)
            {
                return Response<NoDataDto>.Fail("Şifreler uyuşmuyor.", 400);
            }

            var result = await _userManager.ChangePasswordAsync(user, changePasswordDto.OldPassword, changePasswordDto.NewPassword);
            if (!result.Succeeded)
            {
               var errors = result.Errors.Select(e => e.Description).ToList();
               return Response<NoDataDto>.Fail(errors, 400);
            }

            return Response<NoDataDto>.Success(200);
        }

        public async Task<Response<NoDataDto>> LogoutAsync(string refreshToken)
        {
             // Logout logic is essentially revoking the refresh token
             return await RevokeRefreshTokenAsync(refreshToken);
        }

        private string GetIpAddress()
        {
            if (_httpContextAccessor.HttpContext?.Request.Headers.ContainsKey("X-Forwarded-For") == true)
                return _httpContextAccessor.HttpContext.Request.Headers["X-Forwarded-For"].ToString();
            
            return _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "0.0.0.0";
        }
    }
}
