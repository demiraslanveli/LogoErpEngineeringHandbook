# 108 — Validation Pipeline ve Domain Kuralları

Referans uygulamada doğrulama yalnızca `if` bloklarından ibaret olmamalıdır. Logo ERP'ye gönderilecek veri; teknik, iş kuralı, referans ve süreç doğrulamalarından geçmelidir.

## Amaç

Hatalı verinin mümkün olduğunca `IData.Post()` aşamasına ulaşmadan engellenmesi.

## Önerilen Akış

```text
Request
  ↓
Basic Validation
  ↓
Reference Validation
  ↓
Business Rule Validation
  ↓
Logo Context Validation
  ↓
Mapping
  ↓
IData / ProductionApplication
  ↓
Post
```

## Doğrulama Katmanları

### 1. Temel Veri Doğrulaması

- zorunlu alanlar
- null / boş değerler
- sayı aralıkları
- tarih alanları
- kod uzunlukları
- enum veya kontrollü değer kümeleri

### 2. Referans Doğrulaması

Logo içindeki referansların gerçekten var olup olmadığı kontrol edilir.

Örnekler:

- cari kodu mevcut mu?
- malzeme kodu mevcut mu?
- birim kartı ilişkisi doğru mu?
- ambar numarası ilgili firmada geçerli mi?
- proje kartı mevcut mu?
- ödeme planı referansı geçerli mi?

Bu kontroller çoğunlukla `IQuery` veya kontrollü SQL okuması ile yapılabilir.

### 3. İş Kuralı Doğrulaması

Örnekler:

- miktar sıfır veya negatif olmamalı
- üretim emri kapanmışsa yeni gerçekleşme girilmemeli
- seri takipli malzemede seri miktarı ile hareket miktarı eşleşmeli
- KDV oranı 0 ise muafiyet/istisna kuralı tamamlanmış olmalı
- iade hareketi kaynak belgeyle uyumlu olmalı

### 4. Firma / Dönem Context Doğrulaması

İşlem başlamadan önce aşağıdaki context doğrulanmalıdır:

```text
CompanyId
PeriodId
Branch / Division
Warehouse
User / Service account
```

Yanlış firma veya döneme kayıt atılması, entegrasyon sistemlerinde en kritik veri bütünlüğü risklerinden biridir.

## Validation Result Modeli

Önerilen model:

```csharp
public sealed class ValidationResult
{
    public bool IsValid { get; set; }
    public List<ValidationError> Errors { get; set; }
}

public sealed class ValidationError
{
    public string Code { get; set; }
    public string Field { get; set; }
    public string Message { get; set; }
}
```

Örnek hata kodları uygulamaya ait olmalıdır:

```text
VAL_REQUIRED_FIELD
VAL_ITEM_NOT_FOUND
VAL_CLIENT_NOT_FOUND
VAL_WAREHOUSE_INVALID
VAL_SERIAL_QTY_MISMATCH
VAL_VAT_EXCEPTION_REQUIRED
```

Logo'nun kendi hata kodları ile uygulama validation kodları birbirine karıştırılmamalıdır.

## Validation Pipeline Arayüzü

```csharp
public interface IValidator<T>
{
    ValidationResult Validate(T request, LogoContext context);
}
```

Birden fazla validator sıralı çalıştırılabilir:

```text
OrderBasicValidator
OrderReferenceValidator
OrderBusinessRuleValidator
OrderLogoContextValidator
```

## Fail Fast mi Aggregate mi?

İki yaklaşım vardır.

### Fail Fast

İlk kritik hatada işlem durur.

Avantajı:

- daha az sorgu
- daha hızlı hata dönüşü

### Aggregate Validation

Mümkün olan tüm hatalar tek seferde kullanıcıya döner.

Avantajı:

- toplu aktarımda düzeltme süresi kısalır
- kullanıcı aynı kaydı tekrar tekrar denemek zorunda kalmaz

Batch entegrasyonlarında aggregate yaklaşımı genellikle daha kullanışlıdır.

## Logo Objects Hatalarıyla Birlikte Kullanım

Validation başarılı olsa bile `Post()` başarısız olabilir.

Bu nedenle hata modeli iki aşamalı olmalıdır:

```text
Validation Errors
        ↓
Logo Post Errors
```

Uygulama sonucu her ikisini taşıyabilmelidir.

## Gerçek Proje Kuralı

```text
Validate
   ↓
Resolve References
   ↓
Map
   ↓
Post
   ↓
Verify Result
```

> Logo Objects'in hata vermesini beklemek validation stratejisi değildir. Entegrasyon katmanı Logo'ya mümkün olduğunca temiz ve doğrulanmış veri göndermelidir.
