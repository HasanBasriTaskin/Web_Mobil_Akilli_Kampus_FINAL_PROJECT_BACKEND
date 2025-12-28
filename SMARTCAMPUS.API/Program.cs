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
using SMARTCAMPUS.API.Services;
using SMARTCAMPUS.API.Hubs;
using AspNetCoreRateLimit;
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

// Rate Limiting Configuration
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.Configure<IpRateLimitPolicies>(builder.Configuration.GetSection("IpRateLimitPolicies"));
builder.Services.AddInMemoryRateLimiting();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

// Add services to the container.

// 2. Database Connection
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<CampusContext>(options =>
    options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 0))));

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

// Part 2 Repositories
builder.Services.AddScoped<ICourseDal, EfCourseDal>();
builder.Services.AddScoped<ICourseSectionDal, EfCourseSectionDal>();
builder.Services.AddScoped<ICoursePrerequisiteDal, EfCoursePrerequisiteDal>();
builder.Services.AddScoped<IEnrollmentDal, EfEnrollmentDal>();
builder.Services.AddScoped<IAttendanceSessionDal, EfAttendanceSessionDal>();
builder.Services.AddScoped<IAttendanceRecordDal, EfAttendanceRecordDal>();
builder.Services.AddScoped<IExcuseRequestDal, EfExcuseRequestDal>();
builder.Services.AddScoped<IClassroomDal, EfClassroomDal>();

// Part 3 Repositories
builder.Services.AddScoped<ICafeteriaDal, EfCafeteriaDal>();
builder.Services.AddScoped<IFoodItemDal, EfFoodItemDal>();
builder.Services.AddScoped<IMealMenuDal, EfMealMenuDal>();
builder.Services.AddScoped<IMealMenuItemDal, EfMealMenuItemDal>();
builder.Services.AddScoped<IMealNutritionDal, EfMealNutritionDal>();
builder.Services.AddScoped<IMealReservationDal, EfMealReservationDal>();
builder.Services.AddScoped<IWalletDal, EfWalletDal>();
builder.Services.AddScoped<IWalletTransactionDal, EfWalletTransactionDal>();
builder.Services.AddScoped<IEventCategoryDal, EfEventCategoryDal>();
builder.Services.AddScoped<IEventDal, EfEventDal>();
builder.Services.AddScoped<IEventRegistrationDal, EfEventRegistrationDal>();
builder.Services.AddScoped<IEventWaitlistDal, EfEventWaitlistDal>();
builder.Services.AddScoped<IScheduleDal, EfScheduleDal>();
builder.Services.AddScoped<IClassroomReservationDal, EfClassroomReservationDal>();

// 5. Business Layer Services (AutoMapper & FluentValidation & Tools)
builder.Services.AddAutoMapper(typeof(MappingProfile));
builder.Services.AddScoped<JwtTokenGenerator>();
builder.Services.AddScoped<IAuthService, AuthManager>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IDepartmentService, DepartmentManager>();

// Email Configuration
builder.Services.Configure<SMARTCAMPUS.EntityLayer.Configuration.EmailSettings>(
    builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddScoped<INotificationService, EmailService>();

// Part 2 Services
builder.Services.AddScoped<ICourseService, CourseManager>();
builder.Services.AddScoped<IEnrollmentService, EnrollmentManager>();
builder.Services.AddScoped<IAttendanceService, AttendanceManager>();
builder.Services.AddScoped<IFacultyRequestService, FacultyRequestManager>();

// Part 3 Services
builder.Services.AddScoped<IQRCodeService, QRCodeManager>();
builder.Services.AddScoped<ICafeteriaService, CafeteriaManager>();
builder.Services.AddScoped<IFoodItemService, FoodItemManager>();
builder.Services.AddScoped<IMealMenuService, MealMenuManager>();
    builder.Services.AddScoped<IMockPaymentService, MockPaymentManager>();
    builder.Services.AddScoped<IPaymentService, IyzicoPaymentManager>();
    builder.Services.AddScoped<IWalletService, WalletManager>();
builder.Services.AddScoped<IMealReservationService, MealReservationManager>();
builder.Services.AddScoped<IEventCategoryService, EventCategoryManager>();
builder.Services.AddScoped<IEventService, EventManager>();
builder.Services.AddScoped<IScheduleService, ScheduleManager>();
builder.Services.AddScoped<IClassroomReservationService, ClassroomReservationManager>();

// Part 4 Services
builder.Services.AddScoped<IAnalyticsService, AnalyticsManager>();
builder.Services.AddScoped<IReportExportService, ReportExportManager>();
builder.Services.AddScoped<IAdvancedNotificationService, AdvancedNotificationManager>();

// SignalR Configuration
builder.Services.AddSignalR();

// Background Services
builder.Services.AddHostedService<SMARTCAMPUS.API.BackgroundServices.AttendanceWarningService>();
builder.Services.AddHostedService<SMARTCAMPUS.API.BackgroundServices.EventReminderService>();
builder.Services.AddHostedService<SMARTCAMPUS.API.BackgroundServices.SensorSimulationService>();

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

// Rate Limiting Middleware
app.UseIpRateLimiting();

// Enable Serilog request logging
app.UseSerilogRequestLogging();

// Add Global Exception Middleware
app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseAuthentication(); // Must be before Authorization
app.UseAuthorization();

app.MapControllers();

// Map SignalR Hub
app.MapHub<NotificationHub>("/hubs/notifications");

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
