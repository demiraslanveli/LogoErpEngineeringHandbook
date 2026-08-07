# 39 — STFICHE / STLINE Alan Sözlüğü

## Amaç

Bu bölüm Logo ERP stok fişi ve stok hareket satırlarının en sık kullanılan alanlarını işlevsel olarak gruplandırır.

> Bu bir "tüm alanlar" sözlüğü değildir. Logo sürümüne göre alan seti genişleyebilir. Buradaki amaç, günlük analiz ve entegrasyon işlerinde en çok kullanılan alanları doğru bağlamda toplamaktır.

## 1. STFICHE — stok fişi üst bilgisi

Temel tablo:

```text
LG_XXX_YY_STFICHE
```

### LOGICALREF

Fişin benzersiz referansıdır.

Bağlantı:

```text
STFICHE.LOGICALREF
       ↓
STLINE.STFICHEREF
```

### FICHENO

Fiş numarasıdır.

Belge kullanıcı tarafında çoğu zaman bu alan üzerinden aranır.

### DATE_

Fiş tarihidir.

Satır tarihleriyle tutarlı olması beklenir.

### TRCODE

Stok fişi işlem türünü belirtir.

`TRCODE` tek başına yorumlanmamalı; işlem bağlamı ve yön bilgileriyle birlikte değerlendirilmelidir.

### SOURCEINDEX

Fişin kaynak ambar bilgisidir.

### DESTINDEX

Hedef ambar kullanılan işlem türlerinde hedef ambar bilgisidir.

### CANCELLED

Fişin iptal durumunu belirtir.

Raporlama ve stok hesabında genellikle iptal kayıtları hariç tutulur.

### CLIENTREF

İşlem cari hesapla ilişkiliyse cari referansını taşıyabilir.

Her stok fişi türünde dolu olmak zorunda değildir.

### INVOICEREF

Stok fişi bir faturaya bağlıysa fatura referansını taşıyan alanlardan biridir.

## 2. STLINE — stok hareket satırı

Temel tablo:

```text
LG_XXX_YY_STLINE
```

### LOGICALREF

Stok hareket satırının benzersiz referansıdır.

### STOCKREF

Malzeme kartı referansıdır.

Bağlantı:

```text
ITEMS.LOGICALREF
      ↓
STLINE.STOCKREF
```

### STFICHEREF

Bağlı stok fişinin referansıdır.

### INVOICEREF

Bağlı faturanın referansıdır.

Faturalı stok hareketlerinde kritik bağlantı alanıdır.

### ORDTRANSREF

Bağlı sipariş satırı referansıdır.

### ORDFICHEREF

Sipariş fişi üst referansı kullanılan senaryolarda değerlendirilir.

### PREVLINEREF

Önceki veya kaynak stok satırı bağlantısı için kullanılabilen self-reference alanıdır.

### SOURCELINK

Bazı işlem zincirlerinde kaynak hareket bağlantısını taşır.

### LINETYPE

Satırın türünü belirler.

Malzeme, hizmet, indirim, masraf gibi satırları birbirinden ayırmada önemlidir.

### TRCODE

Satır işlem türüdür.

Üst fiş `TRCODE` ile çoğu zaman tutarlıdır ancak analizlerde satır alanı ayrıca kontrol edilmelidir.

### DATE_

Stok hareket tarihidir.

### AMOUNT

Hareket miktarıdır.

Miktarın stok etkisi `IOCODE`, `TRCODE`, işlem yönü ve iptal durumu ile birlikte değerlendirilmelidir.

### PRICE

Satır birim fiyatıdır.

Kur, birim dönüşümü, indirim ve fatura bağlamı olmadan tek başına maliyet veya döviz fiyatı olarak yorumlanmamalıdır.

### VAT

KDV oranıdır.

### VATEXCEPTCODE / VATEXCEPTREASON

KDV muafiyet/istisna bilgilerinin tutulduğu alanlardır.

KDV oranı `0` olan satırlarda muafiyet sebebi kontrolleri için önemlidir.

### UOMREF

Satırda kullanılan birim referansıdır.

### UINFO1 / UINFO2

Birim çevrim bilgileridir.

