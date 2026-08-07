# 15 — SQL Server ve Performans

## 1. Bölümün Amacı

Bu bölüm, Logo Tiger / Tiger Wings Enterprise veritabanlarında SQL Server performans problemlerini sistematik biçimde analiz etmeyi açıklar. Amaç yalnızca yavaş sorgu düzeltmek değil; darboğazın CPU, RAM, disk, tempdb, blocking, network veya sorgu tasarımından hangisinde olduğunu ayırabilmektir.

> Performans problemi tek bir sorguya bakılarak teşhis edilmez. Sunucu kaynakları, wait istatistikleri ve sorgu davranışı birlikte değerlendirilmelidir.

---

## 2. İlk Ayrım: Sunucu mu, Sorgu mu?

Yavaşlık geldiğinde önce şu sorular sorulmalıdır:

- Tüm kullanıcılar mı yavaş?
- Tek ekran mı yavaş?
- Tek firma mı yavaş?
- SQL Server CPU yüksek mi?
- RAM baskısı var mı?
- Disk latency yüksek mi?
- Blocking var mı?
- `tempdb` beklemeleri var mı?
- İstemci veriyi yavaş mı tüketiyor?

Bu ayrım yapılmadan indeks eklemek veya SQL Server ayarı değiştirmek risklidir.

---

## 3. Wait Statistics

SQL Server bekleme tipleri darboğazın kaynağı hakkında güçlü ipucu verir.

Sık görülen örnekler:

### ASYNC_NETWORK_IO

SQL Server sonucu üretmiş olabilir; istemci veriyi yeterince hızlı tüketmiyordur.

Olası nedenler:

- çok fazla satır dönmesi,
- `SELECT *`,
- yavaş Excel/Power BI istemcisi,
- network gecikmesi,
- uygulamanın satırları yavaş işlemesi.

### PAGEIOLATCH_*

Diskten veri sayfası okunması bekleniyor olabilir.

Olası nedenler:

- yavaş storage,
- yetersiz buffer cache,
- büyük scan,
- eksik indeks.

### PAGELATCH_*

Disk I/O değil, bellekteki sayfalar üzerinde contention işaretidir.

Özellikle `tempdb` allocation contention senaryolarında görülebilir.

### RESOURCE_SEMAPHORE

Memory grant bekleyen sorgular olabilir.

### WRITELOG

Transaction log yazma gecikmesi olabilir.

---

## 4. Aktif Sorguları İnceleme

Örnek teşhis sorgusu:

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
    t.text AS SqlText
FROM sys.dm_exec_requests r
CROSS APPLY sys.dm_exec_sql_text(r.sql_handle) t
WHERE r.session_id <> @@SPID;
```

Tek başına elapsed time yeterli değildir. CPU, logical read, wait type ve blocking birlikte okunmalıdır.

---

## 5. Blocking

Blocking ile yavaş sorgu aynı şey değildir.

Bir sorgu hızlı çalışabilecek durumda olsa bile başka bir transaction kilidi nedeniyle bekliyor olabilir.

Kontrol edilmesi gerekenler:

- `blocking_session_id`,
- açık transaction,
- uzun süren update/delete,
- kullanıcı ekranında açık bırakılan işlem,
- batch entegrasyonları,
- trigger içindeki ağır sorgular.

---

## 6. Deadlock

Deadlock, iki veya daha fazla işlemin birbirinin kaynaklarını beklediği kilit döngüsüdür.

Çözüm yalnızca retry değildir. Şunlar incelenmelidir:

- tabloların erişim sırası,
- transaction süresi,
- indeksler,
- gereksiz geniş update,
- aynı kayıtların farklı sırayla işlenmesi.

---

## 7. Logical Reads

Logo gibi büyük ERP tablolarında sorgu optimizasyonunun en önemli göstergelerinden biri logical read miktarıdır.

Bir sorgu az satır döndürmesine rağmen milyonlarca sayfa okuyorsa indeks veya sorgu tasarımı sorunlu olabilir.

```sql
SET STATISTICS IO ON;
SET STATISTICS TIME ON;
```

ile test yapılabilir.

---

## 8. Execution Plan

Execution plan üzerinde özellikle şu operatörler incelenmelidir:

- Table Scan,
- Clustered Index Scan,
- Index Seek,
- Key Lookup,
- Sort,
- Hash Match,
- Spool,
- Parallelism.

Bir `Scan` tek başına hata değildir. Küçük tabloda scan, yanlış indeksten daha ucuz olabilir.

Önemli olan tahmini ve gerçek satır sayılarını karşılaştırmaktır.

---

## 9. İndeks Tasarımı

Logo standart tablolarına indeks eklenirken kontrollü hareket edilmelidir.

İyi bir indeks şu soruya cevap vermelidir:

> Hangi sorgu için, hangi filtre ve join kolonlarını hızlandırıyorum?

Örnek:

```sql
WHERE STOCKREF = @StockRef
  AND DATE_ >= @StartDate
