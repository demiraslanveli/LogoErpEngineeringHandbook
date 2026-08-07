# 22 — Logo Tablo Sözlüğü

## 1. Amaç

Logo ERP veritabanını analiz ederken tablo isimlerini ezberlemekten daha önemli olan konu, tabloların hangi iş ailesine ait olduğunu ve birbirleriyle nasıl bağlandığını bilmektir.

Bu bölüm en sık kullanılan Logo tablo ailelerini geliştirici ve veri analisti gözüyle sınıflandırır.

> Not: Tablo adları ürün, sürüm, firma ve dönem yapısına göre değişebilir. Buradaki kalıplar genel Logo ERP mimarisini anlamak için kullanılmalıdır.

---

## 2. Firma ve Dönem Tablo Yapısı

Logo tablolarında sık görülen iki temel kalıp vardır.

Firma bazlı:

```text
LG_XXX_TABLOADI
```

Firma + dönem bazlı:

```text
LG_XXX_YY_TABLOADI
```

Örnek:

```text
LG_040_ITEMS
LG_040_01_INVOICE
LG_040_01_STLINE
```

Burada:

```text
040 = Firma numarası
01  = Dönem numarası
```

---

## 3. Malzeme Kartları

### LG_XXX_ITEMS

Malzeme kartlarının ana tablosudur.

Sık kullanılan alanlar:

```text
LOGICALREF
CODE
NAME
STGRPCODE
SPECODE
SPECODE2
SPECODE3
SPECODE4
SPECODE5
UNITSETREF
ACTIVE
CARDTYPE
```

Örnek:

```sql
SELECT
    LOGICALREF,
    CODE,
    NAME,
    STGRPCODE,
    UNITSETREF
FROM LG_040_ITEMS;
```

---

## 4. Birim Setleri

### LG_XXX_UNITSETF

Birim setinin üst bilgisidir.

### LG_XXX_UNITSETL

Birim seti satırlarını içerir.

Sık kullanılan alanlar:

```text
LOGICALREF
UNITSETREF
CODE
NAME
MAINUNIT
LINENR
```

### LG_XXX_ITMUNITA

Malzeme ile birim seti satırları arasındaki ilişkiyi içerir.

Önemli alanlar:

```text
ITEMREF
UNITLINEREF
CONVFACT1
CONVFACT2
```

Birim dönüşümlerini analiz ederken `ITMUNITA` kritik tablolardan biridir.

---

## 5. Barkodlar

### LG_XXX_UNITBARCODE

Malzeme/birim bazlı barkod bilgilerini içerir.

Sık kullanılan alanlar:

```text
ITEMREF
UNITLINEREF
BARCODE
LINENR
```

Barkod ekleme işlemlerinde aynı barkodun başka bir malzemede kullanılıp kullanılmadığı kontrol edilmelidir.

---

## 6. Cari Hesaplar

### LG_XXX_CLCARD

Cari hesap kartlarının ana tablosudur.

Sık kullanılan alanlar:

```text
LOGICALREF
CODE
DEFINITION_
SPECODE
CYPHCODE
ACTIVE
TAXNR
TCKNO
```

---

## 7. Cari Hareketler

### LG_XXX_YY_CLFLINE

Cari hesap hareketlerinin temel tablolarından biridir.

Sık kullanılan alanlar:

```text
LOGICALREF
CLIENTREF
DATE_
MODULENR
TRCODE
SOURCEFREF
SIGN
AMOUNT
TRNET
CANCELLED
```

Yaşlandırma, borç takip ve fatura-cari bağlantılarında sık kullanılır.

---

## 8. Siparişler

### LG_XXX_YY_ORFICHE

Sipariş üst bilgisi.

### LG_XXX_YY_ORFLINE

Sipariş satırları.

Önemli bağlantılar:

```text
ORFICHE.LOGICALREF
ORFLINE.ORDFICHEREF
ORFLINE.LOGICALREF
STLINE.ORDTRANSREF
```

