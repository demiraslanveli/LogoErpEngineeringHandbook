# Part 08 — Geliştirici Kılavuzu ve Referans Uygulama

Bu bölüm 100. bölümden itibaren başlayan uygulamalı geliştirici kılavuzunu kapsar.

Amaç yalnızca Logo Objects metotlarını listelemek değil; gerçek projede kullanılabilecek sürdürülebilir bir .NET Framework 4.8 entegrasyon uygulamasının nasıl tasarlanacağını adım adım göstermektir.

## Mevcut Bölümler

- [100 — Referans .NET Çözüm Mimarisi](100_Referans_DotNet_Cozum_Mimarisi.md)
- [101 — Configuration ve Company / Period Context](101_Configuration_ve_CompanyPeriod_Context.md)
- [102 — Logo Session Factory ve IApplication Wrapper](102_Logo_Session_Factory_ve_IApplication_Wrapper.md)
- [103 — IData / IQuery Helper ve Adapter Katmanı](103_IData_IQuery_Helper_ve_Adapter_Katmani.md)
- [104 — LogoOperationResult ve Generic Hata Parser](104_LogoOperationResult_ve_Generic_Hata_Parser.md)
- [105 — Structured Logging, CorrelationId ve Audit Context](105_Structured_Logging_CorrelationId_ve_Audit_Context.md)
- [106 — Transaction Sınırı, Idempotency ve Retry Politikası](106_Transaction_Siniri_Idempotency_ve_Retry_Politikasi.md)
- [107 — Application Service, Repository ve Mapper Ayrımı](107_Application_Service_Repository_ve_Mapper_Ayrimi.md)
- [108 — Validation Pipeline ve Domain Kuralları](108_Validation_Pipeline_ve_Domain_Kurallari.md)
- [109 — Configuration Encryption ve Secret Yönetimi](109_Configuration_Encryption_ve_Secret_Yonetimi.md)
- [110 — Batch Processor ve Toplu İşlem Mimarisi](110_Batch_Processor_ve_Toplu_Islem_Mimarisi.md)
- [111 — Background Worker ve Windows Service Çalışma Modeli](111_Background_Worker_ve_Windows_Service_Calisma_Modeli.md)
- [112 — Health Check ve Dependent Service Kontrolleri](112_Health_Check_ve_Dependent_Service_Kontrolleri.md)
- [113 — Fake Adapter, Test Doubles ve Mock Stratejisi](113_Fake_Adapter_Test_Doubles_ve_Mock_Stratejisi.md)
- [114 — Integration Test Standardı ve Test Ortam Stratejisi](114_Integration_Test_Standarti_ve_Test_Ortam_Stratejisi.md)
- [115 — Malzeme Kartı Servisi Referans Implementasyonu](115_Malzeme_Karti_Servisi_Referans_Implementasyonu.md)
- [116 — Cari Kart Servisi Referans Implementasyonu](116_Cari_Kart_Servisi_Referans_Implementasyonu.md)
- [117 — Sipariş Servisi Referans Implementasyonu](117_Siparis_Servisi_Referans_Implementasyonu.md)
- [118 — İrsaliye / Fatura Servisi Referans Implementasyonu](118_Irsaliye_Fatura_Servisi_Referans_Implementasyonu.md)
- [119 — Üretim Entegrasyon Servisi Referans Implementasyonu](119_Uretim_Entegrasyon_Servisi_Referans_Implementasyonu.md)
- [120 — Ortak Mapper ve Validator Kütüphanesi](120_Ortak_Mapper_ve_Validator_Kutuphanesi.md)
- [121 — Reconciliation Repository ve Karşılaştırma Modeli](121_Reconciliation_Repository_ve_Karsilastirma_Modeli.md)
- [122 — Idempotency Store SQL Şeması](122_Idempotency_Store_SQL_Semasi.md)
- [123 — Windows Service Host ve Runtime Yönetimi](123_Windows_Service_Host_ve_Runtime_Yonetimi.md)
- [124 — Composition Root ve Dependency Wiring](124_Composition_Root_ve_Dependency_Wiring.md)
- [125 — Uçtan Uca Örnek Entegrasyon Akışı](125_Uctan_Uca_Ornek_Entegrasyon_Akisi.md)
- [126 — Referans Solution ve Klasör İskeleti](126_Referans_Solution_ve_Klasor_Iskeleti.md)
- [127 — Örnek Interface ve Class Sözleşmeleri](127_Ornek_Interface_ve_Class_Sozlesmeleri.md)
- [128 — SQL Bootstrap, Migration ve Schema Versioning](128_SQL_Bootstrap_Migration_ve_Schema_Versioning.md)
- [129 — Deployment, Rollback ve Release Prosedürü](129_Deployment_Rollback_ve_Release_Proseduru.md)
- [130 — Production Readiness Checklist](130_Production_Readiness_Checklist.md)
- [131 — Release Versioning ve Uyumluluk Modeli](131_Release_Versioning_ve_Uyumluluk_Modeli.md)
- [132 — Program.cs ve Service Entry Point](132_ProgramCS_ve_Service_EntryPoint.md)
- [133 — Windows Service Class ve Lifecycle Kodu](133_Windows_Service_Class_ve_Lifecycle_Kodu.md)
- [134 — Composition Root Gerçek Kod İskeleti](134_CompositionRoot_Gercek_Kod_Iskeleti.md)
- [135 — Migration Runner Gerçek Kod İskeleti](135_MigrationRunner_Gercek_Kod_Iskeleti.md)
- [136 — SQL Idempotency Repository Gerçek Kod](136_SQL_IdempotencyRepository_Gercek_Kod.md)
- [137 — HealthCheckRunner Gerçek Kod](137_HealthCheckRunner_Gercek_Kod.md)
- [138 — IntegrationTestFixture Gerçek Kod](138_IntegrationTestFixture_Gercek_Kod.md)
- [139 — Logo SDK Binding ve Session Doğrulama Standardı](139_Logo_SDK_Binding_ve_Session_Dogrulama_Standarti.md)
- [140 — IData Bridge ve Master Data Mapping Standardı](140_IData_Bridge_ve_Master_Data_Mapping_Standarti.md)
- [141 — Belge IData Header / Line Mapping Standardı](141_Belge_IData_Header_Line_Mapping_Standardi.md)
- [142 — ProductionApplication Bridge ve SDK İzolasyonu](142_ProductionApplication_Bridge_ve_SDK_Izolasyonu.md)

