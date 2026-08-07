# 42 — Üretim Maliyet Analizleri

## Amaç

Bu bölüm Logo ERP detaylı üretim kullanan yapılarda üretim maliyetlerini analiz ederken izlenmesi gereken veri zincirini açıklar.

Amaç yalnızca toplam maliyeti görmek değil; maliyetin hangi üretim emri, hangi operasyon, hangi sarf, hangi lot ve hangi ek maliyetlerden oluştuğunu izlenebilir hale getirmektir.

## 1. Maliyet neden yalnızca stok fiyatı değildir?

Bir üretim maliyeti aşağıdaki bileşenlerden oluşabilir:

- hammadde tüketimleri,
- yarı mamul tüketimleri,
- işçilik,
- makine zamanı,
- operasyon giderleri,
- genel üretim giderleri,
- fire,
- yan ürün etkileri,
- kur farkları,
- fiyat farkları,
- ek maliyet dağıtımları.

Bu nedenle yalnızca `STLINE.PRICE` alanını toplayarak üretim maliyeti hesaplamak eksik sonuç verebilir.

## 2. Temel analiz zinciri

Kavramsal olarak:

```text
Üretim Emri
   ↓
Operasyon / İş Emri
   ↓
Sarf Hareketleri
   ↓
Üretim Girişleri
   ↓
Seri/Lot Bağlantıları
   ↓
Maliyetlendirme Sonuçları
```

İlk hedef, üretim emrine ait bütün stok hareketlerini doğru şekilde ayırmaktır.

## 3. Üretim emri bağlantısı

Logo detaylı üretimde üretim emri ile stok hareketleri arasında çeşitli referans alanları bulunabilir.

Sürüm ve işlem türüne göre aşağıdaki alanlar önem kazanabilir:

- `PRODORDERREF`
- `SOURCEWSREF`
- `SOURCEPOLNREF`
- `FACTORYNR`
- `PROJECTREF`
- üretim fişi referansları.

Bir stok satırında `PRODORDERREF` bulunuyorsa ilk analiz şu şekilde yapılabilir:

```sql
SELECT
    SL.LOGICALREF,
    SL.PRODORDERREF,
    SL.STOCKREF,
    SL.AMOUNT,
    SL.PRICE,
    SL.TRCODE,
    SL.IOCODE,
    SL.DATE_
FROM LG_102_01_STLINE SL
WHERE SL.PRODORDERREF = @ProdOrderRef
ORDER BY SL.DATE_, SL.LOGICALREF;
```

## 4. Sarf ve üretim girişlerini ayırmak

Üretim maliyet analizinde hareketleri yönüne göre sınıflandırmak gerekir.

Kavramsal sınıflar:

```text
Sarf          → stoktan çıkış
Üretim girişi → stoğa giriş
Fire          → stoktan çıkış / kayıp
Yan ürün      → ayrı üretim girişi
```

Sınıflandırma yapılırken:

- `TRCODE`,
- `IOCODE`,
- `LINETYPE`,
- üretim referansları

birlikte değerlendirilmelidir.

## 5. Sarf maliyeti

Basit yaklaşım:

```text
Sarf maliyeti = tüketilen miktar × hareket maliyet fiyatı
```

Ancak hareket maliyet fiyatının hangi aşamada ve hangi maliyetlendirme yöntemiyle oluştuğu önemlidir.

Logo’da dönemsel maliyetlendirme çalıştırılmadan önce görülen fiyat ile maliyetlendirme sonrası oluşan değer aynı olmayabilir.

## 6. Üretim maliyetini hareket bazında inceleme

Örnek teşhis sorgusu:

```sql
SELECT
    SL.PRODORDERREF,
    SL.LOGICALREF,
    I.CODE AS MALZEME_KODU,
    I.NAME AS MALZEME_ADI,
    SL.TRCODE,
    SL.IOCODE,
    SL.AMOUNT,
    SL.PRICE,
    SL.AMOUNT * SL.PRICE AS HAREKET_TUTARI,
    SL.SOURCEINDEX,
    SL.DATE_
FROM LG_102_01_STLINE SL
LEFT JOIN LG_102_ITEMS I
    ON I.LOGICALREF = SL.STOCKREF
WHERE SL.PRODORDERREF = @ProdOrderRef
  AND SL.CANCELLED = 0
ORDER BY SL.DATE_, SL.LOGICALREF;
```

Bu sorgu operasyonel fiyatları gösterir; resmi maliyet sonucu olarak doğrudan kabul edilmemelidir.

## 7. Son alış fiyatı ile maliyet aynı şey değildir

Bir malzemenin son satınalma fiyatı, üretim maliyetinde kullanılan gerçek stok maliyetini temsil etmek zorunda değildir.

Örneğin:

```text
Son alış fiyatı = 10 USD
Stok maliyet yöntemi sonucu = 9.40 USD
```

olabilir.

Farkın nedenleri:

- geçmiş stok katmanları,
- ortalama maliyet,
- FIFO etkisi,
- kur farkları,
- ek maliyetler,
- iade hareketleri,
- dönemsel maliyet hesapları.

Bu nedenle son alış fiyatı ancak kontrol/reference metriği olarak kullanılmalıdır.

## 8. Birim dönüşümü etkisi

