# 📅 Çizelgeleme Algoritması (Part 3)

Bu doküman, SmartCampus projesindeki **Otomatik Ders Programı Oluşturma** modülünün kullandığı algoritma ve mantıksal kısıtları açıklar.

## 1. Problem Tanımı (CSP - Constraint Satisfaction Problem)
Ders programı oluşturma problemi, bir dizi dersin (değişkenler), belirli zaman dilimlerine ve sınıflara (değerler), belirli kurallara (kısıtlar) uyacak şekilde atanmasıdır.

### Değişkenler (Variables)
- $S_1, S_2, ..., S_n$: Programlanacak ders bölümleri (Course Sections).

### Alanlar (Domains)
- $D_i$: Her ders için olası `(Zaman, Sınıf)` çiftleri.
  - Zaman: Pazartesi 08:30, Salı 10:30 vb.
  - Sınıf: A-101 (Kapasite: 50), B-203 (Kapasite: 30) vb.

### Kısıtlar (Constraints)
#### Sert Kısıtlar (Hard Constraints) - Kesinlikle İhlal Edilemez
1.  **Ders Çakışması:** Bir ders saati ve sınıf aynı anda sadece bir ders tarafından kullanılabilir.
2.  **Eğitmen Çakışması:** Bir eğitmen aynı anda iki farklı derste olamaz.
3.  **Kapasite:** Sınıf kapasitesi, dersin kontenjanından küçük olamaz (`Classroom.Capacity >= Section.Capacity`).

#### Esnek Kısıtlar (Soft Constraints) - Olabildiğince Sağlanmalı (Kodda Heuristics Olarak Eklendi)
1.  **Erken Saat Tercihi:** Dersler mümkünse sabah saatlerinde olsun.
2.  **Gün Dağılımı:** Dersler haftaya yayılmalı.

---

## 2. Kullanılan Algoritma: Backtracking with Heuristics

Problemi çözmek için **Backtracking (Geri İzleme)** algoritması kullanılmıştır. Performansı artırmak için **MRV (Minimum Remaining Values)** ve **LCV (Least Constraining Value)** sezgisel yöntemleri entegre edilmiştir.

### Algoritma Akışı (Pseudocode)

```csharp
function BacktrackingSchedule(assignments, unassigned_sections):
    // 1. Bitiş Kontrolü
    if unassigned_sections is empty:
        return true (Çözüm Bulundu!)

    // 2. Değişken Seçimi (MRV Heuristic)
    // En az yasal atama seçeneği olan dersi seç (Önce zoru çöz)
    section = SelectUsingMRV(unassigned_sections)

    // 3. Değer Sıralaması (LCV Heuristic)
    // Kalan dersleri en az kısıtlayacak zaman/sınıf çiftlerini önce dene
    ordered_values = OrderDomainValues(section)

    // 4. Deneme Döngüsü
    foreach value in ordered_values:
        if IsConsistent(section, value, assignments):
            // Atama Yap
            Add (section, value) to assignments
            
            // Recursive Adım
            result = BacktrackingSchedule(assignments, unassigned_sections - section)
            
            if result is true:
                return true
            
            // Backtrack (Geri Al)
            Remove (section, value) from assignments

    return false (Çözüm Yok)
```

## 3. Uygulama Detayları (`ScheduleManager.cs`)

### MRV Implementasyonu
Kod içerisinde `SelectUnassignedVariable` metodu, domain boyutu en küçük olan dersi seçer. Eşitlik durumunda kapasitesi en büyük olan derse öncelik verilir.

### LCV Implementasyonu
`OrderDomainValues` metodu, uygun zaman aralıklarını sıralarken sabah saatlerine (`Item3` - StartTime) öncelik verir.

### Çakışma Yönetimi
`IsConsistentAssignment` metodu:
- Veritabanındaki mevcut rezervasyonları kontrol eder.
- O anki çözüm yolundaki (`assignments`) diğer derslerle çakışmayı kontrol eder.

## 4. Genetik Algoritma (Opsiyonel / İleri Seviye)
Şu anki implementasyon deterministik Backtracking kullanır. +5 Puanlık opsiyonel gereksinim olan Genetik Algoritma (GA) entegrasyonu için altyapı uygundur ancak aktif edilmemiştir. GA kullanılsaydı `Mutation` ve `Crossover` operatörleri ile popülasyon bazlı bir yaklaşım izlenecekti.
