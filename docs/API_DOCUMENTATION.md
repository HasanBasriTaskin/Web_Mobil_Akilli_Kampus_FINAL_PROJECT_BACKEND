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
      "confirmPassword": "Password123!",
      "fullName": "Ahmet Yılmaz",
      "userType": "Student",
      "departmentId": 1,
      // userType="Student" ise:
      "studentNumber": "2023001",
      // userType="Faculty" ise:
      "employeeNumber": "EMP001",
      "title": "Dr.",
      "officeLocation": "A-101"
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
          "accessTokenExpiration": "2025-12-08T16:00:00Z",
          "refreshToken": "7c9e...",
          "refreshTokenExpiration": "2025-12-15T15:00:00Z"
      }
    }
    ```

### 3. Verify Email (Email Doğrulama)
*   **URL**: `/Auth/verify-email`
*   **Method**: `POST`
*   **Query**: `?userId=xxx&token=xxx`
*   **Response (200 OK)**:
    ```json
    {
      "isSuccessful": true,
      "message": "Email verified successfully."
    }
    ```

### 4. Refresh Token
*   **URL**: `/Auth/refresh-token`
*   **Method**: `POST`
*   **Body**: `{ "token": "your_refresh_token" }`
*   **Response (200 OK)**: Yeni TokenDto döner.

### 5. Revoke Token
*   **URL**: `/Auth/revoke-token`
*   **Method**: `POST`
*   **Body**: `{ "token": "your_refresh_token" }`
*   **Response (200 OK)**: Token iptal edilir.

### 6. Forgot Password
*   **URL**: `/Auth/forgot-password`
*   **Method**: `POST`
*   **Body**: `{ "email": "student@smartcampus.edu" }`

### 7. Reset Password
*   **URL**: `/Auth/reset-password`
*   **Method**: `POST`
*   **Body**: `{ "userId": "...", "token": "...", "newPassword": "..." }`

### 8. Change Password
*   **URL**: `/Auth/change-password`
*   **Method**: `POST`
*   **Header**: `Authorization: Bearer <token>`
*   **Body**:
    ```json
    {
      "userId": "user-guid",
      "currentPassword": "OldPassword123!",
      "newPassword": "NewPassword123!"
    }
    ```

### 9. Logout
*   **URL**: `/Auth/logout`
*   **Method**: `POST`
*   **Body**: `{ "token": "your_refresh_token" }`
*   **Response (200 OK)**: Oturum sonlandırılır.

---

## 👤 User Management Endpoints (`/Users`)

> **Not**: Tüm `/Users` endpoint'leri `Authorization: Bearer <token>` header'ı gerektirir.

### 1. Get Me (Profilim)
Giriş yapmış kullanıcının bilgilerini getirir.

*   **URL**: `/Users/me`
*   **Method**: `GET`
*   **Response (200 OK)**:
    ```json
    {
      "isSuccessful": true,
      "payload": {
        "idString": "user-guid",
        "fullName": "Ahmet Yılmaz",
        "email": "student@smartcampus.edu",
        "profilePictureUrl": "/uploads/profile.jpg",
        "roles": ["Student"]
      }
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
*   **Method**: `POST` (Multipart/Form-Data)
*   **Form-Data**: `file` (Image)

### 4. Get User by ID
*   **URL**: `/Users/{id}`
*   **Method**: `GET`
*   **Permissions**: Admin veya kendi profili

### 5. Update User
*   **URL**: `/Users/{id}`
*   **Method**: `PUT`
*   **Permissions**: Admin veya kendi profili

### 6. Delete User
*   **URL**: `/Users/{id}`
*   **Method**: `DELETE`
*   **Permissions**: Sadece `Admin` rolü

### 7. List Users (Admin Only)
*   **URL**: `/Users`
*   **Method**: `GET`
*   **Query**: `?pageNumber=1&pageSize=10`
*   **Permissions**: Sadece `Admin` rolü erişebilir.

### 8. Assign Roles (Admin Only)
*   **URL**: `/Users/{id}/roles`
*   **Method**: `POST`
*   **Body**: `["Admin", "Student"]` (rol listesi)
*   **Permissions**: Sadece `Admin` rolü
