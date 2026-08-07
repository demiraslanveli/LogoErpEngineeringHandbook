# 46 — UNITSETL / ITMUNITA / UNITBARCODE

## Amaç

Bu bölüm, Logo ERP’de malzeme birimleri ve barkod ilişkilerini açıklar. Özellikle ikinci birim, çevrim katsayıları, barkod güncelleme ve yanlış birim seçimi analizlerinde bu tablolar birlikte değerlendirilmelidir.

## Temel tablolar

```text
LG_<FIRMA>_UNITSETF
LG_<FIRMA>_UNITSETL
LG_<FIRMA>_ITMUNITA
LG_<FIRMA>_UNITBARCODE
```

Bu tablolar firma bazlıdır; dönem eki taşımaz.

## UNITSETF

Birim setinin üst kaydıdır.

Tipik alanlar:

- `LOGICALREF`
- `CODE`
- `NAME`
- aktiflik ve yetki alanları

Bir malzemenin bağlı olduğu birim seti:

```text
ITEMS.UNITSETREF → UNITSETF.LOGICALREF
```

## UNITSETL

Birim setinin satırlarını içerir.

Sık kullanılan alanlar:

| Alan | Açıklama |
|---|---|
| `LOGICALREF` | Birim satır referansı |
| `UNITSETREF` | Birim seti üst referansı |
| `CODE` | Birim kodu (`ADET`, `KG`, `KUTU` vb.) |
| `NAME` | Birim açıklaması |
| `LINENR` | Birim sıra numarası |
| `MAINUNIT` | Ana birim göstergesi |
| `CONVFACT1` | Genel çevrim katsayısı |
| `CONVFACT2` | Genel çevrim katsayısı |

## ITMUNITA

Malzeme ile birim satırı arasındaki atamayı ve malzeme bazlı çevrim bilgisini tutar.

Önemli alanlar:

```text
ITEMREF
UNITLINEREF
CONVFACT1
CONVFACT2
LINENR
```

Tipik ilişki:

```text
ITEMS.LOGICALREF
      ↓ ITEMREF
ITMUNITA
      ↓ UNITLINEREF
UNITSETL.LOGICALREF
```

## Malzemenin birimlerini getirme

```sql
SELECT
    I.CODE AS MALZEME_KODU,
    I.NAME AS MALZEME_ADI,
    U.CODE AS BIRIM,
    U.MAINUNIT,
    IU.CONVFACT1,
    IU.CONVFACT2
FROM LG_040_ITEMS I
INNER JOIN LG_040_ITMUNITA IU
    ON IU.ITEMREF = I.LOGICALREF
INNER JOIN LG_040_UNITSETL U
    ON U.LOGICALREF = IU.UNITLINEREF
WHERE I.CODE = @MalzemeKodu
ORDER BY IU.LINENR;
```

## Ana birimi bulma

```sql
SELECT
    I.CODE,
    U.CODE AS ANA_BIRIM
FROM LG_040_ITEMS I
INNER JOIN LG_040_UNITSETL U
    ON U.UNITSETREF = I.UNITSETREF
   AND U.MAINUNIT = 1;
```

Sürüm ve veri yapısında `MAINUNIT` değerinin gerçek kayıtta doğrulanması önerilir.

## UINFO alanlarıyla ilişki

Hareket satırlarında:

```text
STLINE.UOMREF
STLINE.USREF
STLINE.UINFO1
STLINE.UINFO2
```

bulunur.

Kart tarafındaki çevrim tanımı ile hareket üzerindeki gerçek birim bilgisi karıştırılmamalıdır.

Örneğin satınalma sırasında kullanıcı ikinci birimi seçtiğinde hareket satırında o işlemin birim ve çevrim bilgisi saklanabilir.

## UNITBARCODE

Malzeme/birim barkod ilişkilerini tutar.

Tipik alanlar:

```text
LOGICALREF
ITEMREF
UNITLINEREF
BARCODE
LINENR
```

Sürüm bazında alan isimleri kontrol edilmelidir.

Örnek barkod sorgusu:

```sql
SELECT
    I.CODE,
    I.NAME,
    U.CODE AS BIRIM,
    B.BARCODE
FROM LG_040_UNITBARCODE B
INNER JOIN LG_040_ITEMS I
    ON I.LOGICALREF = B.ITEMREF
LEFT JOIN LG_040_UNITSETL U
    ON U.LOGICALREF = B.UNITLINEREF
WHERE I.CODE = @MalzemeKodu;
```

## Barkod eklerken dikkat

Barkod yalnızca string alanı değildir. Şunlar birlikte doğrulanmalıdır:

- malzeme referansı,
- birim referansı,
- sıra numarası,
- aynı barkodun başka malzemede kullanılıp kullanılmadığı,
- Logo ekranındaki birim/barkod davranışı.

## Duplicate barkod kontrolü

```sql
SELECT
    BARCODE,
    COUNT(*) AS ADET
FROM LG_040_UNITBARCODE
WHERE ISNULL(BARCODE, '') <> ''
GROUP BY BARCODE
HAVING COUNT(*) > 1;
```

Bu sonuç mutlaka iş kuralı ile yorumlanmalıdır.

## Çevrim katsayısı

Sahada kullanılan genel yaklaşım:

```text
Ana miktar = Hareket miktarı × UINFO2 / UINFO1
```

veya kart tarafında `CONVFACT1/CONVFACT2` üzerinden benzer mantık görülebilir.

Ancak kesin yönü ezberlemek yerine gerçek bir örnek hareket üzerinden doğrulamak daha güvenlidir.

Örnek:

```text
1 KUTU = 12 ADET
```

Gerçek kayıttaki `UINFO1/UINFO2` değerleri kontrol edilerek formül doğrulanmalıdır.

## Yanlış birim seçimi nasıl bulunur?

Yalnızca miktar çevrimi yeterli değildir. Satınalma birim fiyatı da güçlü bir sinyaldir.

Örneğin aynı malzeme normalde:

```text
ADET → 10 TL
KUTU → 120 TL
```

şeklinde alınıyorsa 1 KUTU hareketinin 10 TL görünmesi yanlış birim seçimini işaret edebilir.

Bu nedenle anomali analizinde birlikte kullanılabilir:

- `UOMREF`,
- `UINFO1/UINFO2`,
- `AMOUNT`,
- `PRICE`,
- geçmiş satınalma fiyat dağılımı.

## Logo Objects yaklaşımı

Birim ve barkod değişikliklerinde doğrudan SQL yerine mümkün olduğunca Logo Objects tercih edilmelidir. Kartın alt satır yapısı ve validasyonları önemlidir.

## Özet

Malzeme birim mimarisi:

```text
ITEMS
  ↓ UNITSETREF
UNITSETF
  ↓
UNITSETL
  ↑ UNITLINEREF
ITMUNITA ← ITEMREF → ITEMS

UNITBARCODE
  ├─ ITEMREF
  └─ UNITLINEREF
```

Stok ve fiyat analizlerinde kart tanımı ile hareket üzerindeki gerçek birim bilgisi birlikte okunmalıdır.
