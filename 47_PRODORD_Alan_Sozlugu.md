# 47 — PRODORD Alan Sözlüğü

## Amaç

Bu bölüm, Logo ERP detaylı üretim yapısında üretim emri başlığını temsil eden `LG_<FIRMA>_<DONEM>_PRODORD` tablosunu geliştirici ve entegrasyon bakış açısıyla açıklar.

Üretim emri; planlama, operasyon, malzeme tüketimi, mamul üretimi ve maliyetlendirme zincirinin merkezindeki kayıtlardan biridir.

## Temel tablo

```text
LG_<FIRMA>_<DONEM>_PRODORD
```

Örnek:

```text
LG_040_01_PRODORD
LG_803_01_PRODORD
```

Üretim emri dönemsel yapıda değerlendirilmelidir.

## Sık kullanılan alanlar

Alan adları ve davranışları sürüme göre değişebildiği için gerçek ortam doğrulaması önemlidir.

| Alan | Açıklama |
|---|---|
| `LOGICALREF` | Üretim emri teknik referansı |
| `FICHENO` | Üretim emri numarası |
| `DATE_` | Kayıt / emir tarihi |
| `ITEMREF` | Üretilecek malzeme referansı |
| `AMOUNT` | Planlanan üretim miktarı |
| `PLNBEGDATE` | Planlanan başlangıç tarihi; sürüm doğrulaması gerekir |
| `PLNENDDATE` | Planlanan bitiş tarihi; sürüm doğrulaması gerekir |
| `ACTBEGDATE` | Gerçek başlangıç tarihi; sürüm doğrulaması gerekir |
| `ACTENDDATE` | Gerçek bitiş tarihi; sürüm doğrulaması gerekir |
| `STATUS` | Üretim emri statüsü |
| `ROUTINGREF` | Rota referansı |
| `BOMMASTERREF` / reçete bağlantıları | Reçete ilişkisi; sürüme göre isim/değer değişebilir |
| `PROJECTREF` | Proje referansı |
| `FACTORYNR` | İşyeri/fabrika bağlamı |
| `SOURCEINDEX` | Ambar/organizasyon bağlamı; senaryoya göre doğrulanmalı |

## Malzeme ilişkisi

Tipik ilişki:

```text
PRODORD.ITEMREF → ITEMS.LOGICALREF
```

Örnek:

```sql
SELECT
    P.LOGICALREF,
    P.FICHENO,
    P.DATE_,
    I.CODE AS MALZEME_KODU,
    I.NAME AS MALZEME_ADI,
    P.AMOUNT
FROM LG_040_01_PRODORD P
LEFT JOIN LG_040_ITEMS I
    ON I.LOGICALREF = P.ITEMREF;
```

## Planlanan ve gerçekleşen üretim

Üretim emri başlığındaki miktar, gerçekleşen stok hareketiyle birebir aynı kabul edilmemelidir.

Gerçek gerçekleşme için üretim emrine bağlı stok hareketleri, iş emirleri ve operasyon kayıtları ayrıca incelenmelidir.

Genel yaklaşım:

```text
PRODORD.AMOUNT
    = plan / hedef miktar

STLINE üretim girişleri
    = gerçek stok gerçekleşmesi
```

## Üretim emri ile stok hareketleri

Logo sürümüne ve üretim modülüne göre bağlantı alanları farklılaşabilir. Analizde yalnızca bir referansa güvenmek yerine aşağıdaki unsurlar birlikte incelenmelidir:

- üretim emri referansı,
- iş emri / operasyon referansı,
- `STLINE.PRODORDERREF`,
- fiş türü,
- `IOCODE`,
- malzeme referansı,
- tarih ve miktar.

Örnek keşif sorgusu:

```sql
SELECT
    S.LOGICALREF,
    S.STOCKREF,
    S.AMOUNT,
    S.TRCODE,
    S.IOCODE,
    S.PRODORDERREF
FROM LG_040_01_STLINE S
WHERE S.PRODORDERREF = @UretimEmriRef;
```

## Planlanan süre ve gerçekleşen süre

Üretim performansında yalnızca miktar değil süre de kritik metriktir.

Tipik analizler:

- planlanan başlangıç / bitiş,
- gerçek başlangıç / bitiş,
- operasyon bazlı süre,
- duruş süreleri,
- gerçekleşen miktar,
- birim başına üretim süresi.

Gerçek operasyon sürelerinin yalnızca `PRODORD` üzerinden çıkmayabileceği unutulmamalıdır.

## Üretim emri statüsü

`STATUS` alanı üretim emrinin yaşam döngüsünü anlamak için önemlidir; fakat enum değerleri sürüm bazında doğrulanmalıdır.

Genel iş akışı kavramsal olarak:

```text
Planlandı
   ↓
Serbest bırakıldı / başladı
   ↓
Üretim devam ediyor
   ↓
Tamamlandı
   ↓
Kapandı
```

Logo’nun gerçek statü değerleri dokümantasyon ve çalışan ortam ile doğrulanmalıdır.

## Reçete ve rota

Üretim emrinin kritik iki bağı:

```text
Üretim Emri
   ├─ Reçete / BOM
   └─ Rota / Operasyonlar
```

Bu ilişkiler üretim maliyeti ve malzeme ihtiyaç planlamasının temelini oluşturur.

## Proje ilişkisi

Proje bazlı üretimde `PROJECTREF` kritik olabilir.

Örnek:

```sql
SELECT
    P.FICHENO,
    P.PROJECTREF,
    I.CODE,
    P.AMOUNT
FROM LG_040_01_PRODORD P
LEFT JOIN LG_040_ITEMS I
    ON I.LOGICALREF = P.ITEMREF
WHERE P.PROJECTREF = @ProjectRef;
```

## Hata analizi yaklaşımı

Bir üretim emrinde sorun olduğunda yalnızca başlık kaydı kontrol edilmemelidir.

Kontrol sırası:

1. `PRODORD` başlığı var mı?
2. Üretilecek malzeme doğru mu?
3. Reçete ilişkisi doğru mu?
4. Rota / operasyon kayıtları var mı?
5. Sarf hareketleri oluşmuş mu?
6. Mamul giriş hareketi oluşmuş mu?
7. Seri/lot kayıtları tamam mı?
8. Maliyetlendirme hareketleri oluşmuş mu?

## Özet

`PRODORD`, üretim emrinin merkezidir ancak tek başına üretimin gerçekte ne olduğunu anlatmaz.

Doğru analiz:

```text
PRODORD
  + iş emirleri / operasyonlar
  + STLINE
  + seri/lot
  + reçete
  + maliyet
```

birlikte ele alınmalıdır.