## Sıradaki Konular

- verified `ILogoSdkBridge` implementasyonu
- verified `ILogoDataObjectFactory` COM wrapper
- verified order/dispatch/invoice Logo Objects field mapping
- verified ProductionApplication bridge implementasyonu
- structured logging sink
- Logo session health persistence
- deployment rollback scripti
- Windows Service host dönüşümü

## Referans Teknoloji Seti

```text
.NET Framework 4.8
Visual Studio 2022
Logo Objects / UnityApplication
ProductionApplication
SQL Server
Windows Service / Worker yaklaşımı
Structured logging
```

Logo Objects sürümüne göre API farklılıkları olduğunda kesin enum veya field isimleri yalnızca doğrulandığı ölçüde kullanılacaktır.

## Hedef Mimari

```text
Client / Job / API
      ↓
Application Service
      ↓
Validation
      ↓
Domain / Mapping
      ↓
Logo Adapter Layer
      ↓
IApplication / IData / IQuery
      ↓
Logo ERP

Yan katmanlar:
Logging
Retry
Idempotency
Audit
Configuration
Monitoring
Testing
Reconciliation
Deployment
Versioning
```

> Bu bölüm kitabın teorik bilgilerini çalışan bir uygulama mimarisine dönüştüren uygulamalı kısımdır.
