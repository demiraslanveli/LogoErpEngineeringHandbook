# 51 — PAYPLANS Ödeme Planı Mantığı

## Amaç

Bu bölüm Logo ERP’de ödeme planı tanımlarının fiyat, cari hesap, sipariş ve fatura süreçlerindeki rolünü açıklar.

Ödeme planı yalnızca vade açıklaması değildir. Fatura tarihinden itibaren ödeme tarihinin, taksitlerin ve ticari koşulların belirlenmesinde kullanılan iş kuralıdır.

## Temel Tablo

Firma bazlı ödeme planı kartları genel olarak:

```text
LG_{FIRMA}_PAYPLANS
```

Örnek:

```text
LG_040_PAYPLANS
```

Detay satırları ve hesaplama tabloları ürün/sürüm yapısına göre ayrıca incelenmelidir.

## Temel Alanlar

| Alan | Açıklama |
|---|---|
| LOGICALREF | Ödeme planı referansı |
| CODE | Ödeme planı kodu |
| DEFINITION_ | Açıklama |
| ACTIVE | Aktiflik bilgisi; sürüm yapısı doğrulanmalı |

## Cari Kart ile İlişki

Cari hesap kartında ödeme planı referansı tutulabilir. Tipik ilişki:

```text
CLCARD.PAYMENTREF
    ↓
PAYPLANS.LOGICALREF
```

Alan adı veya davranış sürüme göre doğrulanmalıdır.

Örnek rapor mantığı:

```sql
SELECT
    C.CODE AS CARI_KODU,
    C.DEFINITION_ AS CARI_ADI,
    P.CODE AS VADE_KODU,
    P.DEFINITION_ AS VADE_ACIKLAMASI
FROM LG_040_CLCARD C
LEFT JOIN LG_040_PAYPLANS P
    ON P.LOGICALREF = C.PAYMENTREF;
```

## Fatura ve Siparişlerde Ödeme Planı

Bir belgenin ödeme planı, cari kart varsayılanından gelebilir ancak belge üzerinde değiştirilebilir. Bu nedenle rapor ihtiyacında “cari kartın vadesi” ile “faturanın vadesi” aynı kabul edilmemelidir.

Doğru soru şudur:

```text
Bu raporda kart varsayılanı mı,
yoksa işlem anında belgeye aktarılmış ödeme planı mı isteniyor?
```

## Vade Tarihi

Ödeme planı kodu ile gerçek vade tarihi aynı şey değildir.

```text
Ödeme planı = kural
Vade tarihi = kuralın işlem tarihine uygulanmış sonucu
```

Cari yaşlandırma ve tahsilat raporlarında mümkün olduğunca gerçek finansal hareket/vade bilgisi kullanılmalıdır.

## PRCLIST ile İlişki

Fiyat kartları bazı senaryolarda ödeme planına bağlı olabilir:

```text
PRCLIST.PAYPLANREF
    ↓
PAYPLANS.LOGICALREF
```

Bu, aynı malzeme için farklı ödeme koşullarında farklı fiyat tanımlanmasına imkan verir.

Dolayısıyla “malzemenin satış fiyatı” sorgusu yazılırken ödeme planı filtresi göz ardı edilirse birden fazla geçerli fiyat bulunabilir.

## Entegrasyon Kontrolü

Sipariş veya fatura aktarırken ödeme planı kullanılacaksa:

1. Koddan `LOGICALREF` çözülmeli.
2. Plan aktif mi kontrol edilmeli.
3. Belge tipinin ilgili alanına doğru referans yazılmalı.
4. Logo Objects kullanılıyorsa field adı kullanılan DataObject üzerinde doğrulanmalı.
5. Sonuç kaydedildikten sonra oluşan cari/vade hareketleri kontrol edilmeli.

## Sık Hatalar

- Cari kart ödeme planını fatura vadesi sanmak.
- Kod ile `LOGICALREF` değerini karıştırmak.
- Fiyat sorgusunda `PAYPLANREF` koşulunu göz ardı etmek.
- İptal/kapalı ödeme planını entegrasyonda kullanmak.
- Ödeme planı değişince geçmiş faturaların da otomatik değişeceğini varsaymak.

## Referans İlkesi

Raporlarda mümkün olduğunca hem kodu hem açıklamayı gösterin:

```text
VADE_KODU
VADE_ACIKLAMASI
```

Bu yaklaşım kullanıcıların sonucu Logo ekranları ile hızlı doğrulamasını sağlar.
