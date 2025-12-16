# 🔌 Smart Campus - API Dokümantasyonu (Part 1)

## 📋 Genel Bilgiler

| Özellik | Değer |
|:--------|:------|
| **Base URL** | `https://localhost:7123/api/v1` |
| **Content-Type** | `application/json` |
| **Authentication** | JWT Bearer Token |

### Standart Response Yapısı

```json
{
  "isSuccessful": true,
  "data": { ... },
  "errors": null
}
```

### Hata Response Yapısı

```json
{
  "isSuccessful": false,
  "data": null,
  "errors": ["Hata mesajı 1", "Hata mesajı 2"]
}
```

---

## 🔐 Authentication Endpoints (8 Adet)

### 1. Register (Kullanıcı Kaydı)

Yeni kullanıcı (Öğrenci veya Akademisyen) kaydı oluşturur.

| Özellik | Değer |
|:--------|:------|
| **URL** | `/Auth/register` |
| **Method** | `POST` |
| **Auth Required** | ❌ Hayır |

**Request Body (Öğrenci için):**

```json
{
  "email": "student@smartcampus.edu",
  "password": "Password123!",
  "fullName": "Ahmet Yılmaz",
  "userType": "Student",
  "departmentId": 1,
  "studentNumber": "2023001"
}
```

**Request Body (Akademisyen için):**

```json
{
  "email": "faculty@smartcampus.edu",
  "password": "Password123!",
  "fullName": "Dr. Mehmet Demir",
  "userType": "Faculty",
  "departmentId": 1,
  "employeeNumber": "EMP001",
  "title": "Dr.",
  "officeLocation": "A-101"
}
```

**Response (201 Created):**

```json
{
  "isSuccessful": true,
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "7c9e4a2b-3d1f-4e5a-8b6c-9d0e1f2a3b4c",
    "accessTokenExpiration": "2025-12-10T16:00:00Z",
    "refreshTokenExpiration": "2025-12-17T15:00:00Z"
  }
}
```

---

### 2. Login (Giriş)

Kullanıcı girişi yapar ve JWT token döner.

| Özellik | Değer |
|:--------|:------|
| **URL** | `/Auth/login` |
| **Method** | `POST` |
| **Auth Required** | ❌ Hayır |

**Request Body:**

```json
{
  "email": "student@smartcampus.edu",
  "password": "Password123!"
}
```

**Response (200 OK):**

```json
{
  "isSuccessful": true,
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "7c9e4a2b-3d1f-4e5a-8b6c-9d0e1f2a3b4c",
    "accessTokenExpiration": "2025-12-10T16:00:00Z",
    "refreshTokenExpiration": "2025-12-17T15:00:00Z",
    "user": {
      "id": "user-guid-here",
      "email": "student@smartcampus.edu",
      "fullName": "Ahmet Yılmaz",
      "userType": "Student",
      "isEmailVerified": true,
      "student": {
        "studentNumber": "2023001",
        "departmentId": 1
      }
    }
  }
}
```

---

### 3. Verify Email (E-posta Doğrulama)

Kayıt sonrası e-posta doğrulaması yapar.

| Özellik | Değer |
|:--------|:------|
| **URL** | `/Auth/verify-email` |
| **Method** | `POST` |
| **Auth Required** | ❌ Hayır |

**Query Parameters:**

| Parametre | Tip | Zorunlu | Açıklama |
|:----------|:----|:--------|:---------|
| `userId` | string | ✅ | Kullanıcı ID |
| `token` | string | ✅ | E-posta doğrulama tokeni |

**Response (200 OK):**

```json
{
  "isSuccessful": true,
  "data": null
}
```

---

### 4. Refresh Token (Token Yenileme)

Access token süre dolduğunda yeni token alır.

| Özellik | Değer |
|:--------|:------|
| **URL** | `/Auth/refresh-token` |
| **Method** | `POST` |
| **Auth Required** | ❌ Hayır |

**Request Body:**

```json
{
  "token": "your-refresh-token-here"
}
```

**Response (200 OK):**

```json
{
  "isSuccessful": true,
  "data": {
    "accessToken": "new-access-token",
    "refreshToken": "new-refresh-token",
    "accessTokenExpiration": "2025-12-10T17:00:00Z",
    "refreshTokenExpiration": "2025-12-17T16:00:00Z"
  }
}
```

---

### 5. Revoke Token (Token İptal)

Aktif refresh tokeni iptal eder.

| Özellik | Değer |
|:--------|:------|
| **URL** | `/Auth/revoke-token` |
| **Method** | `POST` |
| **Auth Required** | ❌ Hayır |

**Request Body:**

```json
{
  "token": "refresh-token-to-revoke"
}
```

**Response (200 OK):**

