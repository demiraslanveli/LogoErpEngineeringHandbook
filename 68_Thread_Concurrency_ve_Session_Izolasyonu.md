# 68 — Thread, Concurrency ve Session İzolasyonu

## Amaç

Logo Objects entegrasyonlarında paralel çalışma performans kazandırabilir; fakat stateful COM nesnelerinin kontrolsüz paylaşılması veri karışmasına ve kararsız davranışlara yol açabilir.

## Temel Kural

Aynı `IApplication` / session instance'ı eşzamanlı bağımsız işlemler arasında paylaşılmamalıdır.

```text
Request A → Session A
Request B → Session B
```

veya kontrollü pool varsa:

```text
Request A → Pool Entry 1 (exclusive lock)
Request B → Pool Entry 2 (exclusive lock)
```

## Riskli Senaryo

```csharp
static UnityApplication App;

Task.Run(() => CreateInvoice(App));
Task.Run(() => CreateOrder(App));
```

Bu tasarımda aynı COM state paralel çağrılara maruz kalır.

## Session Isolation

Bir session üzerinde çalışan operasyon tamamlanmadan ikinci işlem o session'ı kullanmamalıdır.

Pool entry:

```csharp
class LogoSessionEntry
{
    public object Application { get; set; }
    public bool InUse { get; set; }
    public int CompanyNr { get; set; }
    public int PeriodNr { get; set; }
}
```

Gerçek uygulamada lock/semaphore mekanizması kullanılmalıdır.

## Semaphore Yaklaşımı

Belirli firma için maksimum eşzamanlı Logo işlemi sınırlandırılabilir.

```csharp
private readonly SemaphoreSlim _logoSemaphore = new SemaphoreSlim(4);
```

Bu sınır gerçek ortam testi ile belirlenmelidir.

## SQL ve Logo Objects Concurrency Farkı

SQL Server onlarca/yüzlerce paralel read query taşıyabilirken Logo Objects COM çağrıları aynı ölçek modeline sahip olmayabilir.

Bu nedenle:

- rapor/read yükü SQL read katmanına,
- resmi write işlemleri Objects katmanına

dağıtılabilir.

## Queue Based Worker

Yoğun entegrasyonda API request'lerinin doğrudan Logo kaydı oluşturması yerine queue yaklaşımı daha kontrollüdür.

```text
API
 ↓
Integration Queue
 ↓
Worker Pool
 ↓
Logo Sessions
 ↓
Logo ERP
```

## Partitioning

Queue partition anahtarı firma veya belge türü olabilir.

Örnek:

```text
Partition 102 → Worker A
Partition 202 → Worker B
```

Aynı dış belge için paralel iki job çalıştırılmamalıdır.

## Optimistic Concurrency

Entegrasyon log tablosunda status transition atomik yapılmalıdır.

Örnek:

```text
Pending → Processing → Completed
                   ↘ Failed
```

İki worker aynı kaydı almamalıdır.

SQL tarafında atomik claim örneği uygun transaction ve locking stratejisiyle uygulanabilir.

## Deadlock ve Lock Beklemeleri

Logo işlemi sırasında SQL'de ek custom transaction açıp uzun süre açık tutmak risklidir.

Özellikle:

1. custom SQL transaction aç,
2. Logo Objects çağır,
3. Logo kendi transaction'ını başlatsın,

şeklindeki iç içe davranış lock süresini büyütebilir.

## Timeout Politikası

Concurrency arttıkça timeout oranı da izlenmelidir.

Metric örnekleri:

- active logo sessions
- queue depth
- avg processing ms
- p95 processing ms
- timeout count
- retry count
- failed count

## Thread Affinity

COM nesnesi belirli thread/apartment üzerinde oluşturulmuşsa başka thread'e taşımak sorun yaratabilir. Bu nedenle nesnenin oluşturulduğu execution context ile kullanıldığı context mümkün olduğunca aynı tutulmalıdır.

## Sonuç

Logo Objects'te concurrency hedefi maksimum paralellik değil, kontrollü throughput'tur. Session izolasyonu ve bounded concurrency temel prensiptir.
