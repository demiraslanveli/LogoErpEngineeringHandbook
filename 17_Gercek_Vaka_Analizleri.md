# 17 — Gerçek Proje ve Vaka Analizleri

## 1. Bölümün Amacı

Bu bölüm, Logo ERP projelerinde sahada karşılaşılan gerçek problem tiplerini çözüm kalıpları üzerinden ele alır. Amaç tek bir müşteriye özgü çözüm vermek değil; benzer problemlerde tekrar kullanılabilecek analiz yöntemlerini dokümante etmektir.

Her vaka şu çerçevede incelenir:

```text
Belirti
↓
Muhtemel Kök Nedenler
↓
Kontrol Adımları
↓
Kalıcı Çözüm
↓
Önleyici Kontrol
```

---

# Vaka 1 — Satınalma İrsaliyesinde Yanlış Ambar

## Belirti

Satınalma irsaliyesinin başlık ambarı doğru görünürken stok satırlarından biri farklı ambara kaydedilmiş olabilir.

Örneğin:

```text
Fiş Başlık Ambarı : 4
Satır Ambarı      : 801
```

Bu durum envanter raporlarında beklenmeyen stok dağılımına neden olabilir.

## Analiz

Kontrol edilmesi gereken ilişki:

```text
STFICHE.LOGICALREF
        ↓
STLINE.STFICHEREF
```

ve ambar alanları:

```text
STFICHE.SOURCEINDEX
STLINE.SOURCEINDEX
```

Satır başka bir sipariş/hareketten türemişse:

```text
SOURCELINK
PREVLINEREF
ORDTRANSREF
```

alanları da incelenmelidir.

## Kalıcı Yaklaşım

- yanlış ambarın hangi event/trigger/script tarafından değiştirildiğini logla,
- `INSERT` ve `UPDATE` sırasında eski/yeni ambar değerini kaydet,
- kullanıcı, host, program ve session bilgilerini logla,
- yalnızca sonucu düzeltmek yerine değişikliği yapan süreci tespit et.

### Önerilen log alanları

```text
LOGICALREF
STFICHEREF
TRCODE
ESKI_SOURCEINDEX
YENI_SOURCEINDEX
SOURCELINK
PREVLINEREF
ORDTRANSREF
LOGIN_ADI
HOST_ADI
PROGRAM_ADI
SESSION_ID
ISLEM_TIPI
```

---

# Vaka 2 — Fatura Tarihini Değiştirince Bağlı Kayıtlar Eski Tarihte Kalıyor

## Belirti

Fatura tarihi güncellenmiş fakat bağlı stok, cari veya muhasebe kayıtlarının tarihi eski kalmıştır.

## Kök Neden

Fatura tek bir tablo değildir.

```text
INVOICE
   ├── STFICHE
   ├── STLINE
   ├── CLFLINE
   ├── EMFICHE
   └── EMFLINE
```

Yalnızca `INVOICE.DATE_` güncellemek belge zincirini bozabilir.

## Kalıcı Yaklaşım

Toplu düzeltmeler için kontrollü procedure kullanılabilir.

Önerilen parametreler:

```text
@FirmaNo
@YeniTarih
@Faturalar
@TrCode
@TestModu
```

`@TestModu = 1` olduğunda update yapılmadan önce etkilenecek kayıtlar gösterilmelidir.

## Önemli Prensip

Listede bulunamayan tek bir fatura diğer geçerli faturaların işlenmesini durdurmamalı; hata sonuç setinde ayrıca raporlanmalıdır.

---

# Vaka 3 — KDV Oranı 0 fakat Muafiyet Sebebi Boş

## Belirti

Fatura satırında KDV oranı `0`, ancak KDV muafiyet kodu veya açıklaması boş bırakılmıştır.

## Risk

- e-fatura/e-arşiv uyumsuzluğu,
- kullanıcı hatası,
- manuel düzeltme ihtiyacı,
- yanlış istisna bilgisi.

## Çözüm Kalıbı

Belge kaydedilmeden önce:

```text
VAT = 0
AND malzeme satırı
AND muafiyet sebebi boş
```

kontrol edilir.

Form üzerindeki açıklama alanı doluysa kullanıcı onayıyla boş satırlara uygulanabilir.

Kod-açıklama eşleşmeleri merkezi bir yapıdan yönetilmelidir.

Örnek yaklaşım:

