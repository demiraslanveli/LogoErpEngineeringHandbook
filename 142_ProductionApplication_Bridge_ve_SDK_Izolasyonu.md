# 142 — ProductionApplication Bridge ve SDK İzolasyonu

Detaylı üretim entegrasyonlarında `ProductionApplication` bağımlılığının application-service katmanına doğrudan taşınması sürdürülebilir değildir. Production SDK, Logo Objects gibi sürüm ve kurulum bağımlılığı taşıdığı için ayrı bir adapter sınırında tutulmalıdır.

## Hedef Mimari

```text
ProductionService
      ↓
IProductionGateway
      ↓
LogoProductionGateway
      ↓
IProductionApplicationBridge
      ↓
ProductionApplication
      ↓
Logo ERP
```

Application katmanı aşağıdaki bilgileri bilmez:

- COM class tipi,
- ProductionApplication oluşturma yöntemi,
- login/session yöntemi,
- production enum değerleri,
- üretim emri field/method adları,
- seri/lot veya operasyon API detayları.

## Command Modeli

Application DTO'su bridge'e doğrudan verilmez. Adapter seviyesinde bir command modeli oluşturulur.

Örnek alanlar:

```text
OrderNumber
ItemCode
PlannedQuantity
PlannedStartDate
PlannedEndDate
```

Böylece ileride ProductionApplication'ın beklediği ek alanlar adapter katmanında eklenebilir.

## Session Yaşam Döngüsü

Referans gateway akışı:

```text
bridge.Open()
     ↓
CreateProductionOrder(command)
     ↓
bridge.Close()
```

`Close()` işlemi `finally` bloğunda çağrılmalıdır.

Bu sayede production işlemi hata verse bile COM/session kaynağının serbest bırakılması hedeflenir.

## Fail-Fast Yaklaşımı

SDK henüz doğrulanmadıysa gerçek işlem yapılmamalıdır.

Referans implementasyon:

```text
UnconfiguredProductionApplicationBridge
```

bu durumda:

```text
PRODUCTION_SDK_NOT_CONFIGURED
```

sonucu döndürür.

Bu davranış, tahmine dayalı ProductionApplication kodunun production sisteminde yanlış kayıt oluşturmasından daha güvenlidir.

## Doğrulama Gerektiren Alanlar

Gerçek bridge implementasyonu yazılmadan önce kullanılan Logo sürümünde aşağıdakiler doğrulanmalıdır:

1. ProductionApplication COM/.NET tipi,
2. nesnenin oluşturulma yöntemi,
3. login ve firma/dönem bağlamı,
4. üretim emri oluşturma API'si,
5. operasyon ve iş emri davranışı,
6. sarf ve üretim girişi işlemleri,
7. seri/lot veri modeli,
8. kalite bağlantıları,
9. hata kodu ve hata açıklaması alma mekanizması,
10. COM nesnelerinin release sırası.

## Üretim Entegrasyonunda Sınırlar

Dış sistem/MES aşağıdaki bilgileri sağlayabilir:

- operasyon gerçekleşmeleri,
- gerçekleşen miktar,
- süre,
- fire,
- lot/seri,
- kalite sonucu.

Ancak resmi ERP sonucu Logo tarafında doğru iş kuralları üzerinden oluşmalıdır.

```text
MES / Middleware
      ↓
ProductionService
      ↓
ProductionApplication Bridge
      ↓
Logo Detailed Production
```

Doğrudan SQL `INSERT/UPDATE` ile üretim emri veya bağlı üretim hareketleri oluşturmak veri bütünlüğü ve maliyetlendirme açısından yüksek risklidir.

## Sonuç

ProductionApplication, genel application kodunun içinde kullanılan bir yardımcı sınıf değil; ayrı yaşam döngüsü, hata yönetimi ve sürüm doğrulaması gerektiren bir entegrasyon boundary'sidir.

Referans projede bu boundary'nin tek görevi doğrulanmış ProductionApplication API'sini uygulamanın geri kalanından izole etmektir.