Satınalma birimi ile stok ana birimi farklıysa maliyet analizi mutlaka birim dönüşümünü dikkate almalıdır.

Örnek:

```text
Satınalma birimi = KOLİ
Ana birim        = ADET
1 KOLİ           = 12 ADET
Koli fiyatı      = 120 TL
Ana birim fiyatı = 10 TL/ADET
```

Birim dönüşümü yapılmadan fiyat karşılaştırması yapmak hatalı sonuç üretir.

Bu nedenle:

- `UOMREF`,
- `UINFO1`,
- `UINFO2`,
- malzeme birim seti

birlikte kontrol edilmelidir.

## 9. Kur etkisi

Dövizli alımlarda üretim maliyetine geçişte kur bilgisi önemlidir.

Fiyat karşılaştırmalarında şu ayrım yapılmalıdır:

```text
Belge döviz fiyatı
Yerel para birimi karşılığı
Maliyetlendirme kuru
Raporlama dövizi
```

Bir satınalma fiyatını USD’ye çevirmek için yanlış kur alanı kullanılması büyük maliyet sapmalarına yol açabilir.

## 10. Maliyet sapma analizi

Üretim emri bazında faydalı metrik:

```text
Planlanan maliyet
Gerçekleşen sarf maliyeti
Gerçekleşen operasyon maliyeti
Toplam gerçekleşen maliyet
Sapma
Sapma %
```

Formül:

```text
Sapma = Gerçekleşen - Planlanan
```

```text
Sapma % = (Gerçekleşen - Planlanan) / Planlanan × 100
```

## 11. Negatif stok ve maliyet

Negatif stok, maliyet hesaplarında önemli bozulmalara yol açabilir.

Örneğin bir sarf hareketi sırasında yeterli stok yoksa:

- sonraki giriş fiyatları geçmiş çıkışlara etki edebilir,
- ortalama maliyet beklenmeyen şekilde değişebilir,
- üretim emri maliyeti sonradan farklılaşabilir.

Bu nedenle maliyet anomalilerinde mutlaka hareket tarihindeki stok seviyesi kontrol edilmelidir.

## 12. Seri/lot maliyet analizi

Seri/lot takipli üretimde maliyet analizi daha detaylı yapılabilir.

Kavramsal zincir:

```text
Üretim Emri
   ↓
Sarf STLINE
   ↓
SLTRANS
   ↓
Hammadde Lotu
```

Bu sayede hangi mamul lotunda hangi hammadde lotlarının tüketildiği ve bu tüketimlerin maliyetleri analiz edilebilir.

## 13. Fire etkisi

Fire iki açıdan değerlendirilmelidir:

1. fiziksel miktar kaybı,
2. maliyetin kalan mamul miktarına dağılımı.

Örneğin:

```text
Toplam sarf maliyeti = 10.000 TL
Planlanan üretim     = 1.000 adet
Gerçek üretim        = 900 adet
```

Fire arttıkça birim mamul maliyeti yükselir.

Basit birim maliyet:

```text
10.000 / 900 = 11,11 TL/adet
```

## 14. Üretim emri maliyet kontrol sorgusu

Kavramsal özet:

```sql
SELECT
    SL.PRODORDERREF,
    SUM(
        CASE
            WHEN SL.IOCODE IN (3, 4)
                THEN SL.AMOUNT * SL.PRICE
            ELSE 0
        END
    ) AS SARF_TUTARI,
    SUM(
        CASE
            WHEN SL.IOCODE IN (1, 2)
                THEN SL.AMOUNT
            ELSE 0
        END
    ) AS URETILEN_MIKTAR
FROM LG_102_01_STLINE SL
WHERE SL.PRODORDERREF = @ProdOrderRef
  AND SL.CANCELLED = 0
GROUP BY SL.PRODORDERREF;
```

`IOCODE` eşlemeleri işlem türüne göre doğrulanmalıdır.

## 15. Maliyet anomalisinde kontrol sırası

```text
1. Üretim emri doğru mu?
2. Sarf satırları eksiksiz mi?
3. Üretim giriş miktarı doğru mu?
4. Birimler doğru mu?
5. Seri/lot miktarları eşleşiyor mu?
6. Negatif stok oluşmuş mu?
7. Satınalma fiyatları doğru mu?
8. Kur değerleri doğru mu?
9. Maliyetlendirme çalıştırılmış mı?
10. Ek maliyetler dağıtılmış mı?
```

## 16. Raporlama için önerilen kolonlar

Üretim maliyet raporunda en az:

```text
Üretim emri
Mamul kodu
Mamul adı
Üretim miktarı
Hammadde kodu
Sarf miktarı
Birim
Birim maliyet
Toplam sarf maliyeti
Lot no
Ambar
Operasyon
Planlanan maliyet
Gerçek maliyet
Sapma
```

bulunmalıdır.

## Sonuç

Üretim maliyeti tek bir fiyat alanından okunabilecek basit bir değer değildir.

Doğru yaklaşım:

```text
Üretim Emri
   ↓
Gerçek Sarflar
   ↓
Birim / Kur / Lot
   ↓
Maliyetlendirme
   ↓
Gerçek Mamul Maliyeti
```

şeklinde tüm zinciri analiz etmektir.
