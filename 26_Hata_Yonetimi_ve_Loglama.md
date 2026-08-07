# 26 — Hata Yönetimi ve Loglama

## 1. Amaç

Logo Objects entegrasyonlarında hata yönetimi yalnızca kullanıcıya bir mesaj göstermekten ibaret değildir. Bir kayıt başarısız olduğunda geliştiricinin daha sonra şu soruların cevabını bulabilmesi gerekir:

- Hangi firma ve dönemdeydi?
- Hangi nesne tipi kullanıldı?
- Hangi kayıt işleniyordu?
- Hangi alanlarda hangi değerler vardı?
- `Post()` neden başarısız oldu?
- Aynı kayıt tekrar işlendi mi?
- İşlem kısmen tamamlandı mı?

Bu nedenle loglama, entegrasyon mimarisinin temel parçasıdır.

---

## 2. Hata Katmanları

Logo entegrasyonlarında hataları üç seviyede düşünmek faydalıdır.

### Teknik hata

Örnek:

```text
COM bağlantısı kurulamadı
Logo Objects yüklenemedi
SQL bağlantısı kesildi
Timeout oluştu
```

### Validasyon hatası

Örnek:

```text
Cari kod bulunamadı
Malzeme kodu geçersiz
Zorunlu alan boş
Birim uyumsuz
Seri/lot toplamı miktarı karşılamıyor
```

### İş kuralı hatası

Örnek:

```text
Sipariş kapalı
Dönem kapalı
Ambar yetkisi yok
Belge numarası mükerrer
Muhasebeleşmiş belge değiştirilemez
```

---

## 3. Post Sonucunu Mutlaka Kontrol Et

Kötü yaklaşım:

```csharp
obj.Post();
```

Doğru yaklaşım:

```csharp
if (!obj.Post())
{
    throw new Exception(obj.ErrorDesc);
}
```

`Post()` sonucu kontrol edilmeden işlem başarılı kabul edilmemelidir.

---

## 4. Logda Tutulması Gereken Alanlar

Önerilen minimum log şeması:

```text
ID
CreatedAt
CompanyNo
PeriodNo
Operation
DataObjectType
LogicalRef
DocumentNo
ExternalId
Success
ErrorCode
ErrorDescription
ValidationDetail
UserName
MachineName
ApplicationName
DurationMs
```

---

## 5. Örnek SQL Log Tablosu

```sql
CREATE TABLE dbo.LogoIntegrationLog
(
    ID BIGINT IDENTITY(1,1) PRIMARY KEY,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    CompanyNo INT NULL,
    PeriodNo INT NULL,
    Operation NVARCHAR(100) NULL,
    DataObjectType NVARCHAR(100) NULL,
    LogicalRef INT NULL,
    DocumentNo NVARCHAR(100) NULL,
    ExternalId NVARCHAR(200) NULL,
    Success BIT NOT NULL,
    ErrorCode INT NULL,
    ErrorDescription NVARCHAR(MAX) NULL,
    ValidationDetail NVARCHAR(MAX) NULL,
    UserName NVARCHAR(100) NULL,
    MachineName NVARCHAR(100) NULL,
    ApplicationName NVARCHAR(200) NULL,
    DurationMs BIGINT NULL
);
```

---

## 6. Log Seviyeleri

Kullanışlı seviyeler:

```text
INFO
WARNING
ERROR
CRITICAL
DEBUG
```

Üretim ortamında her alan değişikliğini debug seviyesinde loglamak performans ve depolama problemi oluşturabilir.

Bu nedenle seviyeler bilinçli kullanılmalıdır.

---

## 7. Correlation ID

Bir entegrasyon isteği birden fazla kayıt oluşturuyorsa tüm logların ortak bir kimlikle bağlanması faydalıdır.

Örnek:

```text
CorrelationId = 8b3d...
```

Aynı işlemde:

```text
Sipariş oluşturuldu
İrsaliye oluşturuldu
Fatura oluşturulamadı
```

olduğunda tüm adımlar aynı `CorrelationId` ile bulunabilir.

---

## 8. External ID

Dış sistemden gelen kayıtların benzersiz anahtarı loglanmalıdır.

```text
ExternalOrderId
ExternalDocumentId
ExternalLineId
```

Bu alanlar duplicate kontrolü ve tekrar işleme senaryolarında kritik öneme sahiptir.

---

## 9. Süre Ölçümü

Performans problemlerinde hangi adımın yavaş olduğu bilinmelidir.

Örnek:

```text
Login: 250 ms
SQL lookup: 30 ms
IData.New: 5 ms
Post: 1850 ms
Total: 2140 ms
```

Bu sayede sorun SQL'de mi, Logo Objects'te mi, ağda mı anlaşılabilir.

---

## 10. Kullanıcıya Gösterilen Mesaj ve Teknik Log Ayrımı

Kullanıcı mesajı:

```text
Fatura kaydedilemedi. Lütfen sistem yöneticisine başvurun.
```

Teknik log:

