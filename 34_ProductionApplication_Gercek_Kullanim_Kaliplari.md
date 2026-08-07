# 34 — ProductionApplication Gerçek Kullanım Kalıpları

## 1. Amaç

Bu bölüm, Logo detaylı üretim kullanan projelerde `ProductionApplication` nesnesinin nasıl konumlandırılması gerektiğini anlatır. Amaç yalnızca metod çağrısı göstermek değil; üretim emri, iş emri, operasyon, sarf, üretim, seri/lot ve maliyet zincirini bozmadan entegrasyon tasarlamaktır.

## 2. ProductionApplication Ne Zaman Kullanılır?

`ProductionApplication`, klasik kart ve fiş işlemlerinden farklı olarak üretim süreçlerine özgü iş kurallarını çalıştırmak için değerlendirilir.

Özellikle:

- Üretim emri oluşturma
- Üretim emri yönetimi
- İş emri / operasyon akışları
- Gerçekleşen üretim miktarları
- Sarf hareketleri
- Fire
- Operasyon süreleri
- Üretim bağlantıları

senaryolarında kullanılır.

## 3. Mimari Konum

Önerilen yapı:

```text
MES / Ara Yazılım
       ↓
Validation Layer
       ↓
ProductionApplication
       ↓
Logo Üretim Kuralları
       ↓
Stok + Seri/Lot + Maliyet + Muhasebe
```

Ara yazılımın doğrudan üretim tablolarına kayıt atması önerilmez.

## 4. Üretim Emri Entegrasyon Akışı

Bir üretim emri dış sistemden Logo'ya gönderilirken önerilen sıra:

```text
Ürün kartı var mı?
↓
Reçete / ürün ağacı geçerli mi?
↓
Rota / operasyon tanımı geçerli mi?
↓
Ambarlar geçerli mi?
↓
Üretim miktarı ve tarih kontrolü
↓
ProductionApplication çağrısı
↓
Üretim emri referansını al
↓
Dış sistem eşlemesini kaydet
```

## 5. Dış Sistem Eşleme Tablosu

Üretim entegrasyonunda mutlaka mapping tutulmalıdır.

Örnek tablo:

```sql
CREATE TABLE Z_PROD_INTEGRATION_MAP
(
    ID                BIGINT IDENTITY PRIMARY KEY,
    SOURCE_SYSTEM     VARCHAR(50) NOT NULL,
    EXTERNAL_ORDER_ID VARCHAR(100) NOT NULL,
    LOGO_PRODORD_REF  INT NULL,
    STATUS            VARCHAR(30) NOT NULL,
    CREATED_AT        DATETIME NOT NULL DEFAULT GETDATE(),
    UPDATED_AT        DATETIME NULL,
    ERROR_MESSAGE     NVARCHAR(MAX) NULL
);
```

Unique index:

```sql
CREATE UNIQUE INDEX UX_Z_PROD_INTEGRATION_MAP
ON Z_PROD_INTEGRATION_MAP(SOURCE_SYSTEM, EXTERNAL_ORDER_ID);
```

Bu yapı duplicate üretim emri oluşmasını engeller.

## 6. Üretim Emri Sonrası Kontrol

Üretim emri oluşturulduktan sonra en az şu bilgiler doğrulanmalıdır:

```text
Üretim emri ref
Ürün
Planlanan miktar
Planlanan tarih
Durum
Bağlı iş emirleri
Operasyonlar
```

SQL read-only kontrolü yapılabilir.

## 7. Gerçekleşen Üretim Miktarı

Sahada önemli iki alan:

```text
Planlanan miktar
Gerçekleşen miktar
```

Üretim raporlarında yalnızca üretim emri başlığı yeterli olmayabilir. Operasyon veya gerçekleşme kayıtları ayrıca incelenmelidir.

Özellikle `ACTAMOUNT` ve `ACTDURATION` benzeri gerçekleşen değerler kullanılan tablo/nesne yapısında doğrulanmalıdır.

## 8. Operasyon Süresi

Operasyon performansı için şu model kullanılabilir:

```text
Planlanan süre
Gerçekleşen süre
Üretim miktarı
Duruş süresi
Fire miktarı
Operatör / iş merkezi
```

Verimlilik:

```text
Gerçekleşen miktar / gerçekleşen süre
```

şeklinde analiz edilebilir; ancak operasyon birimi ve süre birimi doğrulanmalıdır.

## 9. Sarf Hareketleri

Üretim emri için sarf hareketi oluşturulurken:

- Hammadde
- Miktar
- Birim
- Ambar
- Lot
- Üretim emri bağlantısı
- Operasyon bağlantısı

bilgileri birlikte değerlendirilmelidir.

Sarf hareketinin sadece `STLINE` olarak oluşturulması üretim bağlantısını eksik bırakabilir.

## 10. Mamul Girişi

Mamul üretiminde:

```text
Mamul
Miktar
Ana/alternatif birim
Ambar
Lot
Üretim tarihi
SKT
Üretim emri
```

bilgileri birlikte oluşturulmalıdır.

## 11. Hammadde Lot → Mamul Lot Traceability

En iyi uygulama:

```text
Hammadde Lot A
Hammadde Lot B
       ↓
Üretim Emri 4427
       ↓
Mamul Lot X
```

Bu ilişki dış sistemde de saklanabilir; fakat Logo tarafındaki resmi seri/lot zinciri eksiksiz olmalıdır.

## 12. Fire

Fire, sadece stok farkı değildir. Üretim emri ve operasyon bağlamında neden/kaynak bilgisi önemlidir.

Örnek fire kayıt modelinde:

```text
Üretim emri
Operasyon
Malzeme
Lot
Miktar
Fire nedeni
Tarih
Kullanıcı
```

alanları tutulabilir.

## 13. Kalite Entegrasyonu

Kalite onayı bekleyen mamul için ara yazılım:

```text
Üretildi
↓
Karantina / kalite statüsü
↓
LIMS sonucu
↓
Onay / Red
↓
Kullanılabilir stok / red süreci
```

akışını yönetebilir.

Ancak resmi stok hareketinin Logo'da doğru statü ve ambar yapısıyla tutulması gerekir.

## 14. İşlem Tekrarı

MES aynı üretim bildirimi tekrar gönderirse sistem ikinci kez sarf veya mamul girişi oluşturmamalıdır.

Her gerçekleşme için benzersiz dış anahtar tutulmalıdır:

```text
MES_TRANSACTION_ID
```

veya:

```text
ProductionOrder + Operation + Sequence
```

## 15. Hata Sonrası Durum

Bir üretim bildirimi şu şekilde parçalı kalmamalıdır:

```text
Sarf oluştu
Mamul girişi oluşmadı
Entegrasyon başarılı işaretlendi
```

Doğru yaklaşım:

```text
Pending
Processing
Succeeded
Failed
NeedsReview
```

status modelidir.

## 16. Reconciliation

Günlük veya periyodik olarak dış sistem ile Logo karşılaştırılmalıdır.

Kontrol örnekleri:

- Dış sistem üretim emri sayısı vs Logo
- Bildirilen üretim miktarı vs Logo gerçekleşen miktar
- Sarf toplamı
- Fire toplamı
- Mamul lotları
- Hammadde lotları
- Eksik/duplicate hareketler

## 17. Performans

ProductionApplication çağrıları yüksek hacimde yapılacaksa:

- Her satırda yeniden login olma
- Gereksiz COM nesnesi oluşturma
- Uzun transaction içinde yüzlerce belge tutma
- Aynı referansı tekrar tekrar sorgulama

kaçınılmalıdır.

Cache edilebilecek bilgiler:

```text
Malzeme ref
Cari ref
Ambar ref
Birim ref
İşyeri ref
Operasyon ref
```

## 18. Güvenli Entegrasyon Kalıbı

```text
Receive payload
↓
Validate
↓
Resolve references
↓
Check idempotency
↓
Call ProductionApplication
↓
Verify result
↓
Write integration map
↓
Reconcile asynchronously / scheduled
```

## 19. Best Practice

- Üretim tablolarına doğrudan insert yapma.
- Dış sistem ID eşlemesi tut.
- Üretim emri ve gerçekleşme işlemlerini ayrı idempotency anahtarlarıyla yönet.
- Seri/lot zincirini eksiksiz kur.
- Sarf ve mamul girişini üretim bağlamından koparma.
- Hata sonrası parçalı kayıtları tespit edecek reconciliation süreci kur.
- ProductionApplication sürüm davranışlarını test ortamında doğrula.

## 20. Özet

ProductionApplication, detaylı üretim entegrasyonunun teknik API katmanı olarak ele alınmalıdır. Başarılı entegrasyon yalnızca üretim emri oluşturmak değildir; sarf, mamul, operasyon, seri/lot, kalite, maliyet ve hata yönetimi zincirinin tamamı korunmalıdır.
