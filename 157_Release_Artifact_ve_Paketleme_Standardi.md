# 157 — Release Artifact ve Paketleme Standardı

Bu bölüm production dağıtım paketinin yapısını ve versiyonlama standardını tanımlar.

## Amaç

Release yalnızca `bin` klasöründen ibaret değildir. Uygulama, migration, config template, deployment scripti ve sürüm metadata'sı birlikte paketlenmelidir.

## Önerilen Paket

```text
release/
├── app/
├── database/
│   └── migrations/
├── deploy/
│   ├── install.ps1
│   ├── upgrade.ps1
│   ├── rollback.ps1
│   └── validate.ps1
├── config/
│   └── environment.template.txt
├── manifest/
│   ├── release.json
│   └── logo-sdk-binding.json
└── CHANGELOG.md
```

## Release Manifest

En az:

```text
ApplicationVersion
GitCommitSha
BuildDate
DbSchemaVersion
SupportedLogoVersion
SupportedObjectsVersion
BindingManifestVersion
TargetFramework
```

## Paket İçeriği İlkeleri

- secret pakete konmaz,
- development config production paketine girmez,
- debug symbol politikası bilinçli belirlenir,
- gereksiz SDK DLL kopyalama yapılmaz,
- lisanslı/vendor assembly dağıtımı lisans ve kurulum modeline uygun olmalıdır.

## SemVer

Uygulama için örnek:

```text
MAJOR.MINOR.PATCH
```

MAJOR: kırıcı mimari/schema davranışı
MINOR: geriye uyumlu özellik
PATCH: hata düzeltmesi

## Artifact Integrity

Paket hash'i üretilebilir ve deployment öncesi doğrulanabilir.

## Changelog

Her release şu sınıflarda özetlenmelidir:

```text
Added
Changed
Fixed
Database
Compatibility
Operational Notes
```

## Saklama

En az son stabil paket ve rollback için gerekli önceki paket erişilebilir tutulmalıdır.

> Tekrar üretilemeyen veya hangi commit'ten çıktığı bilinmeyen binary production release kabul edilmemelidir.
