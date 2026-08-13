# 154 — Health Persistence ve Operational Status Modeli

Bu bölüm runtime health bilgisinin yalnızca console çıktısı olmaktan çıkarılıp izlenebilir operasyon verisine dönüştürülmesini tanımlar.

## Health Boyutları

```text
SQL
Logo Session
Logo SDK Binding
Queue
Reconciliation
Worker Loop
ProductionApplication
```

## Health Record

Önerilen alanlar:

```text
CheckName
Status
CheckedAt
DurationMs
Message
ErrorCode
Company
Period
HostName
ApplicationVersion
```

## Status Değerleri

```text
Healthy
Degraded
Unhealthy
Unknown
```

## Persistence

Health verisi application-owned bir SQL tablosuna yazılabilir. Logo standart tablolarına health kaydı yazılmaz.

## Heartbeat

Worker periyodik heartbeat üretmelidir. Son başarılı heartbeat'in yaşı operasyon ekibi tarafından izlenebilmelidir.

## Alert Kuralları

Örnek:

- SQL 3 ardışık kontrolde unhealthy,
- Logo login 5 dakikadan uzun başarısız,
- queue backlog eşik üstünde,
- reconciliation mismatch artıyor,
- worker heartbeat gecikmiş.

## Health ve Business Error Ayrımı

Tek bir fatura post hatası host health'i doğrudan unhealthy yapmamalıdır. Ancak sistematik SDK/login/SQL erişim hatası health durumunu düşürmelidir.

## Startup Gate

Critical dependency unhealthy ise worker business processing başlatmamalıdır.

```text
Config Valid
   ↓
SQL Healthy
   ↓
SDK Binding Valid
   ↓
Logo Login Healthy
   ↓
Start Processing
```

> Health persistence operasyon ekibine yalnızca 'servis çalışıyor mu' değil, 'entegrasyon gerçekten iş yapabiliyor mu' sorusunun cevabını verir.
