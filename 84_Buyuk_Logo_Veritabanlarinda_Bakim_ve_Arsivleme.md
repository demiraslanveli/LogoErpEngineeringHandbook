# 84 — Büyük Logo Veritabanlarında Bakım ve Arşivleme

Logo ERP veritabanları yıllar içinde yüz milyonlarca satıra ulaşabilir. Bu noktada yalnızca sorgu optimizasyonu yeterli değildir; veri yaşam döngüsü, bakım, arşivleme ve kapasite planlaması birlikte ele alınmalıdır.

## Temel hedefler

- aktif iş yükünü hızlı tutmak,
- geçmiş veriyi erişilebilir korumak,
- bakım sürelerini yönetilebilir seviyede tutmak,
- backup/restore sürelerini kontrol etmek,
- disk büyümesini öngörülebilir hale getirmek.

## Önce veri büyümesini ölç

```sql
SELECT
    t.name AS Tablo,
    SUM(p.rows) AS SatirSayisi
FROM sys.tables t
JOIN sys.partitions p
    ON p.object_id = t.object_id
WHERE p.index_id IN (0,1)
GROUP BY t.name
ORDER BY SatirSayisi DESC;
```

En büyük tablolar tespit edilmeden arşiv stratejisi oluşturulmamalıdır.

## Logo tarafında tipik büyük tablolar

Sisteme göre değişmekle birlikte şu tablolar hızlı büyüyebilir:

- `STLINE`
- `CLFLINE`
- `EMFLINE`
- `ORFLINE`
- seri/lot hareket tabloları
- entegrasyon log tabloları

## Bakım başlıkları

### İstatistik güncelleme

Yanlış cardinality tahminleri kötü execution plan üretebilir.

### Index bakımı

Her indexi her gece rebuild etmek doğru yaklaşım değildir.

Karar:
- fragmentation,
- index boyutu,
- kullanım sıklığı,
- bakım penceresi

ile verilmelidir.

### DBCC CHECKDB

Veri bütünlüğü kontrolü düzenli planlanmalıdır.

### Backup stratejisi

- full,
- differential,
- transaction log

politikası RPO/RTO ihtiyacına göre belirlenmelidir.

## Arşivleme yaklaşımı

Logo'nun resmi veri ilişkileri dikkate alınmadan `DELETE FROM LG_...` ile eski hareket silmek güvenli değildir.

Tercih edilen yaklaşım:

```text
Aktif Logo DB
    ↓
Read-only historical/reporting DB
    ↓
BI / raporlama katmanı
```

Arşivleme iş kuralları ve Logo ürün davranışıyla uyumlu tasarlanmalıdır.

## Partitioning

Çok büyük özel log tablolarında tarih bazlı partitioning faydalı olabilir.

Logo'nun standart tablolarında partitioning gibi fiziksel değişiklikler destek/sürüm etkileri değerlendirilmeden uygulanmamalıdır.

## Özel log tabloları için retention

Örnek:

```text
0–90 gün     → online
3–12 ay      → archive DB
12+ ay       → compressed archive
```

Süreler iş ve mevzuat ihtiyacına göre belirlenir.

## Dosya büyümesi

Data ve log dosyalarında küçük ve sık autogrowth yerine kontrollü başlangıç boyutu ve sabit büyüme tercih edilmelidir.

## Shrink konusu

`DBCC SHRINKDATABASE` rutin bakım değildir.

Shrink:
- fragmentation oluşturabilir,
- yoğun I/O yaratabilir,
- kısa süre sonra dosyanın tekrar büyümesine neden olabilir.

Yalnızca olağan dışı ve kalıcı veri küçülmesi sonrası kontrollü uygulanmalıdır.

## Kapasite raporu

Aylık olarak şu metrikler izlenebilir:

- DB boyutu,
- aylık büyüme,
- en büyük 20 tablo,
- log boyutu,
- backup boyutu/süresi,
- disk boş alanı,
- index boyutu,
- tempdb büyümesi.

## Reporting replica yaklaşımı

Yoğun rapor yükü üretim Logo veritabanını etkiliyorsa ayrı reporting veritabanı veya uygun replikasyon/ETL mimarisi değerlendirilebilir.

## Kritik prensip

Arşivleme performans projesi kadar veri bütünlüğü projesidir.

> Logo hareket tablolarında yalnızca disk alanı kazanmak amacıyla ilişkileri bilmeden veri silinmemelidir.
