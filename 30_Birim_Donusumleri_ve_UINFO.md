# 30 — Birim Dönüşümleri ve UINFO Alanları

## 1. Amaç

Logo ERP'de malzemeler birden fazla birim ile takip edilebilir. Ana birim, ikinci birim, satınalma birimi, satış birimi ve alternatif birimler arasında dönüşüm tanımları bulunabilir.

Bu bölüm, birim dönüşümlerinin tablo ve hareket mantığını; özellikle `UNITSETL`, `ITMUNITA`, `UINFO1` ve `UINFO2` alanlarını doğru yorumlama yaklaşımını açıklar.

## 2. Temel Yapı

Birim mimarisi genel olarak şu ilişki üzerinden okunur:

```text
ITEMS
  ↓
UNITSETREF
  ↓
UNITSETL

ITEMS
  ↓
ITMUNITA
  ↓
UNITSETL
```

Malzeme kartı bir birim setine bağlıdır. Birim seti içindeki birimler `UNITSETL` üzerinden tanımlanır. Malzemeye özel birim ilişkileri ise `ITMUNITA` üzerinden takip edilir.

## 3. Ana Birim

Ana birim, stok raporlamasının temel referansıdır.

Örnek sorgu:

```sql
SELECT
    I.LOGICALREF,
    I.CODE,
    I.NAME,
    U.CODE AS ANA_BIRIM
FROM LG_040_ITEMS I
JOIN LG_040_UNITSETL U
    ON U.UNITSETREF = I.UNITSETREF
   AND U.MAINUNIT = 1;
```

## 4. ITMUNITA

`ITMUNITA`, malzemenin birim dönüşümlerini malzeme bazında tanımlar.

Örnek:

```sql
SELECT
    IU.ITEMREF,
    IU.UNITLINEREF,
    IU.CONVFACT1,
    IU.CONVFACT2
FROM LG_040_ITMUNITA IU
WHERE IU.ITEMREF = @ItemRef;
```

## 5. CONVFACT1 / CONVFACT2

Birim dönüşüm oranlarını belirlemek için kullanılır.

Genel mantık:

```text
Birim Miktarı × dönüşüm oranı = ana birim karşılığı
```

Ancak dönüşümün yönü ve oran formülü ilgili Logo tanımına göre doğrulanmalıdır.

Saha geliştirmesinde sabit formül varsaymak yerine kart üzerindeki gerçek birim tanımı test edilmelidir.

## 6. STLINE Üzerindeki UINFO1 / UINFO2

Hareket satırlarında birim dönüşüm bilgisi çoğunlukla `UINFO1` ve `UINFO2` alanları üzerinden taşınır.

Örnek inceleme:

```sql
SELECT
    LOGICALREF,
    STOCKREF,
    UOMREF,
    AMOUNT,
    UINFO1,
    UINFO2,
    PRICE
FROM LG_040_01_STLINE
WHERE STOCKREF = @ItemRef;
```

Bu alanlar özellikle hareketin hangi dönüşüm oranı ile kaydedildiğini anlamada önemlidir.

## 7. Ana Birim Fiyatına Dönüşüm

Satınalma veya satış hareketlerinde fiyat farklı birim üzerinden girilmiş olabilir.

Bu nedenle iki fiyatı doğrudan karşılaştırmak hatalı olabilir.

Doğru yaklaşım:

1. Satır birimini belirle.
2. Satırdaki `UINFO1/UINFO2` değerlerini incele.
3. Malzemenin `ITMUNITA` tanımıyla karşılaştır.
4. Fiyatı ortak bir referans birimine normalize et.
5. Sonra kıyaslama yap.

## 8. Yanlış Birim Seçimi Nasıl Tespit Edilir?

Sahada önemli bir kontrol yöntemi satınalma birim fiyatıdır.

Örneğin malzemenin normal alış fiyatı ana birimde yaklaşık 10 USD ise ve aynı malzeme bir hareket satırında 1 USD görünüyorsa bu fark:

- yanlış birim seçimi,
- koli/adet dönüşümü,
- yanlış `UINFO` değeri,
- gerçek fiyat değişikliği

nedenlerinden biri olabilir.

Bu nedenle sadece miktar değil fiyat davranışı da birim hatası tespitinde kullanılmalıdır.

## 9. Birim Bazlı Envanter

Bir malzemenin ana ve ikinci birimde stok miktarı ayrı ayrı izlenecekse hareket satırları kendi kayıtlı birimleri üzerinden gruplanabilir.

Önemli prensip:

> Kullanıcının görmek istediği değer gerçek hareket birimi ise tüm miktarları zorla ana birime çevirmek doğru olmayabilir.

Örnek yaklaşım:

```sql
SELECT
    STOCKREF,
    UOMREF,
    SUM(AMOUNT) AS MIKTAR
FROM LG_040_01_STLINE
WHERE
    LINETYPE = 0
    AND CANCELLED = 0
GROUP BY
    STOCKREF,
    UOMREF;
```

Hareket yönü ayrıca dikkate alınmalıdır.

## 10. Birim Fiyat Karşılaştırma Kalıbı

Bir satınalma kontrol sisteminde şu alanlar birlikte tutulmalıdır:

```text
Malzeme
Hareket birimi
Ana birim
İkinci birim
UINFO1
UINFO2
Satır fiyatı
Normalize edilmiş ana birim fiyatı
Son alış ana birim fiyatı
Sapma yüzdesi
```

Bu yapı yanlış birim ve yanlış fiyat girişlerini yakalamada oldukça etkilidir.

## 11. Barkod ve Birim İlişkisi

Barkodlar da birim ile ilişkilidir. Aynı malzemenin adet, kutu ve koli barkodları farklı `UNITLINEREF` değerlerine bağlı olabilir.

Bu nedenle barkod kaydı eklerken yalnızca `ITEMREF` değil doğru birim referansı da kullanılmalıdır.

## 12. Best Practice

- Birim dönüşümünü sabit katsayı ile kodlama.
- `ITMUNITA` ve `UNITSETL` tanımlarını veri kaynağı olarak kullan.
- Hareket satırındaki `UINFO1/UINFO2` alanlarını ihmal etme.
- Fiyat karşılaştırmalarını ortak birime normalize et.
- Yanlış birim kontrolünde geçmiş fiyat davranışını kullan.
- Birim dönüşümünü değiştirmeden önce geçmiş hareketlere etkisini değerlendir.

## 13. Özet

Logo'da birim yönetimi yalnızca kart üzerindeki birim adından ibaret değildir. `UNITSETL`, `ITMUNITA`, `UOMREF`, `UINFO1` ve `UINFO2` birlikte değerlendirilmelidir. Özellikle fiyat ve stok analizlerinde birim dönüşüm mantığı doğru kurulmazsa sonuçlar ciddi şekilde sapabilir.
