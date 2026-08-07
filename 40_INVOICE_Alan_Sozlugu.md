# 40 — INVOICE Alan Sözlüğü

## Amaç

Bu bölüm Logo ERP’de satış ve satınalma faturalarının üst bilgi tablosu olan `INVOICE` için günlük analiz, entegrasyon ve bakım çalışmalarında en sık kullanılan alanları işlevsel olarak gruplandırır.

Temel tablo:

```text
LG_XXX_YY_INVOICE
```

Örnek:

```text
LG_102_01_INVOICE
```

> Alanların tam seti ve bazı davranışları Logo sürümüne göre değişebilir. Üretim ortamında tablo metadata’sı ve çalışan örnek kayıtlarla doğrulama yapılmalıdır.

## 1. Kimlik ve belge bilgileri

### LOGICALREF

Faturanın benzersiz referansıdır.

Birçok ilişkide ana bağlantı noktasıdır.

### FICHENO

Fatura numarasıdır.

Kullanıcıların ekranda gördüğü ve operasyonel aramalarda kullandığı temel numaradır.

### DATE_

Fatura tarihidir.

Bağlı stok, cari ve muhasebe hareketleriyle tarih tutarlılığı önemlidir.

### TIME_

İşlem saat bilgisini taşıyan alanlardan biridir.

## 2. İşlem türü

### TRCODE

Faturanın işlem türünü belirtir.

Örneğin satış ve satınalma faturaları farklı `TRCODE` değerleri kullanır.

`TRCODE` sözlüğü ayrı bölümde ele alınmıştır.

### CANCELLED

Faturanın iptal durumudur.

Raporlama ve entegrasyon sorgularında çoğu zaman:

```sql
WHERE CANCELLED = 0
```

filtresi gerekir.

## 3. Cari bağlantısı

### CLIENTREF

Faturanın bağlı olduğu cari kart referansıdır.

Kavramsal ilişki:

```text
CLCARD.LOGICALREF
        ↓
INVOICE.CLIENTREF
```

Örnek:

```sql
SELECT
    INV.FICHENO,
    C.CODE,
    C.DEFINITION_
FROM LG_102_01_INVOICE INV
LEFT JOIN LG_102_CLCARD C
    ON C.LOGICALREF = INV.CLIENTREF
WHERE INV.LOGICALREF = @InvoiceRef;
```

## 4. Stok fişi bağlantısı

Fatura ile stok fişi arasında işlem türüne göre bağlantı bulunabilir.

Analizde şu tablolar birlikte incelenmelidir:

```text
INVOICE
STFICHE
STLINE
```

Satır seviyesinde `STLINE.INVOICEREF` en kritik bağlantılardan biridir.

Örnek:

```sql
SELECT *
FROM LG_102_01_STLINE
WHERE INVOICEREF = @InvoiceRef;
```

## 5. Muhasebe bağlantısı

Fatura muhasebeleştirilmişse muhasebe fişi bağlantı alanları bulunabilir.

Analiz yaklaşımı:

```text
INVOICE
   ↓ muhasebe referansı
EMFICHE
   ↓
EMFLINE
```

Muhasebe fişi her faturada bulunmak zorunda değildir.

Bu nedenle sorgu ve bakım prosedürleri eksik muhasebe bağlantısında kontrollü şekilde devam edebilmelidir.

## 6. Proje bağlantısı

### PROJECTREF

Fatura proje ile ilişkilendirilmişse proje referansı kullanılabilir.

Proje bazlı gelir/gider ve maliyet raporlarında önemlidir.

## 7. İşyeri, bölüm ve ambar bilgileri

Fatura üst bilgilerinde organizasyonel yapı ile ilgili çeşitli alanlar bulunabilir.

Sık kullanılan kavramlar:

- işyeri,
- bölüm,
- fabrika,
- ambar,
- masraf merkezi.

Bu alanların isimleri ve kullanım kapsamı işlem türüne göre kontrol edilmelidir.

## 8. Döviz alanları

Faturada işlem dövizi ve raporlama dövizi ile ilgili kur ve tutar alanları bulunabilir.

Döviz analizi yapılırken şu ayrım önemlidir:

```text
Yerel para birimi
İşlem dövizi
Raporlama dövizi
```

