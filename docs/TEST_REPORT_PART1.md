# 🧪 Test Raporu - Part 1

## Genel Bakış
Part 1 kapsamında backend servislerinin doğruluğunu sağlamak amacıyla **Unit Test** çalışmaları yapılmıştır.

## Test Kapsamı
Mevcut testler `SMARTCAMPUS.Tests` projesi altında bulunmaktadır. İş mantığının (Business Logic) en yoğun olduğu **AuthManager** sınıfına odaklanılmıştır.

### AuthManager Tests

| Kategori | Test Sayısı | Test Adları |
|:---------|:-----------:|:------------|
| **Login** | 4 | `LoginAsync_WithInvalidEmail_ReturnsFail`, `LoginAsync_WithInactiveAccount_ReturnsFail`, `LoginAsync_WithWrongPassword_ReturnsFail`, `LoginAsync_WithValidCredentials_ReturnsSuccessAndToken` |
| **Register** | 4 | `RegisterAsync_WithExistingEmail_ReturnsFail`, `RegisterAsync_UserCreationFails_ReturnsFail`, `RegisterAsync_ExceptionThrown_ReturnsFail`, `RegisterAsync_Success_ReturnsToken` |
| **Verify Email** | 3 | `VerifyEmailAsync_UserNotFound_ReturnsFail`, `VerifyEmailAsync_ConfirmationFails_ReturnsFail`, `VerifyEmailAsync_Success_ActivatesUser` |
| **Refresh Token** | 4 | `CreateTokenByRefreshTokenAsync_TokenNotFound_ReturnsFail`, `CreateTokenByRefreshTokenAsync_TokenInvalid_ReturnsFail`, `CreateTokenByRefreshTokenAsync_UserNotFound_ReturnsFail`, `CreateTokenByRefreshTokenAsync_Success_ReturnsNewToken` |
| **Revoke Token** | 2 | `RevokeRefreshTokenAsync_NotFound_ReturnsFail`, `RevokeRefreshTokenAsync_Success_RevokesToken` |
| **Forgot Password** | 2 | `ForgotPasswordAsync_UserNotFound_ReturnsSuccess`, `ForgotPasswordAsync_Success_SendsEmail` |
| **Reset Password** | 3 | `ResetPasswordAsync_UserNotFound_ReturnsFail`, `ResetPasswordAsync_ResetFails_ReturnsFail`, `ResetPasswordAsync_Success_ReturnsSuccess` |

**Toplam: 22 Test**

## Test Sonuçları & Coverage

| Modül | Test Sayısı | Durum | Tahmini Coverage |
|:------|:-----------:|:-----:|:----------------:|
| **Auth Service** | 22 | ✅ Geçti | ~85% |
| **User Service** | 0 | ⚠️ Planlanıyor | %0 |
| **Controllers** | 0 | ⚠️ Planlanıyor | %0 |

> **Not:** AuthManager için kapsamlı test senaryoları yazılmıştır. İlerleyen aşamalarda Controller ve diğer servis testlerinin eklenmesi planlanmaktadır.

## Nasıl Çalıştırılır?

```powershell
# Testleri çalıştır
dotnet test

# Coverage raporu ile çalıştır
.\run-tests-with-coverage.ps1
```
