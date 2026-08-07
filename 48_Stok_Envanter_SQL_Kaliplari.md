# 48 — Stok Envanter SQL Kalıpları

## Amaç

Bu bölüm Logo ERP stok bakiyesi, fiili stok, ambar bazlı stok ve hareket kontrolü için kullanılabilecek SQL yaklaşım kalıplarını toplar.

> Not: Logo sürümü, kullanılan modüller ve veri hacmi sorgu tasarımını etkiler. Canlı sistemde execution plan ve gerçek hareket yapısı kontrol edilmelidir.

## 1. Malzeme kartı ile stok hareketini ayır

`ITEMS` kart bilgisidir. Gerçek stok miktarı doğrudan `ITEMS` tablosundan okunmaz.

Temel zincir:

```text
ITEMS.LOGICALREF
     ↓
STLINE.STOCKREF
```

## 2. Hareket toplamı yaklaşımı

Basit stok analizi hareket yönüne göre toplama gerektirir.

Kavramsal örnek:

```sql
SELECT
    S.STOCKREF,
    SUM(
        CASE
            WHEN S.IOCODE IN (1, 2) THEN S.AMOUNT
            WHEN S.IOCODE IN (3, 4) THEN -S.AMOUNT
            ELSE 0
        END
    ) AS STOK
FROM LG_040_01_STLINE S
WHERE S.LINETYPE = 0
GROUP BY S.STOCKREF;
```

`IOCODE` yönleri kullanılmadan önce gerçek fiş örnekleriyle doğrulanmalıdır.

## 3. Malzeme açıklamasıyla stok

```sql
WITH HAREKET AS
(
    SELECT
        STOCKREF,
        SUM(
            CASE
                WHEN IOCODE IN (1, 2) THEN AMOUNT
                WHEN IOCODE IN (3, 4) THEN -AMOUNT
                ELSE 0
            END
        ) AS STOK
    FROM LG_040_01_STLINE
    WHERE LINETYPE = 0
    GROUP BY STOCKREF
)
SELECT
    I.CODE,
    I.NAME,
    ISNULL(H.STOK, 0) AS STOK
FROM LG_040_ITEMS I
LEFT JOIN HAREKET H
    ON H.STOCKREF = I.LOGICALREF;
```

## 4. Ambar bazlı stok

Ambar analizi yapılırken hareket yönüne göre `SOURCEINDEX` / `DESTINDEX` mantığı doğru kurulmalıdır.

Basit tek yönlü raporlar yanıltıcı olabilir.

Kavramsal model:

```text
Giriş hareketi  → DESTINDEX veya ilgili hedef ambar
Çıkış hareketi  → SOURCEINDEX
```

Gerçek değerler fiş türü ve IOCODE ile birlikte test edilmelidir.

## 5. Fiili stok

Fiili stok hesabında amaç ana birime çevrilmiş gerçek fiziksel stok toplamını görmekse hareket birimleri ayrıca ele alınmalıdır.

Örnek yaklaşım:

```text
Hareket miktarı
   × UINFO2 / UINFO1
   → ana birim miktarı
```

Formül gerçek kayıt üzerinden doğrulanmalıdır.

## 6. İkinci birim stoğu

İkinci birim stoğu raporlanırken otomatik çevrim yapmak yerine, iş ihtiyacına göre hareketin kaydedildiği birim bazında ayrı toplam alınabilir.

Bu yaklaşım özellikle kullanıcıların yanlış birim seçip seçmediğini analiz ederken değerlidir.

## 7. Hareketsiz stok

Örnek son hareket tarihi:

```sql
SELECT
    I.CODE,
    I.NAME,
    MAX(S.DATE_) AS SON_HAREKET_TARIHI
FROM LG_040_ITEMS I
LEFT JOIN LG_040_01_STLINE S
    ON S.STOCKREF = I.LOGICALREF
   AND S.LINETYPE = 0
GROUP BY I.CODE, I.NAME;
```

Daha sonra örneğin son 4 ay veya 1 yıl hareket görmeyen stoklar filtrelenebilir.

## 8. Son giriş tarihi

```sql
SELECT
    STOCKREF,
    MAX(DATE_) AS SON_GIRIS
FROM LG_040_01_STLINE
WHERE IOCODE IN (1, 2)
  AND LINETYPE = 0
GROUP BY STOCKREF;
```

IOCODE değerleri ortamda doğrulanmalıdır.

## 9. Negatif stok kontrolü

Negatif stok tespiti için hareket toplamı hesaplanabilir.

```sql
WITH S AS
(
    SELECT
        STOCKREF,
        SUM(
            CASE
                WHEN IOCODE IN (1, 2) THEN AMOUNT
                WHEN IOCODE IN (3, 4) THEN -AMOUNT
                ELSE 0
            END
        ) AS STOK
    FROM LG_040_01_STLINE
    WHERE LINETYPE = 0
    GROUP BY STOCKREF
)
SELECT *
FROM S
WHERE STOK < 0;
```

## 10. Stok yaşlandırma

Stok yaşlandırmada yalnızca son giriş tarihine bakmak çoğu zaman yeterli değildir.

Daha doğru modeller:

- FIFO katmanları,
- kalan giriş miktarı,
- `REMAMOUNT` benzeri alanlar,
- seri/lot bazında giriş tarihi,
- maliyet katmanları.

## 11. Seri/Lot bazlı stok

Seri/lot stok için `STLINE` tek başına yeterli değildir.

```text
STLINE
  ↓
SLTRANS
  ↓
SERILOTN
```

zinciri izlenmelidir.

## 12. Sipariş stok değildir

Açık sipariş miktarı stok bakiyesine eklenmemelidir.

Sipariş ve fiziksel stok ayrı kavramlardır:

```text
ORFLINE = talep / sipariş
STLINE  = gerçekleşmiş stok hareketi
```

## 13. Performans

Büyük `STLINE` tablolarında sık filtrelenen alanlar:

- `STOCKREF`,
- `DATE_`,
- `TRCODE`,
- `IOCODE`,
- `SOURCEINDEX`,
- `DESTINDEX`,
- `LINETYPE`,
- `PROJECTREF`.

Index önerisi execution plan görmeden yapılmamalıdır.

## 14. En önemli kural

Stok sorgularında önce şu üç soru cevaplanmalıdır:

1. Hangi birim?
2. Hangi ambar?
3. Hangi hareket türleri dahil?

Bu üçü tanımlanmadan bulunan “stok” rakamı teknik olarak doğru olsa bile iş açısından yanlış olabilir.
