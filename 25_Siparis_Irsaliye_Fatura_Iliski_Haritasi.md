# 25 — Sipariş, İrsaliye ve Fatura İlişki Haritası

## 1. Amaç

Logo ERP’de satış ve satınalma süreçlerini doğru analiz etmek için belgeleri tek tek değil, bir zincir olarak görmek gerekir.

Tipik süreç:

```text
Sipariş
   ↓
İrsaliye
   ↓
Fatura
   ↓
Cari Hareket
   ↓
Muhasebe
```

Her aşama farklı tablo ailelerinde tutulur ve referans alanları üzerinden birbirine bağlanır.

---

## 2. Sipariş Üst ve Satır Tabloları

```text
ORFICHE
ORFLINE
```

Temel ilişki:

```text
ORFICHE.LOGICALREF
    ↓
ORFLINE.ORDFICHEREF
```

Örnek:

```sql
SELECT
    F.LOGICALREF,
    F.FICHENO,
    L.LOGICALREF AS LINE_REF,
    L.STOCKREF,
    L.AMOUNT
FROM LG_040_01_ORFICHE F
JOIN LG_040_01_ORFLINE L
    ON L.ORDFICHEREF = F.LOGICALREF
WHERE F.LOGICALREF = @OrderRef;
```

---

## 3. Sipariş Satırından Stok Hareketine

Sipariş satırının sonraki belgeye taşınmasında önemli alanlardan biri:

```text
STLINE.ORDTRANSREF
```

Genel ilişki:

```text
ORFLINE.LOGICALREF
    ↓
STLINE.ORDTRANSREF
```

Bu bağlantı sayesinde bir sipariş satırının hangi irsaliye veya fatura satırına dönüştüğü analiz edilebilir.

---

## 4. Stok Fişi ve Satır İlişkisi

```text
STFICHE.LOGICALREF
    ↓
STLINE.STFICHEREF
```

Örnek:

```sql
SELECT
    F.FICHENO,
    F.TRCODE,
    L.LOGICALREF,
    L.STOCKREF,
    L.AMOUNT
FROM LG_040_01_STFICHE F
JOIN LG_040_01_STLINE L
    ON L.STFICHEREF = F.LOGICALREF
WHERE F.LOGICALREF = @StFicheRef;
```

---

## 5. Fatura Üst Bilgisi

```text
INVOICE
```

Stok satırının faturaya bağlantısı:

```text
STLINE.INVOICEREF
    ↓
INVOICE.LOGICALREF
```

Stok fişi tarafında da:

```text
STFICHE.INVOICEREF
```

alanı bulunabilir.

---

## 6. Siparişten Faturaya Zincir

Basitleştirilmiş ilişki:

```text
ORFICHE
   ↓
ORFLINE
   ↓ ORDTRANSREF
STLINE
   ↓ INVOICEREF
INVOICE
```

Bu zincir sipariş karşılama oranı, kalan miktar ve sevk/fatura kontrolü için kullanılabilir.

---

## 7. Sipariş Kalan Miktarı

Temel mantık:

```text
Sipariş Miktarı
-
Karşılanan Miktar
=
Kalan Miktar
```

Örnek yaklaşım:

```sql
SELECT
    O.LOGICALREF AS ORDER_LINE_REF,
    O.AMOUNT AS ORDER_AMOUNT,
    ISNULL(SUM(S.AMOUNT), 0) AS DELIVERED_AMOUNT,
    O.AMOUNT - ISNULL(SUM(S.AMOUNT), 0) AS REMAINING_AMOUNT
FROM LG_040_01_ORFLINE O
LEFT JOIN LG_040_01_STLINE S
    ON S.ORDTRANSREF = O.LOGICALREF
   AND S.CANCELLED = 0
WHERE O.LOGICALREF = @OrderLineRef
GROUP BY O.LOGICALREF, O.AMOUNT;
```

Gerçek uygulamada iade ve belge türleri ayrıca değerlendirilmelidir.

