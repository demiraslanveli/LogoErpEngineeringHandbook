# 75 — Muhasebe Entegrasyon Hataları ve Kontrol Listesi

## 1. Amaç

Bu bölüm, Logo ERP'de ticari belge ile muhasebe fişi arasındaki entegrasyonda görülen yaygın hata sınıflarını ve sistematik kontrol yöntemini açıklar.

## 2. Tipik Belge Zinciri

```text
INVOICE / STFICHE
    ↓
CLFLINE
    ↓
EMFICHE
    ↓
EMFLINE
```

Her belge türü aynı zinciri üretmeyebilir; ancak muhasebeleşen hareketlerde üst belge ile muhasebe fişi arasındaki referans ilişkisi kontrol edilmelidir.

## 3. Yaygın Hata: Muhasebe Fişi Yok

Kontroller:

- Belge muhasebeleşmeye uygun mu?
- Muhasebeleştirme parametreleri doğru mu?
- Hesap planı eşleştirmeleri mevcut mu?
- Muhasebe bağlantı kodları tanımlı mı?
- Belge iptal edilmiş mi?
- Dönem kapalı mı?

## 4. Yaygın Hata: Fiş Var, Satırlar Eksik

Olası nedenler:

- KDV hesabı eksik
- İskonto hesabı eksik
- Masraf hesabı eksik
- Malzeme muhasebe kodu eksik
- Cari hesap muhasebe bağlantısı eksik
- Proje / masraf merkezi bağlantısı hatalı

## 5. Borç / Alacak Kontrolü

Bir muhasebe fişinin temel kontrolü:

```text
SUM(DEBIT) = SUM(CREDIT)
```

Fiş dengeli değilse sistemsel veya veri kaynaklı problem araştırılmalıdır.

## 6. Tarih Tutarlılığı

Bağlı belgelerde tarih kontrolü:

```text
INVOICE.DATE_
STFICHE.DATE_
CLFLINE.DATE_
EMFICHE.DATE_
```

Özellikle manuel tarih değişikliklerinde bu zincirin yalnızca bir parçasının değiştirilmesi tutarsızlık yaratabilir.

## 7. Proje / Masraf Merkezi

Finansal raporlama için aşağıdaki alanların doğru aktarılması önemlidir:

- PROJECTREF
- CENTERREF
- ACCOUNTREF

Ticari belge ile muhasebe satırı arasında proje/masraf merkezi kaybolursa finansal analiz hatalı çıkar.

## 8. Döviz Kontrolü

Muhasebe fişinde:

- İşlem dövizi
- Raporlama dövizi
- Kur
- Dövizli borç/alacak

alanları ticari belgeyle tutarlı olmalıdır.

## 9. KDV Kontrolü

Kontrol edilecekler:

- KDV oranı
- Matrah
- KDV tutarı
- KDV hesabı
- İstisna / muafiyet durumu

Özellikle KDV=0 satırlarda istisna sebebi ile muhasebe kodlaması birlikte değerlendirilmelidir.

## 10. Otomatik Kontrol Sorgusu Yaklaşımı

Bir kontrol view/procedure şu sonuçları üretmelidir:

```text
BELGE_REF
BELGE_NO
BELGE_TARIHI
CARI_REF
MUHASEBE_FIS_REF
MUHASEBE_FIS_NO
BORC_TOPLAM
ALACAK_TOPLAM
FARK
HATA_TIPI
```

## 11. Hata Sınıfları

### Eksik Bağlantı

Muhasebe fişi veya satırı oluşmamış.

### Hesap Eşleme Hatası

Muhasebe hesabı boş veya yanlış.

### Tutar Farkı

Ticari belge ile muhasebe fişi toplamı farklı.

### Tarih Farkı

Bağlı kayıtların tarihleri farklı.

### Proje / Merkez Farkı

Analitik boyutlar taşınmamış.

## 12. Düzeltme Prensibi

Muhasebe fişini doğrudan SQL ile düzeltmek ilk tercih olmamalıdır.

Öncelik:

1. Sorunun kaynağını belirle.
2. Logo parametre/eşleme hatasını düzelt.
3. Belgeyi kontrollü şekilde yeniden muhasebeleştir.
4. SQL DML yalnızca zorunlu ve doğrulanmış senaryoda kullanılsın.

## 13. Reconciliation

Periyodik reconciliation örnekleri:

- Fatura toplamı ↔ cari hareket
- Fatura toplamı ↔ muhasebe fişi
- KDV toplamı ↔ KDV muhasebe hesabı
- Cari toplamı ↔ 120/320 hesap satırı

## 14. Sonuç

Muhasebe entegrasyonunda doğru yaklaşım yalnızca eksik fişi bulmak değildir.

Belge zinciri, hesap eşlemeleri, analitik boyutlar ve finansal toplamlar birlikte doğrulanmalıdır.
