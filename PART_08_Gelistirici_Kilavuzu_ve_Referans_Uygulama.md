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

## Sıradaki Konular

- validation pipeline
- configuration encryption
- batch processor
- background worker
- health check
- test doubles / fake adapters
- integration test standardı
- örnek malzeme kartı servisi
- örnek cari kart servisi
- örnek sipariş servisi
- örnek irsaliye/fatura servisi
- örnek üretim entegrasyon servisi

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
```

> Bu bölüm kitabın teorik bilgilerini çalışan bir uygulama mimarisine dönüştüren uygulamalı kısımdır.
