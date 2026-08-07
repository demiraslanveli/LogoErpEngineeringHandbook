# 35 — SQL Server Performans Teşhis Rehberi

## 1. Amaç

Logo ERP veritabanlarında performans problemi çoğu zaman tek bir nedenden oluşmaz. Yavaşlık; disk I/O, tempdb, bellek baskısı, execution plan, index yapısı, blocking, istemci tarafı veri tüketimi veya büyük sorgular gibi farklı katmanlardan kaynaklanabilir.

Bu bölüm Logo ortamlarında pratik performans teşhis sırasını standartlaştırır.

## 2. Önce Darboğazı Sınıflandır

Performans sorununu önce şu kategorilerden birine yaklaştır:

- CPU
- Memory
- Disk I/O
- Blocking / locking
- tempdb contention
- Network / client consumption
- Execution plan
- Large scan
- Parameter sensitivity
- Application design

Her yavaş sorguya doğrudan index eklemek doğru yaklaşım değildir.

## 3. Aktif Sorgular

```sql
SELECT
    r.session_id,
    r.status,
    r.command,
    r.cpu_time,
    r.total_elapsed_time,
    r.logical_reads,
    r.reads,
    r.writes,
    r.wait_type,
    r.blocking_session_id,
    DB_NAME(r.database_id) AS DatabaseName,
    t.text
FROM sys.dm_exec_requests r
CROSS APPLY sys.dm_exec_sql_text(r.sql_handle) t
WHERE r.session_id <> @@SPID
ORDER BY r.total_elapsed_time DESC;
```

Bu sorgu ilk teşhis için iyi başlangıç noktasıdır.

## 4. Wait Statistics

```sql
SELECT TOP 30
    wait_type,
    waiting_tasks_count,
    wait_time_ms / 1000.0 AS wait_time_sec
FROM sys.dm_os_wait_stats
ORDER BY wait_time_ms DESC;
```

### ASYNC_NETWORK_IO

SQL Server sonucu üretmiş olabilir ancak istemci veriyi yavaş tüketiyordur.

Olası nedenler:

- Çok büyük result set
- Excel / Power BI istemcisi
- Uygulamanın veriyi yavaş okuması
- Network gecikmesi

### PAGEIOLATCH_*

Verinin diskten belleğe okunması sırasında bekleme olabilir. Disk gecikmesi veya buffer cache baskısı araştırılır.

### PAGELATCH_*

Disk I/O değildir; bellek içi page latch contention gösterebilir. `tempdb` önemli adaylardan biridir.

### RESOURCE_SEMAPHORE

Memory grant bekleyen sorgular olabilir.

## 5. tempdb Kontrolü

```sql
SELECT
    name,
    type_desc,
    physical_name,
    size / 128.0 AS SizeMB,
    growth,
    is_percent_growth
FROM tempdb.sys.database_files;
```

Genel öneriler:

- tempdb data dosyalarını dengeli yapılandır
- dosyaları eşit boyutlandır
- kontrollü sabit büyüme kullanmayı değerlendir
- disk latency ölç
- gereksiz küçük başlangıç boyutundan kaçın

Dosya sayısı workload ve contention durumuna göre belirlenmelidir.

## 6. Disk Latency

```sql
SELECT
    DB_NAME(vfs.database_id) AS DatabaseName,
    mf.name,
    mf.type_desc,
    mf.physical_name,
    CASE WHEN vfs.num_of_reads = 0 THEN 0
         ELSE vfs.io_stall_read_ms * 1.0 / vfs.num_of_reads END AS AvgReadMs,
    CASE WHEN vfs.num_of_writes = 0 THEN 0
         ELSE vfs.io_stall_write_ms * 1.0 / vfs.num_of_writes END AS AvgWriteMs
FROM sys.dm_io_virtual_file_stats(NULL, NULL) vfs
JOIN sys.master_files mf
    ON mf.database_id = vfs.database_id
   AND mf.file_id = vfs.file_id
ORDER BY AvgReadMs DESC;
```

Sürekli onlarca milisaniye seviyelerinde I/O gecikmesi görülüyorsa depolama katmanı ayrıca incelenmelidir.

## 7. SQL Server Bellek Ayarı

```sql
EXEC sp_configure 'max server memory (MB)';
```

Ayrıca:

```sql
SELECT
    physical_memory_in_use_kb / 1024 AS SQLMemoryMB,
    process_physical_memory_low,
    process_virtual_memory_low
FROM sys.dm_os_process_memory;
```

`process_physical_memory_low = 1` görülürse memory pressure araştırılmalıdır.

## 8. Memory Grants

```sql
SELECT
    session_id,
    requested_memory_kb,
    granted_memory_kb,
    required_memory_kb,
    wait_time_ms
FROM sys.dm_exec_query_memory_grants
ORDER BY requested_memory_kb DESC;
```

Sürekli bekleyen memory grant talepleri varsa sorgu planları ve bellek yapılandırması birlikte incelenmelidir.

