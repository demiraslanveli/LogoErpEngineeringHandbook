# 115 — Malzeme Kartı Servisi Referans Implementasyonu

Bu bölüm, referans .NET Framework 4.8 entegrasyon mimarisinde malzeme kartı işlemlerinin nasıl servisleştirileceğini açıklar.

## Amaç

Malzeme kartı oluşturma, güncelleme, okuma ve kontrollü pasife alma işlemlerini doğrudan SQL DML yerine Logo Objects `IData` üzerinden yönetmek.

## Katmanlar

```text
MaterialApplicationService
        ↓
MaterialValidator
        ↓
MaterialMapper
        ↓
IMaterialRepository
        ↓
LogoMaterialRepository
        ↓
IData
```

## DTO

```csharp
public sealed class MaterialRequest
{
    public string Code { get; set; }
    public string Name { get; set; }
    public string GroupCode { get; set; }
    public string MainUnitCode { get; set; }
    public bool Active { get; set; }
}
```

## Repository sözleşmesi

```csharp
public interface IMaterialRepository
{
    LogoOperationResult<int> Create(MaterialRequest request, LogoContext context);
    LogoOperationResult Update(int logicalRef, MaterialRequest request, LogoContext context);
    MaterialDto GetByCode(string code, LogoContext context);
}
```

## Application Service

```csharp
public sealed class MaterialApplicationService
{
    private readonly IMaterialRepository _repository;
    private readonly IMaterialValidator _validator;
    private readonly IIntegrationLogger _logger;

    public MaterialApplicationService(
        IMaterialRepository repository,
        IMaterialValidator validator,
        IIntegrationLogger logger)
    {
        _repository = repository;
        _validator = validator;
        _logger = logger;
    }

    public LogoOperationResult<int> Create(MaterialRequest request, LogoContext context)
    {
        var validation = _validator.Validate(request);

        if (!validation.Success)
            return LogoOperationResult<int>.Fail(validation.Errors);

        _logger.Info("Material.Create.Start", context.CorrelationId);

        var result = _repository.Create(request, context);

        _logger.Info(
            result.Success ? "Material.Create.Success" : "Material.Create.Fail",
            context.CorrelationId);

        return result;
    }
}
```

## Logo repository yaklaşımı

Gerçek `IData` nesnesinin `DataObjectType` değeri ve alan adları kullanılan Logo Objects sürümüne göre doğrulanmalıdır.

Genel akış:

```text
NewDataObject(...)
    ↓
New()
    ↓
DataFields.FieldByName(...).Value
    ↓
Lines / birim bilgileri
    ↓
Post()
    ↓
ErrorCode / ErrorDesc / ValidationErrors
```

## Birim ilişkileri

Malzeme kartı yalnızca `ITEMS` kaydı değildir.

Birim ilişkileri:

```text
ITEMS
  ↓
ITMUNITA
  ↓
UNITSETL
  ↓
UNITBARCODE
```

Bu nedenle kart oluşturulduktan sonra birim ve barkod ilişkileri de kontrollü biçimde ele alınmalıdır.

## Duplicate kontrolü

Yeni kart öncesinde en az:

- malzeme kodu
- barkod
- gerekiyorsa özel entegrasyon anahtarı

kontrol edilmelidir.

SQL okuma için `IQuery` veya read-only repository kullanılabilir.

## Update stratejisi

Update sırasında önce mevcut kart okunmalı, ardından yalnızca değiştirilmesine izin verilen alanlar güncellenmelidir.

```text
GetByCode / Read(logicalRef)
        ↓
Mevcut durum
        ↓
Diff
        ↓
IData update
        ↓
Post()
```

## Pasife alma

Kartı fiziksel olarak silmek yerine, iş kuralı uygunsa pasif hale getirmek çoğu entegrasyon senaryosunda daha güvenlidir.

Silme işlemi geçmiş hareket ilişkileri nedeniyle ayrıca değerlendirilmelidir.

## Idempotency

Dış sistem aynı malzeme kartı mesajını iki kez gönderirse ikinci çağrı yeni kart üretmemelidir.

Öneri:

```text
IdempotencyKey = SourceSystem + EntityType + ExternalId
```

## Log alanları

- CorrelationId
- CompanyNo
- PeriodNo
- MaterialCode
- LogicalRef
- Operation
- Result
- Logo error details

## Test senaryoları

1. yeni malzeme kartı
2. duplicate kod
3. geçersiz ana birim
4. mevcut kart güncelleme
5. pasif kartı güncelleme
6. Logo Post hatası
7. tekrar gönderilen aynı mesaj
8. birim/barkod ilişki hatası

> Malzeme kartı servisi, Logo tablosuna kayıt atan bir CRUD servisi değil; Logo Objects iş kurallarını kontrollü biçimde kullanan bir ERP application service olmalıdır.
