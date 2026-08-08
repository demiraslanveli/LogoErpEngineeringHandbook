# 60 — Döviz Alanları ve Kur Mantığı

## Amaç

Logo ERP belgelerinde işlem dövizi, raporlama dövizi ve yerel para tutarlarının birbirine karıştırılmasını önlemek.

## Temel alan ailesi

Logo tablolarında döviz işlemleri çoğunlukla aşağıdaki alan aileleriyle izlenir:

- `TRCURR`
- `TRRATE`
- `REPORTRATE`
- yerel para tutarları
- işlem dövizi tutarları
- raporlama dövizi tutarları

Alanların kesin kullanımı tabloya ve sürüme göre doğrulanmalıdır.

## TRCURR

İşlem döviz türünü temsil eder. Sayısal değerlerin hangi para birimine karşılık geldiği Logo döviz tanımlarından kontrol edilmelidir.

## TRRATE

İşlem dövizi kurudur. Birim fiyat analizlerinde yalnızca `PRICE` alanına bakmak ciddi hata üretebilir; fiyatın hangi para biriminde tutulduğu ve `TRRATE` ile nasıl çevrildiği birlikte değerlendirilmelidir.

## REPORTRATE

Raporlama dövizi kuru için kullanılan alandır. Raporlama dövizi ile işlem dövizi aynı kavram değildir.

## Son alış fiyatı örneği

USD bazlı son alış fiyatı hesaplanırken şu bilgiler birlikte incelenmelidir:

```text
PRICE
TRCURR
TRRATE
DATE_
INVOICEREF
STFICHEREF
UINFO1 / UINFO2
```

Birim dönüşümü yapılmadan doğrudan `PRICE / TRRATE` gibi tek formül kullanmak her senaryoda doğru değildir.

## Kontrol sorgusu

```sql
SELECT
    LOGICALREF,
    STOCKREF,
    DATE_,
    PRICE,
    TRCURR,
    TRRATE,
    REPORTRATE,
    UINFO1,
    UINFO2,
    INVOICEREF,
    STFICHEREF
FROM LG_XXX_01_STLINE
WHERE STOCKREF = @StockRef
ORDER BY DATE_ DESC, LOGICALREF DESC;
```

## Best Practice

Dövizli fiyat karşılaştırmalarında üç ayrı normalizasyon gerekebilir:

1. fiyat para birimi normalizasyonu
2. birim normalizasyonu
3. tarih/kura göre normalizasyon

## Bilgi güven seviyesi

Temel alan ailesi: **Saha bilgisi**.
Kur ve fiyat formülleri: **Belge tipi, döviz türü ve sürüm bazında doğrulanmalı**.
