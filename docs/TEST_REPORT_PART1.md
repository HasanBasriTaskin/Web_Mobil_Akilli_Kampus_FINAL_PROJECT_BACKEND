# 🧪 Test Raporu - Part 1

## Genel Bakış
Part 1 kapsamında backend servislerinin doğruluğunu sağlamak amacıyla **Unit Test** çalışmaları yapılmıştır.

## Test Kapsamı
Mevcut testler `SMARTCAMPUS.Tests` projesi altında bulunmaktadır ve öncelikli olarak iş mantığının (Business Logic) en yoğun olduğu **AuthManager** sınıfına odaklanılmıştır.

### 1. AuthManager Tests
`AuthManager` sınıfı için aşağıdaki senaryolar test edilmiştir:

*   ✅ **Login_Successful**: Doğru bilgilerle giriş yapıldığında Token dönülmesi.
*   ✅ **Login_Failed_WrongPassword**: Yanlış şifre ile girişin engellenmesi.
*   ✅ **Register_Successful**: Başarılı kullanıcı kaydı.
*   ✅ **Register_Failed_EmailExists**: Var olan email ile kaydın engellenmesi.

## Test Sonuçları & Coverage

| Modül | Test Sayısı | Durum | Tahmini Coverage |
| :--- | :---: | :---: | :---: |
| **Auth Service** | 4 | ✅ Geçti | %90 |
| **User Service** | 0 | ⚠️ Eksik | %0 |
| **Controllers** | 0 | ⚠️ Eksik | %0 |

> **Not:** Proje teslim süresi kısıtları nedeniyle şu an için sadece Kritik Yol (Critical Path) olan Authentication servisi test edilmiştir. İlerleyen aşamalarda Controller ve diğer servis testlerinin yazılması (%85 hedefine ulaşılması) planlanmaktadır.

## Nasıl Çalıştırılır?
Testleri çalıştırmak için terminalde şu komutu kullanabilirsiniz:

```powershell
dotnet test
```
