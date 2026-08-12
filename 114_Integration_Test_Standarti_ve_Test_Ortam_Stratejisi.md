# 114 — Integration Test Standardı ve Test Ortam Stratejisi

Logo entegrasyonlarında unit testler tek başına yeterli değildir. Gerçek `IData`, gerçek şirket/dönem, Logo iş kuralları ve bağlı tablolar yalnızca integration test ile doğrulanabilir.

## Amaç

Integration test şu soruya cevap vermelidir:

> Bu kod gerçek Logo ortamında doğru ERP kaydını oluşturuyor mu?

## Test Ortamı

Production veritabanında integration test çalıştırılmamalıdır.

Önerilen ayrım:

```text
DEV
TEST / QA
UAT
PRODUCTION
```

Logo test firması mümkünse gerçek yapının kopyası veya temsilî bir versiyonu olmalıdır.

## Test Datası

Test verisi kontrollü hazırlanmalıdır.

Örnek master data:

```text
ITEM_TEST_001
CLIENT_TEST_001
WAREHOUSE_TEST
PROJECT_TEST
```

Testler rastgele production kartlarına bağlı olmamalıdır.

## Test Türleri

### Create Test

Kart/fiş oluşturulur ve sonuç doğrulanır.

### Readback Test

Oluşan kaydın Logo tarafında gerçekten oluştuğu kontrol edilir.

### Update Test

Güncellenebilir alanlar değiştirilir ve tekrar okunur.

### Delete / Cancel Test

Silme veya iptal davranışı belge türünün Logo kurallarına göre test edilir.

### Relationship Test

Bağlı tablolar kontrol edilir.

Örnek:

```text
INVOICE
  ↓
STFICHE / STLINE
  ↓
CLFLINE
  ↓
EMFICHE / EMFLINE
```

## Test Transaction Yaklaşımı

Logo Objects işlemlerini SQL transaction içine zorla sarmalamak her zaman mümkün veya doğru değildir.

Bu nedenle test cleanup ayrı tasarlanmalıdır.

Yaklaşımlar:

- test firmasını periyodik resetlemek
- test kayıtlarını özel prefix ile üretmek
- API/Objects üzerinden güvenli cleanup yapmak
- snapshot/restore kullanmak

## Test Naming

Örnek:

```text
CreateItem_WhenValidRequest_ShouldPostSuccessfully
CreateInvoice_WhenClientMissing_ShouldFailValidation
CreateInvoice_WhenPostFails_ShouldReturnLogoError
CreateSerialMovement_WhenQuantityMismatch_ShouldFail
```

## Arrange / Act / Assert

```text
Arrange
  ↓
Test master data + request

Act
  ↓
Application Service çağrısı

Assert
  ↓
Operation result
Logo readback
Linked tables
```

## Readback Kontrolü

Sadece `Success=true` yeterli değildir.

Örnek doğrulamalar:

- `LOGICALREF` oluşmuş mu?
- belge numarası doğru mu?
- tarih doğru mu?
- cari/malzeme referansı doğru mu?
- satır miktarı doğru mu?
- ambar doğru mu?
- döviz/KDV alanları doğru mu?
- bağlı hareketler oluşmuş mu?

## Regression Test

Logo sürüm güncellemesi veya entegrasyon kod değişikliği sonrası kritik senaryolar yeniden çalıştırılmalıdır.

Minimum regression paketi:

- malzeme kartı create/update
- cari kart create/update
- sipariş
- irsaliye
- fatura
- seri/lot hareketi
- üretim hareketi
- muhasebe bağlantısı

## Sürüm Matrisi

Logo Objects davranışı sürüme bağlı olabileceğinden test sonucu şu bilgilerle kaydedilmelidir:

```text
Logo Product Version
Logo Objects Version
Database Version
Company
Period
Test Date
```

## Otomasyon

Tam CI otomasyonu COM/Windows/Logo lisans koşulları nedeniyle zor olabilir.

Yine de mümkünse ayrılmış Windows test agent üzerinde integration test paketi çalıştırılabilir.

## Test Sonucu Loglama

Her integration test:

- TestRunId
- CorrelationId
- CompanyId
- PeriodId
- LogoVersion
- Result
- LogicalRef
- Error

bilgilerini kaydedebilir.

## Release Gate

Kritik entegrasyon değişikliği production'a çıkmadan önce ilgili integration test paketi başarıyla tamamlanmalıdır.

> Logo entegrasyonunda gerçek güven, yalnızca kodun compile olmasıyla değil; gerçek Logo iş kuralları altında beklenen bağlı kayıtları oluşturduğunun doğrulanmasıyla sağlanır.
