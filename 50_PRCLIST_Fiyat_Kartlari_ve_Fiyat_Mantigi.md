# 50 — PRCLIST Fiyat Kartları ve Fiyat Mantığı

## Amaç

Bu bölüm Logo ERP’de malzeme ve hizmet fiyatlarının nasıl tutulduğunu, `PRCLIST` tablosunun raporlama ve entegrasyonlarda nasıl ele alınması gerektiğini açıklar.

> Fiyat kartı, yalnızca `PRICE` alanından ibaret değildir. Fiyat türü, başlangıç/bitiş tarihi, cari hesap, ödeme planı, birim, döviz ve öncelik koşulları birlikte değerlendirilmelidir.

## Temel Tablo

Firma bazlı yapı genel olarak:

```text
LG_{FIRMA}_PRCLIST
```

Örnek:

```text
LG_040_PRCLIST
```

## Sık Kullanılan Alanlar

| Alan | Açıklama |
|---|---|
| LOGICALREF | Kayıt referansı |
| CARDREF | Malzeme / hizmet kartı referansı |
| PTYPE | Fiyat türü |
| PRICE | Fiyat |
| UOMREF | Birim referansı |
| CURRENCY | Döviz türü |
| BEGDATE | Başlangıç tarihi |
| ENDDATE | Bitiş tarihi |
| PAYPLANREF | Ödeme planı referansı |
| CLIENTCODE | Cari koşulu; sürüme göre kullanım kontrol edilmelidir |
| ACTIVE | Aktif/pasif durumu; sürüm ve ürün ailesine göre kontrol edilmelidir |

## Fiyat Türü

`PTYPE` alanı fiyatın alış mı satış mı olduğunu belirleyen temel alanlardan biridir. Kullanılan Logo sürümünde kesin değerler doğrulanmalıdır.

Rapor geliştirirken `PTYPE` değerlerini sabit ezberle kullanmak yerine çalışan ortamdan örnek kayıtlarla doğrulamak güvenlidir.

## Malzeme ile İlişki

```sql
SELECT
    I.CODE,
    I.NAME,
    P.PRICE,
    P.PTYPE,
    P.BEGDATE,
    P.ENDDATE
FROM LG_040_ITEMS I
INNER JOIN LG_040_PRCLIST P
    ON P.CARDREF = I.LOGICALREF;
```

Bu sorgu başlangıç örneğidir. Üretim ortamında aktiflik, tarih, birim ve fiyat türü koşulları eklenmelidir.

## Geçerli Fiyatı Bulma

Bir tarihte geçerli fiyat aranıyorsa tarih koşulu mutlaka açık yazılmalıdır:

```sql
DECLARE @Tarih DATE = GETDATE();

SELECT
    P.*
FROM LG_040_PRCLIST P
WHERE @Tarih >= P.BEGDATE
  AND (@Tarih <= P.ENDDATE OR P.ENDDATE IS NULL);
```

Logo sürümünde boş tarihlerin nasıl tutulduğu kontrol edilmelidir.

## Birim Konusu

Malzemenin fiyatı ana birimden farklı bir birime bağlı olabilir. Bu nedenle `PRICE` doğrudan ana birim fiyatıdır varsayımı yapılmamalıdır.

Kontrol zinciri:

```text
PRCLIST.UOMREF
    ↓
UNITSETL.LOGICALREF
    ↓
ITMUNITA üzerinden malzeme-birim ilişkisi
```

## Son Alış Fiyatı ile Karıştırılmamalı

`PRCLIST` kart fiyatıdır. Gerçekleşmiş son alış fiyatı ise çoğu senaryoda `STLINE` veya fatura/stok hareketlerinden hesaplanır.

Dolayısıyla:

```text
PRCLIST = tanımlı fiyat
STLINE.PRICE = gerçekleşmiş hareket fiyatı
```

Bu ikisi farklı iş ihtiyaçlarına cevap verir.

## Dövizli Fiyatlar

Dövizli fiyat analizinde yalnızca `PRICE` yeterli değildir. `CURRENCY`, işlem kuru, rapor tarihi ve gerekiyorsa fiyatın dövizli/net karşılığı birlikte ele alınmalıdır.

## Performans

Fiyat sorgularında tipik filtreler:

- `CARDREF`
- `PTYPE`
- tarih aralığı
- `UOMREF`

Çok büyük fiyat listelerinde sorgu planı kontrol edilmeli, mevcut indeksler incelenmeden rastgele indeks eklenmemelidir.

## Entegrasyon İlkesi

Fiyat kartı oluşturma/güncelleme işleminde mümkün olduğunda Logo Objects kullanılmalıdır. Doğrudan SQL update, Logo’nun iş kuralları veya sürüm farklılıklarını atlayabileceği için kontrollü istisna olarak ele alınmalıdır.

## Kontrol Listesi

Bir fiyat sorgusu yazarken:

1. Firma doğru mu?
2. Malzeme `CARDREF` ile doğru bağlandı mı?
3. Fiyat türü doğru mu?
4. Tarih geçerliliği kontrol edildi mi?
5. Birim doğru mu?
6. Döviz dikkate alındı mı?
7. Cari/ödeme planı gibi özel koşullar var mı?

Bu kontroller yapılmadan bulunan ilk `PRICE` değerini “malzemenin fiyatı” olarak kabul etmek hatalı sonuç üretebilir.
