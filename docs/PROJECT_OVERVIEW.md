# 🎓 Smart Campus Project - Part 1: Backend Overview

## 📋 Proje Tanımı
**Smart Campus (Akıllı Kampüs)**, üniversite öğrencilerinin ve öğretim üyelerinin günlük işlemlerini dijitalleştirmeyi amaçlayan kapsamlı bir web ve mobil platformudur. 

**Part 1 (Mevcut Aşama)**, projenin temel altyapısını, güvenli kimlik doğrulama sistemini (Authentication) ve kullanıcı yönetimi (User Management) modüllerini kapsar.

## 🛠️ Teknoloji Stack'i

Bu proje aşağıdaki modern teknolojiler kullanılarak geliştirilmiştir:

| Alan | Teknoloji | Açıklama |
| :--- | :--- | :--- |
| **Backend Framework** | **.NET Core 8** | Yüksek performanslı, cross-platform uygulama çatısı. |
| **Dil** | **C# 12** | Modern, güvenli ve güçlü programlama dili. |
| **Veritabanı** | **MySQL** | İlişkisel veri tabanı yönetim sistemi. |
| **ORM** | **Entity Framework Core** | Veritabanı işlemleri için Code-First yaklaşımı. |
| **Auth** | **JWT (JSON Web Token)** | Güvenli, stateles kimlik doğrulama. |
| **Loglama** | **Serilog** | Yapılandırılmış loglama kütüphanesi. |
| **Validasyon** | **FluentValidation** | Gelişmiş veri doğrulama kuralları. |
| **Mapping** | **AutoMapper** | Entity-DTO dönüşümleri. |

## 🏗️ Proje Mimarisi (N-Layer Architecture)

Proje, **Clean Architecture** prensiplerine uygun olarak 4 temel katmana ayrılmıştır:

1.  **SMARTCAMPUS.EntityLayer**: Veritabanı tablolarına karşılık gelen POCO sınıfları (`User`, `Student`, `Faculty` vb.) bulunur.
2.  **SMARTCAMPUS.DataAccessLayer**: Veritabanı erişim kodları (Repository Pattern) ve `DbContext` yapılandırması buradadır.
3.  **SMARTCAMPUS.BusinessLayer**: İş mantığı, validasyon kuralları ve DTO dönüşümleri burada işlenir.
4.  **SMARTCAMPUS.API**: Dış dünyaya açılan kapıdır. Controller'lar ve Middleware'ler burada bulunur.

## 👥 Grup Üyeleri
*(Burayı kendi bilgilerinizle güncelleyiniz)*

1.  **[Ad Soyad]** - Backend Developer / Team Lead
2.  **[Ad Soyad]** - Frontend Developer
3.  **[Ad Soyad]** - Database Administrator
