# 24 — IQuery Gerçek Sorgu Kalıpları

## 1. Amaç

`IQuery`, Logo Objects üzerinden SQL sorguları çalıştırmak için kullanılan arayüzlerden biridir. Özellikle okuma, kontrol, referans bulma ve entegrasyon ön doğrulama işlemlerinde kullanışlıdır.

Temel prensip:

> `IQuery` çoğunlukla okumak ve kontrol etmek için; `IData` ise Logo iş nesnesini değiştirmek için kullanılmalıdır.

---

## 2. Temel Kullanım

Genel akış:

```text
CreateQuery
    ↓
SetSqlText
    ↓
ExecuteDirect
    ↓
First / Next
    ↓
GetFieldValue
    ↓
Clear
```

Örnek script yaklaşımı:

```text
CreateQuery(qry)
qry.SetSqlText(SqlText)
qry.ExecuteDirect()

if qry.First() then
    qry.GetFieldValue(1, 1, Value)
end if

qry.Clear()
```

---

## 3. Tek Değer Okuma

Örnek: stok fişinin `TRCODE` değerini bulmak.

```text
SqlText = "SELECT TRCODE " + _
          "FROM LG_" + FrmNo + "_01_STFICHE " + _
          "WHERE LOGICALREF = " + LogrefStr

CreateQuery(qry)
qry.SetSqlText(SqlText)
qry.ExecuteDirect()

if qry.First() then
    qry.GetFieldValue(1, 1, TrCode)
end if

qry.Clear()
```

Bu tip kullanım popup, form script veya kullanıcı aksiyonu sırasında hızlı kontrol için uygundur.

---

## 4. Firma Numarasını Dinamik Oluşturma

Logo form scriptlerinde firma numarası çoğu zaman uygulama üzerinden alınır.

Örnek mantık:

```text
comId = Application.CompanyId
str(comId, strID)
yeniID = "00" + strID
FrmNo = yeniID.SubStr(yeniID.size - 3, yeniID.size)
```

Amaç firma numarasını üç haneli tablo formatına çevirmektir.

Örnek:

```text
40  → 040
102 → 102
```

Sonra tablo adı oluşturulur:

```text
LG_040_01_STFICHE
```

---

## 5. LOGICALREF Üzerinden Kayıt Bulma

En güvenli sorgu kalıplarından biri `LOGICALREF` üzerinden tek kayıt okumaktır.

```sql
SELECT
    LOGICALREF,
    FICHENO,
    TRCODE,
    DATE_
FROM LG_040_01_STFICHE
WHERE LOGICALREF = @LogicalRef;
```

Bu yöntem `FICHENO` gibi iş alanlarına göre daha güvenilir teknik referans sağlar.

---

## 6. Malzeme Kodundan LOGICALREF Bulma

```sql
SELECT
    LOGICALREF
FROM LG_040_ITEMS
WHERE CODE = '150.001';
```

Ardından bulunan referans `IData.Read(...)` veya başka sorgular için kullanılabilir.

---

## 7. Cari Koddan Referans Bulma

```sql
SELECT
    LOGICALREF,
    CODE,
    DEFINITION_
FROM LG_040_CLCARD
WHERE CODE = 'CARI.001';
```

Entegrasyonda cari kod dış sistemden geliyorsa önce varlık kontrolü yapılmalıdır.

---

## 8. Duplicate Barkod Kontrolü

```sql
SELECT
    ITEMREF,
    UNITLINEREF,
    BARCODE
FROM LG_040_UNITBARCODE
WHERE BARCODE = @Barcode;
```

Amaç yeni barkod kaydından önce çakışmayı önlemektir.

---

## 9. Sipariş Satırı Bağlantısını Bulma

```sql
SELECT
    S.LOGICALREF,
    S.STOCKREF,
    S.ORDTRANSREF,
    O.ORDFICHEREF
FROM LG_040_01_STLINE S
LEFT JOIN LG_040_01_ORFLINE O
    ON O.LOGICALREF = S.ORDTRANSREF
WHERE S.LOGICALREF = @StLineRef;
```

Bu sorgu sevk/fatura satırının hangi sipariş satırından geldiğini analiz etmek için kullanılabilir.

---

## 10. Fatura ve İrsaliye Bağlantısı

```sql
SELECT
    I.LOGICALREF AS INVOICE_REF,
    I.FICHENO AS INVOICE_NO,
    F.LOGICALREF AS STFICHE_REF,
    F.FICHENO AS STFICHE_NO
FROM LG_040_01_INVOICE I
LEFT JOIN LG_040_01_STFICHE F
    ON F.INVOICEREF = I.LOGICALREF
WHERE I.LOGICALREF = @InvoiceRef;
```

Bu bağlantı veri düzeltme ve tarih kontrol işlemlerinde önemlidir.

---

## 11. Faturaya Bağlı Stok Satırları

```sql
SELECT
    LOGICALREF,
    STOCKREF,
    AMOUNT,
    PRICE,
    TOTAL,
    VAT,
    STFICHEREF,
    INVOICEREF
FROM LG_040_01_STLINE
WHERE INVOICEREF = @InvoiceRef;
```

---

## 12. Cari Hareket Kontrolü

```sql
SELECT
    LOGICALREF,
    CLIENTREF,
    DATE_,
    TRCODE,
    MODULENR,
    SOURCEFREF,
    AMOUNT
FROM LG_040_01_CLFLINE
WHERE SOURCEFREF = @InvoiceRef;
```

