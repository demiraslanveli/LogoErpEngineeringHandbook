# 94 — Backup, Restore ve Disaster Recovery Stratejisi

## Amaç

Logo ERP veritabanı için yedek almak tek başına yeterli değildir. Gerçek güvence, gerektiğinde verinin doğru noktaya ve kabul edilebilir sürede geri döndürülebilmesidir.

Bu nedenle backup stratejisi mutlaka restore testi ve disaster recovery planı ile birlikte ele alınmalıdır.

---

## Temel kavramlar

### RPO

Recovery Point Objective.

Kabul edilebilir maksimum veri kaybı süresidir.

Örnek:

```text
RPO = 15 dakika
```

En fazla 15 dakikalık veri kaybı kabul edilebilir anlamına gelir.

### RTO

Recovery Time Objective.

Sistemin ne kadar sürede tekrar çalışır hale gelmesi gerektiğidir.

Örnek:

```text
RTO = 60 dakika
```

---

## Backup tipleri

SQL Server tarafında temel backup tipleri:

- Full backup
- Differential backup
- Transaction log backup

Örnek politika:

```text
Pazar        : Full
Her gece     : Differential
15 dakikada  : Transaction Log
```

Bu yalnızca örnektir; gerçek sıklık RPO/RTO hedeflerine göre belirlenmelidir.

---

## Recovery model

Logo veritabanında recovery model bilinmeden backup politikası tasarlanamaz.

Kontrol:

```sql
SELECT name, recovery_model_desc
FROM sys.databases
WHERE name = DB_NAME();
```

`FULL` recovery model kullanılıyorsa log backup zinciri düzenli çalışmalıdır.

Log backup alınmıyorsa transaction log kontrolsüz büyüyebilir.

---

## Backup doğrulama

Backup dosyasının oluşması yeterli değildir.

En azından:

```sql
RESTORE VERIFYONLY
FROM DISK = 'D:\Backup\LogoERP.bak';
```

uygulanabilir.

Ancak gerçek doğrulama yöntemi farklı bir sunucuda restore testidir.

---

## Restore tatbikatı

Periyodik olarak şu senaryo test edilmelidir:

```text
Son Full Backup
    ↓
Son Differential
    ↓
Transaction Log zinciri
    ↓
Test sunucusuna restore
    ↓
DBCC CHECKDB
    ↓
Logo uygulama bağlantı testi
```

---

## Point-in-time restore

Yanlış toplu `UPDATE` veya `DELETE` gibi olaylarda point-in-time restore kritik olabilir.

Örnek senaryo:

```text
10:00 yanlış update
09:59:55 noktasına geri dönme ihtiyacı
```

Bunun için FULL recovery model ve sağlıklı log backup zinciri gerekir.

---

## Backup lokasyonu

Aynı fiziksel disk üzerindeki backup gerçek felaket koruması değildir.

Önerilen katmanlar:

```text
Primary SQL disk
    ↓
Yerel backup diski
    ↓
Farklı sunucu / NAS
    ↓
Off-site veya immutable storage
```

---

## Encryption ve erişim

Backup dosyaları hassas ticari veri içerir.

Bu nedenle:

- Backup klasörü minimum yetki ile korunmalıdır.
- Backup kopyaları kontrolsüz paylaşılmamalıdır.
- Mümkünse backup encryption kullanılmalıdır.
- Restore yapabilecek hesaplar sınırlandırılmalıdır.

---

## Logo özel kontrol listesi

Restore sonrası yalnızca database ONLINE olması yeterli değildir.

Kontrol:

- Firma ve dönem tabloları erişilebilir mi?
- Logo kullanıcıları login olabiliyor mu?
- Objects / REST Service bağlantısı çalışıyor mu?
- SQL Agent job'ları doğru sunucuyu mu kullanıyor?
- Database Mail profilleri doğru mu?
- Entegrasyon servis connection string'leri doğru mu?
- Dosya yolu / attachment / e-belge entegrasyonları çalışıyor mu?

---

## Tail-log backup

Database hasarlı fakat log erişilebilir durumdaysa, restore öncesi tail-log backup veri kaybını azaltabilir.

Bu operasyon mutlaka deneyimli DBA tarafından olayın durumuna göre uygulanmalıdır.

---

## Sonuç

Backup bir dosya üretme işlemi değildir.

Gerçek backup stratejisi:

```text
Backup
+ off-site copy
+ monitoring
+ restore test
+ RPO/RTO
+ runbook
```

bütünüdür.
