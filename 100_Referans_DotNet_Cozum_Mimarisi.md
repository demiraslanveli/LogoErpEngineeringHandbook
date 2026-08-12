# 100 — Referans .NET Çözüm Mimarisi

Bu bölüm Logo ERP entegrasyonları için kullanılabilecek sürdürülebilir bir .NET Framework 4.8 çözüm mimarisini tanımlar.

Amaç, Logo Objects kodlarını form event'leri veya tek dosyalık yardımcı sınıflar içine dağınık şekilde yazmak yerine; test edilebilir, loglanabilir ve tekrar kullanılabilir katmanlara ayırmaktır.

## 1. Önerilen Solution Yapısı

```text
LogoIntegration.sln
│
├── LogoIntegration.Core
│   ├── Models
│   ├── Contracts
│   ├── Results
│   ├── Validation
│   └── Exceptions
│
├── LogoIntegration.Application
│   ├── Services
│   ├── Commands
│   ├── Queries
│   ├── Mapping
│   └── Workflows
│
├── LogoIntegration.LogoAdapter
│   ├── Session
│   ├── DataObjects
│   ├── Query
│   ├── Production
│   ├── ErrorHandling
│   └── Mapping
│
├── LogoIntegration.Infrastructure
│   ├── Sql
│   ├── Logging
│   ├── Configuration
│   ├── Retry
│   ├── Idempotency
│   └── Queue
│
├── LogoIntegration.Worker
│   ├── Jobs
│   ├── Scheduling
│   └── HealthChecks
│
└── LogoIntegration.Tests
    ├── Unit
    └── Integration
```

## 2. Katmanların Sorumlulukları

### Core

Logo'dan bağımsız temel modelleri ve sözleşmeleri içerir.

Core katmanı mümkün olduğunca `UnityObjects`, COM veya SQL Server bağımlılığı taşımamalıdır.

Örnek:

```csharp
public sealed class OperationResult<T>
{
    public bool Success { get; set; }
    public T Data { get; set; }
    public string ErrorCode { get; set; }
    public string ErrorMessage { get; set; }
    public string CorrelationId { get; set; }
}
```

### Application

İş akışını yönetir.

Örnek:

```text
Sipariş oluştur
    ↓
input validation
    ↓
cari kontrol
    ↓
malzeme kontrol
    ↓
Logo adapter çağrısı
    ↓
sonuç doğrulama
    ↓
audit/log
```

Bu katman Logo Objects'in düşük seviyeli COM detaylarını bilmemelidir.

### LogoAdapter

Logo'ya özgü teknik katmandır.

Burada aşağıdaki bileşenler bulunur:

- IApplication oluşturma
- login / company login
- IData oluşturma
- IQuery oluşturma
- ProductionApplication erişimi
- DataFields / Lines mapping
- Logo hata parser
- COM cleanup

Bu katman sistemin Logo SDK sınırıdır.

### Infrastructure

SQL, loglama, configuration, retry, queue ve idempotency gibi teknik altyapıları içerir.

### Worker

Periyodik veya kuyruk tabanlı işlemleri çalıştırır.

Örnek:

```text
Queue Poller
    ↓
Job Dispatcher
    ↓
Application Service
    ↓
Logo Adapter
```

## 3. Logo Session Factory

Logo session oluşturma kodu uygulamanın farklı noktalarına dağılmamalıdır.

Önerilen sözleşme:

```csharp
public interface ILogoSessionFactory
{
    ILogoSession Create(int firmNo, int periodNo);
}
```

Session nesnesi aşağıdaki bilgileri taşıyabilir:

```text
FirmNo
PeriodNo
UserName
CorrelationId
IApplication
CreatedAt
```

Session'ın yaşam döngüsü açıkça yönetilmelidir.

## 4. Application Service Örneği

```csharp
public class MaterialService
{
    private readonly IMaterialRepository _materialRepository;

    public MaterialService(IMaterialRepository materialRepository)
    {
        _materialRepository = materialRepository;
    }

    public OperationResult<int> Create(MaterialCreateModel model)
    {
        // validation
        // duplicate control
        // Logo adapter call
        // audit
        // result
        return null;
    }
}
```

Burada `MaterialService`, `IData` detaylarını doğrudan bilmez.

## 5. Adapter / Repository Sınırı

Repository terimi Logo ERP entegrasyonunda klasik ORM repository'si gibi düşünülmemelidir.

Örnek:

```csharp
public interface IMaterialRepository
{
    OperationResult<int> Create(MaterialCreateModel model);
    OperationResult<bool> Update(int logicalRef, MaterialUpdateModel model);
    OperationResult<MaterialModel> Get(int logicalRef);
}
```

