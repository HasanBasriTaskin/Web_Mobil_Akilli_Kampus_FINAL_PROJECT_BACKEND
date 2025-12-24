# Smart Campus Backend - System Architecture

## Overview

Smart Campus is a comprehensive university management system built with **ASP.NET Core 8** using **N-Layer Architecture**. The system provides APIs for student management, course enrollment, attendance tracking, cafeteria services, event management, and real-time notifications.

---

## Technology Stack

| Component | Technology | Version |
|-----------|------------|---------|
| Framework | ASP.NET Core | 8.0 |
| ORM | Entity Framework Core | 8.0 |
| Database | MySQL | 8.0 |
| Authentication | JWT + ASP.NET Identity | - |
| Real-time | SignalR | - |
| Validation | FluentValidation | - |
| Mapping | AutoMapper | - |
| Logging | Serilog | - |
| Excel Export | ClosedXML | 0.102.2 |
| PDF Export | QuestPDF | 2024.10.2 |
| Rate Limiting | AspNetCoreRateLimit | 5.0.0 |
| Payment | Iyzico (Sandbox) | - |
| Containerization | Docker | - |

---

## Project Structure

```
SMARTCAMPUS.sln
├── SMARTCAMPUS.API/                 # Presentation Layer
│   ├── Controllers/                 # 24 API Controllers
│   ├── Hubs/                        # SignalR Hubs
│   ├── BackgroundServices/          # Hosted Services
│   ├── Middleware/                  # Custom Middleware
│   └── Services/                    # API-level Services
│
├── SMARTCAMPUS.BusinessLayer/       # Business Logic Layer
│   ├── Abstract/                    # 28 Service Interfaces
│   ├── Concrete/                    # Service Implementations
│   ├── Common/                      # Response<T>, NoDataDto
│   ├── ValidationRules/             # FluentValidation Rules
│   ├── Mappings/                    # AutoMapper Profiles
│   └── Tools/                       # JwtTokenGenerator, etc.
│
├── SMARTCAMPUS.DataAccessLayer/     # Data Access Layer
│   ├── Abstract/                    # Repository Interfaces
│   ├── Concrete/                    # EF Core Repositories
│   ├── Context/                     # CampusContext (DbContext)
│   └── Migrations/                  # EF Core Migrations
│
├── SMARTCAMPUS.EntityLayer/         # Entity Layer
│   ├── Models/                      # 35 Entity Models
│   ├── DTOs/                        # Data Transfer Objects
│   └── Enums/                       # Enumerations
│
└── SMARTCAMPUS.Tests/               # Unit & Integration Tests
```

---

## N-Layer Architecture Diagram

```
┌─────────────────────────────────────────────────────────────────┐
│                        CLIENT APPLICATIONS                       │
│              (Web Frontend, Mobile App, Admin Panel)            │
└─────────────────────────────────────────────────────────────────┘
                                  │
                                  ▼
┌─────────────────────────────────────────────────────────────────┐
│                     PRESENTATION LAYER (API)                     │
│  ┌─────────────┐ ┌─────────────┐ ┌─────────────┐ ┌───────────┐  │
│  │ Controllers │ │  SignalR    │ │ Middleware  │ │Background │  │
│  │  (24 total) │ │    Hubs     │ │  (Global)   │ │ Services  │  │
│  └─────────────┘ └─────────────┘ └─────────────┘ └───────────┘  │
└─────────────────────────────────────────────────────────────────┘
                                  │
                                  ▼
┌─────────────────────────────────────────────────────────────────┐
│                      BUSINESS LOGIC LAYER                        │
│  ┌─────────────┐ ┌─────────────┐ ┌─────────────┐ ┌───────────┐  │
│  │  Services   │ │ Validation  │ │  AutoMapper │ │   JWT     │  │
│  │ (28 total)  │ │   Rules     │ │  Profiles   │ │ Generator │  │
│  └─────────────┘ └─────────────┘ └─────────────┘ └───────────┘  │
└─────────────────────────────────────────────────────────────────┘
                                  │
                                  ▼
┌─────────────────────────────────────────────────────────────────┐
│                      DATA ACCESS LAYER                           │
│  ┌─────────────┐ ┌─────────────┐ ┌─────────────┐                │
│  │ Repositories│ │  DbContext  │ │ Data Seeder │                │
│  │  (Generic)  │ │ (EF Core)   │ │             │                │
│  └─────────────┘ └─────────────┘ └─────────────┘                │
└─────────────────────────────────────────────────────────────────┘
                                  │
                                  ▼
┌─────────────────────────────────────────────────────────────────┐
│                        ENTITY LAYER                              │
│  ┌─────────────┐ ┌─────────────┐ ┌─────────────┐                │
│  │  Entities   │ │    DTOs     │ │    Enums    │                │
│  │ (35 total)  │ │             │ │             │                │
│  └─────────────┘ └─────────────┘ └─────────────┘                │
└─────────────────────────────────────────────────────────────────┘
                                  │
                                  ▼
┌─────────────────────────────────────────────────────────────────┐
│                          MySQL DATABASE                          │
└─────────────────────────────────────────────────────────────────┘
```

