# 41 — Seri / Lot Gerçek SQL Sorguları

## Amaç

Bu bölüm Logo ERP’de seri ve lot takibi yapılan malzemeler için pratik SQL analiz kalıpları sunar.

Amaç doğrudan veri değiştirmek değil; stok hareketi, seri/lot kaydı, kalan miktar ve kaynak belge zincirini güvenli şekilde teşhis etmektir.

> Seri/lot tabloları ve alan davranışları Logo sürümüne göre değişebilir. Aşağıdaki sorgular üretim ortamına alınmadan önce test veritabanında ve gerçek bir örnek hareket üzerinden doğrulanmalıdır.

## 1. Temel yaklaşım

Seri/lot analizinde tek tabloya bakmak çoğu zaman yeterli değildir.

Kavramsal zincir:

```text
ITEMS
  ↓
STLINE
  ↓
Seri/Lot hareket bağlantısı
  ↓
Seri/Lot kartı veya numarası
```

Analizde şu bilgiler birlikte ele alınmalıdır:

- malzeme referansı,
- stok satırı referansı,
- seri/lot referansı,
- hareket miktarı,
- kalan miktar,
- ambar,
- işlem türü,
- tarih,
- kaynak belge.

## 2. Seri/lot takipli malzemeleri bulma

İlk adım malzeme kartında izleme türünü doğrulamaktır.

Alan adı sürüme göre doğrulanmalıdır.

Örnek yaklaşım:

```sql
SELECT
    LOGICALREF,
    CODE,
    NAME,
    TRACKTYPE
FROM LG_102_ITEMS
WHERE TRACKTYPE <> 0;
```

Buradaki `TRACKTYPE` alanı kavramsal örnektir; gerçek sistem metadata’sı ile doğrulanmalıdır.

## 3. Bir stok satırının seri/lot kayıtlarını bulma

Seri/lot hareket bağlantılarında Logo kurulumuna göre `SLTRANS`, `SERILOTN` ve ilgili bağlantı tabloları kullanılabilir.

Kavramsal sorgu:

```sql
SELECT
    SL.LOGICALREF AS STLINE_REF,
    SL.STOCKREF,
    SL.DATE_,
    SL.AMOUNT,
    SLR.*
FROM LG_102_01_STLINE SL
LEFT JOIN LG_102_01_SLTRANS SLR
    ON SLR.STTRANSREF = SL.LOGICALREF
WHERE SL.LOGICALREF = @StlineRef;
```

> `SLTRANS` alan adları ve bağlantı kolonları sürüm bazında kontrol edilmelidir.

## 4. Seri/lot numarasını görmek

Kavramsal bağlantı:

```text
STLINE
  ↓
SLTRANS
  ↓
SERILOTN
```

Örnek:

```sql
SELECT
    SL.LOGICALREF AS STLINE_REF,
    I.CODE AS MALZEME_KODU,
    S.CODE AS SERI_LOT_NO,
    T.AMOUNT
FROM LG_102_01_STLINE SL
INNER JOIN LG_102_ITEMS I
    ON I.LOGICALREF = SL.STOCKREF
INNER JOIN LG_102_01_SLTRANS T
    ON T.STTRANSREF = SL.LOGICALREF
INNER JOIN LG_102_01_SERILOTN S
    ON S.LOGICALREF = T.SLREF
WHERE SL.LOGICALREF = @StlineRef;
```

Bu sorgudaki `SLREF`, `STTRANSREF` ve `AMOUNT` alanları gerçek sürümde doğrulanmalıdır.

## 5. Seri/lot bazında hareket geçmişi

Belirli bir seri veya lot numarasının bütün hareketlerini izlemek için:

```sql
SELECT
    S.CODE AS SERI_LOT_NO,
    SL.DATE_,
    SL.TRCODE,
    SL.IOCODE,
    SL.SOURCEINDEX,
    SL.DESTINDEX,
    T.AMOUNT,
    SL.STFICHEREF,
    SL.INVOICEREF
FROM LG_102_01_SERILOTN S
INNER JOIN LG_102_01_SLTRANS T
    ON T.SLREF = S.LOGICALREF
INNER JOIN LG_102_01_STLINE SL
    ON SL.LOGICALREF = T.STTRANSREF
WHERE S.CODE = @SerialLotNo
ORDER BY SL.DATE_, SL.LOGICALREF;
```

## 6. Seri/lot stok miktarı hesaplama

Seri/lot stok hesabında yalnızca hareket miktarlarını toplamak yeterli değildir.

Şunlar birlikte değerlendirilmelidir:

- `IOCODE`,
- `TRCODE`,
- giriş/çıkış yönü,
- iptal kayıtları,
- ambar,
- üretim hareketleri,
- transfer hareketleri.

Kavramsal hesap:

```text
Seri/Lot Stok = Toplam Giriş - Toplam Çıkış
```

SQL örneği:

