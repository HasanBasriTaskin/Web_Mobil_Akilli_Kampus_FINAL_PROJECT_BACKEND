# Docker Kullanım Kılavuzu

Bu kılavuz, SmartCampus projesini Docker ile çalıştırma adımlarını içerir.

## 📁 Dizin Yapısı

```
Documents/GitHub/
├── Web_Mobil_Akilli_Kampus_FINAL_PROJECT_BACKEND/
│   ├── docker-compose.yml          # Geliştirme ortamı
│   ├── docker-compose.prod.yml     # Production ortamı
│   ├── docker/
│   │   └── frontend.dev.Dockerfile # Frontend dev Dockerfile
│   ├── nginx/
│   │   ├── nginx.conf              # Production nginx config (SSL)
│   │   └── nginx.dev.conf          # Development nginx config
│   └── SMARTCAMPUS.API/
│       └── Dockerfile              # Backend Dockerfile
└── Web_Mobil_Akilli_Kampus_FINAL_PROJECT_FRONTEND/
    ├── Dockerfile                  # Frontend prod Dockerfile
    └── .env.local                  # API URL tanımı
```

## 🛠️ Geliştirme Ortamı

### Ön Gereksinimler

1. [Docker Desktop](https://www.docker.com/products/docker-desktop/) kurulu olmalı
2. Frontend projesinde `.env.local` dosyası oluşturulmalı:

```bash
# Web_Mobil_Akilli_Kampus_FINAL_PROJECT_FRONTEND/.env.local
NEXT_PUBLIC_API_URL=http://localhost/api
```

3. Backend projesinde `.env` dosyası oluşturulmalı:

```bash
# Web_Mobil_Akilli_Kampus_FINAL_PROJECT_BACKEND/.env
DB_PASSWORD=your_secure_password
```

### Servisleri Başlatma

```bash
# Backend proje dizinine gidin
cd Web_Mobil_Akilli_Kampus_FINAL_PROJECT_BACKEND

# Tüm servisleri başlatın
docker-compose up -d --build
```

### Erişim Adresleri

| Servis | URL | Açıklama |
|--------|-----|----------|
| Frontend | http://localhost | Next.js uygulaması |
| Backend API | http://localhost/api | .NET API |
| Swagger | http://localhost/swagger | API dokümantasyonu |
| phpMyAdmin | http://localhost/phpmyadmin | Veritabanı yönetimi |
| MySQL (direkt) | localhost:3307 | Veritabanı bağlantısı |
| Backend (direkt) | localhost:5150 | API direkt erişim |

### Yararlı Komutlar

```bash
# Servisleri durdur
docker-compose down

# Logları görüntüle
docker-compose logs -f

# Belirli bir servisin loglarını görüntüle
docker-compose logs -f backend
docker-compose logs -f frontend

# Servisleri yeniden başlat
docker-compose restart

# Tüm verileri sil (veritabanı dahil)
docker-compose down -v

# Sadece belirli servisleri çalıştır
docker-compose up -d db backend
```

## 🚀 Production Ortamı

Production ortamı için `docker-compose.prod.yml` dosyası kullanılır. Bu dosya SSL sertifikaları ve production ayarları içerir.

```bash
docker-compose -f docker-compose.prod.yml up -d --build
```

> ⚠️ **Not:** Production ortamında SSL sertifikaları ve domain yapılandırması gereklidir.

## 🐛 Sorun Giderme

### Port Çakışması
Eğer 80, 3307 veya 5150 portları kullanımdaysa:

```bash
# Windows'ta portu kullanan uygulamayı bulun
netstat -ano | findstr :80

# Veya docker-compose.yml'da portları değiştirin
```

### Container Başlamıyor
```bash
# Container durumunu kontrol edin
docker-compose ps

# Logları inceleyin
docker-compose logs <servis_adı>
```

### Veritabanı Bağlantı Hatası
Backend, MySQL'in hazır olmasını bekler. İlk başlatmada 30-60 saniye sürebilir.

```bash
# Veritabanı durumunu kontrol edin
docker-compose logs db
```

### Frontend Değişiklikleri Yansımıyor
Development modunda hot reload aktiftir. Eğer çalışmıyorsa:

```bash
docker-compose restart frontend
```