---

## Module Overview

### Part 1: Authentication & User Management
- JWT-based authentication with refresh tokens
- Role-based authorization (Admin, Faculty, Student)
- Email verification and password reset
- User profile management

### Part 2: Academic Management
- Course and section management
- Student enrollment with quota control
- Grade management (Midterm, Final, Letter Grade)
- GPS-based attendance with QR codes
- Excuse request workflow

### Part 3: Campus Services
- Cafeteria and meal menu management
- Wallet system with Iyzico payment integration
- Meal reservations with nutritional info
- Event management with registration/waitlist
- Automatic schedule optimization

### Part 4: Final Integration
- Admin dashboard analytics
- Excel/PDF report exports
- SignalR real-time notifications
- Background services (attendance warnings, event reminders)
- Rate limiting for API protection

---

## API Endpoints Summary

| Module | Base Route | Controllers |
|--------|------------|-------------|
| Auth | `/api/v1/auth` | AuthController |
| Users | `/api/v1/users` | UsersController |
| Courses | `/api/v1/courses` | CoursesController, SectionsController |
| Enrollment | `/api/v1/enrollments` | EnrollmentsController |
| Attendance | `/api/v1/attendance` | AttendanceController, ExcuseRequestsController |
| Cafeteria | `/api/v1/cafeterias` | CafeteriasController, FoodItemsController |
| Meals | `/api/v1/mealmenus` | MealMenusController, MealReservationsController |
| Wallet | `/api/v1/wallet` | WalletController |
| Events | `/api/v1/events` | EventsController, EventCategoriesController |
| Schedules | `/api/v1/schedules` | SchedulesController, ClassroomReservationsController |
| Analytics | `/api/v1/analytics` | AnalyticsController |
| Reports | `/api/v1/reports` | ReportsController |
| Notifications | `/api/v1/notifications` | NotificationsController |

---

## Security Features

1. **JWT Authentication** - Access tokens (15 min) + Refresh tokens (7 days)
2. **Role-Based Access Control** - Admin, Faculty, Student roles
3. **Rate Limiting** - 100 req/min, 1000 req/hour per IP
4. **Input Validation** - FluentValidation on all DTOs
5. **CORS Policy** - Configured for allowed origins
6. **Password Hashing** - ASP.NET Identity with BCrypt

---

## Real-time Features

### SignalR Hub
- **Endpoint**: `/hubs/notifications`
- **Features**: User-based groups, role-based broadcast
- **Events**: `ReceiveNotification`

### Background Services
| Service | Interval | Purpose |
|---------|----------|---------|
| AttendanceWarningService | Daily 08:00 | Warn students with 20%+ absenteeism |
| EventReminderService | Every 30 min | Send 24h and 1h event reminders |

---

## Database

- **Engine**: MySQL 8.0
- **ORM**: Entity Framework Core 8
- **Tables**: 35+ tables across all modules
- **Migrations**: Code-first with `dotnet ef migrations`

See [DATABASE_SCHEMA.md](DATABASE_SCHEMA.md) for complete table documentation.

---

## Deployment Options

1. **Development**: `dotnet run --project SMARTCAMPUS.API`
2. **Docker**: `docker-compose -f docker-compose.dev.yml up`
3. **Production**: `docker-compose -f docker-compose.prod.yml up`

See [DEPLOYMENT_GUIDE.md](DEPLOYMENT_GUIDE.md) for detailed instructions.