---

## 8. Kısmi Sevk

Bir sipariş satırı birden fazla irsaliyeye bölünebilir.

```text
Sipariş satırı: 100 adet

İrsaliye 1: 30
İrsaliye 2: 40
İrsaliye 3: 30
```

Bu nedenle `ORDTRANSREF` tek bir satıra değil, birden fazla stok hareket satırına bağlanabilir.

---

## 9. İade Etkisi

Kalan sipariş hesabında iade hareketleri dikkate alınmalıdır.

Basit `SUM(AMOUNT)` yaklaşımı her zaman doğru değildir.

Gerekli kontroller:

- `TRCODE`
- `IOCODE`
- iade belge türü
- `CANCELLED`
- satır türü

---

## 10. Fatura ve Cari Hareket

Fatura kayıt edildiğinde cari hareket oluşabilir.

İlişki analizinde:

```text
INVOICE
   ↓
CLFLINE
```

bağlantısı kaynak referans alanları üzerinden izlenir.

Örnek sorgu:

```sql
SELECT
    C.LOGICALREF,
    C.CLIENTREF,
    C.DATE_,
    C.MODULENR,
    C.TRCODE,
    C.SOURCEFREF,
    C.AMOUNT
FROM LG_040_01_CLFLINE C
WHERE C.SOURCEFREF = @InvoiceRef;
```

Bağlantı alanı senaryoya göre doğrulanmalıdır.

---

## 11. Fatura ve Muhasebe

Muhasebeleştirme sonrası:

```text
Fatura
   ↓
Muhasebe fişi
   ↓
Muhasebe satırları
```

Tablolar:

```text
EMFICHE
EMFLINE
```

Muhasebeleştirilmiş bir faturada tarih düzeltme gibi işlemler yalnızca `INVOICE` üzerinde yapılmamalıdır.

---

## 12. Tarih Tutarlılığı

Belge zincirinde kontrol edilmesi gereken alanlar:

```text
ORFICHE.DATE_
ORFLINE.DATE_
STFICHE.DATE_
STLINE.DATE_
INVOICE.DATE_
CLFLINE.DATE_
EMFICHE.DATE_
EMFLINE.DATE_
```

Her işlemde tüm tabloların tarihinin aynı olması gerekmez; ancak iş sürecine göre tutarlılık kontrol edilmelidir.

---

## 13. Ambar Bilgileri

Stok hareketlerinde:

```text
SOURCEINDEX
DESTINDEX
```

alanları kritik öneme sahiptir.

Özellikle ambar transferi veya iade süreçlerinde üst fiş ve satır ambar bilgilerinin tutarlı olması gerekir.

Örnek kontrol:

```sql
SELECT
    F.SOURCEINDEX AS HEADER_WAREHOUSE,
    L.SOURCEINDEX AS LINE_WAREHOUSE
FROM LG_040_01_STFICHE F
JOIN LG_040_01_STLINE L
    ON L.STFICHEREF = F.LOGICALREF
WHERE F.LOGICALREF = @Ref;
```

---

## 14. Proje Bağlantısı

Sipariş veya stok hareketleri proje ile ilişkilendirilebilir.

Önemli alan:

```text
PROJECTREF
```

Proje bazlı lojistik entegrasyonlarda bu referansın siparişten stok hareketine taşınması kritik olabilir.

---

## 15. Veri Düzeltme Senaryolarında Yaklaşım

Örneğin fatura tarihi değiştirilecekse:

```text
1. Faturayı bul
2. TRCODE doğrula
3. İrsaliye bağlantısını bul
4. Stok satırlarını bul
5. Cari hareketi bul
6. Muhasebe bağlantısını bul
7. Test modunda sonucu göster
8. Gerçek modda transaction içinde güncelle
```

Bu yaklaşım tek tablo güncellemesinden çok daha güvenlidir.

---

## 16. Kayıp Bağlantı Tespiti

