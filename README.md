# Logo ERP Engineering Handbook

**Logo ERP, Logo Objects, ProductionApplication, SQL Server ve Kurumsal Entegrasyonlar için Teknik Referans**

Bu repository yalnızca Logo Objects kullanımını anlatan bir kitap değildir. İçerik; Logo ERP veri modeli, SDK geliştirme, detaylı üretim, seri/lot, kalite, maliyetlendirme, SQL Server, finans, entegrasyon, operasyon ve güvenlik başlıklarını kapsayan yaşayan bir mühendislik el kitabıdır.

## Kitap Mimarisi

- [Kitap Mimarisi](00_Kitap_Mimarisi.md)

## Çalışan Referans Uygulama

Handbook içindeki 100+ uygulamalı bölümün gerçek `net48` proje iskeleti repository içinde oluşturulmaya başlanmıştır.

- [Reference Application](REFERENCE_APPLICATION.md)
- `LogoErp.Reference.sln`
- `src/`
- `tests/`
- `database/migrations/`
- `deploy/`

## Ana Bölümler

1. [Logo ERP Core ve Veri Modeli](PART_01_Logo_ERP_Core_ve_Veri_Modeli.md)
2. [Logo Objects SDK ve Uygulama Geliştirme](PART_02_Logo_Objects_SDK_ve_Uygulama_Gelistirme.md)
3. [Üretim, Seri/Lot, Kalite ve Maliyet](PART_03_Uretim_SeriLot_Kalite_ve_Maliyet.md)
4. [SQL Server, Veritabanı ve Performans](PART_04_SQL_Server_Veritabanı_ve_Performans.md)
5. [Entegrasyon Mimarileri ve Servisler](PART_05_Entegrasyon_Mimarileri_ve_Servisler.md)
6. [Finans, Muhasebe ve Elektronik Belgeler](PART_06_Finans_Muhasebe_ve_EBelge.md)
7. [Operasyon, Güvenlik, Backup ve DR](PART_07_Operasyon_Guvenlik_Backup_ve_DR.md)
8. [Geliştirici Kılavuzu ve Referans Uygulama](PART_08_Gelistirici_Kilavuzu_ve_Referans_Uygulama.md)

## Güncel Uygulamalı Seri

100. bölümden itibaren kitap, teorik ve saha bilgisini çalışan referans uygulama mimarisine dönüştürmektedir.

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
- [112 — Health Check ve Dependent Service Kontrolleri](112_HealthCheck_ve_Dependent_Service_Kontrolleri.md)
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

## Temel Prensipler

- Resmi kart ve fiş işlemlerinde mümkün olduğunca Logo Objects kullanılmalıdır.
- Doğrudan SQL `INSERT` / `UPDATE` / `DELETE` yalnızca istisnai ve kontrollü senaryolarda değerlendirilmelidir.
- Logo veritabanı ERP iş kurallarının ürettiği ilişkili veri modelidir.
- Veri bütünlüğü, maliyetlendirme, seri/lot izlenebilirliği ve muhasebe ilişkileri birlikte ele alınmalıdır.
- Entegrasyonlarda idempotency, retry, reconciliation, correlation id ve loglama standart kabul edilir.
- Kesin doğrulanmamış Logo enum, field veya tablo davranışı sürüm bağımlılığı belirtilmeden kesin bilgi olarak yazılmaz.

## Bilgi Güven Seviyeleri

- **Doğrulanmış Bilgi:** Resmi dokümantasyon, çalışan kod veya tekrar test ile doğrulanmış bilgi.
- **Saha Gözlemi:** Gerçek Logo ortamında gözlemlenmiş davranış.
- **Mimari Öneri:** Sürdürülebilir çözüm için önerilen mühendislik yaklaşımı.
- **Sürüm Bağımlı:** Logo / Objects sürümüne göre değişebilecek bilgi.
- **Kontrol Edilmeli:** Henüz kesin doğrulanmamış bilgi.

## Repository Yaklaşımı

Mevcut 1–99 bölüm dosyaları bağlantı geçmişini korumak için kök dizinde bırakılmıştır. İçerik `PART_01`–`PART_08` indeksleri üzerinden alan bazında okunur.

> Amaç Logo ERP üzerinde geliştirme, entegrasyon ve operasyon yapan ekipler için tekrar kullanılabilir bir mühendislik referansı oluşturmaktır.
