# Smart Campus Database Schema - Part 4 Tables

## Overview

Part 4 introduces notification tables for real-time notifications and IoT sensor tables for campus monitoring.

---

## Notifications Table

Stores all user notifications.

```sql
CREATE TABLE Notifications (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Title VARCHAR(200) NOT NULL,
    Message VARCHAR(1000) NOT NULL,
    Type INT NOT NULL DEFAULT 0,           -- NotificationType enum
    Category INT NOT NULL DEFAULT 0,       -- NotificationCategory enum
    IsRead TINYINT(1) NOT NULL DEFAULT 0,
    ReadAt DATETIME NULL,
    RelatedEntityType VARCHAR(50) NULL,
    RelatedEntityId INT NULL,
    UserId VARCHAR(255) NOT NULL,
    CreatedDate DATETIME NOT NULL,
    UpdatedDate DATETIME NULL,
    IsActive TINYINT(1) NOT NULL DEFAULT 1,
    
    FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id) ON DELETE CASCADE,
    INDEX idx_notifications_user_read (UserId, IsRead),
    INDEX idx_notifications_created (CreatedDate)
);
```

### NotificationType Enum
| Value | Name | Description |
|-------|------|-------------|
| 0 | Info | General information |
| 1 | Warning | Warning message |
| 2 | Error | Error notification |
| 3 | Success | Success message |
| 4 | Reminder | Scheduled reminder |

### NotificationCategory Enum
| Value | Name | Description |
|-------|------|-------------|
| 0 | System | System notifications |
| 1 | Academic | Course, grade related |
| 2 | Attendance | Attendance warnings |
| 3 | Event | Event reminders |
| 4 | Payment | Payment confirmations |
| 5 | Meal | Meal reservations |

---

## NotificationPreferences Table

Stores user preferences for notification channels.

```sql
CREATE TABLE NotificationPreferences (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Category INT NOT NULL,                 -- NotificationCategory enum
    InAppEnabled TINYINT(1) NOT NULL DEFAULT 1,
    EmailEnabled TINYINT(1) NOT NULL DEFAULT 1,
    UserId VARCHAR(255) NOT NULL,
    CreatedDate DATETIME NOT NULL,
    UpdatedDate DATETIME NULL,
    IsActive TINYINT(1) NOT NULL DEFAULT 1,
    
    FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id) ON DELETE CASCADE,
    UNIQUE KEY uk_user_category (UserId, Category)
);
```

---

## Entity Relationships

```
┌──────────────────┐
│   AspNetUsers    │
│    (User)        │
└────────┬─────────┘
         │
         │ 1:N
         ▼
┌──────────────────┐     ┌──────────────────────────┐
│  Notifications   │     │  NotificationPreferences │
│                  │     │                          │
│  - Title         │     │  - Category              │
│  - Message       │     │  - InAppEnabled          │
│  - Type          │     │  - EmailEnabled          │
│  - Category      │     │                          │
│  - IsRead        │     └──────────────────────────┘
└──────────────────┘
```

---

## Recommended Indexes

```sql
-- Fast lookup for unread notifications
CREATE INDEX idx_notifications_unread 
ON Notifications(UserId, IsActive, IsRead);

-- Cleanup old notifications
CREATE INDEX idx_notifications_cleanup 
ON Notifications(CreatedDate, IsRead);

-- User preferences lookup
CREATE INDEX idx_preferences_user 
ON NotificationPreferences(UserId, IsActive);
```

---

## Migration Command

```bash
dotnet ef migrations add Part4_NotificationSystem \
  --project SMARTCAMPUS.DataAccessLayer \
  --startup-project SMARTCAMPUS.API

dotnet ef database update \
  --project SMARTCAMPUS.DataAccessLayer \
  --startup-project SMARTCAMPUS.API
```

---

## Related Entities from Previous Parts

Part 4 analytics queries use these existing tables:
- `Students` - GPA, attendance calculations
- `Enrollments` - Grade distribution, pass/fail rates
- `Departments` - Department-wise statistics
- `CourseSections` - Occupancy rates
- `AttendanceSessions` / `AttendanceRecords` - Attendance stats
- `Events` / `EventRegistrations` - Event reminders

---

## IoT Sensor Tables

### Sensors Table

```sql
CREATE TABLE Sensors (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    SensorId VARCHAR(50) NOT NULL,
    Name VARCHAR(100) NOT NULL,
    Type INT NOT NULL DEFAULT 0,           -- SensorType enum
    Location VARCHAR(100) NULL,
    ClassroomId INT NULL,
    IsOnline TINYINT(1) NOT NULL DEFAULT 1,
    LastReading DATETIME NULL,
    CreatedDate DATETIME NOT NULL,
    UpdatedDate DATETIME NULL,
    IsActive TINYINT(1) NOT NULL DEFAULT 1,
    
    FOREIGN KEY (ClassroomId) REFERENCES Classrooms(Id),
    UNIQUE KEY uk_sensor_id (SensorId),
    INDEX idx_sensor_type (Type)
);
```

### SensorReadings Table

```sql
CREATE TABLE SensorReadings (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    SensorId INT NOT NULL,
    Value DOUBLE NOT NULL,
    Unit VARCHAR(20) NULL,
    Timestamp DATETIME NOT NULL,
    CreatedDate DATETIME NOT NULL,
    UpdatedDate DATETIME NULL,
    IsActive TINYINT(1) NOT NULL DEFAULT 1,
    
    FOREIGN KEY (SensorId) REFERENCES Sensors(Id) ON DELETE CASCADE,
    INDEX idx_reading_sensor_time (SensorId, Timestamp DESC)
);
```

### SensorType Enum
| Value | Name | Description |
|-------|------|-------------|
| 0 | Temperature | Temperature sensors (°C) |
| 1 | Humidity | Humidity sensors (%) |
| 2 | Occupancy | Room occupancy (%) |
| 3 | Energy | Energy consumption |
| 4 | AirQuality | Air quality index |
| 5 | Light | Light level (lux) |

---

## IoT Sensor Migration

```bash
dotnet ef migrations add IoT_Sensors \
  --project SMARTCAMPUS.DataAccessLayer \
  --startup-project SMARTCAMPUS.API

dotnet ef database update \
  --project SMARTCAMPUS.DataAccessLayer \
  --startup-project SMARTCAMPUS.API
```
