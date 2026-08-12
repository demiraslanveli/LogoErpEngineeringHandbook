# 129 — Deployment, Rollback ve Release Prosedürü

Bu bölüm Logo ERP entegrasyon servislerinin production ortama güvenli çıkarılması için standart release prosedürünü tanımlar.

## Amaç

Deployment işlemini yalnızca dosya kopyalama olmaktan çıkarıp kontrollü bir operasyon haline getirmek.

## Release Paketi

Önerilen paket:

```text
release/
├─ app/
├─ database/
├─ config/
├─ scripts/
├─ checks/
├─ rollback/
└─ RELEASE_NOTES.md
```

## Release İçeriği

Her release için en az şu bilgiler bulunmalıdır:

- application version
- git commit/tag
- database schema version
- Logo Objects/ERP uyumluluk notu
- değişen servisler
- yeni migration'lar
- configuration değişiklikleri
- bilinen riskler
- rollback yöntemi

## Pre-Deployment Checklist

Deployment öncesi:

```text
Doğru müşteri / ortam mı?
Doğru database mi?
Doğru Logo firma/dönem konfigurasyonu mu?
Backup doğrulandı mı?
Disk alanı yeterli mi?
Servis hesabı mevcut mu?
Gerekli DLL/Objects sürümü kurulu mu?
Migration ön koşulları tamam mı?
Bekleyen kritik queue işlemleri var mı?
```

## Servis Durdurma

Worker yeni işlem almadan kontrollü kapanmalıdır.

İdeal sıra:

```text
Stop accepting new work
    ↓
Finish or safely release current work
    ↓
Persist state
    ↓
Dispose Logo session
    ↓
Stop service
```

## Database Deployment

Migration uygulanmadan önce:

- hedef DB doğrulanmalı
- schema version okunmalı
- backup/restore noktası doğrulanmalı
- migration sırası kontrol edilmeli

## Binary Deployment

Eski release klasörü doğrudan silinmemelidir.

Önerilen yapı:

```text
C:\Services\LogoIntegration\
├─ releases\
│  ├─ 1.4.0\
│  ├─ 1.4.1\
│  └─ 1.5.0\
└─ current\
```

Windows Service çalışma yöntemi ve kurum politikası bu yapıyı desteklemiyorsa aynı mantık farklı klasörleme ile uygulanabilir.

## Configuration

Secret içeren production config repository'de tutulmamalıdır.

Deployment sırasında:

- environment-specific config uygulanır
- secret çözümleme test edilir
- connection bilgileri loglanmaz

## Post-Deployment Kontrolleri

Servis başladıktan sonra:

```text
Service running?
SQL connection OK?
Logo session açılabiliyor mu?
Firma/dönem doğru mu?
Queue okunabiliyor mu?
Idempotency store erişilebilir mi?
Health check başarılı mı?
Smoke test tamam mı?
```

## Smoke Test

Production ortamında mümkünse veri değiştirmeyen test tercih edilir.

Write testi gerekiyorsa özel test kaydı ve kontrollü geri alma prosedürü kullanılmalıdır.

## Rollback Türleri

### Binary rollback

Önceki binary sürümüne dönüş.

### Configuration rollback

Önceki config snapshot'una dönüş.

### Database rollback

Yalnızca önceden test edilmiş rollback scripti varsa.

### Forward fix

Veri kaybı riski varsa çoğu production migration için eski şemaya geri dönmek yerine düzeltici yeni migration daha güvenli olabilir.

## Rollback Kararı

Rollback tetikleyicileri önceden tanımlanmalıdır:

- servis başlayamıyor
- Logo login başarısız
- kritik transaction hatası
- duplicate kayıt üretiliyor
- queue hızla büyüyor
- veri bütünlüğü kontrolü başarısız
- beklenmeyen yüksek hata oranı

## Release Log

Her deployment kaydı tutulmalıdır:

```text
ReleaseVersion
CommitSha
SchemaVersion
Environment
StartedAt
CompletedAt
Operator
Result
RollbackVersion
Notes
```

## Kural

> Geri dönüş planı olmayan production deployment tamamlanmış bir release planı değildir.
