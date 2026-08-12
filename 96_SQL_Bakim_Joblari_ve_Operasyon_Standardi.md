# 96 — SQL Bakım Job'ları ve Operasyon Standardı

## Amaç

Logo ERP veritabanlarında bakım işlemleri rastgele veya yalnızca problem çıktığında yapılmamalıdır. Index, statistics, integrity check, backup ve history temizliği kontrollü bir operasyon standardı içinde çalışmalıdır.

---

## Temel bakım başlıkları

- Backup
- DBCC CHECKDB
- Index maintenance
- Statistics maintenance
- Job history cleanup
- Database Mail history cleanup
- Backup history cleanup
- TempDB ve dosya büyüme takibi
- Disk alanı kontrolü
- Uzun süren job kontrolü

---

## Index maintenance

Her index'i her gece rebuild etmek doğru yaklaşım değildir.

Karar aşağıdaki metriklerle verilmelidir:

- Page count
- Fragmentation
- Kullanım sıklığı
- Bakım süresi
- Log üretimi
- Availability Group etkisi

Örnek yaklaşım:

```text
Küçük index              → işlem yapma
Orta fragmentation       → reorganize
Yüksek fragmentation     → rebuild
```

Eşikler sistem bazında ölçülmelidir; sabit internet reçeteleri körü körüne uygulanmamalıdır.

---

## Statistics maintenance

Logo hareket tablolarında veri dağılımı hızlı değişebilir.

Özellikle:

- STLINE
- CLFLINE
- INVOICE
- ORFLINE
- EMFLINE

gibi büyük tablolarda eski statistics kötü execution plan üretebilir.

Kontrol:

```sql
SELECT
    OBJECT_NAME(s.object_id) AS TableName,
    s.name AS StatisticsName,
    STATS_DATE(s.object_id, s.stats_id) AS LastUpdate
FROM sys.stats s
WHERE OBJECTPROPERTY(s.object_id,'IsUserTable') = 1;
```

---

## DBCC CHECKDB

Veri bütünlüğü kontrolü planlı yapılmalıdır.

```sql
DBCC CHECKDB WITH NO_INFOMSGS;
```

Çok büyük veritabanlarında bakım penceresi ve I/O etkisi hesaba katılmalıdır.

Integrity check başarısızlığı kritik alarm olarak değerlendirilmelidir.

---

## Job çalışma penceresi

Bakım job'ları Logo kullanıcı yoğunluğunun düşük olduğu saatlere konumlandırılmalıdır.

Ancak yalnızca saat seçmek yeterli değildir.

Aynı anda şu işlerin çakışmaması gerekir:

- Full backup
- Index rebuild
- ETL
- Büyük raporlar
- Costing işlemleri
- Üretim entegrasyon batch'leri

---

## Autogrowth

Yüzdesel autogrowth özellikle büyük veritabanlarında kontrolsüz büyüme adımları oluşturabilir.

Genellikle sabit MB/GB büyüme daha öngörülebilirdir.

Data ve log dosyaları için büyüme politikası ayrı değerlendirilmelidir.

---

## Disk alanı alarmı

Bakım job'ları disk alanı tüketebilir.

En azından şu alanlar izlenmelidir:

```text
Data disk
Log disk
Backup disk
TempDB disk
```

---

## History cleanup

`msdb` zamanla büyüyebilir.

Temizlenmesi değerlendirilecek alanlar:

- Backup history
- Job history
- Database Mail history

Temizlik politikası denetim ihtiyacına göre belirlenmelidir.

---

## Bakım başarısı nasıl ölçülür?

Sadece job status = success yeterli değildir.

Kontrol:

```text
Job başarıyla tamamlandı mı?
Beklenen objeleri işledi mi?
Süre normal aralıkta mı?
Disk doldu mu?
Log aşırı büyüdü mü?
Blocking oluşturdu mu?
AG redo/send queue büyüdü mü?
```

---

## Sonuç

Bakım planı otomatik çalışan scriptler bütünü değil, ölçülen ve izlenen bir operasyon sürecidir.