Bağlantı alanlarının senaryoya göre değişebileceği unutulmamalıdır.

---

## 13. Son Satınalma Fiyatı Sorgulama Kalıbı

Sık kullanılan iş ihtiyaçlarından biri malzemenin son alış fiyatını bulmaktır.

Temel yaklaşım:

```sql
SELECT TOP 1
    S.DATE_,
    S.PRICE,
    S.TRRATE,
    S.INVOICEREF
FROM LG_040_01_STLINE S
WHERE S.STOCKREF = @StockRef
  AND S.LINETYPE = 0
  AND S.CANCELLED = 0
  AND S.DATE_ <= @AsOfDate
ORDER BY S.DATE_ DESC, S.LOGICALREF DESC;
```

Ancak gerçek maliyet analizi yapılırken:

- işlem türü,
- iade kayıtları,
- döviz kuru,
- birim çevrimi,
- fatura/irsaliye ilişkisi

gibi alanlar da dikkate alınmalıdır.

---

## 14. Stok Miktarı Sorgusu

Basit stok hareket toplamı için:

```sql
SELECT
    STOCKREF,
    SUM(CASE
            WHEN IOCODE IN (1, 2) THEN AMOUNT
            WHEN IOCODE IN (3, 4) THEN -AMOUNT
            ELSE 0
        END) AS STOCK_AMOUNT
FROM LG_040_01_STLINE
WHERE CANCELLED = 0
  AND LINETYPE = 0
GROUP BY STOCKREF;
```

Bu yalnızca örnek mantıktır. Logo stok yönünün gerçek yorumunda kullanılan fiş türü ve `IOCODE` kombinasyonları doğrulanmalıdır.

---

## 15. Proje Bazlı Hareket

```sql
SELECT
    PROJECTREF,
    STOCKREF,
    SUM(AMOUNT) AS AMOUNT
FROM LG_040_01_STLINE
WHERE PROJECTREF = @ProjectRef
  AND CANCELLED = 0
  AND LINETYPE = 0
GROUP BY PROJECTREF, STOCKREF;
```

Gerçek net stok hesabında giriş/çıkış yönü ayrıca uygulanmalıdır.

---

## 16. Tarih Filtresi

İndeks kullanımını bozabilecek yaklaşım:

```sql
WHERE YEAR(DATE_) = 2026
```

Daha iyi yaklaşım:

```sql
WHERE DATE_ >= '20260101'
  AND DATE_ <  '20270101'
```

Bu prensip büyük Logo veritabanlarında performans açısından önemlidir.

---

## 17. Dinamik Tablo Adı Güvenliği

Firma ve dönem dinamik olduğunda tablo adı string olarak oluşturulur.

Örnek:

```text
LG_" + FirmNo + "_" + PeriodNo + "_STLINE
```

Ancak firma ve dönem değerleri kullanıcı tarafından serbest metin olarak alınmamalıdır.

Doğrulama:

```text
FirmNo sadece rakam
PeriodNo sadece rakam
Beklenen uzunlukta
Sistemden alınmış değer
```

---

## 18. SQL Injection

`IQuery` kullanırken dinamik değerleri SQL stringine doğrudan eklemek risklidir.

Özellikle kullanıcı girdileri:

```text
Malzeme kodu
Cari kodu
Fatura numarası
Açıklama
```

kaçış veya parametreleme mekanizması olmadan birleştirilmemelidir.

Logo Objects sürümündeki query API parametre desteği doğrulanmalı; destek yoksa veri sıkı biçimde validate edilmelidir.

---

## 19. First / Next Döngüsü

Birden fazla kayıt okunacaksa genel mantık:

```text
if qry.First() then
    repeat
        GetFieldValue(...)
    until not qry.Next()
end if
```

Gerçek syntax kullanılan Logo script ortamına göre doğrulanmalıdır.

---

## 20. Clear Kullanımı

Query nesnesi kullanıldıktan sonra temizlenmelidir.

```text
qry.Clear()
```

Uzun yaşayan form ve uygulamalarda nesne yaşam döngüsünü düzgün yönetmek önemlidir.

---

## 21. IQuery Ne İçin Kullanılmalı?

Uygun kullanım alanları:

- referans bulma,
- kayıt var mı kontrolü,
- duplicate kontrolü,
- raporlama,
- analiz,
- entegrasyon ön kontrolü,
- performanslı veri okuma.

---

## 22. IQuery Ne İçin İlk Tercih Olmamalı?

Aşağıdaki işlemlerde doğrudan SQL ilk tercih olmamalıdır:

```text
INSERT
UPDATE
DELETE
```

özellikle:

- kartlar,
- siparişler,
- irsaliyeler,
- faturalar,
- üretim kayıtları,
- seri/lot hareketleri.

Bu işlemlerde `IData` veya ilgili Logo nesne katmanı tercih edilmelidir.

---

## 23. Okuma + IData Güncelleme Hibrit Kalıbı

```text
IQuery
  ↓
Kayıt ara
  ↓
LOGICALREF bul
  ↓
IData.Read(LOGICALREF)
  ↓
Alanları değiştir
  ↓
Post()
```

Bu, Logo entegrasyonlarında çok güçlü bir genel kalıptır.

---

## 24. Sonuç

`IQuery` Logo veritabanını hızlı ve kontrollü okumak için önemli bir araçtır.

Temel prensip:

> SQL ile doğru kaydı bul, Logo'nun iş nesnesini `IData` ile değiştir.