```json
{
  "isSuccessful": true,
  "data": null
}
```

---

### 6. Forgot Password (Şifremi Unuttum)

Şifre sıfırlama e-postası gönderir.

| Özellik | Değer |
|:--------|:------|
| **URL** | `/Auth/forgot-password` |
| **Method** | `POST` |
| **Auth Required** | ❌ Hayır |

**Request Body:**

```json
{
  "email": "student@smartcampus.edu"
}
```

**Response (200 OK):**

```json
{
  "isSuccessful": true,
  "data": null
}
```

> ⚠️ **Not:** Güvenlik nedeniyle, e-posta bulunamasa bile başarılı yanıt döner.

---

### 7. Reset Password (Şifre Sıfırlama)

E-posta ile gelen token ile şifreyi sıfırlar.

| Özellik | Değer |
|:--------|:------|
| **URL** | `/Auth/reset-password` |
| **Method** | `POST` |
| **Auth Required** | ❌ Hayır |

**Request Body:**

```json
{
  "email": "student@smartcampus.edu",
  "token": "password-reset-token",
  "newPassword": "NewPassword123!"
}
```

**Response (200 OK):**

```json
{
  "isSuccessful": true,
  "data": null
}
```

---

### 8. Change Password (Şifre Değiştirme)

Mevcut şifreyi değiştirir (oturum açmış kullanıcı için).

| Özellik | Değer |
|:--------|:------|
| **URL** | `/Auth/change-password` |
| **Method** | `POST` |
| **Auth Required** | ✅ Evet |

**Headers:**

```
Authorization: Bearer <access_token>
```

**Request Body:**

```json
{
  "userId": "user-guid-here",
  "oldPassword": "CurrentPassword123!",
  "newPassword": "NewPassword456!",
  "confirmNewPassword": "NewPassword456!"
}
```

**Response (200 OK):**

```json
{
  "isSuccessful": true,
  "data": null
}
```

---

### 9. Logout (Çıkış)

Oturumu sonlandırır ve refresh tokeni iptal eder.

| Özellik | Değer |
|:--------|:------|
| **URL** | `/Auth/logout` |
| **Method** | `POST` |
| **Auth Required** | ❌ Hayır |

**Request Body:**

```json
{
  "token": "refresh-token-to-invalidate"
}
```

**Response (200 OK):**

```json
{
  "isSuccessful": true,
  "data": null
}
```

---

## 👤 User Management Endpoints (8 Adet)

> ⚠️ **Not:** Tüm `/Users` endpoint'leri `Authorization: Bearer <token>` header'ı gerektirir.

---

### 1. Get My Profile (Profilim)

Oturum açmış kullanıcının bilgilerini getirir.

| Özellik | Değer |
|:--------|:------|
| **URL** | `/Users/me` |
| **Method** | `GET` |
| **Auth Required** | ✅ Evet |

**Response (200 OK):**

```json
{
  "isSuccessful": true,
  "data": {
    "id": "user-guid-here",
    "email": "student@smartcampus.edu",
    "fullName": "Ahmet Yılmaz",
    "userType": "Student",
    "role": "Student",
    "isEmailVerified": true,
    "isActive": true,
    "phoneNumber": "5551234567",
    "profilePictureUrl": "/uploads/profile-pictures/image.jpg",
    "createdAt": "2025-01-15T10:30:00Z",
    "roles": ["Student"],
    "student": {
      "studentNumber": "2023001",
      "departmentId": 1
    }
  }
}
```

---

### 2. Update My Profile (Profil Güncelle)

Oturum açmış kullanıcının profilini günceller.

| Özellik | Değer |
|:--------|:------|
| **URL** | `/Users/me` |
| **Method** | `PUT` |
| **Auth Required** | ✅ Evet |

**Request Body:**

```json
{
  "fullName": "Ahmet Yeni Soyad",
  "email": "yeni@smartcampus.edu",
  "phoneNumber": "5559876543"
}
```

**Response (200 OK):**

```json
{
  "isSuccessful": true,
  "data": null
}
```

---

### 3. Upload Profile Picture (Profil Fotoğrafı Yükle)

Kullanıcı profil fotoğrafı yükler.

| Özellik | Değer |
|:--------|:------|
| **URL** | `/Users/me/profile-picture` |
| **Method** | `POST` |
| **Content-Type** | `multipart/form-data` |
| **Auth Required** | ✅ Evet |

**Form Data:**

| Alan | Tip | Zorunlu | Açıklama |
|:-----|:----|:--------|:---------|
| `file` | File | ✅ | Profil fotoğrafı (JPG, JPEG, PNG) |

**Kısıtlamalar:**
- Maksimum dosya boyutu: **5 MB**
- İzin verilen formatlar: `.jpg`, `.jpeg`, `.png`

