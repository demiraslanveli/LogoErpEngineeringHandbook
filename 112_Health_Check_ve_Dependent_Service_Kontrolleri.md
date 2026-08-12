# 112 — Health Check ve Dependent Service Kontrolleri

Logo entegrasyon servisinin çalışıyor görünmesi, gerçekten sağlıklı olduğu anlamına gelmez. Sağlık kontrolü; prosesin ayakta olmasının yanında bağımlılıkların da erişilebilirliğini ölçmelidir.

## Kontrol Edilecek Katmanlar

- Windows Service durumu
- SQL Server bağlantısı
- Logo Objects login/session
- Logo şirket/dönem erişimi
- queue erişimi
- SMTP / Database Mail gerekiyorsa erişim
- dış sistem API bağlantıları
- disk alanı
- log yazma erişimi

## Health State

Basit durum modeli:

```text
Healthy
Degraded
Unhealthy
```

### Healthy

Kritik tüm bağımlılıklar çalışıyor.

### Degraded

Servis çalışıyor ancak kritik olmayan bir bağımlılık sorunlu.

### Unhealthy

Ana işlem gerçekleştirilemiyor.

## Örnek Health Result

```csharp
public sealed class HealthCheckResult
{
    public string Name { get; set; }
    public string Status { get; set; }
    public string Message { get; set; }
    public TimeSpan Duration { get; set; }
}
```

## SQL Health Check

Sadece connection açılması yeterlidir.

Ağır sorgu çalıştırılmamalıdır.

Örnek amaç:

```sql
SELECT 1
```

## Logo Health Check

Logo tarafında mümkün olan en hafif kontrol tercih edilmelidir.

Örnek akış:

```text
Create session
  ↓
Login / Context verify
  ↓
Lightweight object/query check
  ↓
Release session
```

Kesin API çağrısı kullanılan Logo Objects sürümünde doğrulanmalıdır.

## Queue Health

Aşağıdaki durumlar kontrol edilebilir:

- pending kayıt sayısı
- en eski pending kaydın yaşı
- retry kuyruğu büyüklüğü
- dead-letter sayısı

Örneğin servis ayakta olsa bile 3 saattir hiçbir kayıt tüketilmediyse sistem pratikte sağlıklı değildir.

## Threshold Örnekleri

```text
Pending age < 5 dk   → Healthy
Pending age 5–30 dk  → Degraded
Pending age > 30 dk  → Unhealthy
```

Değerler sistemin SLA'ine göre belirlenmelidir.

## Startup Health

Servis açılırken:

- config okunuyor mu?
- secret decrypt oluyor mu?
- SQL erişilebilir mi?
- Logo login yapılabiliyor mu?

kontrol edilmelidir.

Kritik bağımlılık yoksa servis ya başlamamalı ya da açıkça degraded/unhealthy durumunda kalmalıdır.

## Liveness ve Readiness Ayrımı

```text
Liveness  → proses yaşıyor mu?
Readiness → iş almaya hazır mı?
```

Windows Service mimarisinde bile bu ayrım log/monitoring seviyesinde değerlidir.

## Health Log

Her health check çağrısını detaylı loglamak gürültü yaratabilir.

Tercih:

- state değişiminde log
- uzun süre unhealthy kalırsa periyodik reminder
- recovery olduğunda recovery event

## Kritik İlke

> "Service Running" bir health check sonucu değildir. Gerçek sağlık, servisin Logo ERP işlemini güvenli şekilde gerçekleştirebildiğini doğrulamalıdır.
