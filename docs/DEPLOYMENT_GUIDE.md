# Smart Campus Backend - Deployment Guide

## Prerequisites

| Requirement | Version | Purpose |
|-------------|---------|---------|
| .NET SDK | 8.0+ | Runtime & Build |
| MySQL | 8.0+ | Database |
| Docker | 20.0+ | Containerization (optional) |

---

## Quick Start (Local Development)

### 1. Clone Repository
```bash
git clone https://github.com/your-repo/Web_Mobil_Akilli_Kampus_FINAL_PROJECT_BACKEND.git
cd Web_Mobil_Akilli_Kampus_FINAL_PROJECT_BACKEND
```

### 2. Configure Environment
```bash
# Copy example files
cp .env.example .env
cp SMARTCAMPUS.API/appsettings.example.json SMARTCAMPUS.API/appsettings.json
```

### 3. Update Connection String
Edit `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=smart_campus_db;User=root;Password=YOUR_PASSWORD;"
  }
}
```

### 4. Run Migrations
```bash
dotnet ef database update --project SMARTCAMPUS.DataAccessLayer --startup-project SMARTCAMPUS.API
```

### 5. Start Application
```bash
dotnet run --project SMARTCAMPUS.API
```

API will be available at: `https://localhost:5001` (or `http://localhost:5000`)

---

## Docker Deployment

### Development Mode
```bash
docker-compose -f docker-compose.dev.yml up --build
```

### Production Mode
```bash
docker-compose -f docker-compose.prod.yml up -d
```

### Docker Services
| Service | Port | Description |
|---------|------|-------------|
| smartcampus-api | 5000 | Backend API |
| mysql | 3306 | Database |
| nginx | 80/443 | Reverse Proxy |

---

## Environment Variables

| Variable | Required | Description |
|----------|----------|-------------|
| `ASPNETCORE_ENVIRONMENT` | Yes | Development / Production |
| `ConnectionStrings__DefaultConnection` | Yes | MySQL connection string |
| `JwtSettings__Secret` | Yes | JWT signing key (min 32 chars) |
| `JwtSettings__Issuer` | Yes | Token issuer |
| `JwtSettings__Audience` | Yes | Token audience |
| `EmailSettings__Host` | No | SMTP server |
| `EmailSettings__Port` | No | SMTP port |
| `EmailSettings__FromEmail` | No | Sender email |
| `EmailSettings__Password` | No | SMTP password |
| `IyzicoSettings__ApiKey` | No | Iyzico API key |
| `IyzicoSettings__SecretKey` | No | Iyzico secret key |

---

## Database Setup

### Create Database
```sql
CREATE DATABASE smart_campus_db CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
```

### Run Migrations
```bash
# Create migration (if needed)
dotnet ef migrations add MigrationName --project SMARTCAMPUS.DataAccessLayer --startup-project SMARTCAMPUS.API

# Apply migrations
dotnet ef database update --project SMARTCAMPUS.DataAccessLayer --startup-project SMARTCAMPUS.API
```

### Seed Data
Data seeding runs automatically on first startup:
- Default admin user
- Sample departments
- Sample students and faculty

---

## Production Checklist

### Security
- [ ] Change JWT secret key (min 32 characters)
- [ ] Configure HTTPS with valid SSL certificate
- [ ] Update CORS origins to production domains
- [ ] Set `ASPNETCORE_ENVIRONMENT=Production`
- [ ] Configure rate limiting thresholds

### Database
- [ ] Use production MySQL instance
- [ ] Enable MySQL SSL connections
- [ ] Configure database backups
- [ ] Review connection pool settings

### Monitoring
- [ ] Configure Serilog for production logging
- [ ] Set up log aggregation (e.g., Seq, ELK)
- [ ] Enable health checks endpoint

---

## API Endpoints

After deployment, access:

| URL | Description |
|-----|-------------|
| `/swagger` | API Documentation (dev only) |
| `/api/v1/auth/login` | Authentication |
| `/hubs/notifications` | SignalR WebSocket |

---

## Troubleshooting

### Database Connection Failed
```bash
# Check MySQL is running
mysql -u root -p -e "SHOW DATABASES;"

# Verify connection string format
Server=localhost;Port=3306;Database=smart_campus_db;User=root;Password=xxx;
```

### Migration Errors
```bash
# Reset migrations (development only!)
dotnet ef database drop --project SMARTCAMPUS.DataAccessLayer --startup-project SMARTCAMPUS.API
dotnet ef database update --project SMARTCAMPUS.DataAccessLayer --startup-project SMARTCAMPUS.API
```

### Port Already in Use
```bash
# Windows
netstat -ano | findstr :5000
taskkill /PID <PID> /F

# Linux/Mac
lsof -i :5000
kill -9 <PID>
```

---

## Health Check

Test API health:
```bash
curl http://localhost:5000/api/v1/auth/login -X GET
# Should return 405 (Method Not Allowed) - means API is running
```

## Frontend Integration

> ⚠️ **Frontend documentation to be completed by frontend developer**

Base API URL: `http://localhost:5000/api/v1`
SignalR Hub: `http://localhost:5000/hubs/notifications`
