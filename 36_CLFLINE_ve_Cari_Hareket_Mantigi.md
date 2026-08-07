# 36 — CLFLINE ve Cari Hareket Mantığı

## Amaç

Bu bölüm Logo ERP’de cari hesap hareketlerinin nasıl oluştuğunu, `CLFLINE` tablosunun hangi amaçla kullanıldığını ve fatura, çek/senet, banka, kasa ve muhasebe hareketleri ile cari hareketler arasındaki bağların nasıl analiz edilmesi gerektiğini açıklar.

> Not: Alan davranışları Logo ürün/sürümüne göre değişebilir. Üretim ortamında kesin ilişki kurulmadan önce çalışan bir işlem üzerinden doğrulama yapılmalıdır.

## 1. CLFLINE nedir?

`LG_XXX_YY_CLFLINE`, dönemsel cari hesap hareket tablosudur.

Genel olarak aşağıdaki tür hareketlerin cari hesap ayağı burada izlenir:

- satış ve satınalma faturaları,
- cari hesap fişleri,
- banka işlemleri,
- kasa işlemleri,
- çek/senet hareketleri,
- bazı mahsup ve virman senaryoları.

Bir işlemin stok hareketi bulunması, cari hareketinin de mutlaka aynı tablo zinciri üzerinden oluştuğu anlamına gelmez. Her modülün kendi kaynak belgesi vardır; `CLFLINE` bu hareketlerin cari hesap etkisini temsil eden katmandır.

## 2. Firma ve dönem yapısı

Örnek:

```text
LG_102_01_CLFLINE
```

- `102`: firma numarası
- `01`: dönem numarası

Cari kartın kendisi dönemsel değildir:

```text
LG_102_CLCARD
```

Hareket tablosu ise dönemlidir:

```text
LG_102_01_CLFLINE
```

## 3. Temel bağlantı mantığı

Cari kart ile cari hareket arasındaki temel ilişki çoğu senaryoda şöyledir:

```text
CLCARD.LOGICALREF
        ↓
CLFLINE.CLIENTREF
```

Bu nedenle bir carinin hareketlerini incelerken temel başlangıç sorgusu şu yapıdadır:

```sql
SELECT
    C.CODE,
    C.DEFINITION_,
    CL.*
FROM LG_102_CLCARD C
INNER JOIN LG_102_01_CLFLINE CL
    ON CL.CLIENTREF = C.LOGICALREF
WHERE C.CODE = 'CARİ_KODU';
```

## 4. MODULENR ve TRCODE birlikte değerlendirilmelidir

`CLFLINE` üzerinde yalnızca `TRCODE` değerine bakarak kaynak belgeyi yorumlamak risklidir.

Aynı `TRCODE` değeri farklı modül bağlamlarında farklı anlam taşıyabilir.

Bu nedenle analizlerde çoğu zaman aşağıdaki ikili birlikte ele alınmalıdır:

```text
MODULENR + TRCODE
```

Örnek yaklaşım:

```sql
SELECT
    MODULENR,
    TRCODE,
    COUNT(*) AS ADET
FROM LG_102_01_CLFLINE
GROUP BY MODULENR, TRCODE
ORDER BY MODULENR, TRCODE;
```

Bu sorgu mevcut sistemde hangi modül/hareket türlerinin gerçekten kullanıldığını anlamak için faydalıdır.

## 5. Kaynak belge bağlantıları

Cari hareketin hangi kaynaktan geldiğini analiz ederken aşağıdaki alanlar önemlidir:

- `SOURCEFREF`
- `MODULENR`
- `TRCODE`
- `CLIENTREF`
- `DATE_`
- `AMOUNT`
- `SIGN`
- `CANCELLED`

`SOURCEFREF` çoğu senaryoda kaynağın fiş veya belge referansını taşır; ancak bağlanacağı tablonun belirlenmesi için `MODULENR` ve hareket türü de değerlendirilmelidir.

Dolayısıyla şu yaklaşım yanlıştır:

```text
CLFLINE.SOURCEFREF = INVOICE.LOGICALREF
```

şeklinde bütün satırlara tek bir join uygulamak.

Doğru yaklaşım:

```text
MODULENR/TRCODE ile kaynak modülü belirle
        ↓
Kaynak tabloyu seç
        ↓
SOURCEFREF ilişkisini doğrula
```

## 6. Borç / alacak yönü

Cari hareketlerde tutarın yönünü değerlendirirken yalnızca `AMOUNT` yeterli değildir.

Logo veri yapısında hareket yönü için kullanılan işaret alanı ayrıca dikkate alınmalıdır.

