# 144 — Logo SDK Binding Manifest ve Uyumluluk Kontrolü

Logo Objects entegrasyonunda en riskli hatalardan biri, bir sürümde doğrulanmış enum/metot/field bilgisini bütün sürümlerde geçerli kabul etmektir.

Bu nedenle referans uygulama SDK bağımlılığını bir **binding manifest** üzerinden tanımlar.

## Amaç

Kod içinde dağınık şekilde:

```text
DataObjectType = ...
PostMethod = ...
ErrorCodeField = ...
ProductionMethod = ...
```

gibi sürüm bağımlı sabitler tutmak yerine doğrulanmış bilgileri tek bir profile toplamak.

## Referans Sınıflar

```text
LogoSdkBindingManifest
LogoSdkBindingKeys
LogoSdkCompatibilityChecker
```

## Manifest İçeriği

Manifest en az şu bilgileri kaydetmelidir:

- Logo ürün/sürüm bilgisi,
- Objects/SDK sürümü,
- UnityApplication concrete type,
- login/logout metotları,
- NewDataObject metodu,
- DataObjectType karşılıkları,
- IData post/error alanları,
- Lines/DataFields erişim modeli,
- ProductionApplication binding bilgileri.

## Örnek Mantık

```text
Target Server
    ↓
Read SDK Version
    ↓
Load Binding Manifest
    ↓
Compatibility Check
    ↓
Required Key Missing?
    ├── Yes → Fail Fast
    └── No  → Enable Adapter
```

## Fail-Fast İlkesi

Binding doğrulanmamışsa uygulama tahmin üretmemelidir.

Örneğin malzeme DataObjectType değeri bilinmiyorsa sistem yanlış bir enum değerini deneyerek kayıt oluşturmaya çalışmaz. Bunun yerine:

```text
SDK_BINDING_INCOMPLETE
```

döndürür.

## Neden Manifest?

Bu model:

- SDK upgrade öncesi fark analizi yapılmasını,
- hangi değerlerin gerçekten doğrulandığının görülmesini,
- test ve production binding'lerinin ayrılmasını,
- yanlış sürümde yanlış COM çağrısının engellenmesini,
- upgrade checklist oluşturulmasını

kolaylaştırır.

## Versiyonlama

Manifest değişikliği uygulama release'i ile birlikte izlenmelidir.

Örnek:

```text
Application: 1.4.0
Binding Profile: LogoObjects-3.x-Verified-2026-08
Database Schema: 7
```

Kesin sürüm değerleri yalnızca hedef ortamdan doğrulandıktan sonra yazılmalıdır.

> Logo SDK sürüm bağımlılığı kodun gizli varsayımı değil, açık bir deployment girdisi olmalıdır.
