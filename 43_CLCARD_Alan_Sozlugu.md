# 43 — CLCARD Alan Sözlüğü

## Amaç

Bu bölüm, Logo ERP cari hesap kartlarının tutulduğu `LG_XXX_CLCARD` tablosunu geliştirici ve entegrasyon bakış açısıyla açıklar. Amaç yalnızca alan isimlerini listelemek değil, hangi alanların raporlama, entegrasyon, mutabakat ve Logo Objects işlemlerinde kritik olduğunu göstermektir.

> Not: Alan davranışları Logo sürümüne göre farklılaşabilir. Özellikle enum benzeri alanlarda gerçek ortam ve sürüm doğrulaması yapılmalıdır.

## Temel tablo

```text
LG_<FIRMA>_CLCARD
```

Örnek:

```text
LG_040_CLCARD
LG_102_CLCARD
LG_803_CLCARD
```

`CLCARD` firma bazlıdır; dönem eki taşımaz.

## Temel alanlar

| Alan | Açıklama | Kullanım |
|---|---|---|
| `LOGICALREF` | Cari kart benzersiz referansı | Diğer tablolardaki `CLIENTREF` ilişkilerinin hedefidir |
| `CODE` | Cari hesap kodu | Ekran ve entegrasyonlarda ana iş anahtarı olarak sık kullanılır |
| `DEFINITION_` | Cari açıklaması / unvanı | Raporlama ve görüntüleme |
| `SPECODE` | Özel kod | Segmentasyon ve raporlama |
| `CYPHCODE` | Yetki kodu | Kullanıcı/yetki bazlı filtreleme |
| `ACTIVE` | Kart aktiflik durumu | Pasif kart kontrolü |
| `CARDTYPE` | Cari kart tipi | Alıcı, satıcı vb. sınıflandırmalar için sürüm doğrulaması gerekir |
| `TAXNR` | Vergi numarası | Kurumsal kimlik ve e-belge süreçleri |
| `TAXOFFICE` | Vergi dairesi | Cari vergi bilgileri |
| `TCKNO` | T.C. kimlik numarası | Şahıs cari hesapları |
| `COUNTRY` | Ülke | Adres ve dış ticaret |
| `CITY` | İl | Adres |
| `TOWN` | İlçe | Adres |
| `POSTCODE` | Posta kodu | Adres |
| `ADDR1`, `ADDR2` | Adres satırları | Sevk/fatura iletişim bilgileri |
| `TELNRS1`, `TELNRS2` | Telefon alanları | İletişim |
| `EMAILADDR` | E-posta | Mail entegrasyonu ve bilgilendirme |
| `WEBADDR` | Web adresi | Kurumsal bilgi |
| `PAYMENTREF` | Ödeme planı referansı | Vadeli işlemler ve ticari koşullar |
| `TRADINGGRP` | Ticari işlem grubu | Raporlama ve muhasebe senaryoları |
| `ACCOUNTREF` | Muhasebe hesap referansı | Muhasebe bağlantıları; kullanım sürüm/proje yapısına göre değişebilir |

## LOGICALREF ilişkileri

Cari kartın `LOGICALREF` değeri birçok dönemsel tabloda `CLIENTREF` olarak görülür.

Örnek:

```sql
SELECT
    C.CODE,
    C.DEFINITION_,
    L.DATE_,
    L.AMOUNT
FROM LG_040_CLCARD C
INNER JOIN LG_040_01_CLFLINE L
    ON L.CLIENTREF = C.LOGICALREF;
```

Bu ilişki Logo veri modelindeki en temel referans zincirlerinden biridir.

## Cari kodu mu LOGICALREF mi?

Entegrasyon tasarımında dış sistemlerde genellikle `CODE` saklanır; Logo iç ilişkilerinde ise `LOGICALREF` kullanılır.

Önerilen yaklaşım:

```text
Dış sistem iş anahtarı = CODE
Logo ilişkisel anahtar = LOGICALREF
```

`LOGICALREF` değerinin firma değişiminde veya farklı veri setlerinde aynı kalacağı varsayılmamalıdır.

## Aktif/Pasif kart kontrolü

Cari seçimlerinde yalnızca kod eşleşmesi yeterli değildir. Kartın aktiflik durumu da kontrol edilmelidir.

```sql
SELECT LOGICALREF, CODE, DEFINITION_, ACTIVE
FROM LG_040_CLCARD
WHERE CODE = 'MER.0086';
```

Entegrasyon katmanında pasif kartın yanlışlıkla kullanılması önlenmelidir.

## Vergi kimlik kontrolü

Aynı vergi numarasına birden fazla cari açılması gerçek projelerde karşılaşılan önemli veri kalitesi problemlerindendir.

```sql
SELECT
    TAXNR,
    COUNT(*) AS ADET
FROM LG_040_CLCARD
WHERE ISNULL(TAXNR, '') <> ''
GROUP BY TAXNR
HAVING COUNT(*) > 1;
```

Bu sorgu tek başına hatalı kayıt anlamına gelmez; şube veya farklı ticari roller bilinçli olarak ayrı kartlarda tutulabilir.

## Cari kart ve dönemsel hareket ayrımı

`CLCARD` kart tablosudur ve dönem eki taşımaz.

Hareketler ise örneğin:

```text
LG_040_01_CLFLINE
LG_040_02_CLFLINE
```

gibi dönemsel tablolardadır.

Dolayısıyla cari bakiye raporlarında yalnızca `CLCARD` okunması yeterli değildir.

## Logo Objects açısından

Cari kart ekleme/güncelleme/silme işlemlerinde mümkün olduğunca `IData` ve ilgili `DataObjectType` kullanılmalıdır.

Doğrudan SQL ile kart güncellemek özellikle şu alanlarda yan etkilere yol açabilir:

- adres/vergi alanları,
- e-belge parametreleri,
- muhasebe bağlantıları,
- ödeme planı ilişkileri,
- alt kayıtlar ve özel tablolar.

## Güvenli cari bulma kalıbı

```sql
SELECT TOP 1
    LOGICALREF,
    CODE,
    DEFINITION_,
    ACTIVE
FROM LG_040_CLCARD
WHERE CODE = @CariKodu;
```

Uygulama tarafında şu kontroller eklenmelidir:

1. Kayıt bulundu mu?
2. Birden fazla kayıt olasılığı var mı?
3. Kart aktif mi?
4. Firma doğru mu?

## Özet

`CLCARD`, Logo ERP’de cari master data’nın merkezidir. En kritik ayrım şudur:

> `CODE` kullanıcı ve entegrasyon iş anahtarıdır; `LOGICALREF` ise Logo iç ilişkilerinin teknik anahtarıdır.

Bu ayrım doğru kurulmadığında cari hareket, fatura ve muhasebe ilişkileri kolayca hatalı yorumlanabilir.
