# Logo ERP Reference Application

Bu bölüm, `Logo ERP Engineering Handbook` içindeki 100–160 uygulamalı serinin çalışan proje iskeletine dönüştürülmüş halidir.

## Solution

Visual Studio 2022 ile açılacak dosya:

```text
LogoErp.Reference.sln
```

## Proje Yapısı

```text
src/
├── LogoErp.Reference.Core/
├── LogoErp.Reference.Application/
├── LogoErp.Reference.Infrastructure/
├── LogoErp.Reference.LogoAdapter/
└── LogoErp.Reference.Worker/

tests/
└── LogoErp.Reference.IntegrationTests/

database/
└── migrations/

deploy/
└── Deploy-ReferenceService.ps1
```

## Nihai Mimari

```text
External System / MES / WMS / LIMS / Job / API
                  ↓
             Worker / Host
                  ↓
            Application Layer
                  ↓
          Gateway Interfaces
                  ↓
             LogoAdapter
        ┌─────────┴─────────┐
        ↓                   ↓
   Logo Objects       ProductionApplication
        ↓                   ↓
       IData            Production API
        └─────────┬─────────┘
                  ↓
               Logo ERP
```

Yan katmanlar:

```text
Configuration
Secret Management
Structured Logging
Health Persistence
Idempotency
Retry
Queue
Reconciliation
SQL Persistence
Deployment
Rollback
Release Versioning
Testing
```

## LogoAdapter Standardı

Session:

```text
LogoSessionAdapter
        ↓
ILogoSdkBridge
        ↓
VerifiedLogoSdkBridge
        ↓
UnityApplication / Logo Objects
```

IData:

```text
Gateway
   ↓
Mapping Profile
   ↓
LogoDataObjectWriter / LogoLineWriter
   ↓
ILogoDataObjectFactory
   ↓
VerifiedLogoDataObjectFactory
   ↓
IData COM Wrapper
```

Production:

```text
ProductionService
      ↓
IProductionGateway
      ↓
LogoProductionGateway
      ↓
IProductionApplicationBridge
      ↓
VerifiedProductionApplicationBridge
```

## SDK Güvenlik Sınırı

Doğrulanmamış Logo enum, field veya metot isimleri production adapter'a yazılmaz.

```text
LogoSdkBindingManifest
LogoSdkBindingKeys
LogoSdkCompatibilityChecker
```

ile hedef Logo/Objects sürümüne ait binding bilgileri doğrulanır. Eksik veya doğrulanmamış binding durumunda sistem fail-fast davranır.

## COM Yaşam Döngüsü

Uzun yaşayan worker/service süreçlerinde COM nesneleri deterministic olarak bırakılmalıdır.

```text
ComReleaseHelper
```

bu amaçla adapter katmanında tutulur. `GC.Collect()` temel kaynak yönetimi yöntemi değildir.

## Hata Normalizasyonu

```text
LogoAdapterErrorNormalizer
OperationResult
```

ile SDK hata kodu/açıklaması ortak formata çevrilir. Exception durumları da adapter sınırında normalize edilir.

## Verified Binding Alanları

100–160 serisi aşağıdaki gerçek binding alanlarının production standardını tanımlar:

- Logo session / UnityApplication
- IData object factory
- malzeme kartı
- cari kart
- satış siparişi
- irsaliye
- fatura
- ProductionApplication

Gerçek SDK değerleri hedef Logo kurulumu üzerinde doğrulandıktan sonra ilgili profile/bridge'e işlenmelidir.

## Runtime Standardı

```text
Load Configuration
   ↓
Validate Secrets
   ↓
Startup Health Gate
   ↓
Validate SDK Binding
   ↓
Open Logo Session
   ↓
Read Work Item
   ↓
Idempotency Check
   ↓
Application Service
   ↓
Logo Adapter
   ↓
Reconciliation
   ↓
Structured Log + Health + Metrics
   ↓
Ack / Retry / Dead Letter
```

## Production Operasyon Standardı

Uygulama için ayrıca aşağıdaki konular tanımlanmıştır:

- structured logging ve correlation id,
- operational health persistence,
- Windows Service host modeli,
- graceful shutdown,
- service recovery,
- deployment / upgrade / rollback runbook,
- release artifact standardı,
- end-to-end integration testleri,
- final production acceptance checklist.

## Uygulamalı Serinin Durumu

**100–160 arasındaki temel uygulamalı mimari seri tamamlanmıştır.**

İlgili kapanış bölümleri:

- [146 — Verified ILogoSdkBridge Implementasyon Planı](146_Verified_ILogoSdkBridge_Implementasyon_Plani.md)
- [147 — Verified ILogoDataObjectFactory COM Wrapper](147_Verified_ILogoDataObjectFactory_COM_Wrapper.md)
- [148 — Malzeme Kartı Verified IData Binding](148_Malzeme_Karti_Verified_IData_Binding.md)
- [149 — Cari Kart Verified IData Binding](149_Cari_Kart_Verified_IData_Binding.md)
- [150 — Sipariş Verified IData Binding](150_Siparis_Verified_IData_Binding.md)
- [151 — İrsaliye / Fatura Verified IData Binding](151_Irsaliye_Fatura_Verified_IData_Binding.md)
- [152 — ProductionApplication Verified Binding](152_ProductionApplication_Verified_Binding.md)
- [153 — Structured Logging ve Operasyonel Telemetri](153_Structured_Logging_ve_Operasyonel_Telemetri.md)
- [154 — Health Persistence ve Operational Status Modeli](154_Health_Persistence_ve_Operational_Status_Modeli.md)
- [155 — Windows Service Host Production Modeli](155_Windows_Service_Host_Production_Modeli.md)
- [156 — Deployment, Upgrade ve Rollback Runbook](156_Deployment_Upgrade_ve_Rollback_Runbook.md)
- [157 — Release Artifact ve Paketleme Standardı](157_Release_Artifact_ve_Paketleme_Standardi.md)
- [158 — End-to-End Integration Test Senaryoları](158_EndToEnd_Integration_Test_Senaryolari.md)
- [159 — Final Production Acceptance Checklist](159_Final_Production_Acceptance_Checklist.md)
- [160 — Referans Uygulama Final Mimari Özeti](160_Referans_Uygulama_Final_Mimari_Ozeti.md)

## Bundan Sonraki Gelişim Modeli

Yeni temel mimari bölüm açmak yerine:

- doğrulanmış SDK kodu mevcut bölümlere eklenmeli,
- gerçek saha vakaları ilgili konu başlıklarına işlenmeli,
- çalışan SQL/C# örnekleri genişletilmeli,
- binding manifest Logo sürümleriyle güncellenmeli,
- performans ve production test sonuçları dokümante edilmelidir.

> Referans uygulamanın hedefi yalnızca Logo Objects kullanımını göstermek değil; Logo ERP entegrasyonunun tasarım, geliştirme, test, deployment ve operasyon yaşam döngüsünü tek mühendislik standardında toplamaktır.
