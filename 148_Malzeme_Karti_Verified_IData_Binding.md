# 148 — Malzeme Kartı Verified IData Binding

Bu bölüm malzeme kartı oluşturma/güncelleme akışının gerçek Logo Objects `IData` binding standardını tanımlar.

## Akış

```text
MaterialService
   ↓
LogoMaterialGateway
   ↓
MaterialDataMappingProfile
   ↓
VerifiedLogoDataObjectFactory
   ↓
IData
```

## Minimum Doğrulama Seti

Production binding öncesinde en az şu bilgiler hedef SDK üzerinde doğrulanmalıdır:

- malzeme kartı `DataObjectType`,
- kart kodu alanı,
- açıklama alanı,
- kart türü/type alanı gerekiyorsa davranışı,
- birim seti ilişkisi,
- ana birim gereksinimleri,
- post/save yöntemi,
- başarılı kayıt sonrası logical reference erişimi.

## Birim Seti

Malzeme kartında yalnızca CODE/NAME set etmek çoğu gerçek kurulum için yeterli kabul edilmemelidir. Birim seti ve ana birim ilişkisi Logo iş kurallarına göre doğrulanmalıdır.

```text
Material
   ↓
Unit Set
   ↓
Main Unit
   ↓
Item Unit Assignment
```

## Duplicate Kontrolü

Aynı kod ile ikinci kayıt açılmasını önlemek için write öncesi lookup veya Logo'nun duplicate hata davranışı kontrollü şekilde ele alınmalıdır.

## Result Metadata

Başarılı işlemde önerilen metadata:

```text
LogicalRef
MaterialCode
ObjectType
Company
```

## Update

Update işleminde doğrudan SQL kullanılmamalıdır. `IData` read/edit/post akışı hedef sürümde doğrulanarak adapter'a eklenmelidir.

## Testler

- yeni malzeme oluşturma,
- duplicate kod,
- eksik birim seti,
- geçersiz field,
- post hatası,
- kayıt sonrası SQL read-back kontrolü.

> Malzeme kartı yalnızca bir ITEMS satırı değildir; ilişkili birim kayıtlarının ve Logo iş kurallarının birlikte oluştuğu ERP nesnesidir.