## 9. Blocking

```sql
SELECT
    session_id,
    blocking_session_id,
    wait_type,
    wait_time,
    wait_resource
FROM sys.dm_exec_requests
WHERE blocking_session_id <> 0;
```

Blocking durumunda önce blocker oturumun yaptığı işlem, açık transaction süresi ve uygulama etkisi anlaşılmalıdır.

## 10. Logical Reads

Bir sorgunun süresi kısa görünse bile çok yüksek logical read üretiyorsa ölçek büyüdüğünde problem oluşturabilir.

```sql
SET STATISTICS IO ON;
SET STATISTICS TIME ON;
```

ile test yapılabilir.

## 11. Execution Plan

Plan üzerinde özellikle şunlar incelenmelidir:

- Table Scan
- Clustered Index Scan
- Key Lookup
- Sort
- Hash Match
- Spool
- Implicit Conversion
- Missing Index önerileri

Missing index önerileri mevcut index yapısı değerlendirilmeden doğrudan uygulanmamalıdır.

## 12. Parameter Sensitivity

Aynı stored procedure bazı parametrelerde hızlı bazı parametrelerde çok yavaşsa plan hassasiyeti araştırılmalıdır.

Kontrol:

- farklı parametrelerle execution plan karşılaştır
- actual row / estimated row farkına bak
- plan cache davranışını incele

Çözüm senaryoya göre değişebilir:

- `OPTION (RECOMPILE)`
- `OPTIMIZE FOR`
- query rewrite
- uygun index

## 13. Büyük Result Set

Özellikle Excel ve Power BI sorgularında fazla kolon ve satır çekmek istemci tüketimini yavaşlatabilir.

Kaçınılması gereken yaklaşım:

```sql
SELECT *
FROM LG_040_01_STLINE;
```

Daha kontrollü yaklaşım:

```sql
SELECT
    DATE_,
    STOCKREF,
    AMOUNT,
    PRICE
FROM LG_040_01_STLINE
WHERE DATE_ >= @StartDate
  AND DATE_ < DATEADD(DAY, 1, @EndDate);
```

## 14. SARGable Tarih Filtreleri

Daha zayıf yaklaşım:

```sql
WHERE YEAR(DATE_) = 2026
```

Daha iyi yaklaşım:

```sql
WHERE DATE_ >= '20260101'
  AND DATE_ < '20270101'
```

Bu yapı uygun index kullanımını kolaylaştırabilir.

## 15. Fonksiyon Kullanımı

JOIN veya WHERE tarafında kolona fonksiyon uygulamak index kullanımını zorlaştırabilir.

Örnek:

```sql
WHERE LEFT(CODE, 3) = '150'
```

İş kuralı uygunsa range filtresi değerlendirilebilir.

## 16. Karmaşık Joinler

Büyük tablolarda `CASE`, `OR`, nested view veya scalar function içeren joinler performansı düşürebilir.

Alternatifler:

- `UNION ALL` ile ayrıştırma
- yardımcı mapping tablo
- hesaplanmış kolon
- sorgu yeniden yazımı
- uygun index

## 17. Index Tasarımı

Index tasarlarken şu sorular sorulmalıdır:

```text
WHERE hangi kolonlarda?
JOIN hangi kolonlarda?
ORDER BY / GROUP BY ne?
SELECT'te hangi kolonlar dönüyor?
Cardinality nasıl?
Write maliyeti ne?
```

Her sorguya ayrı index oluşturmak uzun vadede write performansını bozabilir.

## 18. Logo Tablolarında Dikkat

Logo'nun kendi index yapısına müdahale etmeden önce:

- ürün güncelleme etkisi
- support politikası
- upgrade davranışı
- insert/update maliyeti

kontrol edilmelidir.

Özel raporlar için kontrollü custom index kullanılabilir.

## 19. View Performansı

View kendi başına performans garantisi değildir. Özellikle şu yapılar varsa actual execution plan incelenmelidir:

- çok sayıda `LEFT JOIN`
- nested view
- scalar function
- `DISTINCT`
- `GROUP BY`
- `CASE` içeren join

## 20. Teşhis Sırası

```text
1. Sorunu tekrar üret
2. Aktif session / wait kontrol et
3. Blocking var mı bak
4. IO ve CPU ölç
5. Execution plan al
6. Disk latency kontrol et
7. Memory / tempdb kontrol et
8. Query rewrite veya index uygula
9. Önce/sonra ölç
```

## 21. Ölçmeden Değişiklik Yapma

Her optimizasyonda önce baseline al:

```text
Duration
CPU
Logical Reads
Physical Reads
Rows
Plan
Wait type
```

Sonra aynı metriklerle tekrar ölç.

## 22. Özet

Logo SQL performans problemlerinde çözüm tahminle değil ölçümle bulunmalıdır. Wait statistics, execution plan, logical reads, disk latency, tempdb, memory ve istemci davranışı birlikte değerlendirilmelidir.