Siparişten irsaliye/fatura oluşumunu analiz ederken `ORDTRANSREF` önemli bir bağlantı alanıdır.

---

## 9. Stok Fişleri

### LG_XXX_YY_STFICHE

Stok fişlerinin üst bilgisidir.

Sık kullanılan alanlar:

```text
LOGICALREF
FICHENO
TRCODE
DATE_
SOURCEINDEX
DESTINDEX
INVOICEREF
CANCELLED
```

### LG_XXX_YY_STLINE

Stok hareket satırlarını içerir.

Sık kullanılan alanlar:

```text
LOGICALREF
STOCKREF
STFICHEREF
INVOICEREF
ORDTRANSREF
TRCODE
DATE_
AMOUNT
PRICE
TOTAL
VAT
LINETYPE
SOURCEINDEX
DESTINDEX
IOCODE
UINFO1
UINFO2
```

Logo stok analizlerinin büyük bölümü bu tablo üzerinden yapılır.

---

## 10. Faturalar

### LG_XXX_YY_INVOICE

Fatura üst bilgisidir.

Sık kullanılan alanlar:

```text
LOGICALREF
FICHENO
TRCODE
DATE_
CLIENTREF
NETTOTAL
TOTALVAT
GRNETTOTAL
ACCOUNTREF
CENTERREF
CANCELLED
```

Fatura satırları çoğunlukla `STLINE` üzerinden izlenir.

Bağlantı:

```text
STLINE.INVOICEREF = INVOICE.LOGICALREF
```

---

## 11. Muhasebe Fişleri

### LG_XXX_YY_EMFICHE

Muhasebe fiş üst bilgisidir.

### LG_XXX_YY_EMFLINE

Muhasebe fiş satırlarıdır.

Sık kullanılan alanlar:

```text
EMFICHE.LOGICALREF
EMFICHE.DATE_
EMFICHE.FICHENO
EMFLINE.ACCOUNTREF
EMFLINE.DEBIT
EMFLINE.CREDIT
```

Fatura veya diğer operasyonların muhasebeleşmiş olup olmadığını kontrol ederken bağlantı alanları ve kaynak referanslar ayrıca incelenmelidir.

---

## 12. Muhasebe Hesap Planı

### LG_XXX_EMUHACC

Muhasebe hesap kartlarını içerir.

Sık kullanılan alanlar:

```text
LOGICALREF
CODE
DEFINITION_
ACTIVE
```

---

## 13. Seri/Lot Tabloları

Logo sürüm ve fonksiyonlarına göre seri/lot hareketleri farklı yardımcı tablo aileleri üzerinden tutulabilir.

Analiz sırasında yalnızca `STLINE` miktarına bakmak yeterli değildir.

Kontrol edilmesi gereken mantık:

```text
Stok hareket satırı
    ↓
Seri/Lot dağıtımı
    ↓
Lot/seri kartı
    ↓
Stok yeri / ambar
```

Seri/lot izlenebilirliğinde referans zincirinin kopmaması esastır.

---

## 14. Üretim Emirleri

### LG_XXX_YY_PRODORD

Üretim emirleriyle ilişkili temel tablolardan biridir.

Sık karşılaşılan alanlar:

```text
LOGICALREF
FICHENO
ITEMREF
PLNAMOUNT
ACTAMOUNT
DATE_
STATUS
```

Detaylı üretim yapısında yalnızca bu tablo yeterli değildir; operasyon, iş emri, malzeme sarfı ve üretim hareketleri birlikte değerlendirilmelidir.

---

## 15. Proje Kartları

Logo'da proje referansı birçok işlem satırında bulunabilir.

Örnek alan:

```text
PROJECTREF
```

Bu alan özellikle:

- stok hareketleri,
- muhasebe hareketleri,
- proje maliyetleri,
- lojistik entegrasyonları

için önemlidir.

---

## 16. Temel Referans Alanları

