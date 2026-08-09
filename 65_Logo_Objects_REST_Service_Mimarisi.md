# 65 — Logo Objects REST Service Mimarisi

## Amaç

Bu bölüm, Logo Objects tabanlı servislerin REST üzerinden dış sistemlere açılması sırasında izlenmesi gereken mimari yaklaşımı anlatır. Buradaki odak belirli bir ürün sürümünün endpoint listesinden çok, güvenli ve sürdürülebilir servis tasarımıdır.

## Temel Mimari

Önerilen katmanlar:

```text
İstemci / MES / WMS / LIMS
        ↓
REST API Katmanı
        ↓
Uygulama Servisi
        ↓
Logo Objects Adapter
        ↓
IApplication / IData / IQuery
        ↓
Logo ERP
```

REST controller katmanının doğrudan `IData` nesnesi üretip tüm iş kurallarını burada toplaması önerilmez. Logo Objects erişimi ayrı bir adapter/service katmanında kapsüllenmelidir.

## Neden Ayrı Adapter Katmanı?

- Logo Objects bağımlılığı HTTP katmanından ayrılır.
- Firma/dönem yönetimi tek yerde tutulur.
- Login/logout yaşam döngüsü kontrol edilir.
- Hata mesajları standartlaştırılır.
- Retry/idempotency uygulanabilir.
- Test doubles / mock servisler üretilebilir.

## Request Modeli

REST servisinde Logo Objects field adlarını istemciye doğrudan açmak yerine domain DTO kullanmak daha sağlıklıdır.

Örnek:

```json
{
  "documentNumber": "EXT-2026-000145",
  "customerCode": "120.01.001",
  "warehouse": 4,
  "lines": [
    {
      "itemCode": "MALZEME.001",
      "quantity": 10,
      "unitCode": "AD"
    }
  ]
}
```

Bu model adapter içinde Logo Objects field'larına çevrilir.

## Response Standardı

Başarılı ve başarısız cevapların standart olması önemlidir.

Örnek başarılı cevap:

```json
{
  "success": true,
  "logoLogicalRef": 123456,
  "documentNumber": "00001234",
  "correlationId": "..."
}
```

Örnek hata cevabı:

```json
{
  "success": false,
  "errorCode": "LOGO_POST_FAILED",
  "message": "Logo kaydı oluşturulamadı.",
  "details": [
    "Cari hesap kodu bulunamadı."
  ],
  "correlationId": "..."
}
```

## Güvenlik

REST servisinde aşağıdaki ilkeler uygulanmalıdır:

- Logo kullanıcı adı/parolası istemciden alınmamalıdır.
- Credential bilgileri server-side secret/config katmanında tutulmalıdır.
- Endpoint bazlı authorization uygulanmalıdır.
- Firma/dönem seçimi serbest string olarak kabul edilmemelidir.
- SQL veya Logo field adı istemciden dinamik olarak çalıştırılmamalıdır.

## Timeout

Logo Objects işlemleri standart web CRUD çağrılarından daha uzun sürebilir.

Bu nedenle:

- API timeout değerleri gerçek işlem süresine göre ayarlanmalıdır.
- Uzun batch işler tek HTTP request içinde işlenmemelidir.
- Büyük aktarım senaryolarında queue/worker yaklaşımı tercih edilmelidir.

## Idempotency

Aynı request'in iki kez gelmesi iki ayrı Logo belgesi oluşturmamalıdır.

Önerilen idempotency anahtarı:

```text
SourceSystem + DocumentType + ExternalDocumentId
```

Örnek entegrasyon tablosu:

```text
ExternalDocumentId
DocumentType
CompanyNr
PeriodNr
Status
LogoLogicalRef
CreatedAt
UpdatedAt
LastError
```

## Sağlık Kontrolü

Servis health check ile en az şu seviyeleri ayırmalıdır:

1. API process çalışıyor mu?
2. SQL erişimi var mı?
3. Logo Objects runtime erişilebilir mi?
4. Logo login yapılabiliyor mu?

Logo login kontrolü çok sık yapılmamalıdır; üretim sisteminde gereksiz session yükü oluşturabilir.

## Loglama

Her request için correlation id kullanılmalıdır.

Log alanları:

- CorrelationId
- Endpoint
- ExternalDocumentId
- CompanyNr
- PeriodNr
- Operation
- StartedAt
- FinishedAt
- DurationMs
- Success
- LogoLogicalRef
- Error

## REST Service Hatalarında İlk Kontrol

Logo Objects REST Service veya özel servis ayağa kalkmıyorsa şu sıra izlenebilir:

```text
Service account
→ dosya/klasör yetkileri
→ Logo Objects runtime
→ Logo install path
→ config
→ lisans
→ SQL erişimi
→ Logo login
→ event log
```

## Sonuç

Logo Objects'i REST üzerinden sunmak yalnızca bir controller yazmak değildir. En önemli konu, stateful Logo Objects davranışı ile stateless HTTP modelinin arasına kontrollü bir uygulama katmanı koymaktır.
