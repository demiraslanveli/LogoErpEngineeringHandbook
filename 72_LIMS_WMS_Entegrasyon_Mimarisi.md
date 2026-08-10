# 72 — LIMS ve WMS Entegrasyon Mimarisi

## 1. Amaç

Bu bölüm, laboratuvar bilgi yönetim sistemleri (LIMS) ve depo yönetim sistemlerinin (WMS) Logo Tiger / Logo Objects altyapısı ile nasıl entegre edilmesi gerektiğini açıklar.

Temel prensip:

> Operasyonel sistemler süreç kararlarını yönetebilir; resmi ERP hareketleri ve mali kayıtlar Logo tarafında tutarlı biçimde oluşmalıdır.

## 2. LIMS Entegrasyonu

Tipik LIMS akışı:

```text
Numune / Lot
    ↓
Laboratuvar Testi
    ↓
Sonuç
    ↓
Onay / Red
    ↓
Logo kalite / lot statüsü
```

LIMS tarafında tutulabilecek bilgiler:

- Numune numarası
- Lot / seri numarası
- Test parametresi
- Alt / üst limit
- Ölçüm sonucu
- Uygun / uygunsuz sonucu
- Analiz tarihi
- Analizi yapan kullanıcı
- Sertifika / rapor referansı

Logo tarafında karşılığı bulunması gereken ana bağlantı genellikle lot/seri veya üretim partisidir.

## 3. WMS Entegrasyonu

WMS entegrasyonunda temel domainler:

- Mal kabul
- Depo adresleme
- İç transfer
- Toplama
- Sevkiyat
- Sayım
- Seri / lot takibi

ERP ile WMS arasındaki sınır açık olmalıdır.

Örnek:

```text
Logo satış siparişi
    ↓
WMS toplama emri
    ↓
WMS fiziksel toplama
    ↓
Onay
    ↓
Logo sevkiyat / irsaliye hareketi
```

## 4. Master Data Senkronizasyonu

Logo → LIMS/WMS yönünde tipik master data:

- Malzeme kartı
- Birim setleri
- Barkodlar
- Ambarlar
- Cari hesaplar
- Projeler
- Seri / lot özellikleri

Her kayıt için dış sistemde Logo `LOGICALREF` veya sabit iş anahtarı tutulması faydalıdır.

## 5. Transaction Kimliği

Entegrasyon tablosunda her işlem için benzersiz dış sistem kimliği saklanmalıdır.

Örnek alanlar:

```text
SOURCE_SYSTEM
EXTERNAL_ID
FIRM_NR
PERIOD_NR
DOCUMENT_TYPE
LOGO_LOGICALREF
STATUS
CREATED_AT
PROCESSED_AT
ERROR_MESSAGE
```

Bu yapı idempotency ve reconciliation sağlar.

## 6. Seri / Lot

Seri-lot hareketlerinde yalnızca stok fişi satırı yeterli değildir.

Kontrol edilmesi gerekenler:

- Malzeme
- Lot / seri kodu
- Miktar
- Birim
- Ambar
- Kaynak hareket
- Hedef hareket
- Son kullanma tarihi
- Üretim tarihi

Seri/lot hareketi ile `STLINE` hareketi toplamlarının tutarlı olması gerekir.

## 7. WMS Transfer Senaryosu

Örnek fiziksel transfer:

```text
Ambar 101
    ↓
WMS toplama
    ↓
Ambar 201
```

Logo kaydında kaynak ve hedef ambarın işlem türüne göre `SOURCEINDEX` / `DESTINDEX` ile doğru eşlenmesi gerekir.

## 8. Hata Yönetimi

LIMS/WMS entegrasyon hataları üç sınıfa ayrılmalıdır:

### Veri Hatası

Örnek:

- Malzeme bulunamadı
- Ambar bulunamadı
- Lot mevcut değil
- Birim eşleşmedi

### İş Kuralı Hatası

Örnek:

- Yetersiz stok
- Lot blokeli
- Kalite onayı yok

### Teknik Hata

Örnek:

- SQL bağlantısı kesildi
- Logo Objects login başarısız
- Servis timeout

## 9. Reconciliation

Günlük reconciliation kontrolü önerilir.

Karşılaştırılabilecek metrikler:

- WMS stok toplamı ↔ Logo stok toplamı
- WMS lot toplamı ↔ Logo lot toplamı
- Sevk edilen miktar ↔ Logo irsaliye miktarı
- LIMS onaylı lot ↔ Logo kullanılabilir lot

## 10. Mimari Öneri

Önerilen yapı:

```text
LIMS / WMS
    ↓
Integration API
    ↓
Queue / Outbox
    ↓
Logo Integration Worker
    ↓
Logo Objects / ProductionApplication
    ↓
Logo ERP
```

Bu yapı doğrudan dış sistemin Logo veritabanına yazmasından daha güvenlidir.

## 11. Sonuç

LIMS ve WMS entegrasyonunda hedef yalnızca veri taşımak değildir.

Doğru entegrasyon:

- Master data kimliklerini korur,
- Seri/lot izlenebilirliğini bozmaz,
- ERP stok hareketlerini doğru üretir,
- Hataları tekrar işlenebilir biçimde saklar,
- Sistemler arası reconciliation yapılmasını mümkün kılar.
