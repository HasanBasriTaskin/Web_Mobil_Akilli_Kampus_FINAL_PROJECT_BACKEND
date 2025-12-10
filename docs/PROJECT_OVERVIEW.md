# 🎓 Smart Campus - Proje Genel Bakış

## 📋 Proje Tanımı

**Smart Campus (Akıllı Kampüs)**, üniversite öğrencilerinin ve öğretim üyelerinin günlük akademik işlemlerini dijitalleştirmeyi amaçlayan kapsamlı bir web ve mobil platformudur.

Proje, modern yazılım geliştirme pratiklerini kullanarak güvenli kimlik doğrulama, kullanıcı yönetimi ve kampüs içi hizmetlerin tek bir platform üzerinden sunulmasını hedeflemektedir.

---

## 🛠️ Teknoloji Stack'i

| Kategori | Teknoloji | Versiyon | Açıklama |
|:---------|:----------|:---------|:---------|
| **Framework** | .NET Core | 8.0 | Cross-platform, yüksek performanslı web framework |
| **Dil** | C# | 12 | Modern, type-safe programlama dili |
| **Veritabanı** | MySQL | - | İlişkisel veritabanı yönetim sistemi |
| **ORM** | Entity Framework Core | 8.0 | Code-First yaklaşımıyla veritabanı yönetimi |
| **Kimlik Doğrulama** | JWT | - | Stateless, güvenli token tabanlı authentication |
| **API Dokümantasyonu** | Swagger/OpenAPI | 6.5 | Otomatik API dokümantasyonu |
| **Loglama** | Serilog | 8.0 | Yapılandırılmış loglama (Console & File) |
| **Validasyon** | FluentValidation | - | Esnek ve okunabilir doğrulama kuralları |
| **Mapping** | AutoMapper | - | Entity ↔ DTO otomatik dönüşümleri |
| **Containerization** | Docker | - | Uygulama containerization ve deployment |

---

## 🏗️ Proje Yapısı (N-Layer Architecture)

Proje, **Clean Architecture** prensiplerine uygun olarak 4 temel katmana ayrılmıştır:

```
SMARTCAMPUS/
├── 📁 SMARTCAMPUS.API/                 # Sunum Katmanı
│   ├── Controllers/                    # API endpoint'leri
│   ├── Middleware/                     # Custom middleware'ler
│   └── Program.cs                      # Uygulama giriş noktası
│
├── 📁 SMARTCAMPUS.BusinessLayer/       # İş Mantığı Katmanı
│   ├── Abstract/                       # Service interface'leri
│   ├── Concrete/                       # Service implementasyonları
│   ├── Mappings/                       # AutoMapper profilleri
│   ├── ValidationRules/                # FluentValidation kuralları
│   └── Tools/                          # Yardımcı araçlar
│
├── 📁 SMARTCAMPUS.DataAccessLayer/     # Veri Erişim Katmanı
│   ├── Abstract/                       # Repository interface'leri
│   ├── Concrete/                       # Repository implementasyonları
│   ├── Context/                        # DbContext sınıfı
│   ├── Configurations/                 # Entity konfigürasyonları
│   └── Migrations/                     # Veritabanı migration'ları
│
├── 📁 SMARTCAMPUS.EntityLayer/         # Entity Katmanı
│   ├── Models/                         # Veritabanı entity'leri
│   └── DTOs/                           # Data Transfer Object'leri
│
├── 📁 SMARTCAMPUS.Tests/               # Test Projesi
│   └── ...                             # Unit & Integration testleri
│
└── 📁 docs/                            # Proje Dokümantasyonu
    ├── PROJECT_OVERVIEW.md
    ├── API_DOCUMENTATION.md
    ├── DATABASE_SCHEMA.md
    └── ...
```

### Katman Açıklamaları

| Katman | Sorumluluk |
|:-------|:-----------|
| **API Layer** | HTTP isteklerini karşılar, Controller'lar ve Middleware'ler içerir |
| **Business Layer** | İş mantığını işler, validasyon ve DTO dönüşümlerini yapar |
| **Data Access Layer** | Veritabanı işlemlerini yönetir (Repository Pattern) |
| **Entity Layer** | POCO sınıfları ve DTO'ları barındırır |

---

## 👥 Grup Üyeleri ve Görev Dağılımı

| # | Ad Soyad | Rol | Sorumluluk Alanı |
|:-:|:---------|:----|:-----------------|
| 1 | **Erdem Bekir AKTÜRK** | Backend Developer | API geliştirme, veritabanı tasarımı |
| 2 | **Hasan Basri TAŞKIN** | Backend Developer | İş mantığı, authentication sistemi |
| 3 | **Neşe SARP** | Frontend Developer | Kullanıcı arayüzü geliştirme |
| 4 | **Gökçenur KÜÇÜK** | Frontend Developer | Kullanıcı arayüzü geliştirme |

---

## 📚 İlgili Dokümantasyon

- [API Dokümantasyonu](./API_DOCUMENTATION.md)
- [Veritabanı Şeması](./DATABASE_SCHEMA.md)
- [Test Raporu](./TEST_REPORT_PART1.md)
- [Kullanıcı Kılavuzu](./USER_MANUAL_PART1.md)
