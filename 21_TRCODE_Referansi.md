# 21 — TRCODE Referansı

## 1. TRCODE Nedir?

Logo ERP veritabanında `TRCODE`, bir hareketin veya belgenin işlem türünü tanımlayan en kritik alanlardan biridir.

Aynı tablo içerisinde farklı belge türleri tutulabildiği için yalnızca tablo adına bakmak çoğu zaman yeterli değildir. Kaydın gerçek iş anlamını anlamak için `TRCODE` mutlaka değerlendirilmelidir.

Örnek:

```sql
SELECT
    LOGICALREF,
    FICHENO,
    TRCODE,
    DATE_
FROM LG_040_01_INVOICE;
```

Burada `INVOICE` tablosu fatura ailesini temsil eder; `TRCODE` ise faturanın satınalma, satış, iade vb. hangi operasyon olduğunu ayırt eder.

---

## 2. TRCODE Tek Başına Yeterli Değildir

Aynı sayısal `TRCODE` değeri farklı tablo ailelerinde farklı iş anlamlarına sahip olabilir.

Bu nedenle doğru yorumlama şu üçlüyle yapılmalıdır:

```text
Tablo + TRCODE + İş Süreci
```

Örneğin:

```text
LG_XXX_YY_INVOICE + TRCODE
LG_XXX_YY_STFICHE + TRCODE
LG_XXX_YY_STLINE + TRCODE
LG_XXX_YY_ORFICHE + TRCODE
LG_XXX_YY_ORFLINE + TRCODE
```

TRCODE değerini tablo bağlamından koparıp yorumlamak hatalı sonuçlara yol açabilir.

---

## 3. Fatura İşlemlerinde TRCODE

Sahada sık kullanılan örneklerden biri satış faturasıdır.

Örneğin satış faturası için yaygın olarak:

```text
TRCODE = 8
```

kullanılır.

Ancak entegrasyon veya bakım kodlarında bunu doğrudan varsaymak yerine işlem türünü parametrik tutmak çoğu zaman daha sağlıklıdır.

Örnek prosedür yaklaşımı:

```sql
CREATE PROCEDURE dbo.SP_ORNEK
    @FirmaNo INT,
    @TrCode INT
AS
BEGIN
    -- işlem
END
```

Bu sayede aynı altyapı farklı fatura türlerine uyarlanabilir.

---

## 4. Stok Fişlerinde TRCODE

`LG_XXX_YY_STFICHE` ve `LG_XXX_YY_STLINE` tablolarında `TRCODE`, stok hareketinin operasyon türünü belirtir.

Örnek kullanım:

```sql
SELECT
    F.LOGICALREF,
    F.FICHENO,
    F.TRCODE,
    F.SOURCEINDEX
FROM LG_803_01_STFICHE F;
```

Satır tarafında:

```sql
SELECT
    S.LOGICALREF,
    S.STFICHEREF,
    S.STOCKREF,
    S.TRCODE,
    S.SOURCEINDEX,
    S.DESTINDEX
FROM LG_803_01_STLINE S;
```

Burada yalnızca `TRCODE` değil, aşağıdaki alanlar da hareketin anlamını belirler:

- `IOCODE`
- `SOURCEINDEX`
- `DESTINDEX`
- `LINETYPE`
- `STFICHEREF`
- `ORDTRANSREF`
- `SOURCELINK`

---

## 5. TRCODE ile Belge Zinciri Analizi

Bir faturayı analiz ederken sadece `INVOICE` kaydına bakmak çoğu zaman yeterli değildir.

Tipik zincir:

```text
INVOICE
   ↓
STFICHE
   ↓
STLINE
   ↓
CLFLINE
   ↓
EMFICHE / EMFLINE
```

Aynı işleme bağlı tablolardaki `TRCODE` değerleri operasyonun farklı katmanlardaki temsilidir.

Bu nedenle veri düzeltme işlemlerinde tüm bağlantılı kayıtlar birlikte değerlendirilmelidir.

---

## 6. Örnek: Fatura Tarihi Güncelleme

Bir satış faturasının tarihi değiştirilecekse yalnızca şu işlem yeterli değildir:

```sql
UPDATE LG_102_01_INVOICE
SET DATE_ = @YeniTarih
WHERE FICHENO = @FaturaNo
  AND TRCODE = @TrCode;
```