```text
Post failed
Company=102
Period=01
Object=SalesInvoice
ExternalId=ABC-456
ErrorCode=...
ErrorDesc=...
```

Teknik detayların tamamını son kullanıcıya göstermek gerekmez.

---

## 11. Retry Mantığı

Her hata tekrar denenmemelidir.

Retry yapılabilecek örnekler:

```text
Geçici ağ hatası
Timeout
SQL bağlantı kesintisi
Servis geçici olarak kullanılamıyor
```

Retry yapılmaması gereken örnekler:

```text
Cari kod yok
Malzeme kodu hatalı
Zorunlu alan boş
Belge numarası mükerrer
```

---

## 12. Retry + Idempotency

Retry mekanizması varsa idempotency zorunlu hale gelir.

Aksi halde:

```text
İlk istek işlendi
Cevap kayboldu
Retry başladı
Aynı belge ikinci kez oluştu
```

senaryosu yaşanabilir.

---

## 13. Dead Letter Queue Yaklaşımı

Toplu entegrasyonlarda başarısız kayıtlar ayrı bir kuyruğa alınabilir.

```text
IncomingQueue
    ↓
Process
    ↓
SuccessQueue
veya
DeadLetterQueue
```

Başarısız kayıt:

```text
ExternalId
Payload
Error
RetryCount
LastAttempt
```

bilgileriyle saklanmalıdır.

---

## 14. Payload Loglama

Dış sistemden gelen JSON/XML payload'ın tamamını loglamak faydalı olabilir.

Ancak:

- kişisel veri,
- parola,
- token,
- finansal hassas veri

varsa maskelenmelidir.

---

## 15. SQL Tarafı Audit Log

Kontrollü SQL düzeltmelerinde ayrı bir audit tablosu tutulmalıdır.

Örnek:

```text
TableName
RecordRef
FieldName
OldValue
NewValue
ChangedAt
ChangedBy
Reason
```

Bu yaklaşım özellikle manuel veri düzeltmelerinde çok değerlidir.

---

## 16. Trigger Logları

Trigger ile yapılan özel kontrollerde log tablosu oluşturmak sorunun kaynağını tespit etmeyi kolaylaştırır.

Örnek alanlar:

```text
LOGICALREF
STFICHEREF
TRCODE
OLD_SOURCEINDEX
NEW_SOURCEINDEX
ORDTRANSREF
LOGIN_NAME
HOST_NAME
PROGRAM_NAME
SESSION_ID
```

Bu tip loglar özellikle beklenmeyen ambar veya bağlantı değişikliklerinde faydalıdır.

---

## 17. SQL Server Session Bilgileri

Problemli SQL davranışında aşağıdaki bilgiler değerlidir:

```text
session_id
host_name
program_name
login_name
status
command
wait_type
blocking_session_id
logical_reads
reads
writes
```

Logo performans sorunlarında yalnızca sorguya değil, session bağlamına da bakılmalıdır.

---

## 18. Hata Mesajı Standardı

Önerilen format:

```text
[MODUL] [ISLEM] [KAYIT] Açıklama
```

Örnek:

```text
[SALES] [POST] [ABC2026000123] Cari hesap kodu bulunamadı.
```

Bu format log aramalarını kolaylaştırır.

---

## 19. Merkezi Exception Wrapper

Servis katmanında ortak exception sınıfı kullanılabilir.

```csharp
public class LogoIntegrationException : Exception
{
    public int? LogoErrorCode { get; }
    public string Operation { get; }

    public LogoIntegrationException(
        string operation,
        string message,
        int? logoErrorCode = null)
        : base(message)
    {
        Operation = operation;
        LogoErrorCode = logoErrorCode;
    }
}
```

---

## 20. Başarı Logu da Tutulmalı mı?

Evet, özellikle kritik entegrasyonlarda.

Yalnızca hata kayıtları tutulursa şu soru cevaplanamaz:

```text
Bu belge gerçekten Logo'ya başarıyla aktarıldı mı?
```

Başarı kaydı:

```text
ExternalId
LogoLogicalRef
DocumentNo
CreatedAt
```

bilgilerini içermelidir.

---

## 21. Monitoring

Log tutmak tek başına yeterli değildir.

Takip edilmesi gereken metrikler:

```text
Saatlik başarılı işlem sayısı
Saatlik hata sayısı
Ortalama Post süresi
Retry sayısı
Dead-letter kayıt sayısı
Logo login hatası sayısı
SQL timeout sayısı
```

---

## 22. Alarm Eşikleri

Örnek:

```text
10 dakikada 20'den fazla hata
Post süresi > 10 saniye
Queue backlog > 1000
Login başarısızlığı > 5
```

Bu durumda e-posta veya izleme sistemi uyarı üretebilir.

---

## 23. Sonuç

İyi bir Logo entegrasyonu yalnızca kayıt atan entegrasyon değildir; ne yaptığını açıklayabilen entegrasyondur.

Temel prensip:

> Her kritik işlemin kimliği, sonucu, süresi ve hata nedeni izlenebilir olmalıdır.
