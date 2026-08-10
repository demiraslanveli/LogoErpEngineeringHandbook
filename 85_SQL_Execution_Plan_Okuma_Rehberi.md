# 85 — SQL Execution Plan Okuma Rehberi

## 1. Amaç

Bu bölüm, Logo ERP üzerinde çalışan ağır SQL sorgularını analiz ederken execution plan'ın nasıl okunacağını açıklar.

Amaç yalnızca `Index Seek` görmek değil; sorgunun nerede CPU, I/O, memory grant veya satır tahmin hatası ürettiğini anlayabilmektir.

## 2. Önce Gerçek Plan

Mümkün olduğunda Estimated Plan yerine Actual Execution Plan incelenmelidir.

Özellikle şu farklar kritik önemdedir:

- Estimated Rows
- Actual Rows
- Actual Number of Executions
- Logical Reads
- Memory Grant
- Spill bilgileri

Estimated ve Actual satır sayıları ciddi farklıysa yalnızca index eklemek çoğu zaman yeterli değildir.

## 3. Temel Operatörler

### Index Seek

Genellikle seçici bir filtre ile uygun index kullanıldığını gösterir.

### Index Scan / Table Scan

Her scan kötü değildir. Çok büyük veri kümesinin önemli bölümü okunacaksa scan normal olabilir.

Sorulması gereken soru:

> Sorgu gerçekten tablonun büyük bölümünü okumak zorunda mı?

### Key Lookup

Az sayıda satır için kabul edilebilir.

Binlerce kez tekrarlandığında pahalı hale gelir.

Bu durumda INCLUDE kolonları değerlendirilebilir.

### Nested Loops

Küçük dış veri kümesinde etkilidir.

Gerçekte milyonlarca satır işleniyorsa kötü plana dönüşebilir.

### Hash Match

Büyük veri kümelerinde normal olabilir.

Memory grant yetersizse tempdb spill oluşabilir.

### Sort

ORDER BY, DISTINCT, GROUP BY ve bazı join stratejileri nedeniyle oluşabilir.

Büyük sort işlemlerinde tempdb ve memory grant kontrol edilmelidir.

## 4. Operator Cost Tek Başına Yeterli Değildir

Grafikte görülen `% cost` oranları optimizer tahminidir.

Gerçek darboğazı belirlerken aşağıdakiler birlikte değerlendirilmelidir:

- elapsed time
- CPU time
- logical reads
- physical reads
- tempdb spill
- actual rows
- executions

## 5. Logo Sorgularında Tipik Plan Problemleri

### STLINE üzerinde geniş tarih aralığı

`LG_XXX_YY_STLINE` tabloları çok hızlı büyüyebilir.

Sorgu hem `STOCKREF`, hem `DATE_`, hem `TRCODE`, hem `CANCELLED` kullanıyorsa doğru index anahtar sırası workload'a göre değerlendirilmelidir.

### Fonksiyon ile filtreleme

Örneğin:

```sql
WHERE YEAR(DATE_) = 2026
```

yerine çoğu durumda:

```sql
WHERE DATE_ >= '20260101'
  AND DATE_ <  '20270101'
```

şeklindeki sargable filtre daha uygundur.

### CAST / CONVERT ile kolon değiştirme

Kolon üzerinde fonksiyon uygulanması index seek kullanımını engelleyebilir.

### OR koşulları

Birden çok farklı filtre pattern'i optimizer tahminini zorlaştırabilir.

Bazı durumlarda UNION ALL ile ayrıştırmak daha iyi sonuç verebilir.

## 6. Actual Rows / Estimated Rows

Örnek:

```text
Estimated Rows = 12
Actual Rows    = 1,200,000
```

Bu durumda optimizer tamamen farklı join stratejisi seçmiş olabilir.

Muhtemel nedenler:

- stale statistics
- skewed data
- parameter sniffing
- correlated columns
- local variable kullanımı
- implicit conversion

## 7. Implicit Conversion

Özellikle Logo entegrasyon sorgularında string/int tip uyuşmazlıkları önemlidir.

Plan üzerinde `CONVERT_IMPLICIT` görülüyorsa join veya filter kolon tipleri karşılaştırılmalıdır.

Yanlış tip kullanımı hem index kullanımını hem cardinality tahminini bozabilir.

## 8. Spill Kontrolü

Hash veya Sort operatörleri memory grant yetmediğinde tempdb'ye spill edebilir.

Bu durumda plan uyarılarında spill bilgileri görülebilir.

Sadece tempdb büyütmek çözüm değildir.

Aşağıdakiler ayrıca incelenmelidir:

- yanlış cardinality tahmini
- gereksiz geniş kolon seçimi
- gereksiz sort
- kötü join sırası

## 9. SELECT * Kullanımı

Logo tabloları çok geniş olabilir.

Özellikle STLINE gibi tablolarda:

```sql
SELECT *
```

kullanımı:

- daha yüksek I/O
- daha fazla network transferi
- daha büyük memory grant
- covering index imkanının azalması

sonuçlarını doğurabilir.

## 10. Ölçüm

Sorgu tuning sırasında kullanılabilecek temel araçlar:

```sql
SET STATISTICS IO ON;
SET STATISTICS TIME ON;
```

Execution plan ile birlikte okunmalıdır.

## 11. Tuning Sırası

Önerilen sıra:

```text
1. Doğru sonucu doğrula
2. Actual plan al
3. Logical read ölç
4. Cardinality farklarını bul
5. Scan / lookup / spill incele
6. Sorguyu sadeleştir
7. Gerekirse index tasarla
8. Tekrar ölç
```

## 12. Temel Prensip

> Execution plan, optimizer'ın sorguyu nasıl çalıştırmaya karar verdiğini gösterir; tuning'in amacı planı güzelleştirmek değil, toplam kaynak tüketimini ve çalışma süresini düşürmektir.
