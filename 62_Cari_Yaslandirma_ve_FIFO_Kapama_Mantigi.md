# 62 — Cari Yaşlandırma ve FIFO Kapama Mantığı

## Amaç

Bu bölüm cari hesap bakiyelerinin yaşlandırılması, borç/alacak hareketlerinin kapatılması ve FIFO tabanlı kalan hesaplamasının nasıl ele alınması gerektiğini açıklar.

## Temel problem

Cari yaşlandırma yalnızca `DATE_` alanına göre hareketleri gruplamak değildir. Doğru sonuç için ödeme/mahsup hareketlerinin hangi borcu ne ölçüde kapattığı dikkate alınmalıdır.

## FIFO yaklaşımı

FIFO kapamada en eski açık borç önce kapatılır.

```text
Borç 1: 1000 TL - 01.01.2026
Borç 2: 2000 TL - 15.01.2026
Ödeme : 1500 TL - 01.02.2026
```

Sonuç:

```text
Borç 1 kalan = 0
Borç 2 kalan = 1500
```

## Yaşlandırma segmentleri

Sık kullanılan örnek segmentler:

- 0–30 gün
- 31–60 gün
- 61–90 gün
- 91–120 gün
- 121–180 gün
- 181–360 gün
- 360+ gün

Segment sınırları şirket politikasına göre değişebilir.

## Veri kaynağı

Cari hareket analizinde tipik olarak `CLFLINE` temel tablodur. Ancak doğru kapama ve kaynak belge analizi için `MODULENR`, `TRCODE`, kaynak referansları ve gerektiğinde fatura/muhasebe ilişkileri birlikte değerlendirilmelidir.

## FIFO algoritması

1. Cari hareketleri tarih ve sıra bazında sırala.
2. Borç hareketlerini açık bakiye olarak kuyruğa ekle.
3. Alacak/ödeme geldiğinde en eski açık borçtan başlayarak kapat.
4. Kısmi kapanışları sakla.
5. Kalan borçları referans tarihe göre yaşlandır.

## SQL tasarım notu

FIFO kapama karmaşık raporlarda yalnızca büyük bir SELECT ile çözülmeye çalışılmamalıdır. Gerektiğinde:

- prosedür,
- geçici tablo,
- window function,
- kontrollü cursor/iteratif hesap,
- önceden hesaplanan kapama tablosu

kullanılabilir.

## Dikkat edilmesi gerekenler

- iade hareketleri
- mahsup fişleri
- dövizli hareketler
- kapama tarihi
- vade tarihi ile belge tarihi farkı
- devreden bakiyeler
- aynı gün çoklu hareket sırası

## Best Practice

Yaşlandırma raporunda toplam açık bakiye ile cari kartın gerçek bakiyesi mutlaka reconciliation edilmelidir. Yaşlandırma toplamı cari bakiye ile uyuşmuyorsa segment raporu doğru kabul edilmemelidir.

## Bilgi güven seviyesi

FIFO yaklaşımı: **Doğrulanmış muhasebe/raporlama pratiği**.
Logo alan ve kaynak ilişkileri: **Firma süreci ve sürüme göre doğrulanmalı**.
