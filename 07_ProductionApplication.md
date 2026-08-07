# 07 — ProductionApplication

## 1. Bölümün Amacı

Bu bölüm, Logo ERP'nin detaylı üretim altyapısıyla entegrasyon geliştirirken kullanılan `ProductionApplication` yaklaşımını kavramsal olarak açıklar.

`IApplication` ve `IData`, Logo Objects tarafında genel kart ve fiş işlemlerinin temelini oluştururken; detaylı üretim senaryolarında üretim emirleri, operasyonlar, iş istasyonları, reçeteler, sarf/üretim ilişkileri ve üretim hareketlerinin iş kurallarına uygun şekilde yönetilmesi gerekir.

Temel prensip:

> Detaylı üretim entegrasyonunda amaç yalnızca stok hareketi oluşturmak değil, üretim sürecinin Logo içinde doğru bağlantılarla ve maliyetlendirmeye uygun biçimde oluşmasını sağlamaktır.

---

## 2. Detaylı Üretim Neden Farklıdır?

Basit bir stok çıkışı ile detaylı üretim aynı şey değildir.

Bir üretim süreci aşağıdaki ilişkileri içerebilir:

- Üretim emri
- Üretim reçetesi / ürün reçetesi
- Operasyonlar
- İş istasyonları
- İş emirleri
- Planlanan üretim miktarı
- Gerçekleşen üretim miktarı
- Sarf edilen hammaddeler
- Fire
- Yan ürün
- Seri/lot takibi
- Kalite süreçleri
- Proje bağlantıları
- Maliyetlendirme

Bu nedenle doğrudan `STFICHE/STLINE` üreterek stok giriş-çıkışı yapmak, fiziksel stok açısından sonuç üretse bile üretim zincirini eksik bırakabilir.

---

## 3. ProductionApplication'ın Rolü

`ProductionApplication`, üretim modülüyle ilgili nesne ve süreçlere erişim sağlayan uygulama katmanıdır.

Genel mimari:

```text
Harici Uygulama / MES
        │
        ▼
Logo Objects
        │
        ├── IApplication
        │     └── Genel kart / fiş işlemleri
        │
        └── ProductionApplication
              └── Detaylı üretim süreçleri
```

Buradaki amaç Logo'nun üretim iş mantığını dış uygulamanın tekrar yazması değildir.

Harici sistem operasyonel veri sağlayabilir; resmi üretim kayıtlarının Logo kurallarına göre oluşması hedeflenir.

---

## 4. MES ve Logo Arasındaki Görev Ayrımı

Detaylı üretim projelerinde en kritik mimari kararlardan biri hangi sistemin hangi verinin sahibi olduğudur.

Örnek görev dağılımı:

### MES / Ara Yazılım

- Operatör ekranları
- Makine bağlantıları
- Gerçek zamanlı üretim miktarı
- Duruş nedenleri
- Operasyon başlangıç/bitiş zamanları
- Barkod/QR okutma
- Tartım
- Hat içi kalite verisi
- Anlık üretim performansı

### Logo ERP

- Resmi malzeme kartları
- Üretim emirleri
- Ürün reçeteleri
- Stok hareketleri
- Seri/lot ana kayıtları
- Resmi sarf ve üretim fişleri
- Finansal/muhasebesel sonuçlar
- Maliyetlendirme

Bu ayrım yapılmadan geliştirilen sistemlerde aynı veri iki tarafta farklı gerçekler olarak yaşamaya başlayabilir.

---

## 5. Üretim Emri Merkezli Tasarım

Detaylı üretim entegrasyonunda üretim emri genellikle ana bağlayıcı kayıttır.

Bir üretim emri üzerinden:

- Üretilecek mamul,
- Planlanan miktar,
- Başlangıç/bitiş tarihleri,
- Reçete,
- Operasyonlar,
- İş emirleri,
- Sarf malzemeleri,
- Gerçekleşmeler

ilişkilendirilebilir.

Harici sistemde Logo üretim emri için genellikle aşağıdaki bilgiler saklanabilir:

```text
LogoProductionOrderRef
LogoProductionOrderNo
ExternalProductionId
IntegrationStatus
LastSyncDate
```

Ancak `LOGICALREF` tek iş anahtarı olmamalıdır. Harici sistem kendi benzersiz ID'sini de korumalıdır.

---

## 6. Üretim Reçetesi

Ürün reçetesi, mamulün hangi girdiler ve operasyonlarla üretileceğini tanımlar.

Reçete tarafında tipik olarak:

