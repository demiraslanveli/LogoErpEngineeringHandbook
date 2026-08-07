# 09 — Satınalma ve Satış

## 1. Bölümün Amacı

Bu bölüm, Logo ERP’de satınalma ve satış süreçlerinin entegrasyon açısından nasıl ele alınması gerektiğini açıklar.

Odak noktaları:

- Sipariş → irsaliye → fatura akışı
- Belge türleri ve `TRCODE`
- Cari, stok ve muhasebe etkileri
- Satır bazlı veri yapısı
- Fatura/irsaliye bağlantıları
- Logo Objects ile güvenli kayıt üretimi

Temel prensip:

> Satınalma ve satış belgeleri yalnızca finansal belge değildir; stok, cari, muhasebe, proje, maliyet ve bazen seri/lot ilişkilerini aynı anda etkileyen çok katmanlı işlemlerdir.

---

## 2. Temel Belge Zinciri

Tipik süreç:

```text
Sipariş
  ↓
İrsaliye
  ↓
Fatura
  ↓
Cari Hareket
  ↓
Muhasebe Fişi
```

Her firmada tüm adımlar kullanılmayabilir.

Örneğin doğrudan fatura kesilebilir veya irsaliye faturaya dönüştürülebilir.

Ancak belge bağlantıları kullanılıyorsa entegrasyon bu zinciri korumalıdır.

---

## 3. Satınalma Süreci

Satınalma tarafında tipik akış:

```text
Satınalma Talebi
      ↓
Satınalma Siparişi
      ↓
Mal Kabul / İrsaliye
      ↓
Satınalma Faturası
      ↓
Cari Borç
      ↓
Muhasebe
```

Stoklu alımlarda irsaliye veya fatura stok miktarını artırır.

Aynı işlem aynı zamanda tedarikçi cari bakiyesini ve muhasebe kayıtlarını etkileyebilir.

---

## 4. Satış Süreci

Satış tarafında tipik akış:

```text
Satış Siparişi
      ↓
Sevkiyat / İrsaliye
      ↓
Satış Faturası
      ↓
Cari Alacak
      ↓
Muhasebe
```

Stoklu satışta stok miktarı azalırken cari hesap ve finansal sonuçlar oluşur.

---

## 5. TRCODE Kavramı

Logo tablolarında aynı fiziksel tablo içinde farklı belge türleri `TRCODE` ile ayrılabilir.

Örneğin `INVOICE`, `STFICHE`, `STLINE` gibi yapılarda `TRCODE` işlem türünü belirleyen kritik alanlardan biridir.

Bu nedenle bir sorguda yalnızca firma/dönem filtresi kullanmak çoğu zaman yeterli değildir.

Örnek:

```sql
SELECT *
FROM LG_102_01_INVOICE
WHERE TRCODE = @TrCode;
```

`TRCODE` değerleri kullanılan Logo modülü ve belge türüne göre resmi tanımlardan doğrulanmalıdır.

---

## 6. Firma ve Dönem Yapısı

Logo’da hareket tabloları genellikle firma ve dönem numarası içerir.

Örnek:

```text
LG_102_01_INVOICE
LG_102_01_STFICHE
LG_102_01_STLINE
```

Burada:

```text
102 = Firma
01  = Dönem
```

olarak yorumlanır.

Kart tabloları ise dönemden bağımsız olabilir.

---

## 7. Fatura Başlık ve Satır Yapısı

Fatura işlemi kavramsal olarak iki ana katmana ayrılır:

```text
INVOICE
  │
  └── STLINE
```

Başlıkta:

- Fatura no
- Tarih
- Cari
- Belge no
- Proje
- Açıklamalar
- Genel toplamlar

satırlarda ise:

- Malzeme/hizmet
- Miktar
- Birim
- Fiyat
- KDV
- İndirim
- Ambar
- Proje
- Seri/lot ilişkileri

bulunur.

---

## 8. İrsaliye Başlık ve Satır Yapısı

İrsaliye tarafında tipik yapı:

```text
STFICHE
  │
  └── STLINE
```

Aynı `STLINE` tablosu farklı stok hareketlerinin detaylarını taşıyabilir.

Bu nedenle satırın hangi belgeye ait olduğu yalnızca `STOCKREF` veya `TRCODE` üzerinden anlaşılmamalıdır; başlık bağlantıları da kontrol edilmelidir.