Bağlı hareketler de kontrol edilmelidir:

```text
INVOICE.DATE_
STFICHE.DATE_
STLINE.DATE_
CLFLINE.DATE_
EMFICHE.DATE_
EMFLINE.DATE_
```

Burada `TRCODE` filtrelemesi yanlış belgeyi güncellememek için güvenlik katmanlarından biridir.

---

## 7. TRCODE Parametrik mi Sabit mi Olmalı?

Bu karar kullanım amacına göre verilmelidir.

### Tek bir iş senaryosu

Sadece satış faturası için çalışan özel bir rutin varsa sabit kullanılabilir.

### Genel amaçlı araç

Birden fazla belge türünde kullanılacaksa parametrik olmalıdır.

Örnek:

```sql
@TrCode INT
```

Bu yaklaşım yeniden kullanılabilirliği artırır ancak yanlış parametre riskini de beraberinde getirir.

Bu nedenle prosedür içinde doğrulama yapılmalıdır.

---

## 8. Güvenli TRCODE Kullanımı

Önerilen kontrol:

```sql
IF @TrCode NOT IN (/* desteklenen işlem türleri */)
BEGIN
    THROW 50001, 'Desteklenmeyen TRCODE.', 1;
END;
```

Genel amaçlı araçlarda desteklenen kodların açıkça sınırlandırılması önemlidir.

---

## 9. Raporlarda TRCODE

Raporlama sorgularında `TRCODE` okunabilir metne çevrilebilir.

Örnek:

```sql
CASE F.TRCODE
    WHEN 8 THEN 'Satış Faturası'
    ELSE 'Diğer'
END AS ISLEM_TURU
```

Ancak uzun vadede çok sayıda `CASE` ifadesi yerine merkezi bir referans yapısı kullanmak daha iyi olabilir.

Örneğin:

```text
REF_TRCODE
----------
TABLE_FAMILY
TRCODE
DESCRIPTION
```

Bu özellikle BI ve veri ambarı projelerinde faydalıdır.

---

## 10. TRCODE ve DataObjectType Farkı

Bu iki kavram karıştırılmamalıdır.

### TRCODE

Veritabanında kayıtlı hareket türünü ifade eder.

### DataObjectType

Logo Objects tarafında oluşturulacak nesnenin tipidir.

Akış:

```text
İş Belgesi
   ↓
DataObjectType
   ↓
IData.Post()
   ↓
Logo tabloları
   ↓
TRCODE
```

Yani biri uygulama nesne katmanında, diğeri veritabanı işlem katmanında karşımıza çıkar.

---

## 11. Sık Yapılan Hatalar

### Tabloyu bilmeden TRCODE yorumlamak

Aynı kod farklı tablo ailelerinde farklı anlamlara gelebilir.

### TRCODE filtresi koymadan toplu UPDATE yapmak

Özellikle `INVOICE`, `STFICHE`, `STLINE` gibi tablolarda çok risklidir.

### Yalnızca üst kaydın TRCODE değerine bakmak

Satır veya bağlı belge farklı ilişkiler taşıyabilir.

### İşlem kodlarını ezbere genellemek

Logo sürümü, modül ve belge ailesi dikkate alınmalıdır.

---

## 12. Analiz Şablonu

Bir hareket incelenirken şu sorgu mantığı kullanılabilir:

```sql
SELECT
    LOGICALREF,
    TRCODE,
    DATE_,
    FICHENO
FROM LG_XXX_YY_INVOICE
WHERE LOGICALREF = @Ref;
```

Sonra ilişkili stok fişi:

```sql
SELECT *
FROM LG_XXX_YY_STFICHE
WHERE INVOICEREF = @Ref;
```

Ardından satırlar:

```sql
SELECT *
FROM LG_XXX_YY_STLINE
WHERE INVOICEREF = @Ref;
```

Bu yöntem TRCODE'un belge zincirindeki gerçek yerini görmeyi sağlar.

---

## 13. Sonuç

`TRCODE`, Logo veritabanını doğru okumak için temel alanlardan biridir; ancak tek başına bir sözlük değildir.

Doğru yaklaşım:

> TRCODE'u her zaman tablo ailesi, bağlantılı kayıtlar ve gerçek iş süreciyle birlikte yorumla.
