# 🗄️ Database Schema - Part 1

## ER Diagram

```mermaid
erDiagram
    AspNetUsers ||--o{ Students : "has one (User is Base)"
    AspNetUsers ||--o{ Faculties : "has one (User is Base)"
    Departments ||--o{ Students : "contains"
    Departments ||--o{ Faculties : "employs"

    AspNetUsers {
        string Id PK "Guid"
        string Email
        string PasswordHash
        string FullName
        string PhoneNumber
    }

    Students {
        string Id PK
        string UserId FK
        string StudentNumber
        float GPA
        float CGPA
        int DepartmentId FK
    }

    Faculties {
        string Id PK
        string UserId FK
        string EmployeeNumber
        string Title "Dr, Prof"
        string OfficeLocation
        int DepartmentId FK
    }

    Departments {
        int Id PK
        string Name
        string Code "CENG, EEE"
        string FacultyName
    }
```

## Tablo Açıklamaları

### 1. AspNetUsers (Taban Kullanıcı Tablosu)
Identity Framework tarafından yönetilen ana kullanıcı tablosudur. Tüm kullanıcıların (Öğrenci, Akademisyen, Admin) ortak bilgileri (Email, Şifre, İsim) burada tutulur.

### 2. Students (Öğrenciler)
Öğrencilere özgü bilgilerin tutulduğu tablodur. `UserId` ile `AspNetUsers` tablosuna 1-1 bağlıdır.
*   **StudentNumber**: Öğrenci numarası.
*   **GPA/CGPA**: Not ortalamaları.
*   **DepartmentId**: Öğrencinin bölümü.

### 3. Faculties (Akademisyenler)
Öğretim üyelerine özgü bilgilerin tutulduğu tablodur. `UserId` ile `AspNetUsers` tablosuna 1-1 bağlıdır.
*   **Title**: Unvan (Dr., Prof. vb.)
*   **OfficeLocation**: Ofis bilgisi.

### 4. Departments (Bölümler)
Üniversitedeki bölümlerin listesidir. Hem öğrenciler hem de akademisyenler bir bölüme bağlıdır.
*   **Code**: Bölüm kısa kodu (örn: CENG).
*   **FacultyName**: Bölümün bağlı olduğu fakülte (örn: Mühendislik Fakültesi).