---

## 9. Fatura–İrsaliye Bağlantısı

Bir fatura irsaliyeden oluşturulduğunda sistemde başlık ve satır seviyesinde bağlantılar bulunabilir.

Örneğin:

- `INVOICEREF`
- `STFICHEREF`
- `ORDTRANSREF`
- `ORDFICHEREF`

benzeri alanlar belge zincirinin izlenmesini sağlar.

Bu ilişkilerin doğrudan SQL ile manuel güncellenmesi yüksek risklidir.

---

## 10. Sipariş Bağlantısı

Siparişten karşılanan bir irsaliye veya fatura satırı sipariş satırına bağlı olabilir.

Bu bağlantı sayesinde:

- Siparişin karşılanan miktarı
- Kalan miktarı
- Kapama durumu
- Sevkiyat durumu

hesaplanabilir.

Doğrudan stok satırı insert edilip sipariş ilişkisi kurulmazsa Logo ekranında sipariş açık görünmeye devam edebilir.

---

## 11. Cari Hareket

Fatura, yalnızca stok tablosuna kayıt oluşturmaz.

Cari tarafta tipik olarak `CLFLINE` benzeri hareket tabloları etkilenir.

Bir satış faturası müşteriye alacak, satınalma faturası ise tedarikçiye borç yaratabilir.

Dolayısıyla şu kontrol önemlidir:

```text
Fatura var mı?
↓
Cari hareket oluşmuş mu?
↓
Tutarlar uyumlu mu?
```

---

## 12. Muhasebe Bağlantısı

Muhasebeleştirilmiş belgelerde:

```text
EMFICHE
EMFLINE
```

benzeri yapılarda kayıt oluşabilir.

Bir fatura tarihini değiştirmek gibi işlemler yalnızca fatura tablosunu değil, bağlı stok, cari ve muhasebe kayıtlarını da etkileyebilir.

Bu nedenle kritik belge güncellemelerinde tüm bağlantı zinciri dikkate alınmalıdır.

---

## 13. Fatura Tarihi Değişikliği Örneği

Gerçek projelerde sık karşılaşılan ihtiyaçlardan biri, özellikle ay geçişlerinde yanlış tarihli faturaların düzeltilmesidir.

Güvenli yaklaşımda aşağıdaki yapılar birlikte değerlendirilir:

- `INVOICE`
- `STFICHE`
- `STLINE`
- `CLFLINE`
- `EMFICHE`
- `EMFLINE`

Tek bir tabloyu güncellemek belge bütünlüğünü bozabilir.

Bu tür operasyonlar mümkünse Logo uygulama katmanından yapılmalı; zorunlu SQL operasyonlarında ise test modu, transaction ve kapsamlı kontrol kullanılmalıdır.

---

## 14. Test Modu Tasarımı

Bakım amaçlı SQL prosedürlerinde güvenli bir desen:

```text
@TestModu = 1
→ Güncelleme yapma
→ Etkilenecek kayıtları göster

@TestModu = 0
→ Transaction içinde güncelle
```

Bu yaklaşım özellikle toplu belge düzeltmelerinde insan hatasını azaltır.

---

## 15. Eksik Bağlantıda Davranış

Her faturanın muhasebe fişi olmak zorunda değildir.

Bu nedenle bakım prosedürlerinde:

```text
Muhasebe bağlantısı yok → tüm işlemi durdur
```

yaklaşımı her zaman doğru değildir.

Daha iyi yaklaşım:

- Fatura mevcutsa devam et.
- Var olan bağlı kayıtları güncelle.
- Olmayan bağlantıları raporla.
- Diğer faturaların işlenmesini durdurma.

---

## 16. Logo Objects ile Fatura Oluşturma

Kavramsal örnek:

```csharp
IData invoice = application.NewDataObject(DataObjectType.doSalesInvoice);

invoice.New();
invoice.DataFields.FieldByName("DATE").Value = DateTime.Today;
invoice.DataFields.FieldByName("ARP_CODE").Value = "120.01.001";
invoice.DataFields.FieldByName("SOURCE_WH").Value = 0;

ILines lines = invoice.DataFields.FieldByName("TRANSACTIONS").Lines;

lines.AppendLine();
var line = lines[lines.Count - 1];

line.FieldByName("TYPE").Value = 0;
line.FieldByName("MASTER_CODE").Value = "150.001";
line.FieldByName("QUANTITY").Value = 10;
line.FieldByName("PRICE").Value = 25.50;

if (!invoice.Post())
{
    // Hata loglanmalıdır.
}
```

