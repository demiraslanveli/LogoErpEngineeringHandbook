# 137 — HealthCheckRunner Gerçek Kod

Bu bölüm entegrasyon servisinin bağımlılıklarını periyodik olarak kontrol eden health-check runner yapısını gösterir.

## Kontrol edilmesi gereken bağımlılıklar

Örnek olarak:

```text
SQL Server
Logo Objects Login
Logo Company / Period erişimi
Queue tablosu
Disk / Log klasörü
Gerekirse ProductionApplication erişimi
```

## Health result modeli

```csharp
public sealed class HealthCheckResult
{
    public string Name { get; set; }
    public bool Healthy { get; set; }
    public string Message { get; set; }
    public long DurationMs { get; set; }
}
```

## Sözleşme

```csharp
public interface IHealthCheck
{
    string Name { get; }
    HealthCheckResult Check();
}
```

## SQL health check

```csharp
public sealed class SqlHealthCheck : IHealthCheck
{
    private readonly ISqlConnectionFactory _connectionFactory;

    public string Name => "SQL Server";

    public SqlHealthCheck(ISqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public HealthCheckResult Check()
    {
        var sw = Stopwatch.StartNew();

        try
        {
            using (var connection = _connectionFactory.Create())
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT 1";
                    command.ExecuteScalar();
                }
            }

            sw.Stop();

            return new HealthCheckResult
            {
                Name = Name,
                Healthy = true,
                DurationMs = sw.ElapsedMilliseconds
            };
        }
        catch (Exception ex)
        {
            sw.Stop();

            return new HealthCheckResult
            {
                Name = Name,
                Healthy = false,
                Message = ex.Message,
                DurationMs = sw.ElapsedMilliseconds
            };
        }
    }
}
```

## Logo health check yaklaşımı

Logo Objects tarafında kesin API çağrısı kullanılan sürüme göre değişebilir. Genel akış:

```text
SessionFactory ile yeni kontrollü session aç
↓
Login / Company / Period context doğrula
↓
Basit read-only işlem yap
↓
Session kapat
```

Health check resmi ERP verisi oluşturmamalıdır.

## Runner

```csharp
public sealed class HealthCheckRunner
{
    private readonly IEnumerable<IHealthCheck> _checks;
    private readonly ILogger _logger;

    public HealthCheckRunner(
        IEnumerable<IHealthCheck> checks,
        ILogger logger)
    {
        _checks = checks;
        _logger = logger;
    }

    public IReadOnlyList<HealthCheckResult> Run()
    {
        var results = new List<HealthCheckResult>();

        foreach (var check in _checks)
        {
            var result = check.Check();
            results.Add(result);

            if (result.Healthy)
            {
                _logger.Info(
                    "HealthCheck OK. Name={Name} DurationMs={DurationMs}",
                    result.Name,
                    result.DurationMs);
            }
            else
            {
                _logger.Error(
                    "HealthCheck FAILED. Name={Name} DurationMs={DurationMs} Message={Message}",
                    result.Name,
                    result.DurationMs,
                    result.Message);
            }
        }

        return results;
    }
}
```

## Readiness ve liveness ayrımı

Mümkünse iki ayrı kavram tutulmalıdır:

```text
Liveness  -> proses çalışıyor mu?
Readiness -> Logo/SQL dahil bağımlılıklarla iş yapabilecek durumda mı?
```

Bir dependency geçici olarak down olduğunda prosesin kendisi alive olabilir ama ready olmayabilir.

> Health check yalnızca ping değildir; servisin gerçek iş yapabilme kapasitesini güvenli ve read-only kontrollerle ölçmelidir.
