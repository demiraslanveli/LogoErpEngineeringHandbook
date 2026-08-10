# 88 — Parameter Sniffing ve Plan Kararlılığı

## 1. Amaç

Parameter sniffing, SQL Server'ın bir prosedürü derlerken ilk veya mevcut parametre değerlerine göre execution plan üretmesiyle ilişkilidir.

Bu davranış çoğu zaman faydalıdır. Ancak Logo raporlarında veri dağılımı dengesizse aynı plan tüm parametreler için uygun olmayabilir.

## 2. Tipik Logo Senaryosu

Bir prosedür:

```sql
EXEC dbo.SP_STOK_HAREKET @StockRef = 100;
```

ile çalıştığında 20 satır döndürüyor olabilir.

Aynı prosedür:

```sql
EXEC dbo.SP_STOK_HAREKET @StockRef = 25000;
```

ile 4 milyon satır okuyabilir.

İlk parametreye göre üretilen Nested Loops ağırlıklı plan ikinci parametrede kötü davranabilir.

## 3. Belirtiler

- prosedür bazen çok hızlı, bazen çok yavaş
- recompilation sonrası davranış değişiyor
- farklı parametrelerde logical reads dramatik değişiyor
- Query Store'da birden fazla plan görülebiliyor

## 4. Kök Neden Analizi

Önce şu sorular sorulmalıdır:

- veri dağılımı gerçekten skewed mı?
- statistics güncel mi?
- parametre tipi kolon tipiyle aynı mı?
- estimated/actual rows farkı var mı?
- aynı sorgu farklı parametrelerde farklı optimal plan gerektiriyor mu?

## 5. Olası Yaklaşımlar

### OPTION (RECOMPILE)

Her çalışmada yeni plan üretir.

Avantaj:

- parametreye özel plan

Dezavantaj:

- compile CPU maliyeti

Yüksek frekanslı prosedürlerde dikkatli kullanılmalıdır.

### OPTIMIZE FOR

Belirli bir tipik parametre değerine göre plan üretilebilir.

Ancak veri dağılımı değişirse eski karar kötüleşebilir.

### OPTIMIZE FOR UNKNOWN

Histogramdaki belirli sniffed değeri yerine daha genel tahmin kullanmayı hedefler.

Her durumda iyi sonuç vereceği varsayılmamalıdır.

### Dynamic SQL

Farklı filter kombinasyonlarına farklı sorgu şekli üretmek için kullanılabilir.

Parametreli `sp_executesql` tercih edilmelidir.

## 6. Local Variable Yanılgısı

Parametreyi local variable'a kopyalamak bazen sniffing etkisini değiştirir fakat bu gerçek bir tuning stratejisi değildir.

Optimizer daha genel tahmin yapar ve bazı workload'larda plan daha da kötüleşebilir.

## 7. Optional Parameter Problemi

Logo raporlarında sık görülen pattern:

```sql
WHERE (@StockRef IS NULL OR STOCKREF = @StockRef)
```

Bu kullanım farklı parametre kombinasyonlarını tek plan altında toplamaya çalışır.

Büyük tablolarda problemli olabilir.

Alternatif olarak filter kombinasyonlarına göre parametrik dynamic SQL düşünülebilir.

## 8. Plan Cache'i Temizlemek Çözüm Değildir

Plan cache temizlemek geçici olarak davranışı değiştirir.

Kök neden çözülmediyse sorun geri gelir.

Üretim ortamında global cache temizleme ayrıca diğer sorguları da etkileyebilir.

## 9. Query Store ile Birlikte Kullanım

Query Store üzerinden:

- hızlı plan
- yavaş plan
- plan değişim zamanı
- runtime istatistikleri

karşılaştırılmalıdır.

## 10. Temel Prensip

> Parameter sniffing'i kapatmaya çalışmak yerine, sorgunun farklı parametre dağılımlarında neden farklı plana ihtiyaç duyduğunu anlamak gerekir.
