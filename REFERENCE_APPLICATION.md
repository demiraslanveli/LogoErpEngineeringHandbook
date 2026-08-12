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
├── LogoErp.Reference.Application/
│   ├── Abstractions/
│   └── Services/
│
├── LogoErp.Reference.Infrastructure/
│   └── Sql/
│
├── LogoErp.Reference.LogoAdapter/
│   ├── Session/
│   └── Materials/
│
└── LogoErp.Reference.Worker/
    └── Program.cs

tests/
└── LogoErp.Reference.IntegrationTests/

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
- `ILogoSession`
- `IIdempotencyStore`

### Application

ERP iş akışını yöneten application-service katmanıdır. Logo COM tipi bilmez.

İlk örnek:

```text
IMaterialService
        ↓
MaterialService
        ↓
ILogoMaterialGateway
```

Validation ve orchestration burada yapılır.

### LogoAdapter

Logo Objects / UnityApplication bağımlılığı yalnızca bu projede bulunmalıdır.

```text
LogoSessionAdapter
LogoMaterialGateway
```

Gerçek Logo SDK referansı hedef Logo kurulumundan doğrulanarak bu projeye eklenmelidir. COM type, DataObjectType enum, field adı veya login davranışı doğrulanmadan framework içinde sabitlenmemelidir.

### Infrastructure

Logo ERP dışındaki teknik altyapılar:

- SQL persistence
- idempotency store
- migration
- logging
- configuration
- queue/reconciliation altyapısı

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

Application katmanının `LogoAdapter` projesine doğrudan referansı yoktur. Böylece Logo SDK'sı olmadan application logic test edilebilir.

## Logo SDK Entegrasyon Kuralı

Resmi Logo kart/fiş oluşturma, değiştirme ve silme işlemlerinde hedef yaklaşım:

```text
Application Service
      ↓
Gateway Interface
      ↓
LogoAdapter
      ↓
IApplication / IData
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

- configuration loader ve protected secret modeli
- gerçek composition root
- fake Logo adapter
- cari kart gateway/service
- sipariş gateway/service
- irsaliye/fatura gateway/service
- ProductionApplication adapter sınırı
- background worker loop
- health-check runner entegrasyonu
- deployment rollback scripti

> Referans uygulama, handbook içindeki mimari prensiplerin gerçek kod karşılığı olarak geliştirilmektedir.
