# Logo ERP Engineering Handbook

**Logo ERP, Logo Objects, ProductionApplication, SQL Server ve Kurumsal Entegrasyonlar için Teknik Referans**

Bu repository yalnızca Logo Objects kullanımını anlatan bir kitap değildir. İçerik; Logo ERP veri modeli, SDK geliştirme, detaylı üretim, seri/lot, kalite, maliyetlendirme, SQL Server, finans, entegrasyon, operasyon, güvenlik ve production-grade referans uygulama mimarisini kapsayan yaşayan bir mühendislik el kitabıdır.

## Kitap Mimarisi

- [Kitap Mimarisi](00_Kitap_Mimarisi.md)

## Ana Bölümler

1. [Logo ERP Core ve Veri Modeli](PART_01_Logo_ERP_Core_ve_Veri_Modeli.md)
2. [Logo Objects SDK ve Uygulama Geliştirme](PART_02_Logo_Objects_SDK_ve_Uygulama_Gelistirme.md)
3. [Üretim, Seri/Lot, Kalite ve Maliyet](PART_03_Uretim_SeriLot_Kalite_ve_Maliyet.md)
4. [SQL Server, Veritabanı ve Performans](PART_04_SQL_Server_Veritabanı_ve_Performans.md)
5. [Entegrasyon Mimarileri ve Servisler](PART_05_Entegrasyon_Mimarileri_ve_Servisler.md)
6. [Finans, Muhasebe ve Elektronik Belgeler](PART_06_Finans_Muhasebe_ve_EBelge.md)
7. [Operasyon, Güvenlik, Backup ve DR](PART_07_Operasyon_Guvenlik_Backup_ve_DR.md)
8. [Geliştirici Kılavuzu ve Referans Uygulama](PART_08_Gelistirici_Kilavuzu_ve_Referans_Uygulama.md)

## Referans Uygulama

100–160 arasındaki uygulamalı mimari seri tamamlanmıştır.

- [Reference Application](REFERENCE_APPLICATION.md)
- `LogoErp.Reference.sln`
- `src/`
- `tests/`
- `database/migrations/`
- `deploy/`

Serinin başlangıcı:

- [100 — Referans .NET Çözüm Mimarisi](100_Referans_DotNet_Cozum_Mimarisi.md)

Serinin kapanış bölümleri:

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

Tüm 100–160 listesi için [Part 08 indeksine](PART_08_Gelistirici_Kilavuzu_ve_Referans_Uygulama.md) bakılabilir.

## Temel Prensipler

- Resmi kart ve fiş işlemlerinde mümkün olduğunca Logo Objects kullanılmalıdır.
- Doğrudan SQL `INSERT` / `UPDATE` / `DELETE` yalnızca istisnai ve kontrollü senaryolarda değerlendirilmelidir.
- Logo veritabanı ERP iş kurallarının ürettiği ilişkili veri modelidir.
- Veri bütünlüğü, maliyetlendirme, seri/lot izlenebilirliği ve muhasebe ilişkileri birlikte ele alınmalıdır.
- Entegrasyonlarda idempotency, retry, reconciliation, correlation id ve loglama standart kabul edilir.
- Kesin doğrulanmamış Logo enum, field veya tablo davranışı sürüm bağımlılığı belirtilmeden kesin bilgi olarak yazılmaz.
- SDK uyumluluğu binding manifest ile doğrulanır.
- COM yaşam döngüsü explicit ve deterministic yönetilir.
- Deployment, rollback, health ve end-to-end test production mimarisinin parçasıdır.

## Bilgi Güven Seviyeleri

- **Doğrulanmış Bilgi:** Resmi dokümantasyon, çalışan kod veya tekrar test ile doğrulanmış bilgi.
- **Saha Gözlemi:** Gerçek Logo ortamında gözlemlenmiş davranış.
- **Mimari Öneri:** Sürdürülebilir çözüm için önerilen mühendislik yaklaşımı.
- **Sürüm Bağımlı:** Logo / Objects sürümüne göre değişebilecek bilgi.
- **Kontrol Edilmeli:** Henüz kesin doğrulanmamış bilgi.

## Bundan Sonraki Gelişim

Temel mimari seri tamamlandığı için yeni bölüm üretmek yerine mevcut içerik yaşayan referans olarak güncellenecektir:

- doğrulanmış SDK kodları,
- gerçek saha vakaları,
- çalışan SQL/C# örnekleri,
- Logo sürüm uyumluluk kayıtları,
- performans analizleri,
- production test sonuçları.

> Amaç Logo ERP üzerinde geliştirme, entegrasyon ve operasyon yapan ekipler için tekrar kullanılabilir ve doğrulanabilir bir mühendislik referansı oluşturmaktır.
