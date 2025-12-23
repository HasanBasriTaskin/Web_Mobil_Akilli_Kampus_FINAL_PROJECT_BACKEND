# 🧪 Test Raporu (Part 3)

## 📌 Durum
Bu rapor, Part 3 kapsamındaki (Ödeme ve Çizelgeleme) modüllerin test sonuçlarını içerecektir.

> **NOT:** Birim testlerin yazımı ve koşumu devam etmektedir (TODO).

## 1. Planlanan Test Senaryoları

### Cüzdan & Ödeme
- [ ] `WalletManager.TopUp`: Mock ödeme ile bakiye artışı.
- [ ] `IyzicoPaymentManager.Initialize`: Token ve HTML dönüşünün doğrulanması.
- [ ] `PaymentWebhookController`: Callback sonrası bakiye güncelleme (Integration Test).
- [ ] **ACID Test:** Ödeme başarılı olup veritabanı hatası alınırsa bakiyenin artmaması.

### Çizelgeleme (Scheduling)
- [ ] `ScheduleManager.CheckConflicts`: Çakışan derslerin tespiti.
- [ ] `ScheduleManager.GenerateSchedule`: Basit veri setiyle (3 ders, 2 sınıf) çözüm bulunması.
- [ ] **Hard Constraints:** Aynı saatte aynı sınıfa iki ders atanamaması.
