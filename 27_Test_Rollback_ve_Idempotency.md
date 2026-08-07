# 27 — Test, Rollback ve Idempotency

## 1. Amaç

Logo ERP entegrasyonlarında veri bütünlüğünü korumak için yalnızca doğru kod yazmak yetmez. İşlemin güvenli biçimde denenebilmesi, gerektiğinde geri alınabilmesi ve aynı isteğin tekrar gelmesi durumunda çift kayıt üretmemesi gerekir.

Bu bölüm üç temel kavramı ele alır:

```text
Test Mode
Rollback
Idempotency
```

---

## 2. Test Mode

Toplu veya kritik işlemlerde ilk aşamada gerçek kayıt oluşturmadan sonuç görmek çok değerlidir.

Örnek yaklaşım:

```text
@TestModu = 1
```

ise:

```text
Kayıtları bul
Bağlantıları kontrol et
Etkilenecek satırları göster
Hata ve eksikleri raporla
UPDATE / POST yapma
```

Gerçek işlem:

```text
@TestModu = 0
```

ise:

```text
Kontrolleri tekrar yap
Transaction başlat
İşlemleri uygula
Sonucu doğrula
Commit et
```

---

## 3. Test Modunda Gösterilmesi Gerekenler

Önerilen çıktı:

```text
Firma No
Dönem
Belge No
TRCODE
Mevcut Tarih
Yeni Tarih
Fatura Ref
Stok Fiş Ref
Stok Satırı Sayısı
Cari Hareket Sayısı
Muhasebe Fişi Var/Yok
Muhasebe Satırı Sayısı
Uyarılar
```

Bu çıktı kullanıcıya gerçek güncellemeden önce kontrol imkânı verir.

---

## 4. Eksik Kayıtta Tüm İşlemi Durdurmak Gerekir mi?

Her zaman değil.

Örneğin toplu fatura tarih güncellemesinde listede bulunmayan bir fatura varsa iki yaklaşım mümkündür.

### Katı yaklaşım

```text
Bir kayıt eksikse tüm işlem durur.
```

### Toleranslı yaklaşım

```text
Eksik kaydı raporla
Diğer geçerli kayıtlarla devam et
```

Operasyonel bakım araçlarında çoğu zaman ikinci yaklaşım daha kullanışlıdır.

Ancak veri bütünlüğü açısından birbirine bağımlı kayıtlar söz konusuysa işlem durdurulmalıdır.

---

## 5. Transaction

SQL düzeltmelerinde temel kalıp:

```sql
BEGIN TRY
    BEGIN TRANSACTION;

    -- UPDATE / INSERT / DELETE

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    THROW;
END CATCH;
```

Bu yapı yarım güncellenmiş kayıt riskini azaltır.

---

## 6. Birden Fazla Tablo Güncelleme

Örneğin fatura tarihi düzeltmesi:

```text
INVOICE
STFICHE
STLINE
CLFLINE
EMFICHE
EMFLINE
```

birlikte ele alınabilir.

Bu tabloların yarısı güncellenip yarısı güncellenmezse belge zinciri tutarsız hale gelir.

Bu nedenle aynı mantıksal işlemin mümkün olduğunca tek transaction içinde yürütülmesi gerekir.

---

## 7. Önce Snapshot Al

Kritik SQL düzeltmelerinde update öncesi eski değerleri audit tablosuna yazmak faydalıdır.

Örnek:

```sql
INSERT INTO dbo.LogoUpdateAudit
(
    TableName,
    LogicalRef,
    OldValue,
    NewValue,
    ChangedAt
)
SELECT
    'LG_102_01_INVOICE',
    LOGICALREF,
    CONVERT(NVARCHAR(30), DATE_, 126),
    CONVERT(NVARCHAR(30), @YeniTarih, 126),
    SYSDATETIME()
FROM LG_102_01_INVOICE
WHERE LOGICALREF = @Ref;
```

Bu rollback ve denetim için değerlidir.

---

## 8. Manuel Rollback Scripti

Bazı bakım araçlarında işlemden önce rollback scripti üretmek faydalıdır.

Örnek mantık:

```text
UPDATE ... SET DATE_ = 'eski tarih' WHERE LOGICALREF = ...;
```

Bu script otomatik veya log üzerinden üretilebilir.

---

## 9. Logo Objects ve Rollback

`IData.Post()` işlemlerinde SQL transaction mantığını dışarıdan zorlamak her zaman mümkün veya doğru değildir.

Bu nedenle Logo Objects entegrasyonunda rollback daha çok süreç seviyesinde tasarlanmalıdır.

Örneğin:

```text
1. Cari oluşturuldu
2. Sipariş oluşturulamadı
```

Bu durumda karar:

```text
Cari kalmalı mı?
Silinmeli mi?
İşlem tekrar mı denenmeli?
```

önceden belirlenmelidir.

---

## 10. Compensation Pattern

Dağıtık veya çok adımlı entegrasyonlarda klasik transaction yerine telafi işlemi kullanılabilir.

Örnek:

```text
Sipariş oluştur
    ↓
Harici servis çağır
    ↓ hata
Siparişi iptal et / sil / statü değiştir
```

Bu yaklaşım compensation olarak düşünülebilir.

---

## 11. Idempotency Nedir?

Aynı isteğin birden fazla kez çalıştırılması sonucunda sistemin tek bir mantıksal kayıt üretmesi hedefidir.

Örnek dış sistem isteği:

```text
OrderId = WEB-2026-1542
```

Bu istek iki kez gelirse Logo'da iki sipariş oluşmamalıdır.

---

## 12. Idempotency Key

