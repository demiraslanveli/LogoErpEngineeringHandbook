# 155 — Windows Service Host Production Modeli

Bu bölüm referans Worker uygulamasının production Windows Service modeline dönüştürülmesini tanımlar.

## Host Sorumluluğu

Windows Service yalnızca process host değildir. Lifecycle, recovery ve kontrollü kapanış davranışlarını yönetmelidir.

## Temel Akış

```text
Service OnStart
   ↓
Load Configuration
   ↓
Run Startup Health Gate
   ↓
Start Worker Loop

Service OnStop
   ↓
Cancel Token
   ↓
Finish Current Safe Unit
   ↓
Close Logo Session
   ↓
Release COM
```

## ServiceBase

.NET Framework 4.8 ortamında gerçek Windows Service host için `ServiceBase` yaklaşımı kullanılabilir. Business processing `ServiceBase` sınıfının içine gömülmemelidir; mevcut `WorkerLoop` ayrı kalmalıdır.

## Graceful Shutdown

Stop sırasında:

- yeni queue item alınmamalı,
- devam eden işlem güvenli noktada tamamlanmalı veya kontrollü şekilde bırakılmalı,
- Logo session kapatılmalı,
- COM nesneleri release edilmeli,
- final log/heartbeat yazılmalı.

## Tek Instance

Aynı firma/dönem/scope için birden fazla worker istenmiyorsa distributed lock veya application lock yaklaşımı düşünülmelidir. Sadece process mutex çoklu sunucu senaryosunu çözmez.

## Recovery

Windows Service recovery ayarları deployment scripti tarafından uygulanmalıdır. Örnek politika:

```text
1. failure → restart
2. failure → restart
3. failure → restart + alert
```

Sonsuz hızlı restart döngüsü engellenmelidir.

## Service Account

Least privilege servis hesabı kullanılmalıdır. Hesap yalnızca ihtiyaç duyduğu:

- Logo runtime erişimi,
- COM activation,
- SQL erişimi,
- log klasörü,
- gerekli network kaynakları

yetkilerine sahip olmalıdır.

## Kabul Testleri

- start/stop,
- server reboot sonrası auto-start,
- SQL unavailable startup,
- Logo login failure,
- stop sırasında aktif işlem,
- service crash recovery,
- COM process sızıntısı kontrolü.

> Worker kodu ile Windows Service host ayrıldığında aynı processing motoru console/test/service ortamlarında yeniden kullanılabilir.
