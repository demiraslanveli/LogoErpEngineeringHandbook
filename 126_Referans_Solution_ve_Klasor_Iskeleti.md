# 126 — Referans Solution ve Klasör İskeleti

Bu bölüm .NET Framework 4.8 tabanlı Logo ERP entegrasyon uygulaması için gerçek proje iskeletini tanımlar.

## Amaç

Kodun Logo Objects çağrıları, domain kuralları, SQL erişimi, background worker ve deployment detayları arasında dağılmasını önlemek.

## Önerilen Solution Yapısı

```text
LogoErpIntegration.sln
│
├─ src
│  ├─ LogoErpIntegration.Core
│  ├─ LogoErpIntegration.Application
│  ├─ LogoErpIntegration.LogoAdapter
│  ├─ LogoErpIntegration.Infrastructure
│  ├─ LogoErpIntegration.Worker
│  └─ LogoErpIntegration.Host
│
├─ tests
│  ├─ LogoErpIntegration.UnitTests
│  └─ LogoErpIntegration.IntegrationTests
│
├─ database
│  ├─ bootstrap
│  ├─ migrations
│  └─ rollback
│
├─ deploy
│  ├─ config
│  ├─ scripts
│  └─ service
│
└─ docs
```

## Core

Logo Objects bağımlılığı bulunmamalıdır.

Örnek içerik:

```text
Domain/
Contracts/
Results/
Validation/
ValueObjects/
```

Burada DTO değil, uygulamanın ortak iş modelleri ve kuralları bulunur.

## Application

Use-case katmanıdır.

```text
Services/
Commands/
Queries/
Validators/
Mappers/
Interfaces/
```

Application katmanı Logo Objects nesnelerini doğrudan bilmemelidir.

## LogoAdapter

Logo SDK bağımlılığı bu projede tutulur.

```text
Session/
Objects/
Queries/
Repositories/
ErrorHandling/
Production/
```

Temel sorumluluklar:

- `IApplication` yaşam döngüsü
- `IData` oluşturma ve Post işlemleri
- `IQuery` sorguları
- ProductionApplication erişimi
- Logo hata mesajlarının ortak modele dönüştürülmesi

## Infrastructure

Logo dışındaki teknik servisler:

```text
Sql/
Logging/
Idempotency/
Reconciliation/
Configuration/
Security/
Mail/
```

## Worker

Queue, batch ve scheduled işlerin çalıştığı katmandır.

Worker doğrudan Logo Objects çağırmak yerine Application service çağırmalıdır.

## Host

Windows Service veya console debug host başlangıç noktasıdır.

Burada yalnızca:

- configuration yükleme
- dependency wiring
- logging bootstrap
- worker başlatma
- graceful shutdown

bulunmalıdır.

## Tests

### UnitTests

Logo bağlantısı olmadan çalışmalıdır.

### IntegrationTests

Kontrollü test firma/döneminde gerçek Logo Objects ve SQL erişimi kullanabilir.

## Database

Uygulamanın kendi tabloları burada versiyonlanır:

- idempotency
- queue
- audit
- reconciliation
- migration history

Logo'nun standart tablolarını oluşturma veya değiştirme scriptleri bu klasöre konulmamalıdır.

## Temel Bağımlılık Yönü

```text
Host
 ↓
Worker
 ↓
Application
 ↓
Core

Infrastructure ──► Application interfaces
LogoAdapter    ──► Application interfaces
```

Application katmanı `Infrastructure` veya `LogoAdapter` implementasyonlarına compile-time olarak bağımlı olmamalıdır.

## Kural

> Logo SDK bağımlılığı solution genelinde yayılmamalı; mümkün olduğunca `LogoAdapter` sınırında tutulmalıdır.
