# 138 — IntegrationTestFixture Gerçek Kod

Bu bölüm referans entegrasyon çözümünün gerçek integration test fixture iskeletini gösterir.

## Amaç

Gerçek test ortamında aşağıdaki zinciri kontrollü olarak doğrulamak:

```text
Test Request
↓
Application Service
↓
Logo Adapter
↓
Logo Test Firması
↓
SQL Verification
```

## Fixture yaklaşımı

```csharp
public sealed class IntegrationTestFixture : IDisposable
{
    public AppSettings Settings { get; }
    public ILogoSessionFactory LogoSessionFactory { get; }
    public ISqlConnectionFactory SqlConnectionFactory { get; }
    public ILogger Logger { get; }

    public IntegrationTestFixture()
    {
        Settings = TestSettingsLoader.Load();

        if (!Settings.EnvironmentName.Equals(
                "TEST",
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Integration tests can only run in TEST environment.");
        }

        Logger = LoggerFactory.Create(Settings.Logging);

        SqlConnectionFactory = new SqlConnectionFactory(
            Settings.Sql.ConnectionString);

        LogoSessionFactory = new LogoSessionFactory(
            Settings.Logo,
            Logger);
    }

    public void Dispose()
    {
        (LogoSessionFactory as IDisposable)?.Dispose();
        (Logger as IDisposable)?.Dispose();
    }
}
```

## Test için güvenlik kilidi

Testlerin yanlışlıkla canlı firmada çalışmasını önlemek için yalnız environment adına güvenilmemelidir.

Ek kontroller önerilir:

```text
beklenen SQL Server adı
beklenen database adı
beklenen Logo firma numarası
beklenen dönem
TEST_ONLY flag
```

Örnek:

```csharp
private void EnsureSafeEnvironment(AppSettings settings)
{
    if (settings.Logo.CompanyNr != 999)
        throw new InvalidOperationException("Unexpected test company.");

    if (!settings.Sql.DatabaseName.EndsWith("_TEST"))
        throw new InvalidOperationException("Unexpected test database.");
}
```

## Örnek malzeme testi

```csharp
[TestMethod]
public void CreateMaterial_Should_Create_And_Read_Back()
{
    var request = new CreateMaterialRequest
    {
        Code = "TEST-" + Guid.NewGuid().ToString("N").Substring(0, 10),
        Description = "Integration Test Material"
    };

    var service = TestCompositionRoot.CreateMaterialService(_fixture);

    var result = service.Create(request);

    Assert.IsTrue(result.Success, result.ErrorMessage);
    Assert.IsTrue(result.LogicalRef.HasValue);

    var exists = MaterialSqlAssertions.Exists(
        _fixture.SqlConnectionFactory,
        _fixture.Settings.Logo.CompanyNr,
        request.Code);

    Assert.IsTrue(exists);
}
```

## Arrange / Act / Assert

Test yapısı okunabilir tutulmalıdır:

```text
Arrange
- request oluştur
- fixture hazırla

Act
- application service çağır

Assert
- service result
- Logo LOGICALREF
- SQL read-back
- ilişkili kayıtlar
```

## Cleanup

Test verisinin silinmesi Logo nesnelerinde doğrudan SQL `DELETE` ile yapılmamalıdır.

Mümkünse:

- özel test firması,
- test kod prefix'i,
- periyodik kontrollü test data cleanup,
- gerekiyorsa Logo Objects üzerinden silme

kullanılmalıdır.

## Özellikle doğrulanması gerekenler

Sipariş/fatura/üretim testlerinde yalnız header oluştu mu kontrolü yeterli değildir.

Ayrıca:

```text
satırlar
birim
ambar
referanslar
seri/lot
cari hareket
muhasebe bağlantısı
miktar/tutar
idempotency kaydı
reconciliation sonucu
```

kontrol edilmelidir.

> Integration test başarılı olmak için yalnız `Post()` sonucuna değil, Logo'da oluşan ilişkili veri bütünlüğüne bakmalıdır.
