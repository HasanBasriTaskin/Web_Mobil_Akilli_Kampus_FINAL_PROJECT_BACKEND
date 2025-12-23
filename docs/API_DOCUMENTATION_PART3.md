# 🔌 Smart Campus - API Dokümantasyonu (Part 3)

## 🩺 Modül: Cüzdan ve Ödeme (Wallet & Payment)

### 1. Bakiye Sorgula
- **URL:** `/api/v1/wallet`
- **Method:** `GET`
- **Auth:** ✅ User
- **Response:**
  ```json
  {
    "balance": 150.00,
    "currency": "TRY",
    "isActive": true
  }
  ```

### 2. İşlem Geçmişi
- **URL:** `/api/v1/wallet/transactions`
- **Method:** `GET`
- **Query:** `page=1&pageSize=20`
- **Response:** Transaction listesi (Tarih, Tutar, Açıklama, Tip).

### 3. Para Yükle (Iyzico)
- **URL:** `/api/v1/wallet/topup/iyzico`
- **Method:** `POST`
- **Body:**
  ```json
  {
    "amount": 100.00,
    "city": "Istanbul",
    "country": "Turkey"
  }
  ```
- **Response:** Iyzico ödeme sayfası içeriği (HTML veya URL).

---

## 📅 Modül: Etkinlikler (Events)

### 1. Etkinlik Listele
- **URL:** `/api/v1/events`
- **Method:** `GET`
- **Query:** `from=2025-01-01&isFree=false`

### 2. Etkinlik Oluştur (Personel)
- **URL:** `/api/v1/events`
- **Method:** `POST`
- **Auth:** ✅ Faculty/Admin
- **Body:**
  ```json
  {
    "title": "Bahar Şenliği",
    "capacity": 500,
    "price": 50.00,
    "startDate": "..."
  }
  ```

### 3. Kayıt Ol (Register)
- **URL:** `/api/v1/events/{id}/register`
- **Method:** `POST`
- **Auth:** ✅ Student
- **Not:** Ücretli ise cüzdandan düşer. Kapasite doluysa hata döner veya waitlist önerir.

### 4. Bekleme Listesine Katıl
- **URL:** `/api/v1/events/{id}/waitlist`
- **Method:** `POST`

### 5. Check-In (QR)
- **URL:** `/api/v1/events/check-in`
- **Method:** `POST`
- **Body:** `{ "qrCode": "EVENT-XYZ-123" }`

---

## 🎓 Modül: Çizelgeleme (Scheduling)

### 1. Otomatik Program Oluştur
- **URL:** `/api/v1/schedules/auto-generate`
- **Method:** `POST`
- **Auth:** ✅ Admin
- **Body:**
  ```json
  {
    "semester": "Fall",
    "year": 2025,
    "maxIterations": 1000
  }
  ```

### 2. Ders Programı Getir
- **URL:** `/api/v1/schedules/student/my-schedule`
- **Method:** `GET`

### 3. iCal İndir
- **URL:** `/api/v1/schedules/student/export-ical`
- **Method:** `GET`
- **Response:** `.ics` dosyası.
