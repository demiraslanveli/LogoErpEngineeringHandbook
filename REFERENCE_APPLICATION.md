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
│   ├── Gateways/
│   └── Services/
│
├── LogoErp.Reference.Infrastructure/
│   ├── Configuration/
│   └── Sql/
│
├── LogoErp.Reference.LogoAdapter/
│   ├── Customers/
│   ├── Documents/
│   ├── Materials/
│   ├── Orders/
│   ├── Production/
│   └── Session/
│
└── LogoErp.Reference.Worker/
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
```

Validation ve orchestration burada yapılır.

### LogoAdapter

Logo Objects / UnityApplication / ProductionApplication bağımlılığı yalnızca bu projede bulunmalıdır.

Mevcut adapter sınırları:

```text
LogoSessionAdapter
LogoMaterialGateway
LogoCustomerGateway
LogoOrderGateway
LogoDispatchInvoiceGateway
LogoProductionGateway
```

Gerçek Logo SDK referansı hedef Logo kurulumundan doğrulanarak bu projeye eklenmelidir. COM type, `DataObjectType`, field adı, login davranışı veya `ProductionApplication` metodu doğrulanmadan framework içinde sabitlenmez.

Bu nedenle henüz doğrulanmamış adapter metotları bilinçli olarak `LOGO_ADAPTER_NOT_CONFIGURED` / `PRODUCTION_ADAPTER_NOT_CONFIGURED` sonucu döndürmektedir. Bu yaklaşım, tahmine dayalı Logo SDK kodunun production entegrasyonuna sızmasını engeller.

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

Sorumlulukları:

- configuration yüklemek,
- dependency graph oluşturmak,
- Logo session açmak/kapatmak,
- application service çalıştırmak,
- structured log üretmek,
- exit code yönetmek.

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

Test projesinde ilk fake adapter'lar oluşturuldu:

```text
FakeLogoMaterialGateway
FakeCustomerGateway
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

## Sıradaki Kod Adımları

- composition root'u configuration loader ile bağlamak
- background worker loop
- health-check runner entegrasyonu
- fake order/document/production gateway'leri
- dispatch/invoice application service
- production application service
- gerçek Logo SDK reference wiring standardı
- verified material/customer IData implementation
- verified order/dispatch/invoice IData implementation
- verified ProductionApplication implementation
- deployment rollback scripti

> Referans uygulama, handbook içindeki mimari prensiplerin gerçek kod karşılığı olarak geliştirilmektedir.
