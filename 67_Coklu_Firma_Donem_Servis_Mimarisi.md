# 67 — Çoklu Firma / Dönem Servis Mimarisi

## Amaç

Tek bir servis üzerinden birden fazla Logo firması ve dönemiyle çalışmak, yalnızca tablo adını dinamik üretmek değildir. Logo Objects session'ı, firma/dönem context'i, authorization ve entegrasyon logları birlikte tasarlanmalıdır.

## Temel Context

Her entegrasyon işlemi en az şu context ile çalışmalıdır:

```text
CompanyNr
PeriodNr
UserId / ServiceIdentity
OperationType
ExternalDocumentId
CorrelationId
```

Bu context işlem boyunca immutable tutulmalıdır.

## Yanlış Yaklaşım

Global değişken kullanımı:

```csharp
static int CurrentCompany;
static int CurrentPeriod;
```

Web veya worker ortamında farklı request'ler birbirinin context'ini ezebilir.

## Doğru Yaklaşım

Firma ve dönem bilgisi request/job scope içinde taşınmalıdır.

```csharp
public sealed class LogoContext
{
    public int CompanyNr { get; init; }
    public int PeriodNr { get; init; }
    public string CorrelationId { get; init; }
}
```

## Session Key

Session pool kullanılıyorsa anahtar en az şu seviyede olmalıdır:

```text
CompanyNr + PeriodNr + LogoUser
```

Aynı company ancak farklı period, farklı context kabul edilmelidir.

## Firma/Dönem Validasyonu

İstemciden gelen firma/dönem serbestçe kabul edilmemelidir.

Whitelist yaklaşımı:

```text
102 / 01 → aktif
202 / 01 → aktif
803 / 01 → rapor-only
```

Yetkisiz firma/dönem çağrısı API seviyesinde engellenmelidir.

## Dynamic Table Name

SQL raporlarında tablo adı oluşturulurken format standardı kullanılmalıdır.

```sql
LG_{FirmNr:000}_{PeriodNr:00}_STLINE
```

Firma bazlı, dönem bağımsız tablolar:

```sql
LG_{FirmNr:000}_ITEMS
LG_{FirmNr:000}_CLCARD
```

System tabloları:

```sql
L_CAPIFIRM
L_CAPIPERIOD
```

## Routing

Büyük yapılarda firma bazlı routing uygulanabilir.

```text
API
 ↓
Context Resolver
 ↓
Company Router
 ├─ Logo Node A → Firma 102, 202
 └─ Logo Node B → Firma 803, 952
```

Bu yaklaşım farklı Logo application server veya farklı SQL instance kullanılan ortamlarda yararlıdır.

## Config Modeli

Örnek config:

```json
{
  "companies": {
    "102": {
      "period": 1,
      "logoServer": "NODE-A",
      "database": "GOPLUS"
    },
    "803": {
      "period": 1,
      "logoServer": "NODE-B",
      "database": "TIGERDB"
    }
  }
}
```

Credential bilgileri config içinde plain text tutulmamalıdır.

## Dönem Geçişi

Yıl sonu / dönem açılışında servislerin eski dönemi kullanmaya devam etmesi ciddi hatadır.

Dönem geçiş checklist:

1. Yeni period `L_CAPIPERIOD` içinde var mı?
2. Servis config güncellendi mi?
3. Table name resolver yeni dönemi üretiyor mu?
4. Logo login yeni period ile başarılı mı?
5. Test kartı / test belge kaydı çalışıyor mu?
6. Scheduled job'lar güncellendi mi?

## Periyotsuz İşlemler

Malzeme ve cari kart gibi bazı master data tabloları firma bazlıdır.

Buna rağmen Objects session'ında period gerekebilir. Bu nedenle domain olarak periyotsuz olan bir işlem ile teknik session context'i birbirinden ayrılmalıdır.

## Loglama

Her entegrasyon logunda `CompanyNr` ve `PeriodNr` bulunmalıdır. Aksi halde aynı logicalref farklı firmalarda aynı değere sahip olabileceği için log tek başına anlamsızlaşır.

Önerilen doğal kimlik:

```text
CompanyNr + PeriodNr + EntityType + LogicalRef
```

## Sonuç

Çoklu firma/dönem mimarisinde en temel kural context izolasyonudur. Firma ve dönem, global state değil işlem context'inin parçası olmalıdır.
