# 44 — ITEMS Alan Sözlüğü

## Amaç

Bu bölüm, Logo ERP malzeme kartlarının tutulduğu `LG_XXX_ITEMS` tablosunu geliştirici gözüyle açıklar. Özellikle stok raporları, birim ilişkileri, barkod, üretim, fiyatlandırma ve entegrasyon senaryolarında hangi alanların kritik olduğunu toplar.

## Temel tablo

```text
LG_<FIRMA>_ITEMS
```

Örnek:

```text
LG_040_ITEMS
LG_202_ITEMS
LG_803_ITEMS
```

`ITEMS` firma bazlıdır, dönem eki taşımaz.

## Temel alanlar

| Alan | Açıklama | Kullanım |
|---|---|---|
| `LOGICALREF` | Malzeme kartı referansı | `STLINE.STOCKREF`, `ORFLINE.STOCKREF` vb. ilişkilerin hedefidir |
| `CODE` | Malzeme kodu | Kullanıcı/entegrasyon iş anahtarı |
| `NAME` | Malzeme açıklaması | Raporlama |
| `STGRPCODE` | Stok grup kodu | Gruplama ve filtreleme |
| `SPECODE` | Özel kod | Sınıflandırma |
| `SPECODE2`... | İlave özel kod alanları | Proje bazlı segmentasyon |
| `CYPHCODE` | Yetki kodu | Yetki bazlı görünürlük |
| `ACTIVE` | Aktif/pasif durum | Kullanım kontrolü |
| `CARDTYPE` | Kart tipi | Malzeme/hizmet vb. ayrımlar; sürüm doğrulaması gerekir |
| `UNITSETREF` | Birim seti referansı | `UNITSETF/UNITSETL` ilişkisi |
| `TRACKTYPE` | İzleme tipi | Seri/lot kullanımında kritik; sürüm bağımlıdır |
| `SHELFLIFE` | Raf ömrü | SKT hesapları ve lot yönetimi |
| `PRODUCERCODE` | Üretici kodu | Üretim/tedarik süreçleri |
| `VAT` | Varsayılan KDV oranı | Ticari işlemlerde başlangıç değeri |
| `PURCHVAT` | Satınalma KDV | Sürüm/proje yapısına göre |
| `SELLVAT` | Satış KDV | Sürüm/proje yapısına göre |

## Temel ilişki

```sql
SELECT
    I.LOGICALREF,
    I.CODE,
    I.NAME,
    I.STGRPCODE,
    I.UNITSETREF
FROM LG_040_ITEMS I;
```

Stok hareketleriyle ilişki:

```sql
SELECT
    I.CODE,
    I.NAME,
    S.AMOUNT,
    S.DATE_
FROM LG_040_ITEMS I
INNER JOIN LG_040_01_STLINE S
    ON S.STOCKREF = I.LOGICALREF;
```

## UNITSETREF neden kritik?

Malzeme kartı üzerindeki `UNITSETREF`, malzemenin hangi birim setine bağlı olduğunu belirler.

Tipik zincir:

```text
ITEMS.UNITSETREF
    ↓
UNITSETF.LOGICALREF
    ↓
UNITSETL.UNITSETREF
```

Malzeme bazlı birim atamaları için ayrıca `ITMUNITA` okunmalıdır.

## Malzeme kodu ve LOGICALREF ayrımı

Cari kartlarda olduğu gibi:

```text
Dış sistem iş anahtarı = CODE
Logo teknik ilişki anahtarı = LOGICALREF
```

Özellikle firma kopyalama, veri taşıma ve test ortamlarında `LOGICALREF` sabit kabul edilmemelidir.

## Aktif kart filtresi

```sql
SELECT LOGICALREF, CODE, NAME
FROM LG_040_ITEMS
WHERE ACTIVE = 0;
```

Logo'da bazı sürümlerde aktif/pasif değerlerin yorumlanması kontrol edilmelidir. Bu nedenle canlı ortamda gerçek kayıt üzerinden doğrulama önerilir.

## Stok grubu

`STGRPCODE`, saha raporlarında sık kullanılan alanlardan biridir.

Örnek:

```sql
SELECT
    STGRPCODE,
    COUNT(*) AS MALZEME_ADEDI
FROM LG_040_ITEMS
GROUP BY STGRPCODE
ORDER BY STGRPCODE;
```

## Seri/Lot ilişkisi

Seri veya lot kontrollü malzemelerde kart üzerindeki izleme parametreleri tek başına yeterli değildir. Hareket bazında gerçek seri/lot kayıtları `SLTRANS`, `SERILOTN` ve ilişkili tablolarda oluşur.

Dolayısıyla:

```text
ITEMS = izleme kuralı
STLINE = stok hareketi
SLTRANS = seri/lot hareket bağlantısı
SERILOTN = seri/lot master kaydı
```

## Stok miktarı ITEMS içinde tutulmaz

En kritik noktalardan biri budur:

> `ITEMS` malzeme kartıdır; gerçek stok bakiyesi kart tablosundan okunmaz.

Stok için hareketler veya envanter tabloları kullanılmalıdır.

## Logo Objects açısından

Malzeme kartı ekleme/güncelleme işlemleri `IData` üzerinden yapılmalıdır. Özellikle şu alt yapılar nedeniyle SQL doğrudan update risklidir:

- birim setleri,
- birim atamaları,
- barkodlar,
- seri/lot parametreleri,
- muhasebe bağlantıları,
- fiyatlar,
- özel kod ve yetki ilişkileri.

## Önerilen malzeme bulma sorgusu

```sql
SELECT TOP 1
    LOGICALREF,
    CODE,
    NAME,
    STGRPCODE,
    UNITSETREF,
    ACTIVE
FROM LG_040_ITEMS
WHERE CODE = @MalzemeKodu;
```

## Özet

`LG_XXX_ITEMS`, stok miktarının değil, malzeme master data’sının merkezidir.

Malzeme ile ilgili doğru analizde temel yaklaşım:

```text
Kart bilgisi        → ITEMS
Birim                → UNITSETL / ITMUNITA
Barkod               → UNITBARCODE
Stok hareketi        → STLINE
Seri/Lot             → SLTRANS / SERILOTN
Sipariş              → ORFLINE
```
