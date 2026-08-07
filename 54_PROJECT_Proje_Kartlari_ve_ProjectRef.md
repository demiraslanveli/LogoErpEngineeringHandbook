# 54 — PROJECT Proje Kartları ve PROJECTREF

## Amaç

Logo ERP’de proje boyutu; stok, satınalma, satış, finans, üretim ve özel raporlama süreçlerini ortak bir iş kırılımı altında izlemek için kullanılabilir. Bu bölüm proje kartlarını ve hareket tablolarındaki `PROJECTREF` ilişkisinin nasıl ele alınması gerektiğini açıklar.

## Proje Kartı

Firma bazlı proje kartı tablosu genel olarak:

```text
LG_{FIRMA}_PROJECT
```

Örnek:

```text
LG_040_PROJECT
```

## Temel Alanlar

Sık kullanılan alanlar:

| Alan | Açıklama |
|---|---|
| LOGICALREF | Proje referansı |
| CODE | Proje kodu |
| NAME | Proje açıklaması; sürüme göre alan adı doğrulanmalı |
| ACTIVE | Aktif/pasif durumu; sürüme göre kontrol edilmeli |

## Hareketlerde PROJECTREF

Birçok hareket tablosunda proje referansı bulunabilir. Tipik mantık:

```text
HAREKET.PROJECTREF
    ↓
PROJECT.LOGICALREF
```

Örneğin stok hareketlerinde:

```text
STLINE.PROJECTREF
```

## Örnek Sorgu

```sql
SELECT
    L.LOGICALREF AS SATIR_REF,
    L.DATE_,
    L.STOCKREF,
    P.CODE AS PROJE_KODU,
    P.NAME AS PROJE_ADI,
    L.AMOUNT
FROM LG_040_01_STLINE L
LEFT JOIN LG_040_PROJECT P
    ON P.LOGICALREF = L.PROJECTREF
WHERE L.CANCELLED = 0;
```

Alan isimleri kullanılan sürümde doğrulanmalıdır.

## Proje Bazlı Stok Takibi

Proje bazında stok tüketimi veya kalan hesabı yapılırken sadece proje kartı ile malzeme eşleştirmek yeterli değildir. Hareket yönü, IOCODE, TRCODE, iade hareketleri ve iptal kayıtları da hesaba katılmalıdır.

Temel model:

```text
Planlanan proje miktarı
- proje çıkışları
+ proje iadeleri
= kalan proje ihtiyacı
```

## Üretim ile İlişki

Üretim hareketlerinde proje kullanılıyorsa üretim emri, sarf ve üretimden giriş hareketleri aynı proje bağlamında kontrol edilmelidir.

## Entegrasyon Tasarımı

Dış sistemlerde proje kendi ID’si ile tutuluyorsa doğrudan Logo `LOGICALREF` saklamak yerine eşleme tablosu kullanılabilir:

```text
DIS_PROJE_ID
LOGO_PROJE_REF
LOGO_PROJE_KODU
FIRMA_NO
AKTIF
```

## Sık Hatalar

- Proje kodu yerine `LOGICALREF` beklenen alana kod yazmak.
- Satır projesi ile başlık projesini aynı kabul etmek.
- İade hareketlerini kalan hesabına eklememek.
- İptal satırlarını dahil etmek.
- Firma değiştiğinde aynı `LOGICALREF` değerinin aynı projeyi temsil edeceğini varsaymak.

## Kontrol İlkesi

Proje raporlarında mümkün olduğunca aşağıdaki alanları birlikte gösterin:

```text
PROJE_REF
PROJE_KODU
PROJE_ADI
BELGE_NO
SATIR_REF
MALZEME
MIKTAR
HAREKET_YONU
```

Bu yapı hata analizini ciddi şekilde kolaylaştırır.
