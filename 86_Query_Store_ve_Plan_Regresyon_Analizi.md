# 86 — Query Store ve Plan Regresyon Analizi

## 1. Amaç

Query Store, SQL Server üzerinde zaman içinde sorgu performansını karşılaştırmak için kullanılabilecek en güçlü araçlardan biridir.

Logo ERP ortamlarında özellikle şu sorular için değerlidir:

- Bu sorgu ne zaman yavaşladı?
- Plan neden değişti?
- Yeni index sonrası gerçekten hızlandı mı?
- Aynı sorgu neden bazen hızlı bazen yavaş?

## 2. Query Store Ne Saklar?

Genel olarak:

- query text
- query identity
- execution plan
- runtime statistics
- execution count
- duration
- CPU
- logical I/O

bilgilerinin zaman içindeki değişimini izlemeye yardımcı olur.

## 3. Logo Ortamında Kullanım Senaryoları

### Rapor bir gün aniden yavaşladı

Önce mevcut plan ile önceki hızlı plan karşılaştırılabilir.

### SQL güncellemesi sonrası performans bozuldu

Plan regresyonu araştırılabilir.

### Aynı prosedür farklı parametrelerde çok farklı davranıyor

Birden fazla plan oluşup oluşmadığı görülebilir.

## 4. Plan Regresyonu

Bir sorgunun eski planı hızlıyken yeni planı yavaş olabilir.

Muhtemel nedenler:

- statistics değişimi
- parameter sniffing
- index ekleme/silme
- compatibility level değişikliği
- cardinality estimator değişikliği
- veri dağılımının değişmesi

## 5. Plan Force Etmek

Query Store plan forcing imkanı sunabilir.

Ancak bu kalıcı çözüm olarak düşünülmemelidir.

Plan force:

- acil stabilizasyon aracı olabilir
- kök neden analizi yapılmadan uygulanmamalıdır
- veri dağılımı değişirse eski plan tekrar kötüleşebilir

## 6. Performans Baseline

Bir sorgunun yalnızca tek çalışmasını ölçmek yerine dönemsel baseline tutulmalıdır.

Örneğin:

```text
Rapor: Cari Yaşlandırma
Ortalama süre: 4.2 sn
P95 süre: 7.8 sn
Logical read: 1.2M
Execution/day: 180
```

Bu değerler tuning öncesi ve sonrası karşılaştırılabilir.

## 7. Query Store ile Tuning Süreci

```text
1. Yavaş sorguyu belirle
2. Geçmiş planları karşılaştır
3. Runtime istatistiklerini kontrol et
4. Plan değişim zamanını belirle
5. Statistics / index / parametre değişimini araştır
6. Fix uygula
7. Sonraki runtime verisini karşılaştır
```

## 8. Kaynak Yönetimi

Query Store veri topladığı için repository boyutu ve retention ayarları kontrol edilmelidir.

Yoğun Logo sistemlerinde gereğinden fazla geçmiş veri tutulması veritabanı yönetimini zorlaştırabilir.

## 9. Temel Prensip

> Query Store yalnızca yavaş sorgu bulma aracı değil, performans davranışının zaman çizelgesidir.
