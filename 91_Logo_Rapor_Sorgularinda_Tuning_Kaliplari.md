# 91 — Logo Rapor Sorgularında Tuning Kalıpları

## 1. Amaç

Bu bölüm, Logo ERP veritabanında rapor ve kontrol sorgularını hızlandırmak için tekrar kullanılabilir tuning kalıplarını toplar.

Odak noktası tek bir sorguyu optimize etmek değil, Logo tablolarında sık görülen performans anti-pattern'lerini sistematik olarak düzeltmektir.

## 2. Firma / Dönem Filtrelerini Baştan Doğru Kur

Logo tablo isimleri firma ve dönem bazlıdır.

Dinamik SQL üretiliyorsa sadece ihtiyaç duyulan firma ve dönem tablosu sorgulanmalıdır.

Gereksiz dönem union'ları büyük I/O üretir.

## 3. Tarih Filtrelerini Sargable Yaz

Kaçınılması gereken örnek:

```sql
WHERE YEAR(DATE_) = 2026
```

Tercih edilen:

```sql
WHERE DATE_ >= '20260101'
  AND DATE_ <  '20270101'
```

## 4. SELECT * Kullanma

Özellikle:

- STLINE
- CLFLINE
- INVOICE
- ORFLINE

çok geniş tablolardır.

Raporun kullanmadığı kolonları taşımak:

- logical read
- memory grant
- network transferi

maliyetini artırır.

## 5. Önce Header Filtrele, Sonra Line'a Git

Örnek olarak sadece belirli işyeri ve tarih aralığı faturaları isteniyorsa önce INVOICE üzerinde set daraltılıp sonra STLINE'a gidilmesi bazı senaryolarda daha verimli olabilir.

Ancak optimizer'ın join sırası seçimine müdahale etmeden önce actual plan incelenmelidir.

## 6. LINETYPE Filtrelerini Net Yaz

Malzeme hareketi raporlarında gerekiyorsa:

```sql
WHERE LINETYPE = 0
```

ile gerçek malzeme satırları ayrılmalıdır.

İskonto/masraf satırlarının gereksiz join ve aggregation'a girmesi engellenebilir.

## 7. CANCELLED Filtresi

İptal hareketlerin rapora dahil olup olmayacağı açıkça tanımlanmalıdır.

Sıklıkla:

```sql
AND CANCELLED = 0
```

kullanılır.

Bu yalnızca performans değil, sonuç doğruluğu açısından da önemlidir.

## 8. COUNT(DISTINCT ...) Dikkati

Büyük join zincirlerinde `COUNT(DISTINCT)` pahalı olabilir.

Önce veri tekrarının neden oluştuğu araştırılmalıdır.

Bazı durumlarda doğru relation ile join edildiğinde DISTINCT ihtiyacı ortadan kalkar.

## 9. SUM Öncesi Duplicate Kontrolü

Logo belge zincirlerinde yanlış join:

```text
ORFLINE -> STLINE -> INVOICE
```

satırları çoğaltabilir.

Sonuç hem yavaş hem yanlış olur.

Aggregation'dan önce ilişki cardinality'si doğrulanmalıdır.

## 10. LEFT JOIN'i Otomatik Kullanma

Her ilişki LEFT JOIN olmak zorunda değildir.

İş kuralı gereği kayıt kesin varsa INNER JOIN daha doğru olabilir.

Join tipi önce veri semantiğine göre seçilmelidir.

## 11. OR Koşulları

Örnek:

```sql
WHERE @CariRef IS NULL OR CLIENTREF = @CariRef
```

kolay yazılır ancak büyük tablolarda plan kararlılığını bozabilir.

Optional filter sayısı arttıkça parametrik dynamic SQL değerlendirilebilir.

## 12. Temp Table Kullanımı

Çok aşamalı raporlarda ara result set tekrar kullanılacaksa temp table faydalı olabilir.

Avantajlar:

- ara sonuç küçültme
- geçici index oluşturma
- optimizer'a yeni statistics sağlama

Ancak tempdb yükü de izlenmelidir.

## 13. CTE Performans Sihri Değildir

CTE kod okunabilirliğini artırır fakat materialize olmak zorunda değildir.

Aynı pahalı CTE birden çok yerde kullanılıyorsa plan incelenmelidir.

## 14. Scalar Function Dikkati

Satır başına çalışan scalar function'lar büyük sonuçlarda ciddi CPU üretebilir.

Mümkünse set-based yaklaşım değerlendirilmelidir.

## 15. View Üstüne View Zinciri

Logo rapor projelerinde şu yapı zamanla oluşabilir:

```text
View A
  ↓
View B
  ↓
View C
  ↓
Power BI / Excel
```

Bu zincir join ve hesaplamaları görünmez hale getirir.

Actual execution plan en alt tablolara kadar incelenmelidir.

## 16. Power BI / Excel İçin Özel Dikkat

Rapor sorgusu yalnızca SQL'de hızlı olmakla yetmez.

- dönen satır sayısı
- kolon sayısı
- refresh sıklığı
- gateway/network

birlikte değerlendirilmelidir.

## 17. Tuning Kontrol Listesi

```text
[ ] Firma/dönem doğru mu?
[ ] Tarih filtresi sargable mı?
[ ] SELECT * var mı?
[ ] LINETYPE doğru filtrelenmiş mi?
[ ] CANCELLED politikası doğru mu?
[ ] Join cardinality doğru mu?
[ ] DISTINCT yanlış join'i gizliyor mu?
[ ] Actual/Estimated rows farkı var mı?
[ ] Logical reads ölçüldü mü?
[ ] Result set gereğinden büyük mü?
```

## 18. Temel Prensip

> Logo rapor tuning'inde ilk hedef sorguyu kısaltmak değil, doğru veri kümesini mümkün olan en erken aşamada daraltmaktır.