Bir satırdaki veya faturadaki fiyatı USD, EUR veya TL olarak yorumlamadan önce ilgili kur alanları mutlaka kontrol edilmelidir.

## 9. Toplam alanları

Fatura üzerinde çeşitli toplam alanları bulunabilir:

- brüt toplam,
- indirim toplamı,
- masraf toplamı,
- KDV toplamı,
- net toplam,
- dövizli toplamlar.

Bu alanların satırlardan bağımsız yeniden hesaplanması yerine Logo’nun belge mantığıyla karşılaştırılması gerekir.

## 10. KDV ve istisna bilgileri

KDV detaylarının önemli kısmı stok/hizmet satırlarında bulunur.

Bu nedenle fatura üst bilgisi kontrolü tek başına yeterli değildir.

Örneğin KDV oranı `0` olan malzeme satırlarını kontrol etmek için:

```sql
SELECT
    SL.LOGICALREF,
    SL.VAT,
    SL.VATEXCEPTCODE,
    SL.VATEXCEPTREASON
FROM LG_102_01_STLINE SL
WHERE SL.INVOICEREF = @InvoiceRef
  AND SL.LINETYPE = 0
  AND SL.VAT = 0;
```

## 11. Fatura → cari hareket ilişkisi

Faturanın cari etkisi `CLFLINE` üzerinde izlenebilir.

Analiz sırasında:

```text
CLIENTREF
MODULENR
TRCODE
SOURCEFREF
```

alanları birlikte değerlendirilmelidir.

## 12. Fatura → stok satırı kontrolü

```sql
SELECT
    INV.FICHENO,
    INV.DATE_ AS FATURA_TARIHI,
    SL.LOGICALREF AS STLINE_REF,
    SL.DATE_ AS SATIR_TARIHI,
    SL.STOCKREF,
    SL.AMOUNT,
    SL.PRICE
FROM LG_102_01_INVOICE INV
LEFT JOIN LG_102_01_STLINE SL
    ON SL.INVOICEREF = INV.LOGICALREF
WHERE INV.LOGICALREF = @InvoiceRef;
```

Bu sorgu tarih ve bağlantı tutarlılığını kontrol etmek için kullanılabilir.

## 13. Fatura tarih düzeltme kontrol listesi

Fatura tarihi değişikliği gibi özel bakım işlemlerinde şu kayıtlar analiz edilmelidir:

```text
INVOICE.DATE_
STFICHE.DATE_
STLINE.DATE_
CLFLINE.DATE_
EMFICHE.DATE_
EMFLINE tarih bağlamı
```

Her tabloda kayıt bulunmak zorunda değildir.

## 14. Fatura numarası ile güvenli arama

```sql
DECLARE @FicheNo VARCHAR(50) = 'FATURA_NO';

SELECT
    LOGICALREF,
    FICHENO,
    DATE_,
    TRCODE,
    CLIENTREF,
    CANCELLED
FROM LG_102_01_INVOICE
WHERE FICHENO = @FicheNo;
```

Firma ve dönem doğru seçilmeden yalnızca fatura numarasına güvenilmemelidir.

## 15. En sık yapılan hatalar

### Yalnızca INVOICE tablosunu güncellemek

Bağlı stok, cari ve muhasebe hareketleri gözden kaçabilir.

### Fiyat alanını döviz fiyatı sanmak

Kur ve para birimi bağlamı kontrol edilmelidir.

### Muhasebe fişini zorunlu kabul etmek

Belge henüz muhasebeleştirilmemiş olabilir.

### TRCODE’u doğrulamadan işlem yapmak

Aynı fatura numarası farklı tür veya firma/dönem bağlamında bulunabilir.

## 16. Minimum teşhis alanları

Bir faturayı analiz ederken en az şu bilgiler alınmalıdır:

```text
LOGICALREF
FICHENO
DATE_
TRCODE
CLIENTREF
CANCELLED
PROJECTREF
Muhasebe bağlantısı
Bağlı STFICHE/STLINE kayıtları
Bağlı CLFLINE kayıtları
```

## Sonuç

`INVOICE`, faturanın üst belge katmanıdır; ancak faturanın gerçek ERP etkisi stok, cari ve muhasebe hareketlerine yayıldığı için analiz hiçbir zaman yalnızca bu tablo ile sınırlandırılmamalıdır.