Kavramsal dönüşüm:

```text
ana birim miktarı ↔ işlem birimi miktarı
```

Bu alanların yönü işlem ve sürüm bağlamında doğrulanmalıdır.

### SOURCEINDEX

Satırın kaynak ambarıdır.

### DESTINDEX

Satırın hedef ambarıdır.

### IOCODE

Stok hareket yönünü anlamada kullanılan temel alanlardan biridir.

### CLIENTREF

Satır seviyesinde cari bağlantısı bulunan senaryolarda kullanılabilir.

### PROJECTREF

Proje bağlantısıdır.

Proje bazlı stok, üretim veya maliyet raporlarında önemlidir.

### CENTERREF

Masraf merkezi / işyeri benzeri finansal sınıflandırma bağlantılarında kullanılabilen referans alanıdır.

### CANCELLED

Satırın bağlı belge iptal durumu veya satır iptal bağlamında değerlendirilir.

## 3. En sık kullanılan joinler

### Malzeme

```sql
SELECT I.CODE, I.NAME, SL.*
FROM LG_102_01_STLINE SL
INNER JOIN LG_102_ITEMS I
    ON I.LOGICALREF = SL.STOCKREF;
```

### Stok fişi

```sql
SELECT F.FICHENO, F.DATE_, SL.*
FROM LG_102_01_STLINE SL
INNER JOIN LG_102_01_STFICHE F
    ON F.LOGICALREF = SL.STFICHEREF;
```

### Fatura

```sql
SELECT INV.FICHENO, SL.*
FROM LG_102_01_STLINE SL
LEFT JOIN LG_102_01_INVOICE INV
    ON INV.LOGICALREF = SL.INVOICEREF;
```

### Sipariş satırı

```sql
SELECT OL.*, SL.*
FROM LG_102_01_STLINE SL
LEFT JOIN LG_102_01_ORFLINE OL
    ON OL.LOGICALREF = SL.ORDTRANSREF;
```

## 4. Stok hareketini analiz etmek için minimum alan seti

Bir stok satırını teşhis ederken en az şu alanları birlikte görmek faydalıdır:

```text
LOGICALREF
STOCKREF
LINETYPE
TRCODE
DATE_
AMOUNT
PRICE
UOMREF
UINFO1
UINFO2
IOCODE
SOURCEINDEX
DESTINDEX
STFICHEREF
INVOICEREF
ORDTRANSREF
PREVLINEREF
SOURCELINK
PROJECTREF
CANCELLED
```

## 5. Örnek teşhis sorgusu

```sql
SELECT
    SL.LOGICALREF,
    I.CODE AS MALZEME_KODU,
    I.NAME AS MALZEME_ADI,
    SL.LINETYPE,
    SL.TRCODE,
    SL.DATE_,
    SL.AMOUNT,
    SL.PRICE,
    SL.UOMREF,
    SL.UINFO1,
    SL.UINFO2,
    SL.IOCODE,
    SL.SOURCEINDEX,
    SL.DESTINDEX,
    SL.STFICHEREF,
    SL.INVOICEREF,
    SL.ORDTRANSREF,
    SL.PREVLINEREF,
    SL.SOURCELINK,
    SL.PROJECTREF
FROM LG_102_01_STLINE SL
LEFT JOIN LG_102_ITEMS I
    ON I.LOGICALREF = SL.STOCKREF
WHERE SL.LOGICALREF = @StlineRef;
```

## 6. UPDATE yapmadan önce

`STFICHE` veya `STLINE` üzerinde doğrudan değişiklik yapılmadan önce şu bağlantılar kontrol edilmelidir:

```text
Sipariş
Fatura
Cari hareket
Muhasebe fişi
Seri/lot hareketi
Üretim hareketi
Proje bağlantısı
```

Çünkü stok satırı çoğu zaman tek başına bağımsız bir kayıt değildir.

## Sonuç

`STFICHE` ve `STLINE`, Logo stok veri modelinin merkezidir. Bir alanın anlamını yalnızca kolon adına bakarak değil, belge türü, satır türü, hareket yönü ve kaynak bağlantıları ile birlikte yorumlamak gerekir.
