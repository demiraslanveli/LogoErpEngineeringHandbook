# 135 — Migration Runner Gerçek Kod İskeleti

Bu bölüm 128. bölümde tanımlanan schema versioning yaklaşımını gerçek C# koduna indirger.

## Hedef

Uygulama ayağa kalkarken veya kontrollü deployment adımında:

```text
Current Schema Version
↓
Bekleyen Migration'ları Bul
↓
Sırayla Çalıştır
↓
Başarılıysa Version Kaydet
↓
Hata varsa Dur ve Logla
```

## Migration sözleşmesi

```csharp
public interface IDatabaseMigration
{
    int Version { get; }
    string Name { get; }
    void Up(System.Data.IDbConnection connection,
            System.Data.IDbTransaction transaction);
}
```

## Runner örneği

```csharp
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

public sealed class MigrationRunner
{
    private readonly ISqlConnectionFactory _connectionFactory;
    private readonly IEnumerable<IDatabaseMigration> _migrations;
    private readonly ILogger _logger;

    public MigrationRunner(
        ISqlConnectionFactory connectionFactory,
        IEnumerable<IDatabaseMigration> migrations,
        ILogger logger)
    {
        _connectionFactory = connectionFactory;
        _migrations = migrations;
        _logger = logger;
    }

    public void Run()
    {
        using (var connection = _connectionFactory.Create())
        {
            connection.Open();

            EnsureVersionTable(connection);

            var currentVersion = GetCurrentVersion(connection);

            var pending = _migrations
                .Where(x => x.Version > currentVersion)
                .OrderBy(x => x.Version)
                .ToList();

            foreach (var migration in pending)
            {
                using (var tx = connection.BeginTransaction())
                {
                    try
                    {
                        migration.Up(connection, tx);
                        SaveVersion(connection, tx, migration);
                        tx.Commit();

                        _logger.Info(
                            "Migration applied. Version={Version} Name={Name}",
                            migration.Version,
                            migration.Name);
                    }
                    catch (Exception ex)
                    {
                        tx.Rollback();

                        _logger.Error(ex,
                            "Migration failed. Version={Version} Name={Name}",
                            migration.Version,
                            migration.Name);

                        throw;
                    }
                }
            }
        }
    }
}
```

## Version tablosu

Örnek:

```sql
CREATE TABLE dbo.APP_SCHEMA_VERSION
(
    VERSION_NO      INT          NOT NULL PRIMARY KEY,
    MIGRATION_NAME  NVARCHAR(200) NOT NULL,
    APPLIED_AT      DATETIME2(0) NOT NULL DEFAULT SYSDATETIME()
);
```

## Migration örneği

```csharp
public sealed class Migration001CreateIdempotencyTable : IDatabaseMigration
{
    public int Version => 1;
    public string Name => "Create idempotency table";

    public void Up(IDbConnection connection, IDbTransaction transaction)
    {
        using (var cmd = connection.CreateCommand())
        {
            cmd.Transaction = transaction;
            cmd.CommandText = @"
CREATE TABLE dbo.APP_IDEMPOTENCY
(
    IDEMPOTENCY_KEY NVARCHAR(200) NOT NULL PRIMARY KEY,
    OPERATION_TYPE  NVARCHAR(100) NOT NULL,
    STATUS          NVARCHAR(30)  NOT NULL,
    LOGICALREF      INT NULL,
    CREATED_AT      DATETIME2(0) NOT NULL,
    COMPLETED_AT    DATETIME2(0) NULL
);";

            cmd.ExecuteNonQuery();
        }
    }
}
```

## Önemli kural

Logo'nun kendi tablolarının schema'sını migration runner ile değiştirmek bu yapının amacı değildir.

Migration sistemi esas olarak entegrasyon uygulamasına ait:

- idempotency tabloları,
- reconciliation tabloları,
- queue tabloları,
- audit tabloları,
- uygulama konfigürasyon tabloları

için kullanılmalıdır.

> Uygulama schema değişiklikleri kod gibi versionlanmalı ve deployment sürecinin parçası olmalıdır.
