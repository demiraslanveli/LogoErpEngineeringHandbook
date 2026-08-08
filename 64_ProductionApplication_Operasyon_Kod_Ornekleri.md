# 64 — ProductionApplication Operasyon Kod Örnekleri

## Amaç

Bu bölüm `ProductionApplication` kullanımında operasyonel akışı kod seviyesinde nasıl yapılandırmak gerektiğini gösterir. Spesifik method ve enum isimleri kullanılan Logo Objects sürümünden doğrulanmalıdır; burada amaç güvenli entegrasyon desenini vermektir.

## Temel akış

```text
Application oluştur
  ↓
Login
  ↓
Firma / dönem bağlamı
  ↓
ProductionApplication erişimi
  ↓
Üretim emrini bul / oluştur
  ↓
Operasyon / gerçekleşme bilgilerini işle
  ↓
Sarf ve üretim hareketlerini kontrol et
  ↓
Seri/lot ve kalite verilerini doğrula
  ↓
Sonuç logu
```

## Servis katmanı önerisi

ProductionApplication çağrıları UI kodunun içine dağılmamalıdır.

Önerilen yapı:

```text
ProductionService
  - LoginContext
  - GetProductionOrder
  - StartOperation
  - CompleteOperation
  - PostConsumption
  - PostProductionReceipt
  - ValidateSerialLot
  - WriteIntegrationLog
```

## Pseudo-code: üretim emri bulma

```csharp
public ProductionOrderResult GetProductionOrder(int logicalRef)
{
    EnsureLoggedIn();

    // ProductionApplication API'sindeki kesin method adı
    // kullanılan Logo Objects sürümünden doğrulanmalıdır.
    var result = productionApp.GetProductionOrder(logicalRef);

    if (result == null)
        throw new InvalidOperationException("Üretim emri bulunamadı.");

    return result;
}
```

## Pseudo-code: operasyon tamamlaması

```csharp
public void CompleteOperation(OperationRequest request)
{
    ValidateRequest(request);
    EnsureLoggedIn();

    // 1. Üretim emri kontrolü
    // 2. İş emri / operasyon kontrolü
    // 3. Gerçekleşen miktar
    // 4. Başlangıç / bitiş zamanı
    // 5. İş merkezi
    // 6. Sarf ilişkileri
    // 7. Seri/lot gereksinimleri
    // 8. Post / commit
}
```

## MES entegrasyonu için DTO örneği

```csharp
public sealed class OperationCompletionDto
{
    public string ExternalId { get; set; }
    public int ProductionOrderRef { get; set; }
    public int OperationRef { get; set; }
    public decimal Quantity { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string WorkCenterCode { get; set; }
    public string LotNo { get; set; }
}
```

## Idempotency

MES aynı gerçekleşmeyi ikinci kez gönderirse tekrar üretim hareketi oluşturmamak gerekir.

```text
ExternalId + OperationRef
        ↓
IntegrationLog kontrolü
        ↓
Daha önce SUCCESS?
    ├─ Evet -> mevcut sonucu dön
    └─ Hayır -> işlemi gerçekleştir
```

## Transaction sınırı

Üretim entegrasyonunda aşağıdaki hareketlerin tek iş süreci olarak ele alınması gerekir:

- operasyon gerçekleşmesi
- sarf
- üretimden giriş
- seri/lot
- kalite sonucu

API izin veriyorsa transaction sınırı buna göre tasarlanmalıdır. İzin vermiyorsa compensating transaction / kontrollü geri alma yaklaşımı gerekir.

## Log örneği

```text
ExternalId        : MES-OP-458771
ProductionOrderRef: 4427
OperationRef      : 891
Quantity          : 250
Result            : SUCCESS
LogoRefs          : ...
DurationMs        : 386
```

## Hata sınıflandırması

### Business hata

- üretim emri kapalı
- operasyon sırası uygun değil
- miktar toleransı aşıldı
- seri/lot zorunlu

### Teknik hata

- login kaybı
- COM/API exception
- timeout
- bağlantı problemi

Business hatalarda otomatik retry yapılmamalıdır.

## Bilgi güven seviyesi

ProductionApplication'ın entegrasyondaki rolü ve mimari kalıplar: **Doğrulanmış saha yaklaşımı**.
Kesin class, method, enum ve field isimleri: **Kullanılan Logo Objects sürümünden doğrulanmalı**.