**Response (200 OK):**

```json
{
  "isSuccessful": true,
  "data": "/uploads/profile-pictures/user123_abc.jpg"
}
```

---

### 4. Get User by ID (Kullanıcı Detay)

Belirli bir kullanıcının bilgilerini getirir.

| Özellik | Değer |
|:--------|:------|
| **URL** | `/Users/{id}` |
| **Method** | `GET` |
| **Auth Required** | ✅ Evet |
| **Permission** | Admin veya kendi profili |

**Response (200 OK):** (Aynı `/Users/me` response yapısı)

---

### 5. Update User (Kullanıcı Güncelle)

Belirli bir kullanıcının bilgilerini günceller.

| Özellik | Değer |
|:--------|:------|
| **URL** | `/Users/{id}` |
| **Method** | `PUT` |
| **Auth Required** | ✅ Evet |
| **Permission** | Admin veya kendi profili |

**Request Body:** (Aynı `/Users/me` PUT request yapısı)

---

### 6. Delete User (Kullanıcı Sil)

Kullanıcıyı sistemden siler (Soft Delete).

| Özellik | Değer |
|:--------|:------|
| **URL** | `/Users/{id}` |
| **Method** | `DELETE` |
| **Auth Required** | ✅ Evet |
| **Permission** | 🔒 Sadece Admin |

**Response (200 OK):**

```json
{
  "isSuccessful": true,
  "data": null
}
```

---

### 7. List Users (Kullanıcı Listele)

Sistemdeki kullanıcıları listeler (sayfalama destekli).

| Özellik | Değer |
|:--------|:------|
| **URL** | `/Users` |
| **Method** | `GET` |
| **Auth Required** | ✅ Evet |
| **Permission** | 🔒 Sadece Admin |

**Query Parameters:**

| Parametre | Tip | Varsayılan | Açıklama |
|:----------|:----|:-----------|:---------|
| `page` | int | 1 | Sayfa numarası |
| `limit` | int | 10 | Sayfa başına kayıt |
| `search` | string | - | İsim veya e-posta araması |
| `departmentId` | int | - | Bölüm filtresi |

**Response (200 OK):**

```json
{
  "isSuccessful": true,
  "data": {
    "items": [
      {
        "id": "user-guid",
        "email": "student@smartcampus.edu",
        "fullName": "Ahmet Yılmaz",
        "isActive": true,
        "roles": ["Student"]
      }
    ],
    "page": 1,
    "limit": 10,
    "totalRecords": 50,
    "totalPages": 5
  }
}
```

---

### 8. Assign Roles (Rol Atama)

Kullanıcıya rol atar.

| Özellik | Değer |
|:--------|:------|
| **URL** | `/Users/{id}/roles` |
| **Method** | `POST` |
| **Auth Required** | ✅ Evet |
| **Permission** | 🔒 Sadece Admin |

**Request Body:**

```json
["Admin", "Student"]
```

**Response (200 OK):**

```json
{
  "isSuccessful": true,
  "data": null
}
```

---

## ❌ Hata Kodları

| HTTP Kodu | Durum | Açıklama |
|:----------|:------|:---------|
| `200` | OK | İşlem başarılı |
| `201` | Created | Kayıt başarıyla oluşturuldu |
| `400` | Bad Request | Geçersiz istek (validasyon hatası, hatalı şifre vb.) |
| `401` | Unauthorized | Kimlik doğrulama gerekli veya token geçersiz |
| `403` | Forbidden | Erişim yetkisi yok |
| `404` | Not Found | Kaynak bulunamadı |
| `500` | Internal Server Error | Sunucu hatası |

### Yaygın Hata Mesajları

| Mesaj | Açıklama |
|:------|:---------|
| `Geçersiz e-posta veya şifre` | Login başarısız |
| `Hesap aktif değil. Lütfen e-postanızı doğrulayın.` | E-posta doğrulanmamış |
| `Bu e-posta adresiyle kayıtlı kullanıcı zaten var` | Kayıtta duplicate e-posta |
| `Kullanıcı bulunamadı` | Geçersiz kullanıcı ID |
| `Token bulunamadı` | Geçersiz refresh token |
| `Token aktif değil` | Süresi dolmuş veya iptal edilmiş token |
| `Şifreler uyuşmuyor` | Şifre değiştirmede onay hatası |
| `Öğrenci numarası zorunludur` | Öğrenci kaydında eksik alan |
| `Sicil numarası ve Ünvan zorunludur` | Akademisyen kaydında eksik alan |
| `Dosya boyutu en fazla 5MB olabilir` | Profil fotoğrafı boyut aşımı |
| `Access Denied: You can only view your own profile.` | Yetki hatası |
