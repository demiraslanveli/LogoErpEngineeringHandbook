# 82 — TempDB, PAGELATCH ve I/O Teşhisi

Logo ERP kullanılan yoğun SQL Server sistemlerinde performans sorunu yalnızca ana veritabanından kaynaklanmaz. `tempdb`, allocation contention, storage gecikmesi ve latch beklemeleri sistem genelinde yavaşlığa neden olabilir.

## PAGELATCH nedir?

`PAGELATCH_*` beklemeleri disk I/O beklemesi değildir. Bellekte bulunan veri sayfalarına erişimde oluşan latch contention'ı gösterir.

Sık görülen türler:

- `PAGELATCH_EX`
- `PAGELATCH_UP`
- `PAGELATCH_SH`

## İlk kontrol

```sql
SELECT
    wait_type,
    waiting_tasks_count,
    wait_time_ms / 1000.0 AS Bekleme_Sn
FROM sys.dm_os_wait_stats
WHERE wait_type LIKE 'PAGELATCH%'
ORDER BY wait_time_ms DESC;
```

## tempdb neden önemlidir?

`tempdb` şu işlemlerde yoğun kullanılır:

- sort,
- hash join,
- temporary table,
- table variable,
- version store,
- spool,
- büyük rapor sorguları,
- bazı cursor ve ara sonuç işlemleri.

## tempdb dosya kontrolü

```sql
SELECT
    name,
    type_desc,
    physical_name,
    size * 8.0 / 1024 AS Boyut_MB,
    growth
FROM tempdb.sys.database_files;
```

## Dosya büyüme yaklaşımı

Yüzde bazlı autogrowth yerine sabit MB/GB büyüme daha öngörülebilirdir.

Örnek prensip:

```text
Başlangıç boyutu: gerçek iş yüküne uygun önceden ayrılmış
Autogrowth: sabit MB/GB
Dosyalar: eşit boyut ve eşit büyüme
```

## Çoklu tempdb data file

Yoğun allocation contention görülen sistemlerde birden fazla eşit boyutlu tempdb data file yararlı olabilir. Dosya sayısı sistemin CPU sayısı, SQL Server sürümü ve gerçek contention ölçümüyle belirlenmelidir.

## I/O gecikmesi kontrolü

```sql
SELECT
    DB_NAME(vfs.database_id) AS Veritabani,
    mf.name AS Dosya,
    mf.type_desc,
    CASE WHEN vfs.num_of_reads = 0 THEN 0
         ELSE vfs.io_stall_read_ms * 1.0 / vfs.num_of_reads END AS Ortalama_Okuma_ms,
    CASE WHEN vfs.num_of_writes = 0 THEN 0
         ELSE vfs.io_stall_write_ms * 1.0 / vfs.num_of_writes END AS Ortalama_Yazma_ms
FROM sys.dm_io_virtual_file_stats(NULL,NULL) vfs
JOIN sys.master_files mf
  ON mf.database_id = vfs.database_id
 AND mf.file_id = vfs.file_id
ORDER BY Ortalama_Okuma_ms DESC;
```

## PAGELATCH ile PAGEIOLATCH farkı

- `PAGELATCH_*` → bellek içi latch contention
- `PAGEIOLATCH_*` → diskten sayfa getirilmesini bekleme

Bu ikisi aynı problem değildir.

## Logo ortamında saha yaklaşımı

Aşağıdaki kombinasyon ciddi performans problemi oluşturabilir:

```text
tempdb küçük
+
% autogrowth
+
tek data file
+
yoğun paralel rapor
+
yavaş storage
```

## Kontrol sırası

1. Wait stats
2. tempdb dosya sayısı/boyutu
3. autogrowth
4. I/O latency
5. yoğun tempdb kullanan sorgular
6. execution plan
7. gereksiz sort/hash/spill

## Disk tipi çıkarımı

Sadece nominal olarak SSD olması yeterli değildir. Gerçek karar I/O latency ve throughput ölçümüyle verilmelidir.

> Tempdb sorunlarında dosya büyütmek tek başına çözüm değildir; contention, sorgu davranışı ve storage birlikte analiz edilmelidir.