```

sorgularında `STOCKREF` ve `DATE_` kombinasyonu değerlendirilebilir.

Fakat indeks eklemenin maliyeti vardır:

- insert/update daha pahalı olur,
- disk alanı artar,
- bakım maliyeti artar.

---

## 10. Missing Index Önerileri

SQL Server missing index DMV'leri faydalı ipucu verir fakat otomatik uygulanmamalıdır.

Benzer öneriler birleştirilmeli ve mevcut indekslerle karşılaştırılmalıdır.

Aksi halde aynı tablo üzerinde onlarca birbirine yakın indeks oluşabilir.

---

## 11. tempdb

Logo ve raporlama yüklerinde `tempdb` önemli darboğaz olabilir.

Kontrol edilmesi gerekenler:

- data file sayısı,
- file boyutları,
- autogrowth,
- disk performansı,
- allocation contention,
- version store kullanımı.

Yüzde bazlı autogrowth yerine kontrollü sabit MB büyüme çoğu üretim sisteminde daha öngörülebilirdir.

---

## 12. PAGELATCH ve tempdb

`PAGELATCH_EX`, `PAGELATCH_UP` ve `PAGELATCH_SH` beklemeleri çok yüksekse bunun disk latency ile karıştırılmaması gerekir.

`PAGELATCH` bellekteki page latch contention'dır.

Özellikle tempdb yoğun sistemlerde:

- yeterli tempdb data file,
- eşit file boyutları,
- uygun autogrowth,
- güncel SQL Server sürümü/patch seviyesi

değerlendirilmelidir.

---

## 13. Disk Latency

Veritabanı dosyalarının I/O süreleri `sys.dm_io_virtual_file_stats` üzerinden analiz edilebilir.

Kabaca:

```text
< 5 ms     çok iyi
5–10 ms    iyi
10–20 ms   kabul edilebilir / izlenmeli
20–50 ms   yavaş
> 50 ms    ciddi problem adayı
```

Bunlar mutlak sınırlar değildir; workload ve storage türüne göre değerlendirilmelidir.

---

## 14. MDF, NDF ve LDF Ayrımı

Data file ve transaction log farklı I/O karakterine sahiptir.

- MDF/NDF daha çok random read/write,
- LDF sıralı write ağırlıklıdır.

Log diskinin gecikmesi transaction performansını doğrudan etkileyebilir.

---

## 15. SQL Server Memory

SQL Server boş RAM bırakmıyor diye tek başına problem olduğu söylenemez. SQL Server kullanılabilir belleği buffer pool için kullanır.

Kontrol edilmesi gerekenler:

- fiziksel RAM,
- `max server memory`,
- OS için kalan RAM,
- memory grants pending,
- process physical memory low,
- paging,
- diğer uygulamaların tüketimi.

`max server memory` fiziksel RAM'in tamamına ayarlanmamalıdır.

---

## 16. Error 701

SQL Server Error 701 genel olarak query execution için yeterli system memory bulunamadığını ifade eder.

Analiz sırasında:

- memory clerk dağılımı,
- plan cache,
- memory grants,
- OS memory pressure,
- `max server memory`,
- büyük ad-hoc sorgular

birlikte incelenmelidir.

Sadece SQL Server servisini restart etmek kök nedeni çözmez.

---

## 17. Plan Cache

Çok fazla benzersiz ad-hoc SQL plan cache'i şişirebilir.

Özellikle uygulamalar parametre yerine her seferinde farklı literal değerlerle SQL üretiyorsa çok sayıda plan oluşabilir.

Parametrik sorgu ve uygun uygulama tasarımı plan cache kullanımını iyileştirebilir.

---

## 18. Parameter Sniffing

Aynı procedure farklı parametrelerde çok farklı satır sayıları döndürüyorsa ilk derlenen plan sonraki çağrılar için kötü olabilir.

Çözüm senaryoya göre:

- query rewrite,
- uygun indeks,
- `OPTION (RECOMPILE)`,
- local variable,
- farklı procedure stratejileri

olabilir.

Her durumda körlemesine `RECOMPILE` eklenmemelidir.

---

## 19. Trigger Performansı

Logo tablolarına eklenen özel trigger'lar ciddi performans problemi yaratabilir.

Trigger içinde:

- cursor,
- büyük tablo scan,
- network çağrısı,
- mail gönderimi,
- uzun transaction,
- satır satır işlem

kaçınılmalıdır.

Trigger mümkün olduğunca kısa sürmeli ve set-based çalışmalıdır.

---

## 20. Database Mail ve Queue Yaklaşımı

İşlem sırasında doğrudan mail göndermek yerine queue yaklaşımı daha güvenlidir.

```text
ERP İşlemi
   ↓