- Ana ürün
- Hammadde / yarı mamul
- Miktar ilişkileri
- Fire oranları
- Operasyon bağlantıları
- İş istasyonu ilişkileri
- Alternatif malzemeler

bulunabilir.

Üretim entegrasyonunda önemli soru şudur:

> Reçete dış sistemden mi yönetilecek, yoksa Logo'daki resmi reçete mi kullanılacak?

Çoğu kurumsal senaryoda resmi ERP reçetesinin Logo tarafında tutulması ve dış sistemin bunu referans alması daha güvenlidir.

---

## 7. Planlanan ve Gerçekleşen Miktar Ayrımı

Üretim sistemlerinde şu iki kavram kesinlikle ayrılmalıdır:

- Planlanan miktar
- Gerçekleşen miktar

Örneğin:

```text
Planlanan üretim: 10.000 adet
Gerçekleşen üretim: 9.760 adet
Fire: 240 adet
```

ERP tarafında gerçekleşen hareketler maliyetlendirmeyi etkileyebilir.

Harici sistem yalnızca üretim emrinin plan miktarını güncelleyerek fiili üretim kaydı yerine geçmemelidir.

---

## 8. ACTAMOUNT ve ACTDURATION

Üretim analizlerinde gerçekleşen miktar ve süre alanları kritik olabilir.

Özellikle operasyon bazlı raporlarda:

- Gerçekleşen miktar (`ACTAMOUNT` benzeri alanlar)
- Gerçekleşen süre (`ACTDURATION` benzeri alanlar)

planlanan değerlerden ayrı değerlendirilmelidir.

Bu bilgiler:

- OEE analizleri,
- Operasyon performansı,
- Kapasite analizi,
- İş istasyonu verimliliği,
- Üretim maliyeti

gibi hesaplamalarda kullanılabilir.

Alanların hangi tabloda ve hangi anlamda tutulduğu kullanılan Logo sürümüne göre doğrulanmalıdır.

---

## 9. Sarf Hareketleri

Üretimde mamul girişi kadar hammadde sarfı da önemlidir.

Örnek:

```text
Mamul: 1000 adet

Sarf:
- Hammadde A: 500 kg
- Hammadde B: 125 kg
- Ambalaj: 1000 adet
```

Sarf hareketi yalnızca stoktan miktar düşürmek değildir.

Doğru üretim bağlantısıyla oluşması gereken ilişkiler şunları etkileyebilir:

- Üretim emri gerçekleşmesi
- Maliyetlendirme
- Seri/lot izlenebilirliği
- Reçete sapma analizi
- Fire analizi

---

## 10. Seri/Lot Takipli Üretim

İlaç, gıda, kimya ve benzeri sektörlerde üretim entegrasyonunun en kritik parçalarından biri seri/lot izlenebilirliğidir.

Örnek zincir:

```text
Hammadde Lot A
Hammadde Lot B
      │
      ▼
Üretim Emri
      │
      ▼
Mamul Lot X
```

Sistem şu soruya cevap verebilmelidir:

> Mamul Lot X üretilirken hangi hammadde lotları kullanıldı?

Ters izlenebilirlik de gerekir:

> Hammadde Lot A hangi mamul lotlarında kullanıldı?

Bu nedenle sarf ve üretim hareketlerinin lot bağlantıları eksiksiz oluşturulmalıdır.

---

## 11. Stok Yeri / Ambar Boyutu

Detaylı üretimde yalnızca malzeme ve lot bilgisi yeterli olmayabilir.

Sarfın geldiği ve üretimin girdiği yerler de önemlidir:

- Ana ambar
- Üretim ambarı
- Hat yanı stok
- Karantina alanı
- Kalite kontrol alanı
- Mamul ambarı

Bu alanların Logo'daki ambar ve stok yeri yapısıyla doğru eşleştirilmesi gerekir.

---

## 12. Kalite Süreçleri

Üretim tamamlandıktan sonra malzemenin doğrudan serbest stoka geçmesi her sektörde doğru değildir.

Örneğin ilaç üretiminde:

```text
Üretim
  ↓
Karantina
  ↓
Kalite Kontrol
  ↓
Onay
  ↓
Serbest Stok
```

benzeri süreçler bulunabilir.

Ara yazılım Logo stok hareketini oluştururken kalite sürecini atlamamalıdır.

---

## 13. Üretim ve Maliyetlendirme

Üretim entegrasyonundaki en büyük mimari hatalardan biri yalnızca miktarsal stok sonucuna odaklanmaktır.

Örneğin:

