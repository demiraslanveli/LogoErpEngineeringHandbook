# 136 — SQL Idempotency Repository Gerçek Kod

Bu bölüm idempotency store'un gerçek repository implementasyonunu gösterir.

## Amaç

Aynı dış işlemin ikinci kez Logo'ya yazılmasını engellemek.

## Sözleşme

```csharp
public interface IIdempotencyRepository
{
    bool Exists(string key);
    bool TryBegin(string key, string operationType);
    void MarkCompleted(string key, int? logicalRef);
    void MarkFailed(string key, string errorMessage);
}
```

## SQL tablosu

```sql
CREATE TABLE dbo.APP_IDEMPOTENCY
(
    IDEMPOTENCY_KEY NVARCHAR(200) NOT NULL PRIMARY KEY,
    OPERATION_TYPE  NVARCHAR(100) NOT NULL,
    STATUS          NVARCHAR(30)  NOT NULL,
    LOGICALREF      INT NULL,
    ERROR_MESSAGE   NVARCHAR(2000) NULL,
    CREATED_AT      DATETIME2(0) NOT NULL DEFAULT SYSDATETIME(),
    COMPLETED_AT    DATETIME2(0) NULL
);
```

## TryBegin örneği

```csharp
public bool TryBegin(string key, string operationType)
{
    using (var connection = _connectionFactory.Create())
    {
        connection.Open();

        using (var command = connection.CreateCommand())
        {
            command.CommandText = @"
INSERT INTO dbo.APP_IDEMPOTENCY
(
    IDEMPOTENCY_KEY,
    OPERATION_TYPE,
    STATUS,
    CREATED_AT
)
VALUES
(
    @Key,
    @OperationType,
    'PROCESSING',
    SYSDATETIME()
);";

            AddParameter(command, "@Key", key);
            AddParameter(command, "@OperationType", operationType);

            try
            {
                return command.ExecuteNonQuery() == 1;
            }
            catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
            {
                return false;
            }
        }
    }
}
```

## MarkCompleted

```csharp
public void MarkCompleted(string key, int? logicalRef)
{
    using (var connection = _connectionFactory.Create())
    {
        connection.Open();

        using (var command = connection.CreateCommand())
        {
            command.CommandText = @"
UPDATE dbo.APP_IDEMPOTENCY
SET
    STATUS = 'COMPLETED',
    LOGICALREF = @LogicalRef,
    COMPLETED_AT = SYSDATETIME(),
    ERROR_MESSAGE = NULL
WHERE IDEMPOTENCY_KEY = @Key;";

            AddParameter(command, "@Key", key);
            AddParameter(command, "@LogicalRef", (object)logicalRef ?? DBNull.Value);

            command.ExecuteNonQuery();
        }
    }
}
```

## MarkFailed

```csharp
public void MarkFailed(string key, string errorMessage)
{
    using (var connection = _connectionFactory.Create())
    {
        connection.Open();

        using (var command = connection.CreateCommand())
        {
            command.CommandText = @"
UPDATE dbo.APP_IDEMPOTENCY
SET
    STATUS = 'FAILED',
    ERROR_MESSAGE = @ErrorMessage
WHERE IDEMPOTENCY_KEY = @Key;";

            AddParameter(command, "@Key", key);
            AddParameter(command, "@ErrorMessage", errorMessage);

            command.ExecuteNonQuery();
        }
    }
}
```

## Kullanım

```csharp
if (!_idempotencyRepository.TryBegin(key, "SALES_ORDER"))
    return LogoOperationResult.AlreadyProcessed(key);

try
{
    var result = _orderService.Create(request);

    if (result.Success)
        _idempotencyRepository.MarkCompleted(key, result.LogicalRef);
    else
        _idempotencyRepository.MarkFailed(key, result.ErrorMessage);

    return result;
}
catch (Exception ex)
{
    _idempotencyRepository.MarkFailed(key, ex.Message);
    throw;
}
```

## Kritik nokta

Sadece `Exists()` kontrol edip sonra insert yapmak yarış koşulu oluşturabilir.

Daha güvenli yaklaşım unique key üzerinde atomik `INSERT` denemesi yapmaktır.

> Idempotency kontrolü uygulama tarafında if kontrolü değil, veritabanı unique constraint ile desteklenen atomik bir sahiplenme mekanizması olmalıdır.