Gerçek alanlar ve nesne tipleri sürüme göre doğrulanmalıdır.

---

## 17. KDV Alanları

Fatura satırlarında KDV oranı, muafiyet kodu ve muafiyet açıklaması gibi alanlar belge türüne göre kritik olabilir.

Özellikle KDV oranı `0` olan satırlarda muafiyet sebebinin boş bırakılması mevzuat ve e-belge süreçlerinde sorun yaratabilir.

Bu nedenle uygulama seviyesinde kontrol yapılabilir:

```text
KDV = 0
AND Muafiyet Sebebi boş
→ Kullanıcıyı uyar
→ Gerekirse belirlenen açıklamayı satıra aktar
```

---

## 18. Muafiyet Kod–Açıklama Standardı

Örnek standartlar:

```text
231 → 17/4-g Metal, Plastik, Lastik, Kauçuk, Kağıt, Cam Hurda ve Atıkların Teslimi
301 → 11/1-a Mal İhracatı
335 → Basılı Kitap ve Süreli Yayınların Teslimleri
351 → KDV - İstisna Olmayan Diğer
```

Bu eşleşmeler hard-code edilecekse merkezi bir yapıdan yönetilmelidir.

Daha iyi yaklaşım:

```text
Code
Description
Active
ValidFrom
ValidTo
```

alanları olan parametrik bir tablo kullanmaktır.

---

## 19. Ambar Bilgisi

Stoklu belgelerde ambar hem başlıkta hem satırda etkili olabilir.

Yanlış ambar seçimi:

- Yanlış stok düşümü
- Yanlış maliyet
- Yanlış üretim bağlantısı
- Yanlış sevkiyat lokasyonu

oluşturabilir.

Bu nedenle entegrasyon ambarı varsayılan değerle körlemesine göndermemelidir.

---

## 20. Proje Bağlantısı

Proje bazlı çalışan firmalarda fatura veya stok satırları `PROJECTREF` benzeri alanlarla projeye bağlanabilir.

Bu bilgi:

- Proje maliyetleri
- Proje kârlılığı
- Sarf kontrolü
- Bütçe takibi

için önemlidir.

Başlık projesi ile satır projesi farklı davranabilir; raporlar buna göre tasarlanmalıdır.

---

## 21. Satınalma Birim Fiyat Kontrolü

Satınalma süreçlerinde son alış fiyatı ile yeni alış fiyatını karşılaştırmak faydalı bir kontrol mekanizmasıdır.

Ancak kıyaslama mutlaka aynı birim ve aynı para birimi üzerinden yapılmalıdır.

Örnek:

```text
Son alış: 0,48 USD / adet
Yeni alış: 0,72 USD / adet
```

anlamlıdır.

Fakat biri koli, diğeri adet ise doğrudan kıyaslama yanlış sonuç verir.

---

## 22. Dövizli Fiyatlar

Satınalma/satış fiyat karşılaştırmalarında:

- `PRICE`
- `TRCURR`
- `TRRATE`
- Belge dövizi
- Raporlama dövizi

birlikte değerlendirilmelidir.

Tek başına `PRICE` alanını kıyaslamak yanıltıcı olabilir.

---

## 23. Fatura İptali

İptal edilen faturalar bazı otomasyonlardan hariç tutulmalıdır.

Örneğin fatura kesildikten 7 gün sonra hatırlatma maili atan bir sistem varsa iptal edilmiş faturaya mail gönderilmemelidir.

Kontrol sırası:

```text
Fatura mevcut mu?
↓
İptal mi?
↓
Gönderim koşulu sağlanıyor mu?
↓
Mail kuyruğuna ekle
```

---

## 24. Mail Queue Deseni

Fatura üzerinden otomatik bildirimlerde doğrudan trigger içinden mail göndermek yerine queue yaklaşımı daha güvenlidir.

Örnek:

```text
Invoice Trigger
     ↓
MailQueue
     ↓
Mail Service / Job
     ↓
Database Mail / SMTP
```

Avantajları:

- Fatura kaydı mail hatasından etkilenmez.
- Retry yapılabilir.
- Hata loglanabilir.
- Gönderim merkezi yönetilir.

