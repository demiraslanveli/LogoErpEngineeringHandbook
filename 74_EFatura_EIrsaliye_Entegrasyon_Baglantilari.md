# 74 — e-Fatura ve e-İrsaliye Entegrasyon Bağlantıları

## 1. Amaç

Bu bölüm Logo ERP içindeki fatura/irsaliye kayıtları ile elektronik belge süreçleri arasındaki ilişkiyi mimari açıdan açıklar.

Elektronik belge entegrasyonunda temel hata, Logo'daki ticari belge ile gönderilen elektronik belgenin birbirinden bağımsız ele alınmasıdır.

## 2. Belge Yaşam Döngüsü

```text
Logo Fatura / İrsaliye
    ↓
Elektronik Belge Hazırlığı
    ↓
XML / UBL oluşturma
    ↓
Entegratör / GİB gönderimi
    ↓
Durum takibi
    ↓
Kabul / Red / Hata
```

## 3. Ana Kimlikler

Takip edilmesi gereken kimlikler:

- Logo `LOGICALREF`
- Fatura / irsaliye numarası
- UUID / ETTN
- Entegratör belge ID
- Gönderim zamanı
- Son durum
- Son hata

Elektronik belge tablosunda Logo `LOGICALREF` saklamak güçlü bir referanstır.

## 4. Fatura Numarası

Belge numarası yalnızca kullanıcıya görünen numara değildir.

Entegrasyonda:

- Numaranın benzersizliği
- Seri düzeni
- Tarih
- Firma
- Belge tipi

birlikte değerlendirilmelidir.

## 5. Tarih Değişikliği Riski

Elektronik belge gönderildikten sonra Logo fatura tarihinin doğrudan değiştirilmesi ciddi tutarsızlık oluşturabilir.

Kontrol edilmesi gerekenler:

- Belge gönderildi mi?
- UUID oluştu mu?
- Muhasebeleşti mi?
- İrsaliye bağlantısı var mı?

Gönderilmiş elektronik belgelerde değişiklik süreci mevzuat ve entegratör akışıyla birlikte ele alınmalıdır.

## 6. İrsaliye → Fatura

Elektronik irsaliye ile faturanın bağlı olduğu senaryolarda Logo tarafındaki `STFICHE`, `STLINE`, `INVOICE` ilişkileri korunmalıdır.

Belge linki yalnızca belge numarası ile kurulmaz.

## 7. Durum Tablosu Örneği

```text
E_DOCUMENT_STATUS
-----------------
ID
FIRM_NR
PERIOD_NR
DOCUMENT_TYPE
LOGO_LOGICALREF
FICHENO
UUID
PROVIDER_ID
STATUS
STATUS_CODE
STATUS_MESSAGE
SENT_AT
UPDATED_AT
```

## 8. Retry Politikası

Teknik bağlantı hatalarında retry yapılabilir.

Ancak aşağıdaki hatalar otomatik retry edilmemelidir:

- Vergi numarası hatalı
- Zorunlu alan eksik
- Belge numarası hatalı
- Vergi matrahı / toplam uyuşmuyor

Bunlar veri/iş kuralı hatasıdır.

## 9. Toplam Kontrolleri

Gönderimden önce kontrol edilmesi önerilir:

```text
Mal/Hizmet Toplamı
İskonto Toplamı
KDV Matrahı
KDV Toplamı
Genel Toplam
Ödenecek Tutar
```

Logo `INVOICE` üst toplamları ile elektronik belge satır toplamları uyumlu olmalıdır.

## 10. KDV İstisna Bilgileri

KDV oranı 0 olan satırlarda istisna/muafiyet kodları elektronik belge açısından kritik olabilir.

Bu nedenle `VATEXCEPTCODE` ve `VATEXCEPTREASON` gibi alanların boş veya hatalı olması gönderim hatasına dönüşebilir.

## 11. Reconciliation

Günlük kontrol:

- Logo'da kayıtlı ama gönderilmemiş belgeler
- Gönderilmiş ama son durumu alınmamış belgeler
- Red olmuş belgeler
- UUID'si olmayan elektronik belgeler
- Logo kaydı silinmiş ancak entegratör kaydı bulunan belgeler

## 12. Sonuç

Elektronik belge entegrasyonu yalnızca XML üretmek değildir.

Sağlıklı yapı:

- Logo belgesini ana kaynak olarak izler,
- Elektronik belge kimliğini saklar,
- durumları tekrar sorgular,
- veri hatası ile teknik hatayı ayırır,
- mali ve vergisel toplamları gönderimden önce doğrular.
