# 159 — Final Production Acceptance Checklist

Bu bölüm referans uygulamanın production'a çıkmadan önce geçmesi gereken son kabul listesidir.

## Architecture

- Core, Application, Infrastructure, LogoAdapter ve Worker bağımlılık yönleri korunuyor mu?
- Application katmanı Logo COM tiplerini doğrudan referans ediyor mu? Etmemeli.
- SDK binding manifest doğrulandı mı?

## Logo Session

- gerçek login/logout test edildi mi?
- firma/dönem context'i doğrulandı mı?
- yanlış kullanıcı/parola kontrollü hata veriyor mu?
- COM release davranışı test edildi mi?

## IData

- malzeme binding doğrulandı mı?
- cari binding doğrulandı mı?
- sipariş binding doğrulandı mı?
- irsaliye/fatura binding doğrulandı mı?
- post sonrası logicalref alınabiliyor mu?
- Logo hata açıklaması korunuyor mu?

## ProductionApplication

- verified bridge aktif mi?
- üretim emri test edildi mi?
- seri/lot, kalite ve maliyet etkileri gerekiyorsa doğrulandı mı?

## SQL

- application-owned migration'lar uygulanmış mı?
- idempotency unique constraint aktif mi?
- queue/reconciliation tabloları sağlıklı mı?
- SQL login least privilege mı?

## Reliability

- retry yalnızca uygun hata sınıflarında mı?
- idempotency doğrulandı mı?
- duplicate event testi geçti mi?
- reconciliation mismatch alarmı var mı?

## Observability

- structured logging aktif mi?
- correlation id uçtan uca taşınıyor mu?
- health persistence aktif mi?
- heartbeat izleniyor mu?
- secret loglanmıyor mu?

## Windows Service

- start/stop test edildi mi?
- graceful shutdown çalışıyor mu?
- recovery policy tanımlı mı?
- service account yetkileri minimum mu?

## Deployment

- release artifact üretildi mi?
- manifest commit SHA içeriyor mu?
- backup/rollback prosedürü test edildi mi?
- migration ve binary sürümleri uyumlu mu?

## Business Acceptance

- Logo ekranı ve SQL read-back sonuçları eşleşiyor mu?
- finansal belge toplamları doğru mu?
- stok miktarları doğru mu?
- kaynak belge ilişkileri doğru mu?
- üretim/maliyet süreçleri beklenen sonucu veriyor mu?

## Go / No-Go

Critical maddelerden herhangi biri başarısızsa release `NO-GO` kabul edilmelidir.

> Production kabulü yalnızca uygulamanın derlenmesi veya servisin çalışması değildir; ERP veri bütünlüğü, operasyonel izlenebilirlik ve geri dönüş kabiliyetinin birlikte kanıtlanmasıdır.
