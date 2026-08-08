# 61 — KDV, İstisna ve Muafiyet Alanları

## Amaç

Bu bölüm KDV oranı sıfır olan satırlarda istisna/muafiyet bilgilerinin nasıl ele alınması gerektiğini ve entegrasyonlarda hangi kontrollerin yapılmasını açıklar.

## Kritik alanlar

Logo stok/fatura satırlarında KDV işlemleri için özellikle şu alanlar önemlidir:

- `VAT`
- `VATAMNT`
- `VATEXCEPTCODE`
- `VATEXCEPTREASON`

Alanların kesin davranışı kullanılan Logo sürümünden doğrulanmalıdır.

## Temel kural

`VAT = 0` olması tek başına yeterli veri değildir. İşlem gerçekten KDV istisnasına giriyorsa ilgili istisna kodu ve açıklaması da doğru oluşturulmalıdır.

## Saha örneği

KDV oranı 0 olup muafiyet sebebi boş bırakılan satırlarda kayıt öncesinde kullanıcıya uyarı verilmesi ve seçilen istisna koduna göre açıklamanın satıra yazılması veri kalitesini artırır.

Örnek kod/açıklama eşleşmeleri saha ortamında ayrıca tanımlanabilir:

```text
231 -> 17/4-g ...
301 -> 11/1-a Mal İhracatı
335 -> Basılı Kitap ve Süreli Yayınların Teslimleri
351 -> İstisna Olmayan Diğer
```

> Bu örnekler müşteri uygulamasından gelen saha bilgisidir; mevzuat ve Logo tanımları güncel ortamdan ayrıca doğrulanmalıdır.

## Kontrol sorgusu

```sql
SELECT
    LOGICALREF,
    STOCKREF,
    VAT,
    VATAMNT,
    VATEXCEPTCODE,
    VATEXCEPTREASON
FROM LG_XXX_01_STLINE
WHERE LINETYPE = 0
  AND VAT = 0;
```

## Validasyon önerisi

```text
VAT = 0 ?
  ↓ Evet
İşlem istisna mı?
  ├─ Evet -> VATEXCEPTCODE + VATEXCEPTREASON zorunlu
  └─ Hayır -> şirket kuralına göre kontrol
```

## Best Practice

- İstisna açıklamasını serbest metin olarak her kullanıcıya bırakmayın.
- Kod–açıklama eşleşmesini merkezi tanımdan yönetin.
- Fatura kaydedilmeden önce satır bazında kontrol edin.
- Doğrudan SQL ile alan doldurmak yerine mümkünse resmi nesne akışını kullanın.

## Bilgi güven seviyesi

Alan adları ve saha kullanım örneği: **Doğrulanmış saha bilgisi**.
Vergisel anlam ve kodların güncelliği: **Mevzuat/Logo tanımlarından ayrıca doğrulanmalı**.