Her dış sistem kaydı için benzersiz bir anahtar kullanılmalıdır.

Örnek:

```text
SourceSystem = WEB
ExternalId   = WEB-2026-1542
```

Birlikte unique olabilir:

```sql
CREATE UNIQUE INDEX UX_Integration_Source_External
ON dbo.IntegrationMap(SourceSystem, ExternalId);
```

---

## 13. Integration Mapping Tablosu

Örnek:

```sql
CREATE TABLE dbo.IntegrationMap
(
    ID BIGINT IDENTITY PRIMARY KEY,
    SourceSystem NVARCHAR(50) NOT NULL,
    ExternalId NVARCHAR(200) NOT NULL,
    LogoCompanyNo INT NOT NULL,
    LogoPeriodNo INT NULL,
    LogoObjectType NVARCHAR(100) NULL,
    LogoLogicalRef INT NULL,
    LogoDocumentNo NVARCHAR(100) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT UQ_IntegrationMap UNIQUE(SourceSystem, ExternalId)
);
```

---

## 14. İşlem Öncesi Duplicate Kontrolü

```sql
SELECT LogoLogicalRef
FROM dbo.IntegrationMap
WHERE SourceSystem = @SourceSystem
  AND ExternalId = @ExternalId;
```

Kayıt varsa yeni `Post()` yapılmamalıdır.

---

## 15. Race Condition

Sadece önce `SELECT`, sonra `INSERT` yapmak yeterli değildir.

İki worker aynı anda çalışabilir:

```text
Worker A → kayıt yok
Worker B → kayıt yok
Worker A → insert
Worker B → insert
```

Bu nedenle database seviyesinde unique constraint bulunmalıdır.

---

## 16. Timeout Sonrası Ne Yapılmalı?

En tehlikeli senaryolardan biri:

```text
Post() başladı
Logo kaydı oluşturdu
İstemci timeout aldı
```

İstemci işlemin başarısız olduğunu sanabilir.

Doğru yaklaşım:

```text
Retry öncesi ExternalId ile kontrol et
Belge oluşmuşsa mevcut kaydı başarı kabul et
Yoksa tekrar dene
```

---

## 17. Belge Numarasını Idempotency Anahtarı Olarak Kullanmak

Her zaman güvenli değildir.

Çünkü:

- numara Logo tarafından üretiliyor olabilir,
- farklı firma/dönemde aynı numara olabilir,
- numara değiştirilebilir,
- iade/özel belge türleri aynı formatı kullanabilir.

Dış sistem ID'si daha sağlıklı bir anahtardır.

---

## 18. Retry Sayısı

Önerilen alanlar:

```text
RetryCount
LastAttemptAt
NextAttemptAt
LastError
Status
```

Durumlar:

```text
Pending
Processing
Succeeded
Failed
DeadLetter
```

---

## 19. Processing Lock

Aynı kaydı iki servis örneğinin aynı anda işlememesi gerekir.

SQL tabanlı yaklaşımda:

```text
Status = Pending
    ↓ atomik claim
Status = Processing
```

mekanizması kullanılabilir.

---

## 20. Test Ortamı

Logo Objects geliştirmeleri mümkünse üretim veritabanında doğrudan denenmemelidir.

İdeal ortam:

```text
Prod DB clone
Test firma/dönem
Logo Objects aynı sürüm
Aynı lisans/konfigürasyon davranışı
```

---

## 21. Regression Test

Logo sürümü değiştiğinde temel entegrasyon senaryoları tekrar çalıştırılmalıdır.

Örnek test seti:

```text
Login
Malzeme oku
Cari oku
Yeni sipariş
Sipariş güncelle
İrsaliye oluştur
Fatura oluştur
Seri/lotlu hareket
Üretim hareketi
Hatalı kayıt validasyonu
```

---

## 22. Test Verisi

Test kayıtlarının kolay ayırt edilmesi gerekir.

Örnek:

```text
TEST.OBJ.001
TEST-CARI-001
TEST202600001
```

Üretim ortamında test yapılması zorunluysa kayıtların sonradan temizlenme planı olmalıdır.

---

## 23. Dry Run Raporu

Toplu işlemler için dry-run çıktısı saklanabilir.

Örnek:

```text
RunId
StartedAt
RequestedCount
FoundCount
MissingCount
WillUpdateCount
WarningCount
```

Gerçek çalıştırma aynı `RunId` üzerinden izlenebilir.

---

## 24. Öncesi ve Sonrası Doğrulaması

İşlem tamamlandıktan sonra yalnızca SQL update sayısına güvenilmemelidir.

Örnek:

```text
Önce: Fatura tarihi 2026-08-01
Sonra: Fatura tarihi 2026-07-31
Bağlı STFICHE: 2026-07-31
Bağlı STLINE: 2026-07-31
Cari hareket: 2026-07-31
Muhasebe: 2026-07-31
```

Sonuç doğrulanmalıdır.

---

## 25. İşlem Özeti

Her toplu operasyon sonunda şu özet üretilmelidir:

```text
İstenen kayıt: 70
Bulunan: 68
Başarılı: 67
Başarısız: 1
Bulunamayan: 2
Rollback: 0
```

---

## 26. Sonuç

Logo entegrasyonunda güvenlik üç katmanlı düşünülmelidir:

```text
Test et
    ↓
Atomik / geri alınabilir çalıştır
    ↓
Tekrar geldiğinde çift kayıt üretme
```

Temel prensip:

> Bir işlemi sadece başarılı çalışacak şekilde değil, başarısız olduğunda ve tekrar çalıştırıldığında da güvenli olacak şekilde tasarla.
