# 119 — Üretim Entegrasyon Servisi Referans Implementasyonu

Bu bölüm, MES veya başka bir üretim sisteminden gelen üretim verisinin Logo tarafında kontrollü biçimde işlenmesi için referans servis tasarımını açıklar.

## Amaç

Üretim emri, iş emri/operasyon, hammadde sarfı, mamul üretimi, seri/lot ve kalite/maliyet ilişkilerini tek entegrasyon akışında yönetmek.

## Katmanlar

```text
ProductionIntegrationService
        ↓
ProductionValidator
        ↓
ProductionReferenceResolver
        ↓
ProductionMapper
        ↓
IProductionAdapter
        ↓
ProductionApplication / Logo Objects
        ↓
Logo ERP
```

## DTO

```csharp
public sealed class ProductionCompletionRequest
{
    public string ExternalProductionId { get; set; }
    public string ProductionOrderNo { get; set; }
    public string FinishedGoodCode { get; set; }
    public decimal ProducedQuantity { get; set; }
    public string UnitCode { get; set; }
    public string WarehouseCode { get; set; }
    public DateTime OperationDate { get; set; }
    public List<ConsumptionLineRequest> Consumptions { get; set; }
    public List<SerialLotRequest> SerialLots { get; set; }
}
```

## Temel akış

```text
MES completion event
      ↓
Idempotency kontrolü
      ↓
Üretim emri çözümleme
      ↓
Malzeme / birim / ambar kontrolü
      ↓
Sarf validasyonu
      ↓
Mamul miktarı validasyonu
      ↓
Seri / lot validasyonu
      ↓
ProductionApplication / Logo Objects işlemleri
      ↓
ERP kayıtları
      ↓
Reconciliation
      ↓
MES'e sonuç
```

## Üretim emri referansı

Dış sistem yalnızca üretim emri numarası gönderebilir.

Servis bunu Logo üretim emri logical reference değerine çözmelidir.

```text
ProductionOrderNo
      ↓
PRODORD lookup
      ↓
LOGICALREF
```

## Planlanan ve gerçekleşen

Servis aşağıdaki değerleri ayırmalıdır:

- planlanan üretim miktarı
- gerçekleşen üretim miktarı
- planlanan sarf
- gerçekleşen sarf
- planlanan süre
- gerçekleşen süre

Dış sistemden gelen gerçek değerler doğrudan plan değerlerinin üzerine yazılmamalıdır.

## Hammadde sarfı

Her tüketim satırı için:

- malzeme
- miktar
- birim
- ambar
- seri/lot
- proje veya üretim emri ilişkisi

kontrol edilmelidir.

Negatif veya anlamsız sarf miktarı validation katmanında reddedilmelidir.

## Mamul üretimi

Mamul girişi ile hammadde sarfı aynı iş olayının parçalarıdır.

İşlem zinciri yarıda kalırsa sistem yarım üretim kaydı bırakmamalıdır.

Bunun için transaction boundary ve reconciliation birlikte tasarlanmalıdır.

## Seri / lot

Seri/lot takibi aktif malzemelerde yalnızca toplam miktar kaydetmek yeterli değildir.

Kontroller:

- lot toplamı = üretilen miktar
- seri adedi = gereken miktar
- duplicate seri yok
- lot/seri formatı geçerli
- kaynak sarf lotları mevcut

## Kalite entegrasyonu

Kalite sonucu üretim akışının öncesinde veya sonrasında sisteme gelebilir.

Örnek statüler:

```text
Produced
Quarantine
Approved
Rejected
Released
```

Gerçek Logo kalite nesne/alanları kullanılan sürüme göre doğrulanmalıdır.

## Maliyet

Üretim kaydının ardından maliyet şu girdilerden etkilenebilir:

- gerçek sarf miktarı
- hammadde maliyetleri
- işçilik/operasyon süreleri
- genel giderler
- döviz
- ambar
- maliyetlendirme zamanı

Bu nedenle üretim entegrasyonu yalnızca stok miktarı açısından test edilmemelidir.

## Idempotency

Önerilen key:

```text
SourceSystem + ProductionCompletion + ExternalProductionId
```

MES aynı completion event'i tekrar gönderdiğinde üretim ve sarf ikinci kez oluşmamalıdır.

## Reconciliation modeli

Saklanması önerilen bilgiler:

- ExternalProductionId
- LogoProductionOrderRef
- üretim hareket referansları
- sarf hareket referansları
- seri/lot referansları
- üretilen miktar
- sarf toplamları
- kayıt zamanı
- result/status

## Retry

Teknik hata ile iş kuralı hatası ayrılmalıdır.

Retry edilebilir:

- geçici servis erişim problemi
- geçici SQL/network problemi

Retry edilmemeli:

- üretim emri bulunamadı
- geçersiz malzeme
- seri duplicate
- miktar uyumsuzluğu
- kapalı üretim emri

## Test senaryoları

1. normal üretim tamamlama
2. birden fazla sarf satırı
3. seri/lot takipli mamul
4. seri/lot takipli hammadde
5. duplicate completion event
6. planı aşan üretim
7. kapalı üretim emri
8. hatalı ambar
9. sarf/mamul zincirinde teknik hata
10. retry sonrası duplicate oluşmaması
11. kalite bekleyen üretim
12. maliyet reconciliation

> Üretim entegrasyon servisi, MES verisini Logo'ya kopyalayan bir servis değil; üretim iş olayını Logo'nun stok, seri/lot, kalite ve maliyet modeline güvenli biçimde dönüştüren orchestration katmanıdır.