```text
231 → Hurda/atık teslimleri
301 → Mal ihracatı
335 → Basılı kitap ve süreli yayınlar
351 → İstisna olmayan diğer
```

Bu değerler mevzuat ve kullanılan Logo sürümüne göre doğrulanmalıdır.

---

# Vaka 4 — Son Alış Fiyatı Yanlış Görünüyor

## Belirti

Bir malzemenin raporlanan son alış fiyatı kullanıcı beklentisinden farklıdır.

## Sık Nedenler

- yanlış belge türü dahil edilmiştir,
- iade hareketi son alış olarak alınmıştır,
- iptal kayıtları filtrelenmemiştir,
- birim dönüşümü yapılmamıştır,
- döviz kuru yanlış yorumlanmıştır,
- mevcut fatura kendisi son alış sorgusuna dahil edilmiştir.

## Çözüm Kalıbı

Son alış hesabında parametreler açık olmalıdır:

```text
Firma
Malzeme
Tarih sınırı
Hariç tutulacak belge
Belge türleri
Birim
Döviz
```

Aynı fatura içinden kontrol yapılıyorsa mevcut `INVOICEREF` hariç tutulabilir.

---

# Vaka 5 — Çift Birimli Malzemede Hatalı Birim Seçimi

## Belirti

Malzemenin ana birimi ile satınalma birimi farklıdır ve kullanıcı bazı işlemlerde yanlış birim seçmiştir.

## Problem

Sadece stok miktarına bakarak yanlış birim tespiti her zaman mümkün değildir.

## Daha Güçlü Kontrol

Geçmiş satınalma fiyatları birim bazında karşılaştırılır.

Örneğin:

```text
KG fiyat aralığı   : 80–120 TL
ADET fiyat aralığı : 15–25 TL
```

Yeni işlemde ADET seçilmiş fakat fiyat 95 TL ise birim hatası adayı olabilir.

Bu kontrol istatistiksel olarak yapılmalı; otomatik düzeltme yerine uyarı mekanizması tercih edilmelidir.

---

# Vaka 6 — SQL Server Error 701

## Belirti

SQL Server:

```text
There is insufficient system memory in resource pool 'internal'...
```

veya Error 701 üretir.

## Yanlış Yaklaşım

Yalnızca SQL Server servisini yeniden başlatmak.

## Kontrol Listesi

- fiziksel RAM,
- SQL `max server memory`,
- OS kullanılabilir RAM,
- `sys.dm_os_memory_clerks`,
- plan cache,
- memory grants,
- `process_physical_memory_low`,
- büyük ad-hoc sorgular,
- aynı sunucudaki diğer servisler.

## Kalıcı Çözüm

Memory tüketiminin hangi clerk veya workload tarafından üretildiği bulunmalıdır.

---

# Vaka 7 — PAGELATCH Beklemeleri Çok Yüksek

## Belirti

Wait statistics içinde:

```text
PAGELATCH_EX
PAGELATCH_UP
PAGELATCH_SH
```

değerleri çok yüksektir.

## Kritik Ayrım

`PAGELATCH` ile `PAGEIOLATCH` aynı değildir.

- `PAGELATCH` → bellekte contention,
- `PAGEIOLATCH` → diskten page bekleme.

## Kontrol

Özellikle `tempdb`:

- data file sayısı,
- file boyut eşitliği,
- autogrowth,
- allocation contention

açısından incelenmelidir.

---

# Vaka 8 — tempdb Çok Küçük ve % Growth Kullanıyor

## Belirti

`tempdb` data file küçük ve autogrowth `%10` gibi yüzde bazlı ayarlanmıştır.

## Risk

Yoğun kullanımda sık büyüme olayı oluşur.

## Yaklaşım

- kullanım geçmişine göre başlangıç boyutu belirle,
- sabit MB autogrowth kullan,
- birden fazla data file gerekiyorsa eşit boyutlandır,
- disk latency ölç.

Ayar değişikliği workload ölçülmeden yapılmamalıdır.

---

# Vaka 9 — ASYNC_NETWORK_IO

## Belirti

Aktif sorgu `ASYNC_NETWORK_IO` beklemektedir.

## Yanlış Yorum

"SQL Server yavaş veri okuyor."

## Gerçek Anlam

Çoğu durumda SQL Server sonucu üretmiş, istemcinin veriyi tüketmesini bekliyordur.

Kontrol:

- dönen satır sayısı,
- sorgu kolon sayısı,
- Excel/Power BI davranışı,
- uygulamanın satır okuma yöntemi,
- network.

`SELECT *` kaldırmak ve filtreyi SQL tarafına taşımak çoğu senaryoda faydalıdır.

---

# Vaka 10 — Trigger İşlemi Yavaşlatıyor

## Belirti

Logo'da fiş kaydetme işlemi belirgin şekilde yavaşlamıştır.

## Analiz

Tabloda özel trigger var mı kontrol edilir.

Trigger içinde:

- cursor,
- büyük SELECT,
- başka database erişimi,
- mail gönderimi,
- web servis çağrısı

varsa transaction süresi uzayabilir.

## Çözüm

Trigger yalnızca gerekli kaydı queue/log tablosuna bırakmalı; ağır işlem başka worker veya job tarafından yapılmalıdır.

---

# Vaka 11 — Mail Gönderimi ERP İşlemini Etkiliyor

## Problem

Trigger veya kayıt işlemi içinde doğrudan `sp_send_dbmail` çağrılır.

SMTP gecikirse işlem süresi uzar.

## Önerilen Mimari

```text
ERP Transaction
     ↓
MailQueue
     ↓
SQL Agent / Windows Service
     ↓
sp_send_dbmail
```

Queue tablosunda:

```text
ID
Email
Subject
Body
IsSent
SentAt
ErrorMessage
```

gibi alanlar tutulabilir.

---

# Vaka 12 — Seri/Lot Stoku ile Fiili Stok Uyuşmuyor

## Belirti

Malzemenin envanter miktarı ile seri/lot toplamı farklıdır.

## Kontrol

- stok satırları,
- seri/lot bağlantıları,
- giriş/çıkış yönleri,
- ambar,
- iptal kayıtları,
- üretim hareketleri

birlikte analiz edilir.

Özellikle geçmişte doğrudan SQL ile müdahale yapılmış sistemlerde bu kontrol önemlidir.

---

# Vaka 13 — Üretim Emri Var, Gerçekleşme Eksik

## Belirti

Üretim emri kapanmış görünür fakat:

- tüm sarflar oluşmamış,
- fire kaydı eksik,
- mamul miktarı farklı,
- operasyon süresi eksik

olabilir.

## Çözüm

Üretim kontrol raporu şu zinciri kıyaslamalıdır:

```text
Planlanan
vs
Gerçekleşen Sarf
vs
Gerçekleşen Operasyon
vs
Mamul Çıktısı
```

`ACTDURATION`, `ACTAMOUNT` gibi gerçekleşme alanları süreç bağlamında değerlendirilmelidir.

---

# Vaka 14 — REST Service Yetki Problemi

## Belirti

Logo Objects REST Service çalışıyor fakat dosya, klasör veya servis hesabı yetkisi nedeniyle işlem başarısız oluyor.

## Kontrol

- Windows service account,
- uygulama klasörü izinleri,
- temp klasörü,
- Logo Objects bileşenleri,
- network share erişimi,
- SQL login,
- DCOM/COM gereksinimleri.

Servisin interaktif kullanıcıyla çalışması ile Windows service account altında çalışması aynı yetki bağlamı değildir.

---

# Vaka 15 — Entegrasyon Aynı Kaydı İki Kez Oluşturuyor

## Belirti

Timeout sonrası kaynak sistem işlemi tekrar gönderir ve Logo'da iki belge oluşur.

## Kök Neden

Idempotency yoktur.

## Çözüm

Her işlem için external ID saklanmalıdır.

```text
ExternalId UNIQUE
LogoRef
Status
```

Yeni kayıt öncesi aynı ID kontrol edilir.

---

## Vaka Analizlerinde Genel Kural

Bir Logo problemi incelenirken ilk hedef hatalı satırı düzeltmek değil, **hatalı satırın hangi iş akışı tarafından üretildiğini bulmak** olmalıdır.

En sağlıklı sıra:

```text
1. Belirtiyi doğrula
2. İlişkili kayıtları çıkar
3. Oluşturan kullanıcı/program/session'ı bul
4. Trigger/script/entegrasyonu belirle
5. Kök nedeni düzelt
6. Mevcut veriyi kontrollü düzelt
7. Tekrarını önleyen kontrol ekle
```

Bir sonraki bölümde tüm kitap boyunca geçen prensipler tek bir **Best Practices** rehberinde toplanacaktır.
