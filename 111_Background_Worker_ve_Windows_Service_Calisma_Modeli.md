# 111 — Background Worker ve Windows Service Çalışma Modeli

Logo entegrasyon servisleri çoğu zaman kullanıcı arayüzü olmadan, sürekli çalışan veya periyodik görev yürüten Windows Service süreçleri şeklinde konumlandırılır.

## Temel Sorumluluklar

Background worker şu işleri yapabilir:

- queue tüketimi
- batch aktarımı
- retry işlemleri
- reconciliation
- mail job'ları
- periyodik kontrol sorguları
- Logo Objects üzerinden veri yazma

## Önerilen Akış

```text
Windows Service
    ↓
Worker Loop
    ↓
Job Dispatcher
    ↓
Application Service
    ↓
Logo Adapter
```

## Worker Loop

Basit örnek yaklaşım:

```csharp
while (!stoppingToken.IsCancellationRequested)
{
    ProcessPendingJobs();
    Thread.Sleep(interval);
}
```

Ancak gerçek sistemde:

- cancellation
- exception isolation
- retry
- graceful shutdown
- health state
- logging

birlikte yönetilmelidir.

## Graceful Shutdown

Servis kapanırken aktif iş yarıda bırakılmamalıdır.

Önerilen süreç:

```text
Stop signal
   ↓
Yeni iş alma durur
   ↓
Aktif iş tamamlanır veya güvenli noktada bırakılır
   ↓
Logo session kapatılır
   ↓
Log flush edilir
   ↓
Service stop
```

## Session Yaşam Döngüsü

Logo `IApplication` nesnesi worker yaşam döngüsüyle bilinçsizce aynı tutulmamalıdır.

İki yaklaşım değerlendirilebilir:

### Job başına session

Daha güvenli fakat login maliyeti daha yüksek olabilir.

### Worker başına kontrollü session

Performans avantajı olabilir ancak session bozulması, timeout veya COM kaynaklarının uzun süre yaşaması iyi yönetilmelidir.

Karar gerçek Logo sürümü ve yük testiyle verilmelidir.

## Exception Isolation

Bir job hatası worker prosesini düşürmemelidir.

```text
Worker
  ├─ Job A → Success
  ├─ Job B → Error → Retry/DeadLetter
  └─ Job C → devam
```

## Polling Interval

Sabit çok kısa polling aralıkları SQL Server'a gereksiz yük bindirebilir.

Tercih:

- uygun polling interval
- boş kuyrukta backoff
- yoğunlukta daha hızlı tüketim

## Single Instance Kontrolü

Aynı job'ın birden fazla servis instance'ı tarafından aynı anda alınması engellenmelidir.

Yöntemler:

- queue status + atomic claim
- SQL locking pattern
- distributed lock
- unique processing token

## Service Recovery

Windows Service recovery seçenekleri değerlendirilmelidir:

```text
First failure  → Restart Service
Second failure → Restart Service
Subsequent     → Restart / Alert
```

Ancak sürekli crash loop oluşması monitoring tarafından görünür olmalıdır.

## Operasyonel Loglar

Her job için:

- JobId
- CorrelationId
- CompanyId
- PeriodId
- start time
- end time
- status
- attempt
- error

saklanmalıdır.

## Kritik İlke

```text
Windows Service yalnızca scheduler değildir.
```

Servis; job yaşam döngüsü, hata izolasyonu, session yönetimi, retry ve observability sorumluluklarını taşıyan operasyonel bir host katmanıdır.
