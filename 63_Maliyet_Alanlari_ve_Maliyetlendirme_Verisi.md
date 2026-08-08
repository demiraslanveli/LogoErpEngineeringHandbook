# 63 — Maliyet Alanları ve Maliyetlendirme Verisi

## Amaç

Bu bölüm stok ve üretim hareketlerinde maliyet bilgisinin hangi perspektiflerle ele alınması gerektiğini açıklar. Amaç tek bir `COST` alanı aramak yerine maliyetin belge, stok hareketi, üretim ve muhasebe boyutlarını birlikte değerlendirmektir.

## Maliyet neden tek alan değildir?

Logo ERP'de maliyet; işlem türü, stok yönü, üretim ilişkisi, dönem, maliyet yöntemi ve maliyetlendirme işlemlerine bağlıdır. Bu nedenle maliyet raporlarında aşağıdaki ayrımlar önemlidir:

- giriş maliyeti
- çıkış maliyeti
- birim maliyet
- toplam maliyet
- üretim emri maliyeti
- malzeme tüketim maliyeti
- işçilik / genel gider etkileri
- muhasebe maliyet hesapları

## STLINE perspektifi

Stok satırlarında maliyet analizi yapılırken hareket miktarı, yönü ve maliyet alanları birlikte değerlendirilmelidir.

Örnek analiz şablonu:

```sql
SELECT
    S.LOGICALREF,
    S.STOCKREF,
    S.TRCODE,
    S.IOCODE,
    S.AMOUNT,
    S.PRICE,
    S.DATE_,
    S.STFICHEREF,
    S.INVOICEREF,
    S.PRODORDERREF
FROM LG_XXX_01_STLINE S
WHERE S.STOCKREF = @StockRef;
```

Kullanılan Logo sürümündeki gerçek maliyet alanları ayrıca doğrulanmalıdır.

## Üretim maliyeti

Bir üretim emrinin maliyeti yalnızca mamul giriş satırından okunmamalıdır. En az şu unsurlar birlikte analiz edilmelidir:

1. sarf edilen hammaddeler
2. yarı mamuller
3. üretimden giriş miktarı
4. fire
5. işçilik / operasyon maliyetleri
6. genel gider dağıtımları
7. varsa fason işlemler

## Maliyet sapması

Planlanan ve gerçekleşen tüketim karşılaştırması:

```text
Planlanan Miktar
Gerçekleşen Miktar
Fark Miktarı
Birim Maliyet
Maliyet Sapması = Fark Miktarı × Birim Maliyet
```

## Dönem etkisi

Maliyet analizleri dönem bağımlıdır. Dönem kapanışı, maliyetlendirme çalıştırılması ve sonradan gelen düzeltme hareketleri rapor sonucunu değiştirebilir.

## Reconciliation

Maliyet raporu aşağıdaki kaynaklarla karşılaştırılmalıdır:

- stok hareketleri
- üretim emri gerçekleşmeleri
- maliyetlendirme sonuçları
- muhasebe fişleri

## Best Practice

- Maliyet alanlarını sürüm doğrulaması olmadan sabit kabul etmeyin.
- Üretim maliyetinde `PRODORDERREF` ilişkisini koruyun.
- İade ve düzeltme hareketlerini ayrı ele alın.
- Birim dönüşümlerini maliyetten önce normalize edin.
- Dövizli alımlarda kur normalizasyonunu unutmayın.

## Bilgi güven seviyesi

Maliyet analiz yaklaşımı: **Doğrulanmış saha/mühendislik pratiği**.
Kesin maliyet kolonları: **Logo sürümü ve tablo şemasından doğrulanmalı**.
