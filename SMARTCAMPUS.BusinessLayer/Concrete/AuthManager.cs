using AutoMapper;
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

        public AuthManager(UserManager<User> userManager, 
                           SignInManager<User> signInManager, 
                           JwtTokenGenerator tokenGenerator, 
                           IMapper mapper, 
                           IUnitOfWork unitOfWork,
                           IHttpContextAccessor httpContextAccessor)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenGenerator = tokenGenerator;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<Response<TokenDto>> LoginAsync(LoginDto loginDto)
        {
            var user = await _userManager.FindByEmailAsync(loginDto.Email);
            if (user == null)
            {
                return Response<TokenDto>.Fail("Invalid email or password", 400); 
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);
            if (!result.Succeeded)
            {
                 return Response<TokenDto>.Fail("Invalid email or password", 400);
            }

            // Get Roles
            var roles = await _userManager.GetRolesAsync(user);

            // Generate Token
            var tokenDto = _tokenGenerator.GenerateToken(user, roles);

            // Save Refresh Token
            var refreshTokenEntity = new RefreshToken
            {
                UserId = user.Id,
                Token = tokenDto.RefreshToken,
                Expires = tokenDto.RefreshTokenExpiration,
                CreatedAt = DateTime.UtcNow, 
                CreatedByIp = GetIpAddress()
            };

            await _unitOfWork.RefreshTokens.AddAsync(refreshTokenEntity);
            await _unitOfWork.CommitAsync();

            return Response<TokenDto>.Success(tokenDto, 200);
        }

        public async Task<Response<TokenDto>> RegisterStudentAsync(RegisterStudentDto registerDto)
        {
            // 1. Check if user exists
            var userExists = await _userManager.FindByEmailAsync(registerDto.Email);
            if (userExists != null)
            {
                return Response<TokenDto>.Fail("User with this email already exists", 400);
            }

            // 2. Create Transaction
            // Since Identity and EF Core share the same context in this setup (CampusContext), 
            // we can use a transaction to ensure both User and Student are created or neither.
            
            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try 
            {
                // 3. Create Identity User
                var user = _mapper.Map<User>(registerDto);
                user.UserName = registerDto.Email; 
                user.CreatedAt = DateTime.UtcNow;
                user.IsActive = true;

                var result = await _userManager.CreateAsync(user, registerDto.Password);
                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(e => e.Description).ToList();
                    return Response<TokenDto>.Fail(errors, 400);
                }

                await _userManager.AddToRoleAsync(user, "Student");

                // 4. Create Student Entity
                var student = new Student
                {
                    UserId = user.Id,
                    StudentNumber = registerDto.StudentNumber,
                    DepartmentId = registerDto.DepartmentId,
                    // FacultyId could be derived from Department -> Faculty relation if needed, 
                    // or let database/navigation handle it if designed that way. 
                    // For now, only DepartmentId is mandatory in DTO.
                };

                await _unitOfWork.Students.AddAsync(student);
                await _unitOfWork.CommitAsync();

                await transaction.CommitAsync();

                // 5. Generate Token
                var roles = new List<string> { "Student" };
                var tokenDto = _tokenGenerator.GenerateToken(user, roles);

                // Save Refresh Token
                var refreshTokenEntity = new RefreshToken
                {
                    UserId = user.Id,
                    Token = tokenDto.RefreshToken,
                    Expires = tokenDto.RefreshTokenExpiration,
                    CreatedAt = DateTime.UtcNow,
                    CreatedByIp = GetIpAddress() 
                };

                await _unitOfWork.RefreshTokens.AddAsync(refreshTokenEntity);
                await _unitOfWork.CommitAsync();

                return Response<TokenDto>.Success(tokenDto, 201);
            }
            catch (Exception ex)
            {
               await transaction.RollbackAsync();
               // Log exception
               return Response<TokenDto>.Fail($"Registration failed: {ex.Message}", 500);
            }
        }

        public async Task<Response<TokenDto>> CreateTokenByRefreshTokenAsync(string refreshToken)
        {
            var existingToken = await _unitOfWork.RefreshTokens.Where(x => x.Token == refreshToken).FirstOrDefaultAsync();

            if (existingToken == null)
            {
                return Response<TokenDto>.Fail("Token not found", 404);
            }

            if (!existingToken.IsActive) // Revoked or Expired
            {
               return Response<TokenDto>.Fail("Token is not active", 400); 
            }

            var user = await _userManager.FindByIdAsync(existingToken.UserId);
            if (user == null)
            {
                 return Response<TokenDto>.Fail("User not found", 404);
            }

            // Logic: Rotate Refresh Token?
            // Yes, standard practice: Revoke old, issue new.
            
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
                CreatedAt = DateTime.UtcNow,
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

            if (existingToken.IsActive)
            {
                existingToken.Revoked = DateTime.UtcNow;
                existingToken.RevokedByIp = GetIpAddress();
                existingToken.ReasonRevoked = "Revoked by user";
                
                _unitOfWork.RefreshTokens.Update(existingToken);
                await _unitOfWork.CommitAsync();
            }

            return Response<NoDataDto>.Success(200);
        }

        private string GetIpAddress()
        {
            if (_httpContextAccessor.HttpContext?.Request.Headers.ContainsKey("X-Forwarded-For") == true)
                return _httpContextAccessor.HttpContext.Request.Headers["X-Forwarded-For"].ToString();
            
            return _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "0.0.0.0";
        }
    }
}