```text
500 kg hammadde çıktı
1000 adet mamul girdi
```

stok açısından doğru görünebilir.

Ancak üretim emri ve maliyet bağlantıları doğru değilse:

- Mamul maliyeti eksik hesaplanabilir.
- Hammadde maliyetleri doğru mamule taşınmayabilir.
- İşçilik/operasyon maliyetleri kaybolabilir.
- Dönem sonu maliyet hesapları beklenmeyen sonuç verebilir.

Bu yüzden üretim entegrasyonunun kabul testi yalnızca stok miktarı üzerinden yapılmamalıdır.

---

## 14. Entegrasyon Akışı

Önerilen genel akış:

```text
MES / Ara Yazılım
        │
        ▼
Üretim emrini doğrula
        │
        ▼
Mamul / reçete / operasyon eşleşmesini doğrula
        │
        ▼
Sarf miktarlarını al
        │
        ▼
Seri/Lot bilgilerini doğrula
        │
        ▼
Gerçekleşen üretim miktarını al
        │
        ▼
ProductionApplication / Logo Objects
        │
        ▼
Logo üretim hareketlerini oluştur
        │
        ▼
Sonuç LOGICALREF'lerini kaydet
        │
        ▼
Mutabakat kontrolü
```

---

## 15. İdempotency

MES entegrasyonlarında aynı üretim bildiriminin iki kez gönderilmesi mümkündür.

Örneğin ağ kesilir:

1. MES isteği gönderir.
2. Logo kaydı oluşur.
3. MES cevabı alamaz.
4. Aynı isteği tekrar yollar.

Eğer entegrasyon idempotent değilse aynı üretim iki kez kaydedilebilir.

Bunu önlemek için dış sistem hareketlerinin benzersiz bir entegrasyon anahtarı olmalıdır:

```text
ExternalTransactionId = PROD-2026-000125-OP10-0007
```

Kayıttan önce bu ID kontrol edilmelidir.

---

## 16. Transaction Yönetimi

Üretim bildirimi birden fazla hareket oluşturabilir.

Örneğin:

- 4 hammadde sarfı
- 1 mamul girişi
- 2 yan ürün
- 3 lot dağıtımı

Bu işlemlerin yarısının oluşup yarısının hata vermesi kabul edilebilir değildir.

Mimari tasarımda işlem atomikliği düşünülmelidir.

Logo Objects'in kendi işlem yapısı kullanılmalı; harici sistem tarafında da başarısız senaryolar için durum yönetimi yapılmalıdır.

---

## 17. Entegrasyon Durumları

Her üretim bildirimi için durum tutulması önerilir.

Örnek:

```text
NEW
VALIDATED
PROCESSING
POSTED
FAILED
RETRY
CANCELLED
```

Ek olarak:

```text
ExternalId
LogoRef
RetryCount
LastError
CreatedDate
ProcessedDate
```

alanları saklanabilir.

Bu yapı üretim entegrasyonunun operasyonel olarak yönetilebilir olmasını sağlar.

---

## 18. Retry Stratejisi

Her hata otomatik tekrar denenmemelidir.

### Geçici hata

- Logo servisi erişilemiyor
- Ağ sorunu
- SQL bağlantı sorunu

Retry uygulanabilir.

### İş kuralı hatası

- Malzeme kodu bulunamadı
- Lot bulunamadı
- Yetersiz stok
- Üretim emri kapalı

Otomatik retry çoğu zaman anlamsızdır. İnsan müdahalesi gerekir.

---

## 19. Doğrudan SQL ile Üretim Hareketi Oluşturmanın Riski

Aşağıdaki yaklaşım yüksek risklidir:

```text
INSERT LG_xxx_yy_STFICHE
INSERT LG_xxx_yy_STLINE
```

Çünkü üretim işlemleri yalnızca bu tablolardan ibaret değildir.

Bağlı tablolar arasında şunlar bulunabilir:

- Üretim emirleri
- İş emirleri
- Seri/lot tabloları
- Stok yeri dağıtımları
- Maliyet ilişkileri
- Muhasebe bağlantıları

Eksik ilişki ilk anda görünmeyebilir; sorun maliyetlendirme veya izlenebilirlik aşamasında ortaya çıkabilir.

---

## 20. SQL Nerede Kullanılmalı?

SQL üretim entegrasyonunda çok değerlidir, ancak rolü doğru olmalıdır.

### Uygun kullanım

- Raporlama
- Mutabakat
- Kontrol sorguları
- Performans analizi
- Üretim durum dashboard'ları
- Hatalı kayıt araştırması