```sql
SELECT
    S.CODE,
    SUM(
        CASE
            WHEN SL.IOCODE IN (1, 2) THEN T.AMOUNT
            WHEN SL.IOCODE IN (3, 4) THEN -T.AMOUNT
            ELSE 0
        END
    ) AS KALAN
FROM LG_102_01_SERILOTN S
INNER JOIN LG_102_01_SLTRANS T
    ON T.SLREF = S.LOGICALREF
INNER JOIN LG_102_01_STLINE SL
    ON SL.LOGICALREF = T.STTRANSREF
WHERE SL.CANCELLED = 0
GROUP BY S.CODE;
```

`IOCODE` yön eşlemesi gerçek işlem türlerinizle doğrulanmalıdır.

## 7. Ambar bazında seri/lot stok

```sql
SELECT
    S.CODE AS SERI_LOT_NO,
    SL.SOURCEINDEX AS AMBAR,
    SUM(
        CASE
            WHEN SL.IOCODE IN (1, 2) THEN T.AMOUNT
            WHEN SL.IOCODE IN (3, 4) THEN -T.AMOUNT
            ELSE 0
        END
    ) AS KALAN
FROM LG_102_01_SERILOTN S
INNER JOIN LG_102_01_SLTRANS T
    ON T.SLREF = S.LOGICALREF
INNER JOIN LG_102_01_STLINE SL
    ON SL.LOGICALREF = T.STTRANSREF
WHERE SL.CANCELLED = 0
GROUP BY S.CODE, SL.SOURCEINDEX;
```

Transfer işlemlerinde `DESTINDEX` ayrıca dikkate alınmalıdır.

## 8. Seri/lot ile stok satırı miktar farkı

Bir stok satırının miktarı ile bağlı seri/lot dağılımı eşleşiyor mu?

```sql
SELECT
    SL.LOGICALREF,
    SL.AMOUNT AS STLINE_MIKTAR,
    SUM(ISNULL(T.AMOUNT, 0)) AS SERILOT_MIKTAR,
    SL.AMOUNT - SUM(ISNULL(T.AMOUNT, 0)) AS FARK
FROM LG_102_01_STLINE SL
LEFT JOIN LG_102_01_SLTRANS T
    ON T.STTRANSREF = SL.LOGICALREF
WHERE SL.LOGICALREF = @StlineRef
GROUP BY SL.LOGICALREF, SL.AMOUNT;
```

Takipli bir malzemede fark oluşması detaylı incelenmelidir.

## 9. Yetim seri/lot hareketleri

Stok satırı bulunmayan seri/lot hareketlerini araştırmak için:

```sql
SELECT T.*
FROM LG_102_01_SLTRANS T
LEFT JOIN LG_102_01_STLINE SL
    ON SL.LOGICALREF = T.STTRANSREF
WHERE SL.LOGICALREF IS NULL;
```

Sonuç çıkması doğrudan bozuk veri hükmü vermek için yeterli değildir; dönemsel tablolar, arşiv, sürüm ve özel kayıt türleri kontrol edilmelidir.

## 10. Seri/lot numarası duplicate kontrolü

Seri takibinde aynı numaranın beklenmeyen tekrarlarını görmek için:

```sql
SELECT
    CODE,
    COUNT(*) AS ADET
FROM LG_102_01_SERILOTN
GROUP BY CODE
HAVING COUNT(*) > 1;
```

Lot takibinde aynı lot numarasının farklı malzemelerde kullanılmasına izin verilip verilmediği iş kuralına göre ayrıca değerlendirilmelidir.

## 11. Kaynak belgeye gitme

Seri/lot hareketi bulunduğunda kaynak belgeyi görmek için:

```text
SERILOTN
   ↓
SLTRANS
   ↓
STLINE
   ↓
STFICHEREF / INVOICEREF / ORDTRANSREF
```

Bu zincir seri/lot problemlerinde en güvenilir teşhis yollarından biridir.

## 12. Üretim senaryosu

Detaylı üretimde aynı lot numarası şu rollerde görülebilir:

- hammadde tüketimi,
- yarı mamul üretimi,
- mamul üretimi,
- fire,
- yan ürün,
- kalite blokajı.

Bu nedenle yalnızca malzeme + lot numarası değil, işlem türü ve üretim bağlantısı da kontrol edilmelidir.

## 13. Doğrudan DELETE / UPDATE neden risklidir?

Seri/lot hareket tabloları stok hareketleriyle sıkı ilişkilidir.

Tek bir seri/lot hareketini doğrudan silmek:

- stok miktarını,
- izlenebilirliği,
- üretim zincirini,
- kalite kayıtlarını,
- sevkiyat geçmişini

bozabilir.

Bu nedenle seri/lot düzeltmeleri mümkün olduğunca Logo iş nesneleri veya Logo’nun kendi operasyon akışları üzerinden yapılmalıdır.

## Sonuç

Seri/lot analizinde temel yaklaşım:

```text
Seri/Lot No
   ↓
Seri/Lot hareketi
   ↓
Stok satırı
   ↓
Belge
   ↓
Ambar / üretim / cari bağlamı
```

olmalıdır.

Tek bir tabloya bakarak sonuç üretmek yerine tüm hareket zinciri doğrulanmalıdır.
