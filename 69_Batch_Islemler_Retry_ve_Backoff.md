# 69 — Batch İşlemler, Retry ve Backoff

## Amaç

Logo Objects entegrasyonlarında yüzlerce veya binlerce kaydı tek işlemde göndermek; timeout, partial failure ve tekrar kayıt riskini artırır. Bu bölüm batch boyutu, retry ve backoff tasarımını ele alır.

## Batch İlkesi

Büyük veri setleri kontrollü parçalara ayrılmalıdır.

```text
10.000 kayıt
→ 100'lük batch
→ her batch bağımsız işlenir
```

Batch boyutu sabit bir doğru değildir; belge tipi ve ortam performansına göre test edilmelidir.

## Transaction Sınırı

Tüm batch tek transaction olarak tutulmamalıdır.

Daha güvenli model:

```text
Her belge = bağımsız transaction
veya
küçük batch = bağımsız transaction
```

Böylece 500. kayıtta hata olduğunda ilk 499 kayıt zorunlu olarak rollback edilmez.

## Retry Edilebilir Hatalar

Her hata retry edilmemelidir.

### Retry Edilebilir

- geçici network kesintisi
- timeout
- geçici SQL bağlantı problemi
- transient service unavailable
- session yeniden kurulabilecek runtime hatası

### Retry Edilmemeli

- cari kart bulunamadı
- malzeme kodu bulunamadı
- zorunlu alan eksik
- geçersiz birim
- iş kuralı ihlali
- duplicate external document

## Exponential Backoff

Örnek politika:

```text
1. deneme → hemen
2. deneme → 2 sn
3. deneme → 5 sn
4. deneme → 15 sn
5. deneme → failed/dead-letter
```

Backoff değerleri sistem ihtiyacına göre ayarlanmalıdır.

## Jitter

Bir servis kesintisinden sonra yüzlerce worker aynı anda retry yaparsa thundering herd oluşabilir.

Bu nedenle küçük random jitter eklenebilir.

```text
delay = baseDelay + random(0..1000ms)
```

## Idempotency ile Retry

Retry yalnızca idempotency kontrolü varsa güvenlidir.

Örnek:

```text
ExternalId = MES-URETIM-20260809-1456
```

Retry öncesi:

1. integration log kontrol edilir,
2. LogoLogicalRef oluşmuş mu bakılır,
3. belge gerçekten Logo'da var mı doğrulanır,
4. yoksa tekrar gönderilir.

## Unknown Outcome

En zor durum:

```text
Logo Post başarılı oldu
↓
network koptu
↓
istemci success cevabını alamadı
```

Bu durumda kör retry duplicate oluşturabilir.

Çözüm:

- external document key,
- integration log,
- Logo tarafında business key araması,
- reconciliation job.

## Dead Letter Queue

Belirli sayıda retry sonrası başarısız kayıt DLQ'ya alınmalıdır.

DLQ kaydı:

```text
Id
ExternalId
Payload
LastError
RetryCount
FirstFailedAt
LastFailedAt
CompanyNr
PeriodNr
```

## Batch Progress

Uzun aktarımda ilerleme ölçülmelidir.

```text
Total: 10.000
Completed: 8.420
Failed: 14
Pending: 1.566
```

## Poison Message

Her çalıştığında aynı hatayı veren kayıt queue'yu bloklamamalıdır.

Belirli retry sayısı sonrası ayrı hata kuyruğuna taşınmalıdır.

## Sonuç

Retry, hatayı tekrar tekrar çalıştırmak değildir. Hatanın transient olup olmadığını sınıflandırmak, idempotency sağlamak ve unknown outcome durumlarını yönetmek gerekir.
