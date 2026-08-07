# 38 — ORDTRANSREF, PREVLINEREF ve SOURCELINK İlişkileri

## Amaç

Bu bölüm Logo ERP’de sipariş, irsaliye, fatura ve stok hareketleri arasındaki satır bazlı bağlantıları anlamada kritik olan `ORDTRANSREF`, `PREVLINEREF` ve `SOURCELINK` alanlarını açıklar.

Bu alanlar özellikle şu soruların cevabını bulmak için kullanılır:

- Bu stok satırı hangi sipariş satırından geldi?
- Bu satır hangi önceki stok hareketine bağlı?
- İade hareketinin kaynak satırı hangisi?
- Sipariş → irsaliye → fatura zinciri nerede koptu?
- Bir satırın ambar bilgisi neden beklenmeyen kaynaktan geliyor?

> Not: Bu alanların davranışı işlem türüne ve Logo sürümüne göre değişebilir. Kesin ilişki çalışan örnek işlem üzerinden doğrulanmalıdır.

## 1. ORDTRANSREF

`ORDTRANSREF`, stok hareket satırının bağlı olduğu sipariş satırını işaret etmek için kullanılan temel referans alanlarından biridir.

Kavramsal ilişki:

```text
ORFLINE.LOGICALREF
        ↓
STLINE.ORDTRANSREF
```

Bir sipariş satırından sevkiyat veya mal kabul hareketi üretildiğinde, oluşan stok satırı üzerinde sipariş satırı bağlantısı izlenebilir.

Örnek:

```sql
SELECT
    SL.LOGICALREF AS STLINE_REF,
    SL.ORDTRANSREF,
    OL.LOGICALREF AS ORFLINE_REF,
    OL.AMOUNT AS SIPARIS_MIKTARI
FROM LG_102_01_STLINE SL
LEFT JOIN LG_102_01_ORFLINE OL
    ON OL.LOGICALREF = SL.ORDTRANSREF
WHERE SL.LOGICALREF = @StlineRef;
```

## 2. ORDFICHEREF ile farkı

`ORDTRANSREF` satır seviyesinde sipariş satırını temsil ederken, `ORDFICHEREF` sipariş fişi üst kaydına işaret eden bağlantılarda kullanılabilir.

Kavramsal yapı:

```text
ORFICHE.LOGICALREF
   ↓
ORFLINE.ORDFICHEREF
```

ve hareket zincirinde:

```text
ORFLINE.LOGICALREF
   ↓
STLINE.ORDTRANSREF
```

Bu nedenle üst fiş ve satır bağlantıları birbirine karıştırılmamalıdır.

## 3. PREVLINEREF

`PREVLINEREF`, bir stok satırının önceki veya kaynak stok hareket satırı ile bağlantısını izlemek için kullanılan alanlardan biridir.

Bu alan özellikle aşağıdaki senaryolarda önem kazanır:

- iade işlemleri,
- bağlantılı sevk hareketleri,
- aktarım veya dönüşüm zincirleri,
- kaynak hareketten türeyen yeni stok satırları.

Kavramsal ilişki:

```text
STLINE.LOGICALREF
       ↓
STLINE.PREVLINEREF
```

Yani aynı tablo içerisinde self-reference ilişkisi oluşabilir.

Örnek analiz:

```sql
SELECT
    SL.LOGICALREF,
    SL.PREVLINEREF,
    PREV.LOGICALREF AS KAYNAK_SATIR_REF,
    PREV.TRCODE AS KAYNAK_TRCODE,
    PREV.SOURCEINDEX AS KAYNAK_AMBAR
FROM LG_102_01_STLINE SL
LEFT JOIN LG_102_01_STLINE PREV
    ON PREV.LOGICALREF = SL.PREVLINEREF
WHERE SL.LOGICALREF = @StlineRef;
```

## 4. SOURCELINK

`SOURCELINK` bazı stok hareketlerinde kaynak satıra veya bağlantılı hareket kaydına referans taşıyan alanlardan biridir.

Özellikle özel senaryolarda `PREVLINEREF` ile birlikte incelenmesi faydalıdır.

Bir kaydın kaynak zincirini çözmek için şu alanları birlikte görmek genellikle en doğrusudur:

```text
LOGICALREF
ORDTRANSREF
ORDFICHEREF
PREVLINEREF
SOURCELINK
STFICHEREF
INVOICEREF
```

## 5. İade hareketi örneği

