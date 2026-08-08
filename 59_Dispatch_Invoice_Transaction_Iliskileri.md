# 59 — Dispatch / Invoice Transaction İlişkileri

## Amaç

Bu bölüm sipariş, irsaliye ve fatura zincirinde satırların birbirine nasıl bağlandığını ve entegrasyon geliştirirken hangi referansların izlenmesi gerektiğini açıklar.

## Temel belge zinciri

```text
ORFICHE / ORFLINE
        ↓
STFICHE / STLINE
        ↓
INVOICE / STLINE
        ↓
CLFLINE
        ↓
EMFICHE / EMFLINE
```

Her senaryoda tüm katmanlar oluşmayabilir. Örneğin sipariş stok hareketi değildir; irsaliye cari hareket üretmeyebilir; muhasebe fişi muhasebeleştirme ayarlarına ve işlem durumuna bağlıdır.

## Kritik bağlantı alanları

Satır bazında özellikle şu alanlar takip edilmelidir:

- `ORDFICHEREF`
- `ORDTRANSREF`
- `STFICHEREF`
- `INVOICEREF`
- `PREVLINEREF`
- `SOURCELINK`

Bu alanların anlamı belge tipine göre değişebildiği için tek bir alan üzerinden ilişki kurmak yerine belge zinciri birlikte analiz edilmelidir.

## Siparişten irsaliyeye

Bir sipariş satırı sevk edildiğinde stok satırında sipariş satırına ilişkin referans tutulabilir. Bu sayede siparişin ne kadarının sevk edildiği hesaplanabilir.

Temel analiz:

```sql
SELECT
    O.LOGICALREF AS ORDER_LINE_REF,
    O.AMOUNT AS ORDER_AMOUNT,
    SUM(ISNULL(S.AMOUNT,0)) AS DISPATCHED_AMOUNT
FROM LG_XXX_01_ORFLINE O
LEFT JOIN LG_XXX_01_STLINE S
    ON S.ORDTRANSREF = O.LOGICALREF
GROUP BY O.LOGICALREF, O.AMOUNT;
```

> Gerçek join alanları ve iade etkileri kullanılan sürece göre kontrol edilmelidir.

## İrsaliyeden faturaya

Faturalanmış stok satırlarında `INVOICEREF` üzerinden fatura üst kaydına erişim yaygın bir ilişkidir.

```sql
SELECT
    S.LOGICALREF,
    S.STFICHEREF,
    S.INVOICEREF,
    I.FICHENO,
    I.DATE_
FROM LG_XXX_01_STLINE S
LEFT JOIN LG_XXX_01_INVOICE I
    ON I.LOGICALREF = S.INVOICEREF;
```

## Faturadan cari harekete

Fatura kaydının finansal etkisi `CLFLINE` tarafında oluşabilir. İlişki kurulurken `MODULENR`, `TRCODE` ve kaynak referans alanları birlikte değerlendirilmelidir.

## Faturadan muhasebeye

Muhasebeleştirilmiş belgelerde `ACCOUNTED`, muhasebe fişi referansı ve ilgili modül kaynak bilgileri kontrol edilmelidir. Muhasebe fişi yokluğu her zaman hata değildir; belge henüz muhasebeleştirilmemiş olabilir.

## Entegrasyon kontrol listesi

Bir dış sistem fatura oluşturduktan sonra yalnızca `INVOICE` tablosunun oluşmasını başarı kabul etmemelidir.

Kontrol edilmesi gerekenler:

1. INVOICE üst kayıt
2. STLINE satırları
3. STFICHE ilişkisi gerekiyorsa mevcut mu?
4. CLFLINE finansal hareket oluşmuş mu?
5. Seri/lot bağlantıları doğru mu?
6. Muhasebeleştirme gerekiyorsa EMFICHE/EMFLINE oluşmuş mu?

## Bilgi güven seviyesi

Belge zinciri ve temel tablo ilişkileri: **Doğrulanmış saha bilgisi**.
Alanların kesin semantiği: **TRCODE ve Logo sürümüne göre ayrıca doğrulanmalı**.