MailQueue INSERT
   ↓
SQL Agent / Servis
   ↓
Mail Gönderimi
```

Böylece SMTP yavaşlığı ana transaction'ı bloklamaz.

---

## 21. Power BI / Excel Etkisi

`ASYNC_NETWORK_IO` görüldüğünde SQL Server'ı suçlamadan önce istemci incelenmelidir.

Excel, Power BI Mashup Engine veya özel uygulama:

- milyonlarca satır çekiyor,
- veriyi satır satır işliyor,
- filtreyi istemci tarafında uyguluyor

olabilir.

Filtre mümkün olduğunca SQL tarafında uygulanmalıdır.

---

## 22. Büyük Logo Tabloları

Özellikle hareket tabloları yıllar içinde çok büyüyebilir.

Performans için:

- doğru dönem kullanımı,
- tarih filtresi,
- gereksiz kolonlardan kaçınma,
- uygun indeks,
- arşiv politikası,
- rapor veri ambarı

değerlendirilebilir.

Operasyonel ERP veritabanını analitik veri ambarı gibi kullanmak uzun vadede sorun yaratabilir.

---

## 23. Shrink

Database shrink rutin bakım işlemi değildir.

Shrink:

- indeks fragmentation oluşturabilir,
- yüksek I/O yaratabilir,
- dosyanın kısa süre sonra yeniden büyümesine neden olabilir.

Sadece gerçekten kalıcı büyük alan boşalması olan özel senaryolarda kontrollü uygulanmalıdır.

---

## 24. Autogrowth

Çok küçük autogrowth değeri sürekli file growth oluşturur. Çok büyük değer ise büyüme sırasında uzun bekleme yaratabilir.

Data ve log dosyaları için workload'a uygun sabit büyüme değerleri seçilmelidir.

---

## 25. Performans Analiz Sırası

Pratik teşhis sırası:

```text
1. Sorunun kapsamını belirle
2. Aktif request'leri gör
3. Wait type analiz et
4. Blocking kontrol et
5. CPU / RAM kontrol et
6. Disk latency kontrol et
7. tempdb kontrol et
8. Sorgunun IO ve planını incele
9. İndeksleri değerlendir
10. Uygulama tarafını incele
```

---

## 26. Sonuç

Logo ERP performansında doğru yaklaşım, önce darboğazı sınıflandırmak sonra müdahale etmektir. Wait statistics, execution plan, logical reads, disk latency, tempdb, memory ve istemci davranışı birlikte değerlendirilmelidir.

Bir sonraki bölümde Logo Objects, SQL Server, ara yazılım, MES/LIMS ve diğer sistemlerin birlikte kullanıldığı **entegrasyon mimarileri** ele alınacaktır.