Örnek: faturaya bağlı stok satırı var ama stok fişi yok.

```sql
SELECT S.*
FROM LG_040_01_STLINE S
LEFT JOIN LG_040_01_STFICHE F
    ON F.LOGICALREF = S.STFICHEREF
WHERE S.INVOICEREF = @InvoiceRef
  AND F.LOGICALREF IS NULL;
```

Bu tip sorgular veri bütünlüğü analizi için önemlidir.

---

## 17. Yetim Sipariş Referansı

```sql
SELECT S.*
FROM LG_040_01_STLINE S
LEFT JOIN LG_040_01_ORFLINE O
    ON O.LOGICALREF = S.ORDTRANSREF
WHERE S.ORDTRANSREF <> 0
  AND O.LOGICALREF IS NULL;
```

Bu sonuçlar entegrasyon veya geçmiş veri müdahalelerinden kaynaklanan sorunlara işaret edebilir.

---

## 18. Belge Zinciri Sorgu Şablonu

Bir faturanın tüm ana referanslarını görmek için:

```sql
SELECT
    I.LOGICALREF AS INVOICE_REF,
    I.FICHENO AS INVOICE_NO,
    F.LOGICALREF AS STFICHE_REF,
    F.FICHENO AS STFICHE_NO,
    S.LOGICALREF AS STLINE_REF,
    S.ORDTRANSREF,
    O.ORDFICHEREF
FROM LG_040_01_INVOICE I
LEFT JOIN LG_040_01_STFICHE F
    ON F.INVOICEREF = I.LOGICALREF
LEFT JOIN LG_040_01_STLINE S
    ON S.INVOICEREF = I.LOGICALREF
LEFT JOIN LG_040_01_ORFLINE O
    ON O.LOGICALREF = S.ORDTRANSREF
WHERE I.LOGICALREF = @InvoiceRef;
```

Bu sorgu teşhis için güçlü bir başlangıç noktasıdır.

---

## 19. Logo Objects Açısından İlişki Zinciri

Logo Objects ile belge üretildiğinde amaç yalnızca üst tabloyu oluşturmak değildir.

`IData.Post()` çağrısı iş kurallarına bağlı olarak ilişki zincirlerinin oluşmasını sağlar.

Bu nedenle doğrudan SQL insert ile:

```text
INVOICE oluştur
STLINE oluştur
```

yaklaşımı güvenli değildir.

Çünkü görünmeyen veya unutulan yardımcı ilişkiler eksik kalabilir.

---

## 20. Entegrasyonlarda External ID

Dış sistem siparişleri Logo'ya aktarılıyorsa dış sistem anahtarı saklanmalıdır.

Örnek:

```text
ERP_ORDER_REF
EXTERNAL_ORDER_ID
EXTERNAL_LINE_ID
```

Böylece aynı siparişin tekrar aktarılması engellenebilir ve belge zinciri takip edilebilir.

---

## 21. Tanılama Kontrol Listesi

Bir belge ilişkisi bozuk görünüyorsa sırayla:

1. Üst belge `LOGICALREF`
2. `TRCODE`
3. `CANCELLED`
4. Satır `STFICHEREF`
5. `INVOICEREF`
6. `ORDTRANSREF`
7. `PROJECTREF`
8. `SOURCEINDEX`
9. `DESTINDEX`
10. Cari hareket
11. Muhasebe hareketi
12. Seri/lot dağıtımı

kontrol edilmelidir.

---

## 22. Sonuç

Logo ERP’de belge ilişkilerini anlamak, tek tek tablo bilmekten daha değerlidir.

Temel ilişki modeli:

```text
Sipariş → İrsaliye → Fatura → Cari → Muhasebe
```

ve satır seviyesinde:

```text
ORFLINE.LOGICALREF
    ↓
STLINE.ORDTRANSREF
```

Logo entegrasyonlarında en önemli ilkelerden biri şudur:

> Belgeyi değil, belge zincirini koru.
