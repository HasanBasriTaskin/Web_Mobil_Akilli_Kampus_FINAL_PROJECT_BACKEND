# Smart Campus User Manual - Part 4

## Overview

Part 4 adds analytics dashboards, report exports, real-time notifications, and automated background services.

---

## Analytics Dashboard (Admin Only)

### Dashboard Overview
```http
GET /api/v1/analytics/dashboard
```
Shows:
- Total users, students, faculty counts
- Active enrollments and course occupancy
- Upcoming events count
- System health status

### Academic Performance Reports
```http
GET /api/v1/analytics/academic-performance
GET /api/v1/analytics/department-stats
GET /api/v1/analytics/grade-distribution
```

### At-Risk Student Monitoring
```http
GET /api/v1/analytics/at-risk-students?gpaThreshold=2.0&attendanceThreshold=20
```
Identifies students with:
- GPA below threshold (default 2.0)
- Absenteeism above threshold (default 20%)

---

## Report Exports

### Excel Reports (Admin/Faculty)

| Report | Endpoint |
|--------|----------|
| Student List | `GET /api/v1/reports/students/excel` |
| Grade Report | `GET /api/v1/reports/grades/{sectionId}/excel` |
| At-Risk Students | `GET /api/v1/reports/at-risk-students/excel` |

### PDF Reports

| Report | Endpoint |
|--------|----------|
| Transcript | `GET /api/v1/reports/transcript/{studentId}/pdf` |
| Attendance Report | `GET /api/v1/reports/attendance/{sectionId}/pdf` |

---

## Notification System

### View Notifications
```http
GET /api/v1/notifications?page=1&pageSize=20
```

### Check Unread Count
```http
GET /api/v1/notifications/unread-count
```

### Mark as Read
```http
PUT /api/v1/notifications/{id}/read
PUT /api/v1/notifications/read-all
```

### Manage Preferences
```http
GET /api/v1/notifications/preferences
PUT /api/v1/notifications/preferences
```

**Request Body (PUT):**
```json
{
  "preferences": [
    { "category": 1, "inAppEnabled": true, "emailEnabled": false },
    { "category": 2, "inAppEnabled": true, "emailEnabled": true }
  ]
}
```

### Admin: Send Notifications
```http
POST /api/v1/notifications/send
POST /api/v1/notifications/broadcast
```

---

## Real-time Notifications (SignalR)

### Connection
```javascript
const connection = new signalR.HubConnectionBuilder()
  .withUrl("/hubs/notifications", {
    accessTokenFactory: () => token
  })
  .build();

connection.on("ReceiveNotification", (notification) => {
  console.log("New notification:", notification);
});

await connection.start();
```

---

## Automatic Background Services

### Attendance Warning Service
- **Schedule**: Daily at 08:00
- **Action**: Warns students with 20%+ absenteeism per course
- **Notification Type**: Warning / Attendance category

### Event Reminder Service
- **Schedule**: Every 30 minutes
- **Actions**:
  - 24-hour reminder before event start
  - 1-hour reminder before event start
- **Notification Type**: Reminder / Event category

---

## Rate Limiting

API requests are limited to:
- **100 requests per minute** per IP
- **1000 requests per hour** per IP

Exceeding limits returns HTTP 429 (Too Many Requests).

---

## IoT Sensor Monitoring

### Sensor Dashboard
```http
GET /api/v1/sensors/dashboard
```
Returns real-time sensor data including:
- Temperature readings (°C)
- Classroom occupancy (%)
- Environment summary (avg, min, max)

### List All Sensors
```http
GET /api/v1/sensors
```

### Sensor Reading History
```http
GET /api/v1/sensors/{sensorId}/readings?limit=50
```

### Real-time Sensor Updates (SignalR)
```javascript
connection.on("SensorUpdate", (data) => {
  console.log("Sensor data:", data);
  // data.latestReadings - Array of sensor readings
  // data.environment - Temperature/occupancy summary
});
```

**Update Frequency**: Every 10 seconds (simulated)

---

## Frontend Integration Notes

> ⚠️ **Section to be completed by frontend developer**

### Suggested Components
- [ ] Admin Dashboard with charts
- [ ] Notification bell with unread count badge
- [ ] Notification preferences settings page
- [ ] Report download buttons
- [ ] IoT Sensor dashboard with real-time gauges

### SignalR Setup
- Connect on login, disconnect on logout
- Handle reconnection on connection loss
- Update UI immediately on notification receive
- Listen for `SensorUpdate` events for IoT dashboard
