# 77 — Monitoring, Observability ve Operasyon Runbook

## 1. Amaç

Bu bölüm, Logo ERP entegrasyon servislerinin yalnızca çalışıyor görünmesini değil, gerçekten sağlıklı olup olmadığının ölçülmesini ve sorun anında sistematik müdahale edilmesini açıklar.

## 2. Monitoring ve Observability Farkı

Monitoring bilinen metrikleri izler.

Observability ise sistemin iç davranışını log, metric ve trace verilerinden anlamaya çalışır.

Logo entegrasyonunda ikisi birlikte gereklidir.

## 3. Temel Metrikler

### Entegrasyon metrikleri

- İşlenen kayıt sayısı
- Başarılı kayıt sayısı
- Hatalı kayıt sayısı
- Retry sayısı
- DeadLetter sayısı
- Queue backlog
- Ortalama işlem süresi
- P95 / P99 işlem süresi

### Logo Objects metrikleri

- Login süresi
- Post süresi
- Hatalı Post sayısı
- COM exception sayısı
- Aktif session sayısı

### SQL metrikleri

- CPU
- Memory pressure
- IO latency
- Blocking
- Deadlock
- Tempdb kullanımı
- Uzun süren sorgular

## 4. Structured Logging

Loglar serbest metin yerine yapılandırılmış olmalıdır.

Örnek:

```json
{
  "level": "Error",
  "service": "LogoIntegrationWorker",
  "operation": "CreateInvoice",
  "firmNr": 40,
  "periodNr": 1,
  "externalId": "MES-12345",
  "correlationId": "abc-123",
  "durationMs": 1850,
  "errorType": "LogoPostError",
  "message": "Post failed"
}
```

## 5. Correlation ID

Bir iş olayının tüm sistemler boyunca aynı correlation ID ile izlenmesi önerilir.

```text
MES
 ↓ correlationId
Integration API
 ↓
Queue
 ↓
Logo Worker
 ↓
Logo ERP
```

Bu yapı hata analizini ciddi biçimde kolaylaştırır.

## 6. Health Check

Servisler için en az iki health check bulunmalıdır.

### Liveness

Process çalışıyor mu?

### Readiness

Gerçekten işlem yapabilir mi?

Readiness kontrolünde:

- SQL bağlantısı
- Queue bağlantısı
- Gerekirse Logo login testi

kontrol edilebilir.

## 7. Alarm Seviyeleri

### Warning

- Retry yükseliyor
- İşlem süresi artıyor
- Queue büyüyor

### Critical

- İşlem tamamen durmuş
- Logo login sürekli başarısız
- SQL erişilemiyor
- DeadLetter hızlı artıyor

## 8. Queue Backlog Alarmı

Örnek metrik:

```text
PendingCount
OldestPendingAgeMinutes
```

Sadece kayıt sayısı değil en eski bekleyen kaydın yaşı da izlenmelidir.

## 9. SLO / SLA Yaklaşımı

Örnek servis seviyesi hedefi:

```text
%99.5 entegrasyon işlemi 5 dakika içinde Logo'ya aktarılmalı.
```

Böylece yalnızca uptime yerine gerçek iş sonucu ölçülür.

## 10. Dashboard

Önerilen dashboard bileşenleri:

- Son 24 saat işlem sayısı
- Başarı oranı
- Hata oranı
- Queue backlog
- Ortalama işlem süresi
- En sık hata tipleri
- Firma bazlı hata dağılımı
- Son başarılı işlem zamanı

## 11. Operasyon Runbook

Her kritik hata için standart müdahale adımları bulunmalıdır.

### Logo Login Hatası

1. Logo servis/process çalışıyor mu?
2. Lisans/session limiti var mı?
3. Kullanıcı yetkisi doğru mu?
4. Firma/dönem erişilebilir mi?
5. Servis hesabı doğru mu?

### Queue Birikiyor

1. Worker aktif mi?
2. Son hata nedir?
3. Logo erişimi var mı?
4. SQL blocking var mı?
5. Aynı kayıt sürekli retry mı oluyor?

### SQL Yavaşlığı

1. Blocking kontrolü
2. Wait stats
3. IO latency
4. Tempdb
5. Uzun sorgular
6. Execution plan

## 12. DeadLetter Operasyonu

DeadLetter kaydı için operatör:

- Hata mesajını görmeli
- Payload'u inceleyebilmeli
- Mapping düzeltmeli
- Retry başlatabilmeli
- Kaydı iptal edebilmeli

## 13. Audit Trail

Kritik manuel operasyonlar loglanmalıdır:

```text
WHO
WHEN
ACTION
OLD_STATUS
NEW_STATUS
REASON
```

Özellikle retry, force-complete ve manual mapping değişiklikleri audit altında olmalıdır.

## 14. Veri Reconciliation Dashboard

Sistem sağlıklı görünse bile veri tutarsızlığı olabilir.

Bu nedenle ayrı reconciliation metrikleri izlenmelidir:

- Kaynak sistem işlem sayısı
- Logo işlem sayısı
- Eksik işlem sayısı
- Mükerrer işlem sayısı
- Tutar farkı
- Stok farkı

## 15. Sonuç

Üretim ortamındaki Logo entegrasyonunun kalitesi sadece kod kalitesiyle ölçülmez.

Sistem:

- İzlenebilir,
- ölçülebilir,
- alarm üretebilir,
- tekrar işlenebilir,
- operasyon runbook'una sahip

olmalıdır.