Analiz sırasında şu yapı tercih edilmelidir:

```sql
SELECT
    DATE_,
    TRCODE,
    SIGN,
    AMOUNT
FROM LG_102_01_CLFLINE
WHERE CLIENTREF = @ClientRef;
```

Raporlama katmanında sistemde doğrulanmış işaret mantığına göre:

```text
Borç
Alacak
Net bakiye
```

hesaplanmalıdır.

> `SIGN` anlamını üretim sisteminizde çalışan örnek hareketlerle doğrulamadan sabit kabul etmeyin.

## 7. İptal kayıtları

Cari bakiye veya yaşlandırma raporlarında iptal edilmiş kayıtlar dikkate alınmamalıdır.

Tipik filtre:

```sql
WHERE CANCELLED = 0
```

Ancak rapor gereksinimine göre dönem kapanışı, devreden hareketler ve özel fiş türleri ayrıca değerlendirilmelidir.

## 8. Fatura → cari hareket kontrolü

Bir faturanın cari hareket oluşturup oluşturmadığını kontrol ederken şu zincir incelenmelidir:

```text
INVOICE
   ↓
Cari kart / CLIENTREF
   ↓
CLFLINE
   ↓
MODULENR + TRCODE + SOURCEFREF doğrulaması
```

Örnek teşhis yaklaşımı:

```sql
DECLARE @InvoiceRef INT = 12345;

SELECT *
FROM LG_102_01_INVOICE
WHERE LOGICALREF = @InvoiceRef;

SELECT *
FROM LG_102_01_CLFLINE
WHERE SOURCEFREF = @InvoiceRef;
```

Bu sorgu yalnızca ilk kontrol içindir. Aynı referansın başka modüllerde de kullanılabileceği düşünülerek `MODULENR` ve `TRCODE` ile ilişki doğrulanmalıdır.

## 9. Cari yaşlandırma tasarımı

Yaşlandırma raporlarında yalnızca `CLFLINE.DATE_` alanına göre gruplama yapmak çoğu zaman yeterli değildir.

Profesyonel yaklaşımda aşağıdakiler değerlendirilmelidir:

- işlem tarihi,
- vade tarihi,
- borç/alacak yönü,
- kapanan hareketler,
- kısmi kapamalar,
- kalan bakiye,
- dönem devri,
- iptal hareketleri.

Basit yaşlandırma mantığı:

```text
Kalan borç
   ↓
Vade tarihi
   ↓
Bugün - Vade
   ↓
0-30 / 31-60 / 61-90 / ...
```

## 10. En sık yapılan hatalar

### Hata 1 — Sadece TRCODE’a göre yorumlamak

Çözüm:

```text
MODULENR + TRCODE birlikte değerlendirilmelidir.
```

### Hata 2 — SOURCEFREF’i bütün modüllerde aynı tabloya bağlamak

Çözüm:

Kaynak tablo modüle göre belirlenmelidir.

### Hata 3 — İptal hareketlerini bakiyeye dahil etmek

Çözüm:

`CANCELLED` kontrol edilmelidir.

### Hata 4 — Cari hareket tablosuna doğrudan UPDATE yapmak

Cari hareket, başka bir resmi Logo belgesinin finansal sonucudur. Kaynak belge düzeltilmeden yalnızca `CLFLINE` üzerinde değişiklik yapmak veri bütünlüğünü bozabilir.

## 11. Güvenli analiz şablonu

```sql
SELECT
    CL.LOGICALREF,
    CL.CLIENTREF,
    C.CODE AS CARI_KODU,
    C.DEFINITION_ AS CARI_ADI,
    CL.DATE_,
    CL.MODULENR,
    CL.TRCODE,
    CL.SIGN,
    CL.AMOUNT,
    CL.SOURCEFREF,
    CL.CANCELLED
FROM LG_102_01_CLFLINE CL
LEFT JOIN LG_102_CLCARD C
    ON C.LOGICALREF = CL.CLIENTREF
WHERE CL.CLIENTREF = @ClientRef
ORDER BY CL.DATE_, CL.LOGICALREF;
```

## Sonuç

`CLFLINE`, Logo ERP finansal veri modelinin en kritik hareket tablolarından biridir. Doğru analiz için cari kart bağlantısı, modül bilgisi, işlem türü, kaynak belge referansı ve hareket yönü birlikte değerlendirilmelidir.

Temel kural:

> Cari hareketi tek başına değil, onu oluşturan kaynak belge zinciriyle birlikte analiz et.
