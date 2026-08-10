# 78 — Index Stratejileri ve Sorgu Optimizasyonu

Logo veritabanlarında performans iyileştirmesi yaparken ilk refleks yeni index eklemek olmamalıdır. Amaç, gerçek sorgu yükünü ve erişim desenlerini ölçerek en az sayıda ve en etkili index tasarımını oluşturmaktır.

## Temel yaklaşım

- Önce yavaş sorgu belirlenir.
- Execution plan ve logical read miktarı incelenir.
- Filtre, join ve order by alanları ayrıştırılır.
- Mevcut indexler kontrol edilir.
- Gerekiyorsa dar ve hedefli nonclustered index oluşturulur.
- INCLUDE alanları yalnızca gerçekten gerekiyorsa eklenir.
- Yazma maliyeti ve index bakım yükü ölçülür.

## Logo tablolarında dikkat edilmesi gereken alanlar

Saha sorgularında sık görülen erişim alanları şunlardır:

- `LOGICALREF`
- `STOCKREF`
- `CLIENTREF`
- `INVOICEREF`
- `STFICHEREF`
- `ORDFICHEREF`
- `ORDTRANSREF`
- `DATE_`
- `TRCODE`
- `CANCELLED`
- `SOURCEINDEX`
- `PROJECTREF`

Ancak yalnızca alanın sık kullanılması index eklemek için yeterli değildir.

## Örnek kontrol sorgusu

```sql
SELECT
    OBJECT_NAME(i.object_id) AS Tablo,
    i.name AS IndexAdi,
    i.type_desc,
    i.is_unique,
    i.is_disabled
FROM sys.indexes i
WHERE i.object_id = OBJECT_ID('LG_040_01_STLINE')
ORDER BY i.index_id;
```

## Sargable sorgu yazımı

Kötü:

```sql
WHERE YEAR(DATE_) = 2026
```

Daha iyi:

```sql
WHERE DATE_ >= '20260101'
  AND DATE_ <  '20270101'
```

Kolon üzerinde fonksiyon kullanılması index seek ihtimalini azaltabilir.

## Dinamik firma/dönem tabloları

Çoklu firma/dönem raporlarında aynı sorgunun farklı `LG_XXX_YY_*` tablolarında çalışması gerekir. Index stratejisi yalnızca tek firma üzerinde değil, yüksek hacimli tüm firmalarda değerlendirilmelidir.

## Index eklemeden önce kontrol listesi

1. Sorgu gerçekten yavaş mı?
2. Asıl bekleme CPU mu, I/O mu, lock mu?
3. Mevcut index benzer alanları kapsıyor mu?
4. Filtre seçiciliği yeterli mi?
5. INCLUDE alanları gereksiz geniş mi?
6. Tablo yoğun INSERT/UPDATE alıyor mu?
7. Index boyutu ve fragmentation takibi yapılacak mı?

## Missing index önerileri

SQL Server missing-index DMV'leri yararlı ipucu verir ancak otomatik uygulanmamalıdır. Aynı tablo için çok sayıda birbirine benzeyen index önerisi oluşabilir.

## Logo Objects açısından önemli sınır

Index eklemek iş kuralını değiştirmez; ancak Logo'nun kendi indexlerini silmek/değiştirmek veya key yapısını bozmak desteklenmeyen sonuçlar doğurabilir.

> Özel index ekleme çalışmaları test ortamında ölçülmeli ve sürüm yükseltmelerinden sonra yeniden gözden geçirilmelidir.
