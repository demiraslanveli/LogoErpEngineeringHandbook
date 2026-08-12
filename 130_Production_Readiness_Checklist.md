# 130 — Production Readiness Checklist

Bu bölüm Logo ERP entegrasyon uygulamasının production ortama çıkmadan önce teknik olarak hazır olup olmadığını kontrol etmek için kullanılacak standart checklist'i tanımlar.

## 1. Mimari

- Application, LogoAdapter ve Infrastructure sınırları ayrılmış mı?
- Logo SDK bağımlılığı kontrollü mü?
- Doğrudan SQL DML kullanımları gerekçelendirilmiş mi?
- Firma/dönem context merkezi mi?
- Multi-company kullanımda session izolasyonu var mı?

## 2. Logo Objects

- Login/logout akışı test edildi mi?
- `IApplication` yaşam döngüsü kontrollü mü?
- `IData.Post()` hataları yakalanıyor mu?
- Hata mesajları ortak result modeline çevriliyor mu?
- Version-dependent field/enum kullanımları doğrulandı mı?
- COM nesneleri worker'lar arasında bilinçsizce paylaşılmıyor mu?

## 3. Veri Bütünlüğü

- Idempotency var mı?
- Duplicate kayıt senaryosu test edildi mi?
- Retry sırasında aynı işlemin ikinci kez oluşması engelleniyor mu?
- Reconciliation mekanizması var mı?
- Kısmi başarı senaryosu tanımlı mı?
- Bağlı belgelerde kaynak referanslar korunuyor mu?

## 4. SQL Server

- Uygulama tabloları migration ile versiyonlanıyor mu?
- Indexler gerçek workload'a göre kontrol edildi mi?
- Uzun süren sorgular analiz edildi mi?
- Blocking/deadlock riski değerlendirildi mi?
- Connection timeout ve command timeout politikaları tanımlı mı?
- Database growth ve disk kapasitesi kontrol edildi mi?

## 5. Logging ve Observability

- CorrelationId tüm zincirde taşınıyor mu?
- Firma/dönem loglanıyor mu?
- OperationKey loglanıyor mu?
- Logo referansları başarı sonrası kaydediliyor mu?
- Exception stack trace korunuyor mu?
- Secret/password loglanmıyor mu?
- Health check var mı?
- Queue backlog izleniyor mu?

## 6. Security

- Servis hesabı least privilege mı?
- SQL login gereksiz yetkilere sahip mi?
- Production secret repository'de bulunmuyor mu?
- Config encryption uygulanıyor mu?
- Log klasörleri erişim kontrollü mü?
- Service account interactive login ihtiyacı değerlendirildi mi?

## 7. Worker ve Queue

- Graceful shutdown çalışıyor mu?
- Retry/backoff sınırı var mı?
- Poison message/dead-letter yaklaşımı var mı?
- Batch size ayarlanabilir mi?
- Worker aynı kaydı eşzamanlı işlemiyor mu?
- Uzun süren kayıtlar diğer queue işlemlerini kilitlemiyor mu?

## 8. Test

- Unit testler çalışıyor mu?
- Integration test ortamı var mı?
- Gerçek Logo login testi var mı?
- Create/update senaryoları test edildi mi?
- Validation failure testleri var mı?
- Duplicate/retry testi var mı?
- Connection loss testi var mı?
- Firma/dönem yanlış konfigürasyon testi var mı?

## 9. Performance

- Beklenen günlük işlem hacmi belirlendi mi?
- Peak load test edildi mi?
- Ortalama ve p95 işlem süresi ölçüldü mü?
- SQL sorgu süreleri baseline'a sahip mi?
- Logo Objects session açma maliyeti ölçüldü mü?
- Batch boyutu gerçek ortamda test edildi mi?

## 10. Deployment

- Release version belli mi?
- Git commit/tag belli mi?
- Schema version belli mi?
- Backup doğrulandı mı?
- Rollback planı var mı?
- Migration test edildi mi?
- Windows Service kurulum scripti test edildi mi?
- Production config hazır mı?

## 11. Operasyon

- Servis sahibi belli mi?
- Hata durumunda kimin müdahale edeceği belli mi?
- Runbook mevcut mu?
- Kritik alertler tanımlı mı?
- Manuel retry prosedürü var mı?
- Reconciliation ekranı/sorgusu mevcut mu?
- Queue temizleme işlemi kontrol altında mı?

## 12. Go-Live Kararı

Production'a çıkmadan önce şu üç soru net cevaplanmalıdır:

```text
1. Aynı işlem iki kez gelirse ne olur?
2. İşlem yarıda kalırsa nasıl anlaşılır ve düzeltilir?
3. Logo veya SQL erişilemezse sistem nasıl davranır?
```

Bu üç soruya güvenilir cevap verilemiyorsa uygulama production-ready kabul edilmemelidir.

## Kural

> Production readiness yalnızca kodun çalışması değil; hata, tekrar, kesinti, izleme ve geri dönüş davranışlarının da tasarlanmış olmasıdır.
