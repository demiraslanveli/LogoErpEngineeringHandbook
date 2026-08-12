# 104 — LogoOperationResult ve Generic Hata Parser

Bu bölüm Logo Objects işlemlerinin başarı/hata sonucunu uygulama genelinde standartlaştırmak için kullanılacak result ve hata modelini tanımlar.

## Problem

Logo Objects entegrasyonlarında hata işleme her serviste farklı yapılırsa kısa sürede şu sorunlar oluşur:

- bazı servisler yalnızca `Post()` sonucunu kontrol eder
- bazıları kullanıcıya ham Logo hata metni döndürür
- bazıları exception fırlatır
- bazıları log tutmaz
- aynı hata retry edilebilirken bazı servislerde kalıcı hata gibi değerlendirilir

Bu nedenle tek bir sonuç modeli kullanılmalıdır.

## Result Modeli

```csharp
public sealed class LogoOperationResult
{
    public bool Success { get; private set; }
    public string ErrorCode { get; private set; }
    public string ErrorMessage { get; private set; }
    public int? LogicalRef { get; private set; }
    public bool Retryable { get; private set; }

    public static LogoOperationResult Ok(int? logicalRef = null)
    {
        return new LogoOperationResult
        {
            Success = true,
            LogicalRef = logicalRef
        };
    }

    public static LogoOperationResult Fail(
        string errorCode,
        string errorMessage,
        bool retryable = false)
    {
        return new LogoOperationResult
        {
            Success = false,
            ErrorCode = errorCode,
            ErrorMessage = errorMessage,
            Retryable = retryable
        };
    }
}
```

## Exception ile Business Error Ayrımı

```text
Logo validation / Post error
        ↓
LogoOperationResult.Fail

COM exception / network / unexpected runtime error
        ↓
Exception + structured log
```

Her Logo validation hatasını exception olarak taşımak gerekli değildir.

## Generic Error Parser

```csharp
public interface ILogoErrorParser
{
    LogoOperationResult Parse(dynamic dataObject);
}
```

Örnek iskelet:

```csharp
public sealed class LogoErrorParser : ILogoErrorParser
{
    public LogoOperationResult Parse(dynamic dataObject)
    {
        if (dataObject == null)
            return LogoOperationResult.Fail("NULL_DATA", "Logo data object oluşturulamadı.");

        string message = string.Empty;

        try
        {
            message = Convert.ToString(dataObject.ErrorDesc);
        }
        catch
        {
            // Sürüme göre ErrorInfo / ErrorDesc erişimi değişebilir.
        }

        if (string.IsNullOrWhiteSpace(message))
            message = "Logo işlemi başarısız oldu ancak hata açıklaması alınamadı.";

        return LogoOperationResult.Fail("LOGO_POST_ERROR", message);
    }
}
```

Gerçek hata property isimleri kullanılan Logo Objects sürümüne göre doğrulanmalıdır.

## Retryable Hata Sınıflandırması

Retry yapılabilecek hatalar ile kalıcı iş kuralı hataları ayrılmalıdır.

Muhtemel retry adayları:

- geçici network problemi
- service unavailable
- transient SQL timeout
- COM activation geçici problemi
- kaynak kilidi / geçici concurrency problemi

Retry edilmemesi gereken tipik hatalar:

- cari kart bulunamadı
- malzeme kodu geçersiz
- zorunlu alan boş
- birim seti hatalı
- muhasebe kodu eksik
- belge tarihi kapalı dönemde

## Error Context

Her hata yalnızca mesajdan oluşmamalıdır.

Loglanması önerilen context:

```text
CorrelationId
CompanyNr
PeriodNr
OperationType
DataObjectType
ExternalId
DocumentNo
LogicalRef
ErrorCode
ErrorMessage
RetryCount
Timestamp
```

## Service Kullanımı

```csharp
var result = _logoDataAdapter.Create(
    session,
    dataObjectType,
    data => _mapper.Map(data, dto));

if (!result.Success)
{
    _logger.Error(result);
    return result;
}
```

## API / Worker Çıkışı

Üst katmana ham COM exception veya Logo nesnesi döndürülmemelidir.

Önerilen dış sonuç:

```json
{
  "success": false,
  "errorCode": "LOGO_POST_ERROR",
  "errorMessage": "...",
  "correlationId": "..."
}
```

## Temel Kural

> Logo'dan gelen hata, uygulamanın geri kalanında standart bir `LogoOperationResult` modeline dönüştürülmelidir.