Gerçek implementasyon Logo Objects kullanabilir:

```text
LogoMaterialRepository
    ↓
IData
    ↓
Logo ERP
```

## 6. Read ve Write Ayrımı

Önerilen yaklaşım:

```text
WRITE
Logo Objects / IData

READ
Logo Objects / IQuery
veya
kontrollü SQL read model
```

Raporlama amacıyla her veriyi IData üzerinden okumak zorunlu değildir.

Ancak resmi ERP kaydı oluşturma, değiştirme veya silme işlemlerinde Logo'nun business rule katmanı mümkün olduğunca kullanılmalıdır.

## 7. Configuration Modeli

Configuration aşağıdaki bilgileri uygulama kodundan ayırmalıdır:

```text
Logo kullanıcı bilgileri
firma
period
SQL connection
queue ayarları
retry politikası
log path / sink
job interval
feature flags
```

Parolalar source code içinde tutulmamalıdır.

## 8. Correlation ID

Her entegrasyon işlemi tekil bir correlation id taşımalıdır.

Örnek:

```text
MES request
CorrelationId = 5f82...

↓
Queue
↓
Worker
↓
Logo Objects
↓
LOGICALREF = 123456
↓
Audit
```

Bu sayede bir hareketin tüm teknik zinciri tek id üzerinden izlenebilir.

## 9. Idempotency

Aynı dış sistem kaydının Logo'da iki kez oluşmasını önlemek için dış sistem referansı saklanmalıdır.

Örnek idempotency anahtarı:

```text
SourceSystem = MES
EntityType   = ProductionReceipt
SourceId     = 781245
```

Bu üçlü daha önce başarıyla işlendiğinde tekrar `Post()` yapılmamalıdır.

## 10. Hata Yönetimi

Hata katmanları birbirinden ayrılmalıdır:

```text
ValidationError
MappingError
LogoBusinessError
LogoSessionError
SqlError
TransientError
UnexpectedError
```

Her hata retry edilmemelidir.

Örneğin:

```text
network timeout -> retry edilebilir
Logo validation error -> otomatik retry genelde anlamsız
```

## 11. Transaction Sınırı

Logo Objects ile yapılan işlemlerde SQL transaction ile COM transaction kavramları birbirine karıştırılmamalıdır.

Bir iş akışı birden fazla ERP belgesi üretiyorsa aşağıdaki yaklaşım tasarlanmalıdır:

```text
Operation State
    ↓
Step 1 success
    ↓
Step 2 success
    ↓
Step 3 failed
    ↓
Compensation / reconciliation
```

Her durumda doğrudan bağlı Logo tablolarını SQL ile geri silmek güvenli kabul edilmemelidir.

## 12. Logging Standardı

Minimum log alanları:

```text
Timestamp
CorrelationId
FirmNo
PeriodNo
Operation
EntityType
SourceSystem
SourceId
LogoLogicalRef
DurationMs
Success
ErrorType
ErrorMessage
```

Payload tamamen loglanacaksa kişisel/vergi/finansal veri açısından maskeleme politikası uygulanmalıdır.

## 13. Test Stratejisi

### Unit Test

Logo SDK çağrısı yapmadan application logic test edilir.

### Integration Test

Test Logo firmasında gerçek Objects bağlantısı ile doğrulanır.

Örnek kontrol:

```text
Create
↓
LOGICALREF döndü mü?
↓
SQL read-back
↓
ilişkili kayıtlar oluştu mu?
↓
delete/rollback test senaryosu
```

## 14. ProductionApplication Ayrımı

Üretim operasyonları için ayrı adapter önerilir:

```text
ILogoProductionAdapter
```

Bu sayede standart kart/fiş işlemleri ile üretim operasyon API'si birbirinden ayrılır.

## 15. Mimari Özet

```text
API / Worker / UI
        ↓
Application Layer
        ↓
Validation + Workflow
        ↓
Logo Adapter
        ↓
IData / IQuery / ProductionApplication
        ↓
Logo ERP

Cross-Cutting:
Logging
Retry
Idempotency
Audit
Configuration
Monitoring
```

## Sonuç

Logo Objects entegrasyonunun sürdürülebilir olması için asıl hedef yalnızca çalışan `Post()` kodu yazmak değildir.

Hedef:

> Logo SDK bağımlılığını kontrollü bir adapter katmanına kapatmak ve ERP entegrasyonunu test edilebilir, izlenebilir, idempotent ve operasyonel olarak yönetilebilir hale getirmektir.
