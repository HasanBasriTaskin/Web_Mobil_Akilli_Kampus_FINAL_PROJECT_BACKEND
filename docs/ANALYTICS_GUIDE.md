# Smart Campus Analytics & Reporting Guide

## Overview

Part 4 introduces comprehensive analytics and reporting features for administrators to monitor campus activities, academic performance, and generate exportable reports.

---

## Analytics API Endpoints

Base URL: `/api/v1/analytics`

### Dashboard Statistics
```http
GET /api/v1/analytics/dashboard
Authorization: Bearer <admin_token>
```

**Response:**
```json
{
  "data": {
    "totalUsers": 150,
    "totalStudents": 120,
    "totalFaculty": 25,
    "dailyActiveUsers": 85,
    "totalCourses": 45,
    "totalCourseSections": 78,
    "activeEnrollments": 450,
    "averageClassOccupancy": 72.5,
    "totalEvents": 12,
    "upcomingEvents": 5,
    "systemHealth": {
      "databaseStatus": "Healthy",
      "apiStatus": "Running",
      "lastChecked": "2024-12-24T10:00:00Z"
    }
  }
}
```

### Academic Performance
```http
GET /api/v1/analytics/academic-performance
```
Returns overall GPA, pass/fail rates, and department-wise breakdown.

### Department Statistics
```http
GET /api/v1/analytics/department-stats
GET /api/v1/analytics/department/{departmentId}
```
Returns GPA averages and student counts per department.

### Grade Distribution
```http
GET /api/v1/analytics/grade-distribution?sectionId=5
```
Returns letter grade distribution (AA, BA, BB, etc.) with percentages.

### At-Risk Students
```http
GET /api/v1/analytics/at-risk-students?gpaThreshold=2.0&attendanceThreshold=20
```
Returns students with low GPA or high absenteeism.

### Course Occupancy
```http
GET /api/v1/analytics/course-occupancy
```
Returns enrollment vs capacity for all course sections.

### Attendance Statistics
```http
GET /api/v1/analytics/attendance-stats
```
Returns overall and per-section attendance rates.

---

## Report Export Endpoints

Base URL: `/api/v1/reports`

### Excel Reports

| Endpoint | Description |
|----------|-------------|
| `GET /students/excel` | Student list (all or by department) |
| `GET /grades/{sectionId}/excel` | Grade report for a section |
| `GET /at-risk-students/excel` | At-risk students list |

**Example:**
```http
GET /api/v1/reports/students/excel?departmentId=3
Authorization: Bearer <token>
```
*Response: Excel file (.xlsx) download*

### PDF Reports

| Endpoint | Description |
|----------|-------------|
| `GET /transcript/{studentId}/pdf` | Student transcript |
| `GET /attendance/{sectionId}/pdf` | Attendance report |

**Example:**
```http
GET /api/v1/reports/transcript/42/pdf
Authorization: Bearer <token>
```
*Response: PDF file download*

---

## Authorization

| Role | Access |
|------|--------|
| Admin | All analytics and reports |
| Faculty | Reports for their courses |
| Student | Own transcript only |

---

## Libraries Used

| Library | Purpose |
|---------|---------|
| ClosedXML | Excel file generation |
| QuestPDF | PDF document generation |

---

## Frontend Integration Notes

> ⚠️ **To be completed by frontend developer**

### Dashboard Component
- Fetch: `GET /api/v1/analytics/dashboard`
- Display charts for occupancy, GPA distribution, attendance

### Report Downloads
- Use `window.open()` or `fetch` with blob handling
- Set appropriate headers for file download

```javascript
// Example: Download Excel report
async function downloadStudentList() {
  const response = await fetch('/api/v1/reports/students/excel', {
    headers: { 'Authorization': `Bearer ${token}` }
  });
  const blob = await response.blob();
  const url = window.URL.createObjectURL(blob);
  const a = document.createElement('a');
  a.href = url;
  a.download = 'students.xlsx';
  a.click();
}
```
