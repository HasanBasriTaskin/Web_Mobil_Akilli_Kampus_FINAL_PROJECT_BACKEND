# 🔌 API Documentation - Part 1

Base URL: `https://localhost:7123/api/v1`

## 🔐 Authentication Endpoints (`/Auth`)

### 1. Register (Kayıt Ol)
Kullanıcı (Öğrenci veya Akademisyen) kaydı oluşturur ve doğrulama emaili gönderir.

*   **URL**: `/Auth/register`
*   **Method**: `POST`
*   **Body**:
    ```json
    {
      "email": "student@smartcampus.edu",
      "password": "Password123!",
      "fullName": "Ahmet Yılmaz",
      "userType": "Student", // "Student" veya "Faculty"
      // userType="Student" ise:
      "studentNumber": "2023001",
      "departmentId": 1,
       // userType="Faculty" ise ek alanlar...
    }
    ```
*   **Response (201 Created)**:
    ```json
    {
      "isSuccessful": true,
      "message": "Registration successful. Please check your email to verify your account."
    }
    ```

### 2. Login (Giriş)
*   **URL**: `/Auth/login`
*   **Method**: `POST`
*   **Body**:
    ```json
    {
      "email": "student@smartcampus.edu",
      "password": "Password123!"
    }
    ```
*   **Response (200 OK)**:
    ```json
    {
      "isSuccessful": true,
      "payload": {
          "accessToken": "eyJh...",
          "refreshToken": "7c9e...",
          "expiration": "2025-12-08T15:00:00Z"
      }
    }
    ```

### 3. Verify Email (Email Doğrulama)
*   **URL**: `/Auth/verify-email`
*   **Method**: `GET/POST` (Query Params: `userId`, `token`)

### 4. Refresh Token
*   **URL**: `/Auth/refresh-token`
*   **Method**: `POST`
*   **Body**: `{ "token": "your_refresh_token" }`

### 5. Forgot Password
*   **URL**: `/Auth/forgot-password`
*   **Method**: `POST`
*   **Body**: `{ "email": "student@smartcampus.edu" }`

### 6. Reset Password
*   **URL**: `/Auth/reset-password`
*   **Method**: `POST`
*   **Body**: `{ "userId": "...", "token": "...", "newPassword": "..." }`

---

## 👤 User Management Endpoints (`/Users`)

### 1. Get Me (Profilim)
Giriş yapmış kullanıcının bilgilerini getirir.

*   **URL**: `/Users/me`
*   **Method**: `GET`
*   **Header**: `Authorization: Bearer <token>`
*   **Response (200 OK)**:
    ```json
    {
      "id": "user-guid",
      "fullName": "Ahmet Yılmaz",
      "email": "student@smartcampus.edu",
      "department": "Computer Engineering"
    }
    ```

### 2. Update Profile (Profil Güncelle)
*   **URL**: `/Users/me`
*   **Method**: `PUT`
*   **Body**:
    ```json
    {
      "fullName": "Ahmet Yeni Soyad",
      "phoneNumber": "5551234567"
    }
    ```

### 3. Upload Profile Picture
*   **URL**: `/Users/me/profile-picture`
*   **Method**: `POST` (Multipart/Web-Form)
*   **Form-Data**: `file` (Image)

### 4. List Users (Admin Only)
*   **URL**: `/Users`
*   **Method**: `GET`
*   **Query**: `?pageNumber=1&pageSize=10`
*   **Permissions**: Sadece `Admin` rolü erişebilir.
