# 16 — Entegrasyon Mimarileri

## 1. Bölümün Amacı

Bu bölüm, Logo ERP'nin dış sistemlerle entegrasyonunda kullanılabilecek mimari yaklaşımları ele alır. Amaç yalnızca veri taşımak değil; veri sahipliği, transaction sınırı, hata yönetimi, tekrar işleme, performans ve izlenebilirlik açısından sürdürülebilir bir entegrasyon katmanı tasarlamaktır.

Logo entegrasyonu yapılırken şu sistemlerle sık karşılaşılır:

- MES,
- LIMS,
- WMS,
- e-ticaret,
- CRM,
- saha uygulamaları,
- özel WinForms/Web uygulamaları,
- Excel/Power BI,
- üçüncü parti muhasebe veya finans sistemleri.

> İyi entegrasyon, iki sistemi birbirine doğrudan bağlamak değil; sorumlulukları ve veri sahipliğini açık tanımlamaktır.

---

## 2. Temel Mimari Seçenekler

### 2.1 Doğrudan Logo Objects

```text
Uygulama
   ↓
Logo Objects
   ↓
Logo ERP
```

Avantajları:

- Logo iş kurallarını kullanır,
- veri bütünlüğünü daha iyi korur,
- kart ve fiş işlemleri için doğru katmandır.

Dezavantajları:

- kurulum ve bağımlılık gerektirir,
- COM/uygulama yaşam döngüsü yönetimi gerekir,
- yüksek hacimde dikkatli tasarım ister.

### 2.2 Ara Servis

```text
İstemci
   ↓
Integration API / Service
   ↓
Logo Objects
   ↓
Logo ERP
```

Kurumsal projelerde önerilen modellerden biridir.

İstemciler Logo Objects'e doğrudan bağımlı olmaz.

### 2.3 SQL Okuma + Objects Yazma

```text
Rapor / Kontrol → SQL
Kayıt İşlemi    → Logo Objects
```

Pratik ve güvenli hibrit yaklaşımdır.

---

## 3. Ara Yazılımın Rolü

Ara yazılım yalnızca veri dönüştürücü olmamalıdır.

Sorumlulukları:

- validation,
- mapping,
- authentication,
- authorization,
- idempotency,
- retry,
- logging,
- queue yönetimi,
- hata sınıflandırma,
- monitoring.

Logo Objects çağrıları bu katmanın altında tutulabilir.

---

## 4. Veri Sahipliği

Her veri için master sistem belirlenmelidir.

Örnek:

| Veri | Master Sistem |
|---|---|
| Malzeme kartı | Logo ERP |
| Üretim makinesi ölçümü | MES |
| Laboratuvar sonucu | LIMS |
| Resmi stok hareketi | Logo ERP |
| Web siparişi | E-ticaret |

Aynı verinin iki sistemde bağımsız değiştirilmesi senkronizasyon problemlerine yol açar.

---

## 5. External ID

Dış sistemden gelen her işleme benzersiz bir external ID verilmesi önerilir.

Örnek:

```text
SourceSystem = MES
ExternalId   = MES-PRD-20260807-000184
```

Logo'ya aktarım sonrası:

```text
ExternalId
LogoLogicalRef
Status
CreatedAt
ErrorMessage
```

saklanabilir.

Bu yapı tekrar işleme ve hata analizini kolaylaştırır.

---

## 6. Idempotency

Aynı mesaj iki kez geldiğinde ikinci Logo kaydı oluşmamalıdır.

Akış:

```text
Mesaj Geldi
   ↓
ExternalId var mı?
   ├─ Evet → mevcut sonucu döndür
   └─ Hayır → işlemi gerçekleştir
```

Özellikle queue, timeout ve retry kullanılan sistemlerde zorunludur.

---

## 7. Transaction Sınırı

Bir entegrasyon işleminin hangi noktada tamamlanmış sayılacağı açık olmalıdır.

Örneğin:

```text
1. Logo fişi oluştu
2. Seri/lot oluştu
3. External mapping kaydedildi
4. Kaynak sisteme ACK gönderildi
```

İkinci adım başarısızsa yalnızca ilk adımı başarılı kabul etmek tutarsızlık yaratabilir.

Logo Objects'in transaction davranışı ve uygulama katmanının kendi transaction sınırı birlikte değerlendirilmelidir.

---

## 8. Queue Mimarisi

Yüksek hacimli entegrasyonlarda senkron çağrı yerine queue kullanılabilir.

```text
Kaynak Sistem
     ↓
Integration Queue
     ↓
Worker Service
     ↓
Logo Objects
     ↓
Logo ERP
```

Avantajları:

- retry,
- load balancing,
- hata izolasyonu,
- kullanıcıyı bekletmeme,
- kontrollü işlem hızı.

---

## 9. Retry Stratejisi

Her hata retry edilmemelidir.

### Retry edilebilir

- geçici network hatası,
- servis geçici erişilemiyor,
- timeout,
- geçici lock.

### Retry edilmemeli

- malzeme kodu bulunamadı,
- zorunlu alan eksik,
- geçersiz cari,
- hatalı birim,
- iş kuralı ihlali.

Bu hatalar manuel düzeltme veya veri düzeltme kuyruğuna alınmalıdır.

---

## 10. Dead Letter Queue

Belirli sayıda denemeden sonra başarısız kayıtlar ayrı hata kuyruğuna taşınmalıdır.

Örnek:

```text
Status      = Failed
RetryCount  = 5
LastError   = "Material not found"
```

Operasyon ekibi buradan kayıtları inceleyebilir.

---

## 11. Logging

Her entegrasyon kaydı için minimum şu bilgiler tutulmalıdır:

- correlation ID,
- source system,
- external ID,
- işlem tipi,
- firma,
- dönem,
- başlangıç zamanı,
- bitiş zamanı,
- sonuç,
- Logo logical ref,
- hata kodu,
- hata mesajı.

Gerekirse request/response payload da maskelenmiş biçimde saklanabilir.

---

## 12. Correlation ID

Bir işlemin sistemler arasında izlenmesi için correlation ID çok değerlidir.

```text
MES → Middleware → Logo → Mail/Log
       aynı CorrelationId
```

Bir kullanıcı "bu üretim neden Logo'ya gitmedi?" dediğinde tüm zincir tek ID ile takip edilebilir.

---

## 13. Mapping Katmanı

Dış sistem kodları Logo kodlarıyla birebir aynı olmayabilir.

Örnek:

```text
MES Warehouse = RAW-01
Logo Ambar     = 4
```

Bu eşleşmeler kod içine gömülmek yerine mapping tablosunda tutulmalıdır.

```text
SourceSystem
MappingType
SourceCode
TargetCode
Active
```

---

## 14. Firma ve Dönem Routing

Çok firmalı entegrasyonda her mesaj hangi Logo firmasına gideceğini açıkça belirtmelidir.

```text
CompanyCode → LogoCompanyId
```

Dönem de belge tarihine veya açık dönem konfigürasyonuna göre belirlenebilir.

Firma/dönem değerlerinin kod içine sabit yazılması ölçeklenebilir değildir.

---

## 15. Tarih Yönetimi

Entegrasyonlarda şu tarihler ayrılmalıdır:

- işlem tarihi,
- belge tarihi,
- kayıt tarihi,
- entegrasyon tarihi,
- kaynak sistem zamanı.

Geç gelen bir mesajın Logo'ya bugünün tarihiyle mi yoksa gerçek işlem tarihiyle mi kaydedileceği iş kuralı olarak tanımlanmalıdır.

---

## 16. Seri/Lot Entegrasyonu

Seri/lot hareketi içeren entegrasyonlarda doğrulamalar:

- malzeme seri/lot takipli mi?
- lot numarası mevcut mu?
- aynı lot başka malzemeye ait mi?
- miktarlar eşit mi?
- SKT/üretim tarihi geçerli mi?
- lot kalite açısından kullanılabilir mi?

Stok fişi başarılı fakat lot bağlantısı başarısız olan işlem tamamlanmış kabul edilmemelidir.

---

## 17. Üretim Entegrasyonu

MES entegrasyonunda yaygın akış:

```text
Logo
  ↓ Üretim Emri
MES
  ↓ Gerçekleşme
Middleware
  ↓
Logo ProductionApplication / Objects
  ├── Sarf
  ├── Fire
  ├── Operasyon gerçekleşmesi
  └── Mamul girişi
```

Bu akışta resmi stok ve maliyet hareketlerinin Logo'da eksiksiz oluşması temel hedeftir.

---

## 18. SQL'in Entegrasyondaki Rolü

SQL şu amaçlarla kullanılabilir:

- lookup,
- doğrulama,
- raporlama,
- kontrol,
- reconciliation.

Ancak kart/fiş oluşturmak için ilk tercih doğrudan SQL olmamalıdır.

---

## 19. Reconciliation

Entegrasyonun başarılı cevap vermesi verilerin gerçekten eşit olduğu anlamına gelmez.

Periyodik reconciliation yapılmalıdır.

Örnek:

```text
MES üretim adedi       1.000
Logo mamul girişi        998
Fark                       2
```

Fark raporları otomatik üretilebilir.

---

## 20. Güvenlik

Entegrasyon servisinde:

- database şifresi istemciye verilmemeli,
- Logo kullanıcı bilgisi merkezi yönetilmeli,
- API authentication kullanılmalı,
- secret'lar config dosyasında açık metin tutulmamalı,
- işlem yetkileri rol bazlı olmalı.

---

## 21. Observability

Kurumsal entegrasyon yalnızca log dosyasından ibaret olmamalıdır.

Takip edilmesi gereken metrikler:

- dakika başına işlem,
- başarı oranı,
- hata oranı,
- ortalama süre,
- queue uzunluğu,
- retry sayısı,
- en eski bekleyen kayıt.

---

## 22. Önerilen Referans Mimari

```text
                 ┌─────────────┐
                 │ MES / LIMS  │
                 └──────┬──────┘
                        │
                 ┌──────▼──────┐
                 │ API / Queue │
                 └──────┬──────┘
                        │
              ┌─────────▼─────────┐
              │ Integration Layer │
              │ Validation        │
              │ Mapping           │
              │ Idempotency       │
              │ Logging           │
              └─────────┬─────────┘
                        │
          ┌─────────────▼─────────────┐
          │ Logo Objects / Production │
          └─────────────┬─────────────┘
                        │
                 ┌──────▼──────┐
                 │  Logo ERP   │
                 └─────────────┘
```

SQL okuma ve reconciliation servisleri ayrıca bu mimariye bağlanabilir.

---

## 23. Sonuç

Logo entegrasyonunun kalitesi yalnızca API çağrısının başarılı olmasına bağlı değildir. Veri sahipliği, mapping, idempotency, transaction sınırı, queue, retry, logging ve reconciliation birlikte tasarlanmalıdır.

Özellikle üretim entegrasyonlarında ara yazılım operasyonel süreçleri yönetebilir; fakat resmi stok, üretim ve maliyet hareketlerinin Logo tarafında doğru oluşması temel prensiptir.

Bir sonraki bölümde gerçek projelerde karşılaşılan hata türleri ve çözüm kalıpları **vaka analizleri** üzerinden ele alınacaktır.
