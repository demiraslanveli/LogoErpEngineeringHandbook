# 83 — SQL Server Error 701 ve Bellek Baskısı

SQL Server Error 701 genel olarak sorgunun çalışması için gerekli belleğin ayrılamadığını gösteren ciddi bir memory pressure sinyalidir. Logo ERP gibi büyük ve uzun ömürlü veritabanlarında yüksek concurrency, büyük execution plan'lar, geniş cache kullanımı ve yanlış `max server memory` ayarları bu durumu tetikleyebilir.

## Error 701 ne anlama gelir?

Özet olarak:

```text
SQL Server gerekli internal memory kaynağını ayıramadı.
```

Bu, her zaman fiziksel RAM'in tamamen bittiği anlamına gelmez.

Sorun şu alanlardan kaynaklanabilir:

- buffer pool baskısı,
- plan cache büyümesi,
- memory grant talepleri,
- query compilation yükü,
- external component / CLR / linked server kullanımı,
- işletim sistemine yeterli RAM bırakılmaması.

## İlk kontrol: SQL Server memory ayarı

```sql
EXEC sp_configure 'max server memory (MB)';
EXEC sp_configure 'min server memory (MB)';
```

`max server memory` tüm fiziksel RAM'e çok yakın verilmemelidir. İşletim sistemi, SQL Server dışı servisler, backup, antivirus, driver ve diğer süreçler için pay bırakılmalıdır.

## Process memory kontrolü

```sql
SELECT
    physical_memory_in_use_kb / 1024 AS SQL_Memory_MB,
    locked_page_allocations_kb / 1024 AS LockedPages_MB,
    large_page_allocations_kb / 1024 AS LargePages_MB,
    memory_utilization_percentage,
    process_physical_memory_low,
    process_virtual_memory_low
FROM sys.dm_os_process_memory;
```

## Memory clerk analizi

```sql
SELECT TOP (30)
    type,
    SUM(pages_kb) / 1024.0 AS Memory_MB
FROM sys.dm_os_memory_clerks
GROUP BY type
ORDER BY Memory_MB DESC;
```

Özellikle yüksek hacimli alanlar incelenmelidir.

## Plan cache kontrolü

```sql
SELECT
    objtype,
    COUNT(*) AS PlanSayisi,
    SUM(size_in_bytes) / 1024.0 / 1024.0 AS Boyut_MB
FROM sys.dm_exec_cached_plans
GROUP BY objtype
ORDER BY Boyut_MB DESC;
```

Ad hoc sorguların aşırı çeşitlenmesi plan cache'i büyütebilir.

## Memory grant beklemeleri

```sql
SELECT
    session_id,
    requested_memory_kb,
    granted_memory_kb,
    required_memory_kb,
    wait_time_ms,
    queue_id
FROM sys.dm_exec_query_memory_grants
ORDER BY requested_memory_kb DESC;
```

Büyük sort/hash sorguları yüksek memory grant isteyebilir.

## Saha teşhis sırası

1. Fiziksel RAM ve OS kullanılabilir RAM
2. `max server memory`
3. SQL process memory
4. memory clerks
5. plan cache
6. active memory grants
7. yüksek compilation/recompile yükü
8. büyük sorguların execution plan'ları
9. SQL error log ve Windows event log

## CACHESTORE_PHDR ve benzeri cache alanları

Belirli memory clerk/cache türlerinin yüksek görünmesi doğrudan sorun olduğu anlamına gelmez. Değer, toplam bellek ve workload ile birlikte değerlendirilmelidir.

## Hatalı müdahaleler

Aşağıdakiler kalıcı çözüm değildir:

- sürekli SQL servisini restart etmek,
- rastgele `DBCC FREEPROCCACHE` çalıştırmak,
- cache temizliğini scheduled job haline getirmek,
- `max server memory` değerini ölçmeden yükseltmek.

Cache temizliği yalnızca kontrollü teşhis veya belirli plan sorunlarında uygulanmalıdır.

## Logo ERP özelinde

Yüksek hacimli Logo ortamlarında şu kombinasyon risklidir:

```text
çok büyük RAM
+
SQL'e neredeyse tamamının verilmesi
+
çok sayıda firma/dönem
+
çok fazla ad hoc rapor
+
uzun sorgular
+
yüksek eşzamanlı kullanıcı
```

## Kalıcı iyileştirme alanları

- SQL memory limitini doğru belirlemek,
- pahalı sorguları optimize etmek,
- gereksiz ad hoc sorgu çeşitliliğini azaltmak,
- memory grant yoğun sorguları iyileştirmek,
- servis/rapor katmanında paging uygulamak,
- kullanılmayan eski rapor ve view'ları temizlemek,
- SQL Server sürüm ve CU seviyesini destek politikası kapsamında değerlendirmek.

> Error 701 bir semptomdur. Kalıcı çözüm için SQL Server'ın belleği hangi bileşende tükettiği ölçülmelidir.