Bir iade hareketinde yalnızca `TRCODE` ve ambar bilgisine bakmak yeterli olmayabilir.

Kaynak satırın ambarını görmek için:

```sql
SELECT
    SL.LOGICALREF,
    SL.TRCODE,
    SL.SOURCEINDEX,
    SL.DESTINDEX,
    SL.PREVLINEREF,
    SL.SOURCELINK,
    SRC.LOGICALREF AS KAYNAK_REF,
    SRC.SOURCEINDEX AS KAYNAK_SOURCEINDEX,
    SRC.DESTINDEX AS KAYNAK_DESTINDEX
FROM LG_102_01_STLINE SL
LEFT JOIN LG_102_01_STLINE SRC
    ON SRC.LOGICALREF = CASE
        WHEN SL.PREVLINEREF <> 0 THEN SL.PREVLINEREF
        ELSE SL.SOURCELINK
    END
WHERE SL.LOGICALREF = @StlineRef;
```

Bu sorgu genel teşhis amacı taşır. `SOURCELINK` ve `PREVLINEREF` değerlerinin hangi işlem türünde öncelikli olduğu çalışan veri üzerinden doğrulanmalıdır.

## 6. Sipariş → irsaliye → fatura zinciri

Tipik satır ilişkisi:

```text
ORFICHE
   ↓
ORFLINE
   ↓ ORDTRANSREF
STLINE
   ↓ INVOICEREF / STFICHEREF
INVOICE / STFICHE
```

Ancak her senaryoda zincir birebir değildir.

Örneğin:

- sipariş parçalı sevk edilebilir,
- bir sipariş satırı birden fazla irsaliye satırına dönüşebilir,
- irsaliye sonradan faturalanabilir,
- fatura doğrudan siparişten oluşturulabilir,
- iade hareketleri ayrı kaynak bağlantıları kullanabilir.

Bu nedenle `ORDTRANSREF` ilişkisinin kardinalitesi pratikte bire-çok olabilir.

## 7. Satır miktarı kontrolü

Siparişten ne kadar sevk edildiğini görmek için:

```sql
SELECT
    OL.LOGICALREF AS SIPARIS_SATIR_REF,
    OL.AMOUNT AS SIPARIS_MIKTARI,
    SUM(ISNULL(SL.AMOUNT, 0)) AS HAREKET_MIKTARI
FROM LG_102_01_ORFLINE OL
LEFT JOIN LG_102_01_STLINE SL
    ON SL.ORDTRANSREF = OL.LOGICALREF
   AND SL.CANCELLED = 0
WHERE OL.LOGICALREF = @OrderLineRef
GROUP BY OL.LOGICALREF, OL.AMOUNT;
```

İade ve işlem yönleri varsa miktar işareti ayrıca işlem türüne göre ele alınmalıdır.

## 8. Kaynak ambar hatası teşhisi

Bir satırda beklenmeyen `SOURCEINDEX` görülüyorsa şu sıra izlenmelidir:

```text
1. STLINE.LOGICALREF
2. STFICHEREF
3. TRCODE / IOCODE
4. SOURCEINDEX / DESTINDEX
5. PREVLINEREF
6. SOURCELINK
7. ORDTRANSREF
8. Kaynak satırın ambarı
```

Bu yaklaşım özellikle iade ve bağlantılı stok hareketlerinde önemlidir.

## 9. Yetim bağlantı kontrolü

Örnek:

```sql
SELECT SL.*
FROM LG_102_01_STLINE SL
LEFT JOIN LG_102_01_ORFLINE OL
    ON OL.LOGICALREF = SL.ORDTRANSREF
WHERE SL.ORDTRANSREF <> 0
  AND OL.LOGICALREF IS NULL;
```

Bu sonuçlar doğrudan veri bozukluğu anlamına gelmez; dönemler arası veya özel işlem davranışları araştırılmalıdır. Ancak entegrasyon hatası incelemesinde güçlü bir sinyaldir.

## 10. Temel kural

Belge ilişkilerinde yalnızca üst fişe bakma.

Logo ERP’de gerçek operasyonel bağ çoğu zaman satır seviyesindedir:

```text
Fiş → Satır → Kaynak Satır → Kaynak Belge
```

Bu nedenle `ORDTRANSREF`, `PREVLINEREF` ve `SOURCELINK` alanları özellikle stok ve sipariş entegrasyonlarında birlikte değerlendirilmelidir.
