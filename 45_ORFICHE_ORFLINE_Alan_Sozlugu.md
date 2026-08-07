# 45 — ORFICHE / ORFLINE Alan Sözlüğü

## Amaç

Bu bölüm, Logo ERP sipariş fişi ve sipariş satırı tablolarını açıklar:

```text
LG_<FIRMA>_<DONEM>_ORFICHE
LG_<FIRMA>_<DONEM>_ORFLINE
```

Siparişlerin irsaliye ve faturaya dönüşümünü doğru analiz etmek için bu iki tablo temel kaynaktır.

## ORFICHE — Sipariş başlığı

Sık kullanılan alanlar:

| Alan | Açıklama |
|---|---|
| `LOGICALREF` | Sipariş fişi referansı |
| `FICHENO` | Sipariş numarası |
| `DATE_` | Sipariş tarihi |
| `TIME_` / zaman alanları | Sürüm bazlı olabilir |
| `TRCODE` | Sipariş türü |
| `CLIENTREF` | Cari kart referansı |
| `NETTOTAL` | Net toplam |
| `GROSSTOTAL` | Brüt toplam |
| `TOTALDISCOUNTS` | İndirim toplamı |
| `TOTALVAT` | KDV toplamı |
| `SOURCEINDEX` | Kaynak ambar / organizasyon alanı; senaryoya göre doğrulanmalı |
| `PROJECTREF` | Proje referansı |
| `CANCELLED` | İptal durumu |
| `STATUS` | Sipariş statüsü; sürüm doğrulaması gerekir |

## ORFLINE — Sipariş satırı

| Alan | Açıklama |
|---|---|
| `LOGICALREF` | Sipariş satırı referansı |
| `ORDFICHEREF` | `ORFICHE.LOGICALREF` bağlantısı |
| `STOCKREF` | Malzeme kartı referansı |
| `LINETYPE` | Satır tipi |
| `AMOUNT` | Sipariş miktarı |
| `PRICE` | Birim fiyat |
| `TOTAL` | Satır toplamı |
| `UOMREF` | Birim referansı |
| `USREF` | Birim seti referansı |
| `UINFO1`, `UINFO2` | Birim dönüşüm bilgileri |
| `CLOSED` | Satır kapanma durumu |
| `SHIPPEDAMOUNT` | Sevk edilen miktar; sürüm doğrulaması önerilir |
| `PROJECTREF` | Proje referansı |
| `SPECODE` | Özel kod |

## Başlık–satır ilişkisi

```sql
SELECT
    O.FICHENO,
    O.DATE_,
    L.LOGICALREF AS ORFLINE_REF,
    I.CODE,
    I.NAME,
    L.AMOUNT,
    L.PRICE
FROM LG_040_01_ORFICHE O
INNER JOIN LG_040_01_ORFLINE L
    ON L.ORDFICHEREF = O.LOGICALREF
LEFT JOIN LG_040_ITEMS I
    ON I.LOGICALREF = L.STOCKREF;
```

## Siparişten irsaliyeye bağlantı

Sipariş satırının sevke dönüşümünde en kritik alanlardan biri:

```text
STLINE.ORDTRANSREF
```

Tipik ilişki:

```text
ORFLINE.LOGICALREF
        ↓
STLINE.ORDTRANSREF
```

Örnek:

```sql
SELECT
    OL.LOGICALREF AS SIPARIS_SATIR_REF,
    OL.AMOUNT AS SIPARIS_MIKTARI,
    SL.LOGICALREF AS STOK_SATIR_REF,
    SL.AMOUNT AS SEVK_MIKTARI
FROM LG_040_01_ORFLINE OL
LEFT JOIN LG_040_01_STLINE SL
    ON SL.ORDTRANSREF = OL.LOGICALREF;
```

## Kısmi sevk analizi

Sipariş miktarı ile bağlı sevk satırlarının toplamı karşılaştırılabilir.

```sql
SELECT
    OL.LOGICALREF,
    OL.AMOUNT AS SIPARIS_MIKTARI,
    ISNULL(SUM(SL.AMOUNT), 0) AS SEVK_MIKTARI,
    OL.AMOUNT - ISNULL(SUM(SL.AMOUNT), 0) AS KALAN
FROM LG_040_01_ORFLINE OL
LEFT JOIN LG_040_01_STLINE SL
    ON SL.ORDTRANSREF = OL.LOGICALREF
GROUP BY OL.LOGICALREF, OL.AMOUNT;
```

Bu hesapta iade, iptal ve satır türleri ayrıca değerlendirilmelidir.

## LINETYPE filtresi

Malzeme bazlı raporlarda genellikle:

```sql
WHERE ORFLINE.LINETYPE = 0
```

şeklinde fiziksel malzeme satırı filtresi kullanılır. Ancak hizmet, indirim, masraf ve promosyon satırları gibi farklı satır tipleri bulunduğundan amaç net belirlenmelidir.

## Sipariş kapanma mantığı

`CLOSED` veya ilgili statü alanları tek başına iş gerçeğini her zaman açıklamayabilir.

Kontrol için birlikte incelenmesi önerilenler:

- sipariş satır miktarı,
- bağlı `STLINE` hareketleri,
- iptal durumu,
- iade hareketleri,
- fiş statüsü.

## Cari ilişkisi

Başlık seviyesinde:

```text
ORFICHE.CLIENTREF → CLCARD.LOGICALREF
```

Bu nedenle cari bazlı sipariş raporunda genellikle başlık tablosu kullanılır.

## Proje ilişkisi

Proje kullanılan sistemlerde `PROJECTREF` hem başlık hem satır düzeyinde görülebilir. Gerçek iş kuralına göre hangi seviyenin esas olduğu doğrulanmalıdır.

## Performans notu

Büyük hareket tablolarında şu alanlar join/filter açısından sık kullanılır:

```text
ORFICHE.LOGICALREF
ORFICHE.CLIENTREF
ORFICHE.DATE_
ORFLINE.ORDFICHEREF
ORFLINE.STOCKREF
STLINE.ORDTRANSREF
```

Özel raporlarda execution plan incelenerek uygun index stratejisi belirlenmelidir.

## Özet

Sipariş zincirinin ana ilişkisi:

```text
ORFICHE
   ↓ ORDFICHEREF
ORFLINE
   ↓ ORDTRANSREF
STLINE
   ↓
STFICHE / INVOICE
```

Siparişin ne kadarının karşılandığını anlamak için yalnızca sipariş statüsüne değil, gerçek bağlı hareketlere de bakılmalıdır.
