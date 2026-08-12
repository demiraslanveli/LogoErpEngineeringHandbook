# 101 — Configuration ve Company / Period Context

Bu bölüm referans .NET Framework 4.8 Logo entegrasyon uygulamasında konfigürasyonun nasıl modellenmesi gerektiğini açıklar.

## Amaç

Logo entegrasyonlarında firma, dönem, kullanıcı, SQL bağlantısı ve servis ayarlarının kod içine gömülmesi uzun vadede ciddi bakım problemi oluşturur. Bu nedenle uygulama başlangıcında tek bir configuration katmanı oluşturulmalı ve tüm servisler bu katmandan beslenmelidir.

## Temel Modeller

```csharp
public sealed class LogoCompanyContext
{
    public int CompanyNr { get; set; }
    public int PeriodNr { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
}
```

```csharp
public sealed class AppSettings
{
    public string SqlConnectionString { get; set; }
    public string LogPath { get; set; }
    public int CommandTimeoutSeconds { get; set; }
}
```

## Firma ve Dönem Formatlama

Logo tablo adlarında firma ve dönem genellikle sabit genişlikte kullanılır.

```csharp
public static class LogoTableName
{
    public static string Firm(int companyNr)
        => companyNr.ToString("000");

    public static string Period(int periodNr)
        => periodNr.ToString("00");

    public static string PeriodTable(
        int companyNr,
        int periodNr,
        string tableName)
    {
        return $"LG_{Firm(companyNr)}_{Period(periodNr)}_{tableName}";
    }

    public static string FirmTable(
        int companyNr,
        string tableName)
    {
        return $"LG_{Firm(companyNr)}_{tableName}";
    }
}
```

Örnek:

```csharp
LogoTableName.PeriodTable(202, 1, "STLINE");
// LG_202_01_STLINE

LogoTableName.FirmTable(202, "ITEMS");
// LG_202_ITEMS
```

## Context İlkesi

Bir servis metodu hangi firma ve dönem üzerinde işlem yaptığını açıkça bilmelidir.

Yanlış yaklaşım:

```csharp
public void SaveInvoice(InvoiceDto dto)
{
    // Company ve period global değişkenlerden okunuyor.
}
```

Tercih edilen yaklaşım:

```csharp
public void SaveInvoice(
    LogoCompanyContext context,
    InvoiceDto dto)
{
}
```

## Çoklu Firma Ortamı

Aynı Windows Service birden fazla Logo firmasıyla çalışıyorsa context her iş kaydıyla birlikte taşınmalıdır.

```text
Queue Item
   ↓
CompanyNr
PeriodNr
OperationType
Payload
CorrelationId
```

Bu yapı sayesinde bir iş yanlışlıkla başka firmanın aktif session'ında çalıştırılmaz.

## Güvenlik

Kullanıcı adı, parola ve SQL connection string düz metin olarak kaynak koda yazılmamalıdır.

Tercihler:

- Windows Credential Manager
- DPAPI ile şifrelenmiş config
- sadece servis hesabının okuyabildiği encrypted configuration
- environment bazlı config

## Validation

Uygulama başlarken configuration doğrulanmalıdır.

```csharp
if (context.CompanyNr <= 0)
    throw new ConfigurationErrorsException("CompanyNr geçersiz.");

if (context.PeriodNr <= 0)
    throw new ConfigurationErrorsException("PeriodNr geçersiz.");
```

## Önerilen Akış

```text
Configuration
    ↓
Validation
    ↓
Company / Period Context
    ↓
Session Factory
    ↓
Application Service
```

## Temel Kural

> Firma ve dönem bilgisi global state olmamalıdır. Her entegrasyon işlemi kendi LogoCompanyContext bilgisiyle çalışmalıdır.
