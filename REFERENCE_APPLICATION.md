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
│   ├── Context/
│   └── Results/
│
└── LogoErp.Reference.Infrastructure/
    └── Sql/

tests/
└── LogoErp.Reference.IntegrationTests/

database/
└── migrations/

deploy/
└── Deploy-ReferenceService.ps1
```

## Şu An Çalışan Temel Parçalar

- `OperationResult`
- `CompanyPeriodContext`
- `ILogoSession`
- `IIdempotencyStore`
- SQL tabanlı `SqlIdempotencyStore`
- `INTEGRATION_IDEMPOTENCY` migration scripti
- MSTest integration-test fixture
- temel PowerShell deployment scripti

## Logo SDK Bağımlılığı

Core ve Infrastructure katmanları mümkün olduğunca Logo SDK tiplerinden bağımsız tutulmaktadır.

Logo Objects / UnityApplication entegrasyonu ayrı adapter projesinde ele alınacaktır:

```text
LogoErp.Reference.LogoAdapter
```

Bu sayede:

- core katmanı unit test edilebilir,
- Logo SDK sürüm bağımlılığı izole edilir,
- gerçek ve fake adapter değiştirilebilir,
- test ortamında Logo kurulumu olmadan application logic test edilebilir.

## Test Veritabanı

Integration test çalıştırmadan önce yalnızca test ortamına ait connection string environment variable olarak verilmelidir:

```text
LOGOERP_TEST_SQL
```

Production veritabanı integration test hedefi olarak kullanılmamalıdır.

## Sıradaki Kod Adımları

- `LogoErp.Reference.Application` projesi
- `LogoErp.Reference.LogoAdapter` projesi
- `LogoErp.Reference.Worker` Windows Service projesi
- configuration loader
- migration runner executable
- health-check runner
- material/customer/order application services
- Logo Objects adapter implementation
- fake Logo adapter
- deployment rollback scripti

> Referans uygulama, handbook içindeki mimari prensiplerin gerçek kod karşılığı olarak geliştirilmektedir.
