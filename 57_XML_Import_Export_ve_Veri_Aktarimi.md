# 57 — XML Import / Export ve Veri Aktarımı

## Amaç

Bu bölüm Logo ERP / Logo Objects entegrasyonlarında XML tabanlı veri aktarımının nerede konumlandığını, hangi durumlarda tercih edilebileceğini ve veri bütünlüğü açısından hangi kontrollerin zorunlu olduğunu açıklar.

## Temel yaklaşım

XML aktarımı yalnızca alan eşleştirme işi değildir. Aktarılan kart veya fişin Logo iş kurallarına uygun biçimde oluşması gerekir. Bu nedenle aktarım tasarımında aşağıdaki katmanlar ayrılmalıdır:

1. Kaynak sistem verisi
2. Mapping / dönüşüm katmanı
3. Logo Objects veya desteklenen import mekanizması
4. İş kuralı validasyonu
5. Sonuç ve hata kaydı
6. Idempotency kontrolü

## Kart ve fiş ayrımı

Kart aktarımında çoğunlukla kod, açıklama, birim, özel kodlar ve yetki kodları gibi master-data alanları ön plandadır.

Fiş aktarımında ise üst bilgi ile satır bilgilerinin yanında cari, ambar, birim, proje, döviz, KDV, seri/lot ve muhasebeleştirme etkileri de değerlendirilmelidir.

## Mapping tablosu

Kaynak sistem alanları doğrudan Logo field isimlerine gömülmemelidir. Ayrı bir mapping katmanı önerilir.

```text
SourceItemCode  -> Logo ITEM CODE
SourceCustomer  -> CLIENTREF / ARP_CODE
WarehouseCode   -> SOURCEINDEX
ProjectCode     -> PROJECTREF
Quantity        -> AMOUNT
UnitCode        -> UOMREF / UNIT_CODE
```

## Idempotency

Aynı XML'in ikinci kez işlenmesi yeni fiş üretmemelidir. Bunun için dış sistem kaydına ait benzersiz anahtar saklanmalıdır.

Örnek:

```text
ExternalSystem = OPERAN
ExternalId     = PROD-2026-000451
LogoLogicalRef = 123456
Status         = SUCCESS
```

## Hata yönetimi

Hata kaydında yalnızca XML'in tamamını saklamak yeterli değildir. En az şu bilgiler tutulmalıdır:

- kaynak kayıt ID
- hedef DataObjectType
- firma / dönem
- Logo hata mesajı
- validasyon mesajı
- oluştuysa LOGICALREF
- işlem tarihi
- retry sayısı

## Güvenli aktarım akışı

```text
XML Al
  ↓
Schema / zorunlu alan kontrolü
  ↓
Master data çözümleme
  ↓
Duplicate / idempotency kontrolü
  ↓
Logo Objects ile kayıt
  ↓
Post doğrulama
  ↓
Başarı / hata logu
```

## Best Practice

XML, Logo veritabanına doğrudan INSERT üretmek için ara format olarak kullanılmamalıdır. Mümkün olan senaryolarda XML yalnızca taşıma formatı olmalı; resmi kayıt Logo Objects veya Logo'nun desteklenen import mekanizması üzerinden oluşturulmalıdır.

## Bilgi güven seviyesi

- Mimari yaklaşım: **Doğrulanmış mühendislik pratiği**
- XML node ve field adları: **Sürüm / nesne tipine göre doğrulanmalı**
