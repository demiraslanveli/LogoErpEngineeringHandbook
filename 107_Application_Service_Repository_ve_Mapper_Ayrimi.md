# 107 — Application Service, Repository ve Mapper Ayrımı

Bu bölüm referans Logo entegrasyon uygulamasında iş akışı, veri erişimi ve Logo field mapping sorumluluklarının birbirinden nasıl ayrılması gerektiğini açıklar.

## Amaç

Entegrasyon kodunda en sık görülen problemlerden biri tek bir sınıfın şu görevlerin tamamını üstlenmesidir:

- DTO validation
- SQL lookup
- Logo login
- IData oluşturma
- field mapping
- Post
- hata parse
- retry
- log

Bu yapı kısa sürede bakım maliyetini artırır.

## Önerilen Katmanlar

```text
Controller / Worker / Job
        ↓
Application Service
        ↓
Validation
        ↓
Repository / Lookup
        ↓
Mapper
        ↓
Logo Adapter
        ↓
IData / IQuery
```

## Application Service

Application Service orkestrasyonu yönetir.

```csharp
public sealed class MaterialApplicationService
{
    private readonly IMaterialRepository _repository;
    private readonly IMaterialMapper _mapper;
    private readonly ILogoDataAdapter _logoDataAdapter;
    private readonly ILogoSessionFactory _sessionFactory;

    public MaterialApplicationService(
        IMaterialRepository repository,
        IMaterialMapper mapper,
        ILogoDataAdapter logoDataAdapter,
        ILogoSessionFactory sessionFactory)
    {
        _repository = repository;
        _mapper = mapper;
        _logoDataAdapter = logoDataAdapter;
        _sessionFactory = sessionFactory;
    }

    public LogoOperationResult Create(
        LogoCompanyContext context,
        MaterialDto dto)
    {
        // validate
        // duplicate kontrolü
        // session aç
        // mapper ile IData doldur
        // Post
        // result döndür
        throw new NotImplementedException();
    }
}
```

## Repository

Repository burada Logo ERP nesnesi yazmak için değil; lookup ve entegrasyon destek verisini okumak için kullanılabilir.

```csharp
public interface IMaterialRepository
{
    int? FindLogicalRefByCode(
        LogoCompanyContext context,
        string code);

    bool BarcodeExists(
        LogoCompanyContext context,
        string barcode);
}
```

Repository SQL Server veya IQuery kullanabilir.

## Mapper

Mapper yalnızca dış model ile Logo data object arasındaki alan eşlemesini bilmelidir.

```csharp
public interface IMaterialMapper
{
    void MapForCreate(dynamic data, MaterialDto dto);
    void MapForUpdate(dynamic data, MaterialDto dto);
}
```

Örnek:

```csharp
public sealed class MaterialMapper : IMaterialMapper
{
    public void MapForCreate(dynamic data, MaterialDto dto)
    {
        LogoFieldHelper.SetString(data, "CODE", dto.Code);
        LogoFieldHelper.SetString(data, "NAME", dto.Name);
    }

    public void MapForUpdate(dynamic data, MaterialDto dto)
    {
        LogoFieldHelper.SetString(data, "NAME", dto.Name);
    }
}
```

Gerçek field adları ilgili Logo Objects sürümünde doğrulanmalıdır.

## DTO

DTO uygulama sınırındaki veri taşıma nesnesidir.

```csharp
public sealed class MaterialDto
{
    public string Code { get; set; }
    public string Name { get; set; }
    public string UnitCode { get; set; }
    public string Barcode { get; set; }
}
```

DTO içinde Logo COM nesnesi tutulmamalıdır.

## Validation Katmanı

```csharp
public interface IValidator<T>
{
    ValidationResult Validate(T model);
}
```

Örnek kontroller:

- malzeme kodu boş olamaz
- ana birim bulunmalı
- barkod duplicate olmamalı
- firma aktif olmalı
- zorunlu mapping alanları mevcut olmalı

## Repository'nin Yapmaması Gerekenler

Repository:

- Logo session açmamalı
- retry yapmamalı
- kullanıcıya mesaj üretmemeli
- mapper görevi üstlenmemeli

## Mapper'ın Yapmaması Gerekenler

Mapper:

- SQL sorgusu çalıştırmamalı
- session yönetmemeli
- iş akışına karar vermemeli
- retry yapmamalı

## Application Service'in Yapmaması Gerekenler

Application Service mümkün olduğunca:

- SQL text üretmemeli
- doğrudan `FieldByName` çağrılarıyla dolmamalı
- COM release detaylarını bilmemeli

## Test Edilebilirlik

Bu ayrım sayesinde Application Service testinde gerçek Logo Objects gerekmeyebilir.

```text
FakeRepository
FakeSessionFactory
FakeLogoDataAdapter
FakeMapper
```

ile orchestration davranışı test edilebilir.

## Temel Kural

> Application Service iş akışını, Repository okumayı, Mapper alan eşlemeyi, Logo Adapter ise Logo Objects erişimini yönetmelidir.
