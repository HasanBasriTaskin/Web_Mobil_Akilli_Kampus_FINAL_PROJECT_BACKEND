using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SMARTCAMPUS.DataAccessLayer.Context;
using SMARTCAMPUS.EntityLayer.Models;
using SMARTCAMPUS.EntityLayer.DTOs;
using Serilog;
using SMARTCAMPUS.DataAccessLayer.Abstract;
using SMARTCAMPUS.DataAccessLayer.Concrete;
using SMARTCAMPUS.DataAccessLayer;
using FluentValidation;
using FluentValidation.AspNetCore;
using SMARTCAMPUS.BusinessLayer.Mappings;
using SMARTCAMPUS.BusinessLayer.ValidationRules.Auth;
using SMARTCAMPUS.BusinessLayer.Tools;
using SMARTCAMPUS.BusinessLayer.Abstract;
using SMARTCAMPUS.BusinessLayer.Concrete;
using SMARTCAMPUS.BusinessLayer.Common;
using SMARTCAMPUS.API.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text; 

// Bootstrap logger for catching startup errors
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting SmartCampus API...");

    var builder = WebApplication.CreateBuilder(args);

    // 1. Configure Serilog
    builder.Host.UseSerilog((context, services, configuration) =>
        configuration.ReadFrom.Configuration(context.Configuration));

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<SMARTCAMPUS.BusinessLayer.Tools.UserClaimsHelper>();

// Add services to the container.

// 2. Database Connection
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<CampusContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// 3. Identity Configuration
builder.Services.AddIdentity<User, Role>(options =>
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 8;

    // User settings
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedEmail = true; // For Part 1 requirement
})
.AddEntityFrameworkStores<CampusContext>()
.AddDefaultTokenProviders();

// 3.1. JWT Authentication Configuration
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
        ValidAudience = builder.Configuration["JwtSettings:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Secret"]!))
    };
});

// 4. Repository & Unit of Work Registration
builder.Services.AddScoped(typeof(IGenericDal<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddScoped<IStudentDal, EfStudentDal>();
builder.Services.AddScoped<IFacultyDal, EfFacultyDal>();
builder.Services.AddScoped<IDepartmentDal, EfDepartmentDal>();

builder.Services.AddScoped<IRefreshTokenDal, EfRefreshTokenDal>();
builder.Services.AddScoped<IPasswordResetTokenDal, EfPasswordResetTokenDal>();
builder.Services.AddScoped<IEmailVerificationTokenDal, EfEmailVerificationTokenDal>();

// Academic Management DALs
builder.Services.AddScoped<ICourseDal, EfCourseDal>();
builder.Services.AddScoped<ICourseSectionDal, EfCourseSectionDal>();
builder.Services.AddScoped<IEnrollmentDal, EfEnrollmentDal>();
builder.Services.AddScoped<IAttendanceSessionDal, EfAttendanceSessionDal>();
builder.Services.AddScoped<IAttendanceRecordDal, EfAttendanceRecordDal>();
builder.Services.AddScoped<IExcuseRequestDal, EfExcuseRequestDal>();
builder.Services.AddScoped<IClassroomDal, EfClassroomDal>();
builder.Services.AddScoped<IAcademicCalendarDal, EfAcademicCalendarDal>();
builder.Services.AddScoped<IAnnouncementDal, EfAnnouncementDal>();

// 5. Business Layer Services (AutoMapper & FluentValidation & Tools)
builder.Services.AddAutoMapper(typeof(MappingProfile));
builder.Services.AddScoped<JwtTokenGenerator>();
builder.Services.AddScoped<IAuthService, AuthManager>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IDepartmentService, DepartmentManager>();
builder.Services.AddScoped<INotificationService, EmailService>();

// Academic Management Services
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<ICourseSectionService, CourseSectionService>();
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();
builder.Services.AddScoped<IGradeService, GradeService>();
builder.Services.AddScoped<IAttendanceService, AttendanceService>();
builder.Services.AddScoped<IExcuseRequestService, ExcuseRequestService>();
builder.Services.AddScoped<ITranscriptService, TranscriptService>();
builder.Services.AddScoped<IAcademicCalendarService, AcademicCalendarService>();
builder.Services.AddScoped<IAnnouncementService, AnnouncementService>();

// Background Jobs
builder.Services.AddHostedService<SMARTCAMPUS.BusinessLayer.Jobs.AbsenceWarningJob>();

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<LoginValidator>(); // Scans assembly for all AbstractValidator<T>

builder.Services.AddControllers();

// Configure ApiBehaviorOptions to use Response<T> format for validation errors
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(e => e.Value?.Errors.Count > 0)
            .SelectMany(e => e.Value!.Errors.Select(error =>
                string.IsNullOrEmpty(error.ErrorMessage)
                    ? e.Key + " is invalid."
                    : error.ErrorMessage))
            .ToList();

        var response = Response<NoDataDto>.Fail(errors, 400);
        return new BadRequestObjectResult(response);
    };
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "SmartCampus API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// 6. CORS Configuration
var clientUrl = builder.Configuration["ClientSettings:Url"] ?? "http://localhost:3000";
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowClient",
        b => b.WithOrigins(clientUrl)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials());
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Enable serving static files from wwwroot
app.UseStaticFiles();

// Enable serving static files from uploads folder
var uploadsPath = Path.Combine(app.Environment.ContentRootPath, "uploads");
if (!Directory.Exists(uploadsPath))
{
    Directory.CreateDirectory(uploadsPath);
}
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(uploadsPath),
    RequestPath = "/uploads"
});

app.UseCors("AllowClient");

// Enable Serilog request logging
app.UseSerilogRequestLogging();

// Add Global Exception Middleware
app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseAuthentication(); // Must be before Authorization
app.UseAuthorization();

app.MapControllers();

// Run Seeder
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await DataSeeder.SeedAsync(services);
}

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
