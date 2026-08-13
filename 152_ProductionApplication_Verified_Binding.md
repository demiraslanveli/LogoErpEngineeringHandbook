# 152 — ProductionApplication Verified Binding

Bu bölüm `IProductionApplicationBridge` sözleşmesinin gerçek `ProductionApplication` API'sine bağlanma standardını tanımlar.

## Hedef Akış

```text
ProductionService
   ↓
LogoProductionGateway
   ↓
IProductionApplicationBridge
   ↓
VerifiedProductionApplicationBridge
   ↓
ProductionApplication
```

## Doğrulanacak API Yüzeyi

- application oluşturma,
- login/session paylaşım modeli,
- firma/dönem context'i,
- üretim emri oluşturma,
- iş emri/operasyon erişimi,
- planlanan miktar ve tarihler,
- malzeme/ürün referans çözümleme,
- seri/lot bilgileri,
- sarf/fire/üretim girişi akışları,
- kalite bağlantısı,
- hata kodu/açıklaması,
- COM yaşam döngüsü.

## Session Stratejisi

ProductionApplication'ın UnityApplication session'ını paylaşabildiği veya ayrı login gerektirdiği hedef sürümde doğrulanmalıdır. Tahmine göre session paylaşımı yapılmaz.

## Üretim Emri

`ProductionApplicationCommand` application katmanının SDK'dan bağımsız taşıma modelidir. Verified bridge bu modeli gerçek API alanlarına map eder.

## Seri/Lot

Seri/lot kayıtları yalnızca stok satırı eklemek şeklinde düşünülmemelidir. İzlenebilirlik, stok yeri ve üretim bağlantıları birlikte test edilmelidir.

## Maliyet ve Kalite

Üretim kaydı teknik olarak başarılı olsa bile maliyet veya kalite sürecini eksik bırakıyorsa entegrasyon tamamlanmış sayılmaz.

## Reconciliation

Production post sonrası aşağıdaki read-back kontrolleri önerilir:

```text
ProductionOrderRef
Produced Item
Planned Quantity
Actual Quantity
Material Consumption
Serial/Lot Links
Warehouse Movements
Costing Visibility
```

## Kabul Testleri

- üretim emri oluşturma,
- invalid item,
- plan tarihi kontrolü,
- sarf hareketleri,
- seri/lotlu üretim,
- kalite kontrollü ürün,
- retry sonrası duplicate üretim oluşmaması,
- SQL/Logo ekranı ile reconciliation.

> ProductionApplication binding'i yalnızca API çağrısı değil; üretim, stok, seri/lot, kalite ve maliyet veri bütünlüğünün birlikte korunmasıdır.
