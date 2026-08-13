# Logo ERP Reference Application

Bu bölüm, `Logo ERP Engineering Handbook` içindeki 100+ uygulamalı bölümlerin çalışan proje iskeletine dönüştürülmüş halidir.

## Solution

Visual Studio 2022 ile açılacak dosya:

```text
LogoErp.Reference.sln
```

## Proje Yapısı

```text
src/
├── LogoErp.Reference.Core/
│   ├── Abstractions/
│   ├── Configuration/
│   ├── Context/
│   └── Results/
│
├── LogoErp.Reference.Application/
│   ├── Abstractions/
│   └── Services/
│
├── LogoErp.Reference.Infrastructure/
│   ├── Configuration/
│   └── Sql/
│
├── LogoErp.Reference.LogoAdapter/
│   ├── Customers/
│   ├── Data/
│   ├── Documents/
│   ├── Materials/
│   ├── Orders/
│   ├── Production/
│   └── Session/
│
└── LogoErp.Reference.Worker/
    ├── Composition/
    ├── Runtime/
    └── Program.cs

tests/
└── LogoErp.Reference.IntegrationTests/
    └── Fakes/

database/
└── migrations/

deploy/
└── Deploy-ReferenceService.ps1
```

## Katman Sorumlulukları

### Core

Logo SDK'sından bağımsız temel sözleşmeler ve ortak modeller:

- `OperationResult`
- `CompanyPeriodContext`
- `LogoErpOptions`
- `ILogoSession`
- `IIdempotencyStore`

### Application

ERP iş akışını yöneten application-service katmanıdır. Logo COM tipi bilmez.

Mevcut gateway sınırları:

```text
ILogoMaterialGateway
ICustomerGateway
IOrderGateway
IDispatchInvoiceGateway
IProductionGateway
```

Mevcut servis örnekleri:

```text
MaterialService
CustomerService
OrderService
DispatchInvoiceService
ProductionService
```

Validation ve orchestration burada yapılır.

### LogoAdapter

Logo Objects / UnityApplication / ProductionApplication bağımlılığı yalnızca bu projede bulunmalıdır.

Session katmanı:

```text
LogoSessionAdapter
        ↓
ILogoSdkBridge
        ↓
Verified Logo SDK Binding
```

`LogoSessionAdapter` yalnızca bridge gerçekten login olduğunda açık kabul edilir. Doğrulanmış SDK binding yoksa `UnconfiguredLogoSdkBridge` kullanılır ve sistem fail-fast davranır.

IData katmanı:

```text
Gateway
   ↓
Mapping Profile
   ↓
ILogoDataObjectFactory
   ↓
ILogoDataObject
   ↓
Verified IData COM Wrapper
```

Mevcut Data ve mapping parçaları:

```text
ILogoDataObject
ILogoDataObjectLine
ILogoDataObjectFactory
MaterialDataMappingProfile
CustomerDataMappingProfile
OrderDataMappingProfile
DispatchInvoiceDataMappingProfile
UnconfiguredLogoDataObjectFactory
```

Malzeme, cari, sipariş, irsaliye ve fatura gateway'leri generic IData bridge üzerinden çalışacak şekilde düzenlenmiştir. Header ve line field adları ile `DataObjectType` anahtarları hedef Logo Objects sürümünde doğrulanmadan production profile'a verilmez.

Belge akışı:

```text
Create IData
    ↓
Set Header Fields
    ↓
Append Lines
    ↓
Set Line Fields
    ↓
Post
```

### ProductionApplication Boundary

Üretim entegrasyonu ayrı bridge ile izole edilmiştir:

```text
ProductionService
      ↓
IProductionGateway
      ↓
LogoProductionGateway
      ↓
IProductionApplicationBridge
      ↓
ProductionApplication
```

Mevcut parçalar:

```text
IProductionApplicationBridge
ProductionApplicationCommand
UnconfiguredProductionApplicationBridge
LogoProductionGateway
```

Doğrulanmış ProductionApplication binding yoksa sistem `PRODUCTION_SDK_NOT_CONFIGURED` sonucu ile fail-fast davranır. `LogoProductionGateway`, bridge yaşam döngüsünü `Open → operation → Close` biçiminde yönetir ve `Close()` çağrısını `finally` içinde yapar.

### Infrastructure

Logo ERP dışındaki teknik altyapılar:

- SQL persistence
- idempotency store
- migration
- logging
- configuration
- queue/reconciliation altyapısı

Configuration örneği:

```text
EnvironmentConfigurationLoader
```

