# 28 — LINETYPE Referansı

## 1. Amaç

Logo ERP hareket tablolarında `LINETYPE`, satırın işlevini belirleyen en kritik alanlardan biridir. Özellikle `STLINE` üzerinde rapor, kontrol ve entegrasyon geliştirirken yalnızca `TRCODE` üzerinden filtre yapmak çoğu zaman yeterli değildir; satırın gerçek niteliği `LINETYPE` ile birlikte değerlendirilmelidir.

Bu bölüm, `LINETYPE` alanını ezberlenecek sabit bir sayı listesi olarak değil, hareket satırını doğru yorumlamak için kullanılan bir sınıflandırma alanı olarak ele alır.

> Not: Kesin kod anlamları kullanılan Logo ürün/sürümüne göre doğrulanmalıdır. Aşağıdaki yaklaşım saha kullanımı için güvenli yorumlama prensiplerini anlatır.

## 2. Neden Önemlidir?

Bir stok fişi veya faturada aynı `STLINE` tablosunda farklı nitelikte satırlar bulunabilir:

- Malzeme satırı
- Promosyon satırı
- İndirim satırı
- Masraf satırı
- Hizmet satırı
- Depozito satırı
- Karma koli / karma paket ilişkili satırlar
- Alt detay veya bağlı hareket satırları

Bu nedenle örneğin stok miktarı hesaplanırken tüm `STLINE` satırlarının toplanması hatalı sonuç üretir.

## 3. En Yaygın Kullanım: Malzeme Satırlarını Ayırmak

Sahada en sık kullanılan filtrelerden biri:

```sql
WHERE LINETYPE = 0
```

Bu filtre çoğunlukla standart malzeme satırlarını ayırmak için kullanılır.

Örnek:

```sql
SELECT
    LOGICALREF,
    STOCKREF,
    AMOUNT,
    PRICE,
    VAT,
    LINETYPE
FROM LG_040_01_STLINE
WHERE LINETYPE = 0;
```

Özellikle stok miktarı, KDV, birim fiyat veya malzeme bazlı hareket analizi yapılırken ilk kontrol edilmesi gereken alanlardan biridir.

## 4. LINETYPE Tek Başına Yeterli Değildir

Doğru hareket yorumu genellikle aşağıdaki alanların birlikte değerlendirilmesini gerektirir:

```text
TRCODE
LINETYPE
IOCODE
SOURCEINDEX
DESTINDEX
INVOICEREF
STFICHEREF
ORDTRANSREF
GLOBTRANS
CANCELLED
```

Örnek olarak `LINETYPE = 0` bir malzeme satırı olduğunu gösterebilir; ancak hareketin giriş mi çıkış mı olduğunu `IOCODE`, belge türünü ise `TRCODE` belirler.

## 5. Raporlama İçin Güvenli Kalıp

```sql
SELECT
    SL.LOGICALREF,
    SL.TRCODE,
    SL.LINETYPE,
    SL.IOCODE,
    SL.STOCKREF,
    SL.AMOUNT,
    SL.PRICE,
    SL.SOURCEINDEX,
    SL.DESTINDEX
FROM LG_040_01_STLINE SL
WHERE
    SL.CANCELLED = 0
    AND SL.LINETYPE = 0;
```

Bu kalıp daha sonra ihtiyaca göre `TRCODE` ve `IOCODE` ile daraltılmalıdır.

## 6. KDV Kontrollerinde LINETYPE

KDV muafiyet kontrollerinde yalnızca gerçek malzeme satırlarının değerlendirilmesi gerekiyorsa:

```sql
SELECT
    VAT,
    VATEXCEPTCODE,
    VATEXCEPTREASON
FROM LG_040_01_STLINE
WHERE
    VAT = 0
    AND LINETYPE = 0;
```

Bu yaklaşım indirim, masraf veya diğer yardımcı satırların yanlışlıkla kontrol kapsamına girmesini engeller.

## 7. Fiyat Analizlerinde LINETYPE

Son alış fiyatı gibi analizlerde `LINETYPE = 0` filtresi kritik önemdedir.

Örnek:

```sql
SELECT TOP 1
    SL.PRICE,
    SL.DATE_,
    SL.INVOICEREF
FROM LG_040_01_STLINE SL
WHERE
    SL.STOCKREF = @StockRef
    AND SL.LINETYPE = 0
    AND SL.CANCELLED = 0
ORDER BY SL.DATE_ DESC, SL.LOGICALREF DESC;
```

Aksi durumda masraf veya indirim satırı yanlışlıkla son alış fiyatı olarak değerlendirilebilir.

## 8. Envanter Hesaplarında LINETYPE

Envanter sorgularında tipik hata:

```sql
SUM(AMOUNT)
```

ifadesini tüm satırlara uygulamaktır.

Daha güvenli yaklaşım:

```sql
SUM(
    CASE
        WHEN LINETYPE = 0 THEN AMOUNT
        ELSE 0
    END
)
```

Ancak gerçek stok yönü ayrıca `IOCODE` veya belge mantığı ile değerlendirilmelidir.

## 9. LINETYPE ve GLOBTRANS

`GLOBTRANS` satırın genel/toplam etkili bir işlem olup olmadığını anlamada yardımcı olabilir. Fiyat, indirim ve masraf analizlerinde `LINETYPE` ile birlikte değerlendirilmelidir.

Bu iki alanı birlikte kontrol etmeden genel indirim/masraf satırlarını malzeme bazlı satır gibi yorumlamak mümkündür.

## 10. Debug Yaklaşımı

Bir belge satırı beklenmedik davranıyorsa şu sorgu ile tüm sınıflandırma alanları birlikte görülmelidir:

```sql
SELECT
    LOGICALREF,
    STFICHEREF,
    INVOICEREF,
    TRCODE,
    LINETYPE,
    IOCODE,
    GLOBTRANS,
    STOCKREF,
    AMOUNT,
    PRICE,
    SOURCEINDEX,
    DESTINDEX,
    ORDTRANSREF
FROM LG_040_01_STLINE
WHERE STFICHEREF = @StFicheRef
ORDER BY LOGICALREF;
```

Bu sorgu özellikle:

- Ambar hataları
- Sipariş bağlantı sorunları
- Yanlış fiyat analizi
- İade hareketi
- KDV kontrolü
- Stok tutarsızlığı

analizlerinde kullanılabilir.

## 11. Best Practice

- `STLINE` sorgularında `LINETYPE` alanını bilinçli şekilde kontrol et.
- Stok analizi yapıyorsan malzeme satırı dışındaki satırları filtrele.
- Fiyat ve KDV analizinde `LINETYPE = 0` ihtiyacını özellikle değerlendir.
- `LINETYPE` yorumunu `TRCODE`, `IOCODE` ve belge bağlantılarıyla birlikte yap.
- Sürüm veya modül farkı olabilecek kodları canlı ortam verisi üzerinden doğrula.

## 12. Özet

`LINETYPE`, Logo hareket satırlarının semantiğini belirleyen temel alanlardan biridir. Doğru kullanılmadığında stok, fiyat, KDV ve belge ilişkisi analizleri kolayca yanlış sonuç verir. Profesyonel Logo SQL geliştirmede `LINETYPE` hiçbir zaman göz ardı edilmemelidir.
