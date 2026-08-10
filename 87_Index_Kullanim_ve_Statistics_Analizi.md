# 87 — Index Kullanım ve Statistics Analizi

## 1. Amaç

Bir index'in var olması, faydalı olduğu anlamına gelmez.

Logo ERP veritabanlarında tablo sayısı ve veri hacmi büyüdükçe şu iki soruya cevap verilmelidir:

1. Hangi index'ler gerçekten kullanılıyor?
2. Optimizer satır sayılarını doğru tahmin edebiliyor mu?

## 2. Index Kullanımı

SQL Server DMV'leri üzerinden index kullanım eğilimleri incelenebilir.

Örnek yaklaşım:

```sql
SELECT
    OBJECT_NAME(I.object_id) AS TableName,
    I.name AS IndexName,
    U.user_seeks,
    U.user_scans,
    U.user_lookups,
    U.user_updates
FROM sys.indexes I
LEFT JOIN sys.dm_db_index_usage_stats U
    ON U.object_id = I.object_id
   AND U.index_id = I.index_id
   AND U.database_id = DB_ID()
WHERE I.object_id > 100;
```

## 3. Dikkat

`dm_db_index_usage_stats` verileri SQL Server restart sonrasında sıfırlanabilir.

Bu nedenle tek anlık snapshot üzerinden index silme kararı verilmemelidir.

## 4. Write Cost

Her ek index:

- INSERT
- UPDATE
- DELETE

işlemlerinin maliyetini artırır.

Logo'nun yoğun STLINE, CLFLINE veya benzeri hareket tablolarında gereksiz index sayısı yazma performansını etkileyebilir.

## 5. Duplicate / Overlapping Index

Benzer index'ler zaman içinde birikebilir.

Örnek:

```text
IX1: STOCKREF, DATE_
IX2: STOCKREF, DATE_, TRCODE
IX3: STOCKREF, DATE_ INCLUDE (AMOUNT)
```

Bu index'lerin üçü de gerekli olmayabilir.

Workload bazlı konsolidasyon yapılmalıdır.

## 6. Statistics Nedir?

Statistics optimizer'ın veri dağılımını anlamasına yardım eder.

Optimizer şu tip kararları statistics üzerinden verir:

- kaç satır gelecek?
- seek mi scan mi?
- nested loops mu hash join mi?
- ne kadar memory grant gerekli?

## 7. Stale Statistics

Büyük hareket tablolarında veri hızlı büyüdüğünde statistics geride kalabilir.

Sonuç:

```text
Estimated Rows << Actual Rows
```

veya tersi olabilir.

## 8. Statistics Kontrolü

Statistics son güncelleme zamanı kontrol edilebilir.

Örnek:

```sql
SELECT
    OBJECT_NAME(S.object_id) AS TableName,
    S.name AS StatisticsName,
    STATS_DATE(S.object_id, S.stats_id) AS LastUpdated
FROM sys.stats S
WHERE S.object_id = OBJECT_ID('LG_040_01_STLINE');
```

## 9. Fullscan Her Zaman Gerekli Değildir

`UPDATE STATISTICS ... WITH FULLSCAN` daha doğru histogram üretebilir ancak büyük tablolarda pahalıdır.

Bakım planı workload ve bakım penceresine göre tasarlanmalıdır.

## 10. Column Correlation

Optimizer bazı kolonlar arasındaki ilişkiyi tam anlayamayabilir.

Örneğin:

```text
TRCODE + DATE_ + SOURCEINDEX
```

birbirinden bağımsız dağılmıyorsa tahmin hataları oluşabilir.

## 11. Logo Raporlarında Tipik Sorun

Bir sorgu şu filtreleri kullanıyor olabilir:

```sql
WHERE STOCKREF = @StockRef
  AND DATE_ BETWEEN @Date1 AND @Date2
  AND CANCELLED = 0
```

Bazı stok kartları milyonlarca hareket içerirken bazıları yalnızca birkaç satır içeriyorsa tek plan her parametre için ideal olmayabilir.

Bu durum parameter sniffing ile birleşebilir.

## 12. Bakım Prensibi

Index bakımında yalnızca fragmentation oranına göre otomatik rebuild yapmak yeterli değildir.

Şunlar birlikte değerlendirilmelidir:

- index size
- usage
- fragmentation
- page count
- write cost
- query workload

## 13. Temel Prensip

> Index tasarımı ve statistics yönetimi birbirinden ayrı değildir; optimizer doğru veri dağılımını görmeden doğru index'i bile verimli kullanamayabilir.