Logo tablolarını analiz ederken en çok karşılaşılan referans alanları:

```text
LOGICALREF
STOCKREF
CLIENTREF
STFICHEREF
INVOICEREF
ORDFICHEREF
ORDTRANSREF
ACCOUNTREF
CENTERREF
PROJECTREF
UNITREF
UNITSETREF
```

Bu alanların büyük bölümü başka bir tablodaki `LOGICALREF` değerini işaret eder.

---

## 17. LOGICALREF

`LOGICALREF`, Logo veritabanındaki kayıtların temel teknik kimliğidir.

İş anlamı taşıyan `CODE` veya `FICHENO` alanlarından farklıdır.

Örnek:

```text
CODE       = 150.001
LOGICALREF = 43338
```

Entegrasyonlarda ilişki kurarken mümkün olduğunca `LOGICALREF` kullanılmalıdır.

---

## 18. LINETYPE

`STLINE` gibi satır tablolarında `LINETYPE`, satırın gerçek türünü ayırmak için önemlidir.

Malzeme, hizmet, indirim, masraf gibi farklı satır tipleri aynı tabloda tutulabilir.

Bu nedenle stok raporlarında çoğu zaman:

```sql
WHERE LINETYPE = 0
```

gibi filtreler görülür.

Ancak kullanılan iş sürecine göre diğer satır tipleri de değerlendirilmelidir.

---

## 19. CANCELLED

İptal edilmiş kayıtları rapora dahil etmemek için `CANCELLED` alanı kontrol edilmelidir.

Örnek:

```sql
WHERE CANCELLED = 0
```

Bu kontrol özellikle:

- satış raporları,
- stok raporları,
- cari hareketler,
- maliyet analizleri

için önemlidir.

---

## 20. Tablo İlişki Haritası

Basitleştirilmiş satış akışı:

```text
CLCARD
  ↑
  │ CLIENTREF
INVOICE
  │
  ├───────────────┐
  ↓               ↓
STFICHE         CLFLINE
  ↓
STLINE
  ↓
ITEMS
```

Sipariş bağlantısı varsa:

```text
ORFICHE
   ↓
ORFLINE
   ↓ ORDTRANSREF
STLINE
```

Muhasebe bağlantısı:

```text
INVOICE / CLFLINE
       ↓
    EMFICHE
       ↓
    EMFLINE
       ↓
    EMUHACC
```

---

## 21. SQL Yazarken Temel Kontroller

Her sorguda şu sorular sorulmalıdır:

1. Doğru firma mı?
2. Doğru dönem mi?
3. `CANCELLED = 0` gerekli mi?
4. `LINETYPE` filtresi gerekli mi?
5. `TRCODE` filtresi gerekli mi?
6. Referans bağlantısı doğru mu?
7. Bir kayıt join nedeniyle çoğalıyor mu?
8. `LEFT JOIN` mi `INNER JOIN` mi kullanılmalı?
9. Tarih filtresi indeks kullanabiliyor mu?
10. Sonuç finansal veya stok bakiyesini gerçekten temsil ediyor mu?

---

## 22. Yazma İşlemleri İçin Uyarı

Logo tablolarının yapısını bilmek, bu tablolara doğrudan `INSERT`, `UPDATE` veya `DELETE` yapılmasının güvenli olduğu anlamına gelmez.

Resmi kart ve fiş işlemlerinde öncelik:

```text
Logo Objects / IData
```

olmalıdır.

SQL yazma işlemleri yalnızca kontrollü, iyi analiz edilmiş ve veri bütünlüğü etkileri bilinen özel senaryolarda kullanılmalıdır.

---

## 23. Sonuç

Logo tablo sözlüğü, SQL sorgusu yazmak için bir başlangıçtır; asıl uzmanlık tablolar arasındaki iş ilişkisini anlamaktır.

Temel prensip:

> Tabloyu değil, işlemin Logo içindeki yaşam döngüsünü takip et.