### Riskli kullanım

- Üretim hareketi INSERT
- Sarf satırı UPDATE
- Lot bağlantısı DELETE
- Üretim emri ilişkilerini elle değiştirme

---

## 21. Mutabakat Kontrolleri

Üretim entegrasyonunda periyodik mutabakat yapılmalıdır.

Örnek kontroller:

### Üretim miktarı

```text
MES gerçekleşen üretim
=
Logo gerçekleşen üretim
```

### Sarf miktarı

```text
MES sarf toplamı
=
Logo üretim emri bağlı sarf toplamı
```

### Lot

```text
MES lot listesi
=
Logo lot dağıtımları
```

### Kayıt sayısı

```text
MES gönderilen işlem sayısı
=
Logo başarılı işlem sayısı + hata kuyruğu
```

---

## 22. Uygulama Katmanı Örneği

Önerilen servis ayrımı:

```text
ProductionIntegrationService
 ├── ProductionOrderService
 ├── MaterialValidationService
 ├── LotValidationService
 ├── ConsumptionService
 ├── ProductionPostingService
 ├── LogoObjectsGateway
 ├── ReconciliationService
 └── IntegrationLogService
```

Logo Objects çağrılarını doğrudan UI koduna veya controller içine yaymak yerine tek bir gateway/service katmanında toplamak bakım kolaylığı sağlar.

---

## 23. Logging

Her işlemde minimum aşağıdaki bilgiler loglanmalıdır:

- External transaction ID
- Logo firma no
- Dönem no
- Üretim emri ref/no
- Mamul kodu
- Miktar
- Lot
- İşlem tipi
- Başlangıç/bitiş zamanı
- Logo sonucunda oluşan ref
- Hata kodu
- Hata açıklaması

Üretim sistemlerinde yalnızca teknik exception logu yeterli değildir; iş bağlamı da tutulmalıdır.

---

## 24. Gerçek Proje Yaklaşımı

Detaylı üretim kullanılan bir ilaç üretim ortamında ideal yaklaşım şudur:

- Operasyonel üretim kolaylığı için ara yazılım kullanılabilir.
- Operatör Logo ekranlarının karmaşıklığına maruz bırakılmayabilir.
- MES barkod, tartım, operasyon ve kalite verisini toplar.
- Logo resmi ERP kayıtlarının sahibi olmaya devam eder.
- Üretim, sarf, seri/lot ve maliyet ilişkileri Logo tarafına eksiksiz aktarılır.
- SQL ile yalnızca kontrol ve raporlama yapılır.

Bu model hem kullanıcı deneyimini iyileştirir hem de ERP veri bütünlüğünü korur.

---

## 25. Best Practices

### Yapılması önerilenler

- Üretim emrini entegrasyonun merkezine koy.
- MES ile ERP veri sahipliğini baştan tanımla.
- Her harici harekete benzersiz ID ver.
- Seri/lot izlenebilirliğini uçtan uca koru.
- Sarf ve üretim hareketlerini Logo iş kurallarıyla oluştur.
- Maliyetlendirme sonucunu entegrasyon kabul testine dahil et.
- Periyodik mutabakat raporu üret.
- Teknik ve iş hatalarını ayrı sınıflandır.

### Kaçınılması gerekenler

- Üretimi yalnızca stok giriş/çıkışı olarak görmek.
- `STLINE` tablolarına doğrudan kayıt atmak.
- Planlanan miktarı gerçekleşen miktar kabul etmek.
- Lot ilişkilerini dış sistemde tutup Logo'ya eksik aktarmak.
- Aynı entegrasyon mesajını birden fazla kez işlemek.
- Maliyetlendirme kontrolü yapmadan projeyi tamamlandı kabul etmek.

---

## 26. Sonuç

`ProductionApplication` ve detaylı üretim entegrasyonu, Logo Objects kullanımının en kritik ve karmaşık alanlarından biridir.

Doğru mimari:

```text
MES / Ara Yazılım
       │
       ▼
Validasyon ve İdempotency
       │
       ▼
ProductionApplication / Logo Objects
       │
       ▼
Üretim + Sarf + Lot + Operasyon bağlantıları
       │
       ▼
Logo Stok ve Maliyet Sistemi
       │
       ▼
Mutabakat ve Raporlama
```

şeklinde tasarlanmalıdır.

Temel hedef, Logo veritabanına kayıt düşürmek değil; **Logo içinde eksiksiz ve maliyetlendirilebilir bir üretim süreci oluşturmaktır.**
