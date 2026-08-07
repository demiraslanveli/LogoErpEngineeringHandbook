# 29 — IOCODE ve Ambar Yönü

## 1. Amaç

`IOCODE`, Logo ERP stok hareketlerinin giriş/çıkış yönünü anlamada kullanılan temel alanlardan biridir. Özellikle `STLINE` tablosu üzerinde envanter, ambar hareketi, transfer ve üretim analizi yapılırken `TRCODE` tek başına yeterli değildir.

Bu bölümde `IOCODE`, `SOURCEINDEX` ve `DESTINDEX` alanlarının birlikte nasıl yorumlanması gerektiği ele alınır.

## 2. IOCODE Ne İşe Yarar?

Bir stok hareketi için şu soruya cevap verir:

> Bu satır stok açısından giriş mi, çıkış mı, yoksa ambarlar arası ilişki içeren özel bir hareket mi?

Bu nedenle stok miktarı hesaplarken sadece `AMOUNT` toplamak doğru değildir.

## 3. Güvenli Yorumlama Prensibi

Saha analizlerinde şu alanlar birlikte incelenmelidir:

```text
TRCODE
IOCODE
SOURCEINDEX
DESTINDEX
STFICHEREF
INVOICEREF
STOCKREF
AMOUNT
```

`IOCODE` değerlerinin kesin anlamı kullanılan sürüm ve hareket tipi ile birlikte doğrulanmalıdır.

## 4. SOURCEINDEX

`SOURCEINDEX`, hareketin kaynak ambarını veya hareketin gerçekleştiği ana ambarı temsil eden kritik alanlardan biridir.

Özellikle:

- Malzeme fişleri
- Satınalma irsaliyeleri
- Satış irsaliyeleri
- Üretim sarfları
- Ambar transferleri

üzerinde sık kullanılır.

Örnek:

```sql
SELECT
    STOCKREF,
    AMOUNT,
    IOCODE,
    SOURCEINDEX
FROM LG_040_01_STLINE
WHERE SOURCEINDEX = 0;
```

## 5. DESTINDEX

`DESTINDEX`, özellikle ambar transferi veya giriş/çıkışın iki farklı ambara bağlandığı hareketlerde hedef ambar bilgisini taşır.

Örnek analiz:

```sql
SELECT
    LOGICALREF,
    TRCODE,
    IOCODE,
    SOURCEINDEX,
    DESTINDEX,
    STOCKREF,
    AMOUNT
FROM LG_040_01_STLINE
WHERE SOURCEINDEX <> DESTINDEX;
```

## 6. Başlık ve Satır Ambarı

Logo'da belge başlığındaki ambar bilgisi ile satırdaki ambar bilgisinin aynı olması beklenen senaryolar vardır. Ancak özel hareketlerde satır bazlı farklılık oluşabilir.

Saha hatalarında aşağıdaki kontrol çok değerlidir:

```sql
SELECT
    F.LOGICALREF      AS FICHE_REF,
    F.FICHENO,
    F.TRCODE,
    F.SOURCEINDEX     AS BASLIK_AMBARI,
    L.LOGICALREF      AS SATIR_REF,
    L.SOURCEINDEX     AS SATIR_AMBARI,
    L.DESTINDEX,
    L.STOCKREF
FROM LG_040_01_STFICHE F
JOIN LG_040_01_STLINE L
    ON L.STFICHEREF = F.LOGICALREF
WHERE ISNULL(F.SOURCEINDEX, -1) <> ISNULL(L.SOURCEINDEX, -1);
```

Bu kontrol özellikle entegrasyon veya trigger sonrası oluşan ambar uyumsuzluklarını bulmak için kullanılabilir.

## 7. Ambar Değişikliği Riskleri

Bir hareketin sadece `SOURCEINDEX` alanını SQL ile değiştirmek risklidir. Çünkü bağlı hareketlerde aşağıdaki alanlar veya kayıtlar etkilenebilir:

- `STFICHE`
- `STLINE`
- Seri/lot hareketleri
- Stok toplamları
- Sipariş bağlantıları
- Üretim ilişkileri
- Maliyet kayıtları

Bu nedenle resmi belge değişiklikleri mümkün olduğunca Logo Objects üzerinden yapılmalıdır.

## 8. Transferlerde Kontrol

Ambar transferlerinde kaynak ve hedef ambar birlikte değerlendirilmelidir.

Kontrol kalıbı:

```sql
SELECT
    STFICHEREF,
    STOCKREF,
    SOURCEINDEX,
    DESTINDEX,
    IOCODE,
    AMOUNT
FROM LG_040_01_STLINE
WHERE
    SOURCEINDEX IS NOT NULL
    AND DESTINDEX IS NOT NULL
    AND SOURCEINDEX <> DESTINDEX;
```

## 9. Envanter Hesabı

Envanter hesabında en kritik konu hareket yönüdür.

Yanlış:

```sql
SUM(AMOUNT)
```

Doğru yaklaşım:

```sql
SUM(
    CASE
        WHEN <giris_kosulu> THEN AMOUNT
        WHEN <cikis_kosulu> THEN -AMOUNT
        ELSE 0
    END
)
```

Buradaki giriş/çıkış koşulu `IOCODE`, `TRCODE` ve hareket tipine göre doğrulanmalıdır.

## 10. Debug Sorgusu

Bir hareket yanlış ambara gidiyorsa:

```sql
SELECT
    L.LOGICALREF,
    L.TRCODE,
    L.IOCODE,
    L.SOURCEINDEX,
    L.DESTINDEX,
    L.STFICHEREF,
    L.INVOICEREF,
    L.ORDTRANSREF,
    L.PREVLINEREF,
    L.STOCKREF,
    L.AMOUNT
FROM LG_040_01_STLINE L
WHERE L.LOGICALREF = @LineRef;
```

Ardından bağlı fiş başlığı ve varsa kaynak hareket incelenmelidir.

## 11. Best Practice

- Ambar analizi yaparken sadece `SOURCEINDEX` kontrol etme; `DESTINDEX` ve `IOCODE` da incelenmelidir.
- Başlık ve satır ambarlarını karşılaştır.
- Transfer senaryolarında kaynak/hedef yönünü ayrı ayrı doğrula.
- Trigger ile ambar değiştiren sistemlerde audit log tut.
- Seri/lot kullanılan malzemelerde ambar değişikliğini sadece `STLINE` üzerinden yapma.
- Üretim hareketlerinde ambar değişikliğinin maliyet ve izlenebilirlik etkisini ayrıca kontrol et.

## 12. Özet

`IOCODE`, `SOURCEINDEX` ve `DESTINDEX`, Logo stok hareketinin fiziksel yönünü açıklayan temel alanlardır. Bu üç alan doğru yorumlanmadığında envanter, transfer ve üretim raporları hatalı sonuç üretebilir.