---

## 25. Doğrudan SQL ile Fatura Oluşturmanın Riski

Aşağıdaki yöntem önerilmez:

```sql
INSERT INTO LG_xxx_yy_INVOICE ...
INSERT INTO LG_xxx_yy_STLINE ...
```

Çünkü eksik kalabilecek yapılar:

- İrsaliye başlığı
- Cari hareket
- Muhasebe hareketi
- Seri/lot dağıtımı
- Sipariş bağlantısı
- Kampanya/indirim bağlantısı
- Maliyet ilişkileri

Bu nedenle resmi belge üretiminde Objects kullanılmalıdır.

---

## 26. Raporlama İçin SQL

SQL, belge analizi için son derece uygundur.

Örnek:

```sql
SELECT
    I.FICHENO,
    I.DATE_,
    C.CODE AS CARI_KOD,
    C.DEFINITION_ AS CARI_UNVAN,
    I.NETTOTAL
FROM LG_102_01_INVOICE I
LEFT JOIN LG_102_CLCARD C
    ON C.LOGICALREF = I.CLIENTREF
WHERE I.TRCODE = 8;
```

Bu tip sorgular okuma/raporlama amaçlıdır.

---

## 27. Belge Mutabakatı

Entegrasyon sonrası aşağıdaki kontroller yapılabilir:

```text
Fatura başlığı var mı?
Satır sayısı doğru mu?
İrsaliye bağlantısı doğru mu?
Cari hareket oluşmuş mu?
Muhasebe bağlantısı var mı?
Toplamlar tutuyor mu?
Seri/lot dağıtımı tamam mı?
```

Bu kontroller otomatik kalite testlerine dönüştürülebilir.

---

## 28. İdempotent Fatura Entegrasyonu

Aynı e-ticaret siparişi veya dış sistem faturası iki kez Logo’ya gönderilmemelidir.

Harici anahtar örneği:

```text
ExternalInvoiceId = ECOM-INV-2026-84517
```

Entegrasyon tablosu:

```text
ExternalInvoiceId
LogoFirm
LogoInvoiceRef
LogoFicheNo
Status
CreatedAt
```

Yeni kayıt öncesinde external ID kontrol edilir.

---

## 29. Hata Yönetimi

Fatura entegrasyonunda yalnızca exception mesajı tutmak yeterli değildir.

Loglanması gerekenler:

- Firma
- Dönem
- Belge türü / TRCODE
- Harici ID
- Fatura no
- Cari kod
- Satır sayısı
- Toplam tutar
- Objects error code
- Objects error description
- Validation errors

---

## 30. Best Practices

### Yapılması önerilenler

- Belge zincirini sipariş → irsaliye → fatura olarak düşün.
- TRCODE filtresini her zaman bilinçli kullan.
- Cari, stok ve muhasebe etkilerini birlikte kontrol et.
- Fatura/irsaliye/sipariş bağlantılarını Objects ile kur.
- KDV ve muafiyet alanlarını validasyona dahil et.
- Döviz ve birim bazını fiyat kontrollerinde normalize et.
- İptal belgeleri otomasyonlardan filtrele.
- Mail işlemlerinde queue deseni kullan.
- Harici belge ID'siyle idempotency sağla.

### Kaçınılması gerekenler

- Faturayı sadece `INVOICE` kaydı olarak görmek.
- `STLINE` satırlarını doğrudan insert etmek.
- Fatura tarihi değiştirirken yalnızca tek tabloyu update etmek.
- Muhasebe fişi olmayan belgeyi otomatik hata kabul etmek.
- Son alış fiyatını farklı birim/dövizlerle doğrudan kıyaslamak.
- İptal faturaya otomatik bildirim göndermek.

---

## 31. Sonuç

Satınalma ve satış belgeleri Logo ERP’de birçok modülün kesişim noktasıdır.

Doğru yaklaşım:

```text
Sipariş
  ↓
İrsaliye
  ↓
Fatura
  ├── Stok
  ├── Cari
  ├── Muhasebe
  ├── Proje
  └── Seri/Lot
```

şeklinde bütünsel düşünmektir.

Entegrasyon başarısı, yalnızca faturanın ekranda görünmesiyle değil, **belge zincirinin ve tüm finansal/stok bağlantılarının doğru oluşmasıyla** ölçülmelidir.
