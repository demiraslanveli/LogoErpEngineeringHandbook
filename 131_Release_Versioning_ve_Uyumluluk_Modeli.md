# 131 — Release Versioning ve Uyumluluk Modeli

Bu bölüm referans Logo ERP entegrasyon uygulamasında uygulama sürümü, database schema sürümü ve Logo ERP/Objects uyumluluğunun birlikte nasıl yönetileceğini tanımlar.

## Neden Ayrı Sürümler Var?

Tek bir versiyon numarası çoğu zaman yeterli değildir.

İzlenmesi gereken başlıca sürümler:

```text
ApplicationVersion
DatabaseSchemaVersion
LogoErpVersion
LogoObjectsVersion
ProductionApplicationVersion
ConfigurationVersion
```

## Application Version

SemVer benzeri yaklaşım kullanılabilir:

```text
MAJOR.MINOR.PATCH
```

Örnek:

```text
2.4.1
```

### MAJOR

Breaking change.

### MINOR

Geriye uyumlu yeni özellik.

### PATCH

Bug fix veya küçük düzeltme.

## Database Schema Version

Migration sırasıyla yönetilmelidir.

Örnek:

```text
V001
V002
V003
```

Application version ile birebir aynı olmak zorunda değildir.

Örnek ilişki:

```text
Application 2.4.1
Database Schema V018
```

## Logo Uyumluluk Matrisi

Her release için test edilen ERP/Objects sürümleri kayıt altına alınmalıdır.

Örnek:

| App Version | Logo ERP | Logo Objects | DB Schema | Durum |
|---|---|---|---|---|
| 2.4.0 | Tiger Wings Enterprise X | Objects X | V017 | Tested |
| 2.4.1 | Tiger Wings Enterprise X | Objects X | V018 | Tested |
| 2.4.1 | Daha yeni Logo sürümü | Kontrol edilmeli | V018 | Not Verified |

Gerçek ürün/sürüm değerleri test ortamında doğrulandığı şekilde doldurulmalıdır.

## Compatibility State

Standart durumlar:

```text
TESTED
SUPPORTED
LIMITED
NOT_VERIFIED
NOT_SUPPORTED
```

## Release Manifest

Her dağıtım paketinde makine tarafından okunabilir manifest bulunması yararlıdır.

Örnek:

```json
{
  "applicationVersion": "2.4.1",
  "databaseSchemaVersion": "V018",
  "gitCommit": "abc123",
  "buildNumber": "2026.08.12.1",
  "environment": "Production"
}
```

Logo sürüm bilgileri de doğrulanabildiği ölçüde manifest veya deployment kaydında saklanabilir.

## Runtime Version Log

Servis başlangıcında şu bilgiler loglanmalıdır:

```text
ApplicationVersion
SchemaVersion
BuildNumber
Environment
MachineName
ServiceName
Firm/Period configuration summary
```

Secret değerler loglanmamalıdır.

## Schema Compatibility

Uygulama başlangıcında minimum database schema sürümü kontrol edilebilir.

Örnek mantık:

```text
CurrentSchema < RequiredSchema
    ↓
Service startup blocked
```

Bu kontrol eski uygulamanın yeni şemayla veya yeni uygulamanın eski şemayla yanlış çalışmasını önler.

## Backward Compatibility

DTO/API sözleşmelerinde geriye uyumluluk ayrıca yönetilmelidir.

Breaking değişikliklerde:

- yeni endpoint/version
- yeni message contract
- controlled migration

yaklaşımları değerlendirilebilir.

## Logo Objects Uyumluluğu

Logo Objects API'sinde enum, field, method veya runtime davranışı sürüme göre farklılık gösterebilir.

Bu nedenle:

```text
Çalıştı = tüm sürümlerde çalışır
```

varsayımı yapılmamalıdır.

Yeni Logo sürümüne geçiş ayrı compatibility test süreci olarak ele alınmalıdır.

## Release Branch/Tag

Her production release Git tag ile işaretlenebilir:

```text
v2.4.1
```

Tag ile şu bilgiler ilişkilendirilebilir:

- release notes
- schema version
- deployment package
- compatibility matrix

## Hotfix

Hotfix release de normal versioning kurallarına tabi olmalıdır.

Örnek:

```text
2.4.1
↓
2.4.2-hotfix veya doğrudan 2.4.2
```

Kurum standardına göre suffix kullanılabilir.

## Release Notes Standardı

Her release note şunları içermelidir:

```text
Added
Changed
Fixed
Database
Configuration
Compatibility
Known Issues
Rollback Notes
```

## Kural

> Bir entegrasyon uygulamasında sürüm yalnızca EXE/DLL sürümü değildir; uygulama, database schema ve Logo runtime uyumluluğu birlikte versiyonlanmalıdır.
