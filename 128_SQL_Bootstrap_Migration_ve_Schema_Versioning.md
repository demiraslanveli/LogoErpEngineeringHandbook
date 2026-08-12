# 128 — SQL Bootstrap, Migration ve Schema Versioning

Bu bölüm entegrasyon uygulamasının kendi SQL nesnelerini kontrollü ve versiyonlanabilir biçimde yönetme standardını tanımlar.

## Temel Kural

Logo'nun standart tabloları migration sistemiyle değiştirilmemelidir.

Migration kapsamı yalnızca uygulamaya ait nesneler olmalıdır:

- integration queue
- idempotency store
- reconciliation
- audit/log
- configuration metadata
- application-specific views/procedures

## Önerilen Klasör Yapısı

```text
database/
├─ bootstrap/
│  ├─ 001_CreateSchema.sql
│  └─ 002_CreateMigrationHistory.sql
│
├─ migrations/
│  ├─ V001__Create_Idempotency.sql
│  ├─ V002__Create_Reconciliation.sql
│  ├─ V003__Create_Audit.sql
│  └─ V004__Add_Queue_Index.sql
│
└─ rollback/
   ├─ R004__Drop_Queue_Index.sql
   └─ ...
```

## Migration History

Örnek tablo:

```sql
CREATE TABLE dbo.APP_SCHEMA_VERSION
(
    ID              BIGINT IDENTITY(1,1) PRIMARY KEY,
    VERSION_NO      VARCHAR(30) NOT NULL,
    SCRIPT_NAME     VARCHAR(255) NOT NULL,
    CHECKSUM_VALUE  VARCHAR(128) NULL,
    APPLIED_AT      DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    APPLIED_BY      VARCHAR(128) NULL,
    SUCCESS         BIT NOT NULL
);
```

## Migration Prensibi

Her migration:

- tek amaçlı olmalı
- tekrar çalıştırılmaya karşı kontrollü olmalı
- production öncesi test ortamında denenmeli
- rollback veya forward-fix stratejisine sahip olmalı
- Logo standart şemasına gereksiz bağımlılık eklememeli

## Idempotent DDL

Örnek:

```sql
IF OBJECT_ID('dbo.APP_IDEMPOTENCY', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.APP_IDEMPOTENCY
    (
        OPERATION_KEY VARCHAR(200) NOT NULL PRIMARY KEY,
        STATUS VARCHAR(30) NOT NULL,
        CREATED_AT DATETIME2 NOT NULL,
        UPDATED_AT DATETIME2 NOT NULL
    );
END;
```

Ancak her migration'ın tamamen idempotent olması zorunlu değildir. Önemli olan migration history üzerinden bir kez ve kontrollü uygulanmasıdır.

## Transaction Kullanımı

DDL işlemlerinin transaction davranışı SQL Server nesnesine göre değerlendirilmelidir.

Örnek yaklaşım:

```sql
SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRAN;

    -- migration

    COMMIT;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK;

    THROW;
END CATCH;
```

Uzun süren index işlemleri için aynı yaklaşım körlemesine kullanılmamalıdır.

## Checksum

Migration dosyalarının sonradan sessizce değiştirilmesini önlemek için checksum saklamak yararlıdır.

Prensip:

```text
Uygulanmış migration değişmez.
Yeni değişiklik = yeni migration.
```

## Logo Firma/Dönem Bağımlılığı

Uygulama tabloları mümkün olduğunca firma/dönemden bağımsız ortak şemada tutulmalıdır.

Kayıtlarda bağlam saklanabilir:

```text
FIRM_NR
PERIOD_NR
COMPANY_KEY
```

Logo tablo adlarını uygulama migration'larında hard-code etmekten kaçınılmalıdır.

## View ve Procedure Güncellemeleri

Uygulamaya ait view/procedure için `CREATE OR ALTER` desteklenen SQL Server sürümlerinde tercih edilebilir.

Eski SQL Server sürümlerinde uyumluluk ayrıca kontrol edilmelidir.

## Deployment Sırası

Önerilen sıra:

```text
Backup / restore point
    ↓
Pre-check
    ↓
Database migrations
    ↓
Application binaries
    ↓
Configuration
    ↓
Service start
    ↓
Health check
    ↓
Smoke test
```

## Kural

> Uygulama binary sürümü ile database schema sürümü birlikte izlenmelidir.
