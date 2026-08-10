# 92 — SQL Performans Release Checklist

## 1. Amaç

Bu bölüm, Logo ERP üzerinde çalışacak yeni bir view, procedure, function veya ağır rapor sorgusu üretime alınmadan önce uygulanacak kontrol listesini tanımlar.

Amaç sorun çıktıktan sonra tuning yapmak yerine, performans risklerini release öncesinde azaltmaktır.

## 2. Sonuç Doğruluğu

Önce performans değil doğruluk kontrol edilir.

```text
[ ] Firma doğru mu?
[ ] Dönem doğru mu?
[ ] İptal kayıt politikası doğru mu?
[ ] LINETYPE filtreleri doğru mu?
[ ] TRCODE kapsamı doğru mu?
[ ] Join'ler satır çoğaltıyor mu?
[ ] SUM / COUNT sonuçları kontrol edildi mi?
```

## 3. Filtreler

```text
[ ] Tarih filtreleri sargable mı?
[ ] Kolon üzerinde gereksiz CAST/CONVERT var mı?
[ ] Optional parameter pattern planı bozuyor mu?
[ ] Gereksiz OR koşulu var mı?
```

## 4. Execution Plan

```text
[ ] Actual plan alındı mı?
[ ] Estimated / Actual row farkları incelendi mi?
[ ] Scan gerçekten gerekli mi?
[ ] Key Lookup tekrar sayısı yüksek mi?
[ ] Hash / Sort spill var mı?
[ ] CONVERT_IMPLICIT uyarısı var mı?
```

## 5. I/O ve CPU

```sql
SET STATISTICS IO ON;
SET STATISTICS TIME ON;
```

ile ölçüm alınmalıdır.

Kaydedilecek minimum bilgiler:

- elapsed time
- CPU time
- logical reads
- returned row count

## 6. Parametre Test Matrisi

Sadece tek örnek parametre ile test yapılmamalıdır.

Örneğin:

```text
Küçük cari
Büyük cari
Az hareketli stok
Çok hareketli stok
1 günlük tarih
1 yıllık tarih
Filtreli
Filtresiz
```

senaryoları test edilmelidir.

## 7. Concurrency Testi

Tek kullanıcıda hızlı olan sorgu eşzamanlı 30 kullanıcıda problem yaratabilir.

Kontrol:

- blocking
- memory grant
- tempdb
- CPU
- I/O

## 8. Result Set Boyutu

```text
[ ] Kullanıcı gerçekten tüm kolonlara ihtiyaç duyuyor mu?
[ ] Satır sayısı kabul edilebilir mi?
[ ] Paging veya aggregation gerekli mi?
[ ] Excel / Power BI tüketim kapasitesi uygun mu?
```

## 9. Index Kararı

Yeni index eklenmeden önce:

```text
[ ] Mevcut index'ler incelendi mi?
[ ] Overlapping index oluşuyor mu?
[ ] INCLUDE gerçekten gerekli mi?
[ ] Write cost değerlendirildi mi?
[ ] Index yalnızca tek rapor için mi ekleniyor?
```

## 10. Statistics

```text
[ ] İlgili tabloların statistics bilgisi güncel mi?
[ ] Cardinality tahmini normal mi?
[ ] Veri dağılımı skewed mı?
```

## 11. tempdb Etkisi

Sorgu:

- büyük sort
- hash join
- temp table
- row versioning

kullanıyorsa tempdb etkisi incelenmelidir.

## 12. Blocking Riski

Uzun transaction veya geniş scan varsa üretimde blocking riski değerlendirilmeli.

`NOLOCK` bunu çözmek için otomatik eklenmemelidir.

## 13. Rollback Planı

Her performans değişikliğinin geri dönüş planı olmalıdır.

Örneğin:

- eski procedure script'i
- eski view script'i
- index drop script'i
- Query Store eski plan bilgisi

## 14. Baseline Kaydı

Release öncesi:

```text
Duration
CPU
Logical Reads
Plan Hash
Row Count
Test Parametreleri
```

kaydedilmelidir.

Release sonrası aynı metrikler tekrar ölçülmelidir.

## 15. Temel Prensip

> SQL tuning tamamlanmış sayılmaz; farklı veri hacimleri, farklı parametreler ve eşzamanlı kullanıcı yükü altında davranış doğrulanmadan release güvenli değildir.
