# 93 — SQL Agent ve Database Mail Mimarisi

## Amaç

Logo ERP çevresinde çalışan arka plan görevleri, bildirim servisleri, kontrol prosedürleri ve rapor üretim süreçlerinde SQL Server Agent ve Database Mail sık kullanılan iki temel bileşendir.

Bu bölümün amacı bu iki bileşeni yalnızca "çalışan job" seviyesinde değil, üretim ortamında sürdürülebilir ve izlenebilir bir mimariyle ele almaktır.

---

## SQL Server Agent ne için kullanılmalı?

SQL Agent aşağıdaki işler için uygundur:

- Periyodik kontrol prosedürleri
- Günlük/haftalık rapor üretimi
- Database Mail tetikleme
- Arşivleme ve bakım işlemleri
- Reconciliation kontrolleri
- Entegrasyon queue tüketimi
- Veri kalite kontrolleri
- Son kullanma tarihi / vade / yasal süre uyarıları

Ancak iş kritik Logo ERP transaction oluşturma süreçleri yalnızca SQL Agent içine gömülmemelidir. Logo Objects veya servis katmanı gerektiren işlemler ayrı worker/service katmanında ele alınmalıdır.

---

## Job tasarım prensibi

Her job aşağıdaki bileşenlere sahip olmalıdır:

```text
Job
 ├── Pre-check
 ├── Business step
 ├── Result validation
 ├── Logging
 └── Error notification
```

Tek bir devasa step yerine ayrıştırılmış step yapısı hata teşhisini kolaylaştırır.

---

## Idempotent job tasarımı

Aynı job ikinci kez çalıştığında aynı kaydı tekrar üretmemelidir.

Örnek kontrol:

```sql
IF NOT EXISTS
(
    SELECT 1
    FROM dbo.LOG10_MAIL_LOG
    WHERE REFID = @RefId
      AND MAILTYPE = @MailType
)
BEGIN
    -- mail üret
END
```

İdeal yaklaşımda dış sistem veya belge bazlı bir `IdempotencyKey` tutulur.

---

## Job log tablosu

Örnek yapı:

```sql
CREATE TABLE dbo.Z_JOB_EXECUTION_LOG
(
    ID BIGINT IDENTITY PRIMARY KEY,
    JOB_NAME SYSNAME NOT NULL,
    START_DATE DATETIME2 NOT NULL,
    END_DATE DATETIME2 NULL,
    STATUS VARCHAR(20) NOT NULL,
    ROW_COUNT INT NULL,
    ERROR_MESSAGE NVARCHAR(MAX) NULL
);
```

Bu yapı SQL Agent history'den bağımsız operasyonel izleme sağlar.

---

## Database Mail mimarisi

Mail gönderimi iş mantığından ayrılmalıdır.

Önerilen akış:

```text
Business Check
    ↓
Mail Queue
    ↓
Mail Worker / sp_send_dbmail
    ↓
Mail Log
```

Doğrudan her trigger veya procedure içinden mail göndermek transaction süresini ve bağımlılığı artırabilir.

---

## Mail queue örneği

```sql
CREATE TABLE dbo.Z_MAIL_QUEUE
(
    ID BIGINT IDENTITY PRIMARY KEY,
    SUBJECT NVARCHAR(500),
    BODY NVARCHAR(MAX),
    RECIPIENTS NVARCHAR(MAX),
    STATUS TINYINT NOT NULL DEFAULT 0,
    TRY_COUNT INT NOT NULL DEFAULT 0,
    CREATED_AT DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    SENT_AT DATETIME2 NULL,
    ERROR_MESSAGE NVARCHAR(MAX) NULL
);
```

`STATUS` örneği:

```text
0 = Bekliyor
1 = Gönderildi
2 = Hata
3 = Retry bekliyor
```

---

## Database Mail hata kontrolü

Mail gönderildi varsayılmamalıdır.

Kontrol edilmesi gereken yapılar:

- `msdb.dbo.sysmail_allitems`
- `msdb.dbo.sysmail_faileditems`
- `msdb.dbo.sysmail_event_log`

Mail başarısızsa iş kaydı gönderilmiş kabul edilmemelidir.

---

## Logo projelerinde tipik kullanım

Gerçek senaryolar:

- Vadesi 7 gün kalan cari borç uyarısı
- Cuma gecesi vadesi geçmiş borç raporu
- Yasal 7 günlük fatura süresi kontrolü
- Alım fiyat kontrol raporu
- Fatura tarih / KDV / veri bütünlüğü kontrol maili
- Background queue hata bildirimi

---

## Dikkat

SQL Agent görevi çalıştı diye iş süreci başarıyla tamamlandı kabul edilmemelidir.

Doğru başarı kriteri:

```text
Job çalıştı
+ business işlem tamamlandı
+ sonuç doğrulandı
+ log yazıldı
+ gerekiyorsa mail gerçekten gönderildi
```

---

## Sonuç

SQL Agent ve Database Mail Logo ekosisteminde çok güçlü araçlardır. Ancak job, queue, retry, idempotency ve loglama ayrıştırılmadan kullanılırsa zamanla sessiz hata üreten operasyonel borca dönüşür.