Secret değerler repository/config dosyasında tutulmaz. Referans environment variable seti:

```text
LOGOERP_FIRM_NUMBER
LOGOERP_PERIOD_NUMBER
LOGOERP_USER
LOGOERP_PASSWORD
LOGOERP_SQL
LOGOERP_WORKER_INTERVAL_SECONDS
```

### Worker

Composition root ve runtime host katmanıdır.

Mevcut runtime parçaları:

```text
CompositionRoot
HealthCheckRunner
WorkerLoop
Program.cs
```

Çalışma sırası:

```text
EnvironmentConfigurationLoader
        ↓
CompositionRoot
        ↓
SQL Health Check
        ↓
WorkerLoop
        ↓
Application Services
        ↓
Gateway / LogoAdapter
```

`CompositionRoot.CreateLogoSession()` firma/dönem context'i ile session üretir. Doğrulanmış SDK bridge bağlanana kadar güvenli default olarak `UnconfiguredLogoSdkBridge` kullanılır.

`Program.cs` SQL health check başarısız olduğunda ayrı exit code döndürür. Worker loop `CancellationToken` ile kontrollü kapanır ve iteration seviyesindeki hatayı host sürecini zorunlu olarak düşürmeden loglar.

## Güncel Solution Projeleri

```text
LogoErp.Reference.Core
LogoErp.Reference.Application
LogoErp.Reference.Infrastructure
LogoErp.Reference.LogoAdapter
LogoErp.Reference.Worker
LogoErp.Reference.IntegrationTests
```

## Temel Bağımlılık Yönü

```text
Worker
  ↓
Application
  ↓
Core

Worker
  ↓
LogoAdapter
  ↓
Application + Core

Worker
  ↓
Infrastructure
  ↓
Core
```

Application katmanının `LogoAdapter` projesine doğrudan referansı yoktur. Böylece Logo SDK'sı olmadan application logic fake gateway'lerle test edilebilir.

## Fake Adapter Stratejisi

Test projesinde mevcut fake adapter'lar:

```text
FakeLogoMaterialGateway
FakeCustomerGateway
FakeOrderGateway
FakeDispatchInvoiceGateway
FakeProductionGateway
```

Fake adapter'lar application-service validation/orchestration testlerini gerçek Logo kurulumu veya COM registration gerektirmeden çalıştırmak içindir.

## Logo SDK Entegrasyon Kuralı

Resmi Logo kart/fiş oluşturma, değiştirme ve silme işlemlerinde hedef yaklaşım:

```text
Application Service
      ↓
Gateway Interface
      ↓
LogoAdapter
      ↓
IApplication / IData / ProductionApplication
      ↓
Logo ERP
```

Doğrudan SQL `INSERT / UPDATE / DELETE`, resmi ERP nesne işleminin yerine kullanılmamalıdır. SQL ağırlıklı olarak read/query, integration metadata, idempotency, reconciliation ve operasyonel altyapı için kullanılmalıdır.

## Test Veritabanı

Integration test çalıştırmadan önce yalnızca test ortamına ait connection string environment variable olarak verilmelidir:

```text
LOGOERP_TEST_SQL
```

Production veritabanı integration test hedefi olarak kullanılmamalıdır.

## Repository Adı

Repository artık:

```text
LogoErpEngineeringHandbook
```

adıyla devam etmektedir.

## Sıradaki Kod Adımları

- verified `ILogoSdkBridge` implementasyonu
- verified `ILogoDataObjectFactory` COM wrapper
- malzeme zorunlu birim seti mapping'i
- cari kart ek zorunlu alan mapping'i
- verified order/dispatch/invoice Logo Objects field mapping
- structured logging sink
- Logo session health persistence
- verified ProductionApplication implementation
- deployment rollback scripti
- worker için service-host dönüşümü

İlgili handbook bölümleri:

- [139 — Logo SDK Binding ve Session Doğrulama Standardı](139_Logo_SDK_Binding_ve_Session_Dogrulama_Standarti.md)
- [140 — IData Bridge ve Master Data Mapping Standardı](140_IData_Bridge_ve_Master_Data_Mapping_Standarti.md)
- [141 — Belge IData Header / Line Mapping Standardı](141_Belge_IData_Header_Line_Mapping_Standardi.md)
- [142 — ProductionApplication Bridge ve SDK İzolasyonu](142_ProductionApplication_Bridge_ve_SDK_Izolasyonu.md)

> Referans uygulama, handbook içindeki mimari prensiplerin gerçek kod karşılığı olarak geliştirilmektedir.
