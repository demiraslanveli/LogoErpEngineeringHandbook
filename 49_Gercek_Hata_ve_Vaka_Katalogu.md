# 49 — Gerçek Hata ve Vaka Kataloğu

## Amaç

Bu bölüm, Logo ERP / Logo Objects / SQL Server projelerinde sahada karşılaşılan hata tiplerini sınıflandırmak için yaşayan bir katalogdur. Amaç tek tek hata mesajlarını ezberlemek değil, hatayı doğru katmanda teşhis etmektir.

## 1. Veri bütünlüğü hataları

Belirtiler:

- fiş başlığı var, satır yok,
- stok hareketi var, cari hareket yok,
- fatura var, muhasebe fişi yok,
- seri/lot hareketi eksik,
- sipariş bağlantısı kopuk,
- iade veya kaynak satır referansı yanlış.

Kontrol edilmesi gerekenler:

```text
INVOICE
STFICHE
STLINE
CLFLINE
EMFICHE
EMFLINE
SLTRANS
SERILOTN
ORFICHE
ORFLINE
```

Temel ders:

> Logo belgesi tek tablo değildir; ilişkili kayıtlar zinciridir.

## 2. Yanlış ambar problemi

Belirti:

- fiş başlığındaki ambar ile satır ambarı uyuşmuyor,
- kaynak satırın ambarı yanlış taşınmış,
- iade sonrası 801 gibi geçici/özel bir ambar değeri kalmış.

Kontrol alanları:

```text
SOURCEINDEX
DESTINDEX
IOCODE
SOURCELINK
PREVLINEREF
TRCODE
```

Analiz:

Önce fiş türü ve hareket yönü belirlenmelidir. Sonra satırın hangi kaynaktan üretildiği takip edilmelidir.

## 3. Sipariş–irsaliye bağlantısı kopuk

Belirti:

- sipariş sevk edilmiş görünmüyor,
- sevk var ancak sipariş açık kalıyor,
- sipariş miktarı ile sevk miktarı farklı.

Kontrol:

```text
ORFLINE.LOGICALREF
    ↓
STLINE.ORDTRANSREF
```

Ek olarak iptal/iade hareketleri değerlendirilmelidir.

## 4. PREVLINEREF / SOURCELINK problemi

Belirti:

- iade satırı yanlış kaynağa bağlı,
- kaynak hareket bulunamıyor,
- ambar veya maliyet bilgisi yanlış taşınıyor.

Kontrol:

```text
PREVLINEREF
SOURCELINK
ORDTRANSREF
```

Bu alanlar aynı şeyi ifade etmez. İşlem türüne göre ilişki semantiği değişir.

## 5. Yanlış birim seçimi

Belirti:

- stok miktarı beklenenden çok büyük/küçük,
- birim fiyat anlamsız seviyede,
- ikinci birim hareketleri yanlış görünüyor.

Kontrol:

```text
UOMREF
USREF
UINFO1
UINFO2
AMOUNT
PRICE
```

Ayrıca:

```text
ITMUNITA
UNITSETL
```

kart tanımı ile karşılaştırılmalıdır.

Güçlü yöntem:

> Birim bilgisini geçmiş satınalma birim fiyatı ile birlikte analiz etmek.

## 6. Seri/Lot bakiyesi uyuşmuyor

Belirti:

- stok var ama lot görünmüyor,
- lot var ama stok yok,
- aynı seri beklenmeyen ambarda,
- üretimde kullanılan lot izlenemiyor.

Kontrol zinciri:

```text
STLINE
  ↓
SLTRANS
  ↓
SERILOTN
```

Miktar yönleri ve ambar bilgileri birlikte değerlendirilmelidir.

## 7. Üretim emri var, stok hareketi yok

Kontrol sırası:

1. `PRODORD` var mı?
2. üretim emri statüsü nedir?
3. operasyonlar oluşmuş mu?
4. sarf hareketleri var mı?
5. mamul giriş hareketi var mı?
6. `PRODORDERREF` bağlantısı var mı?
7. seri/lot hareketleri oluşmuş mu?

## 8. Maliyetlendirme sonrası negatif veya beklenmeyen maliyet

Belirti:

- stok maliyeti beklenmeyen seviyede,
- negatif seviye,
- son alış fiyatı ile maliyet arasında büyük fark.

Kontrol:

- hareket tarih sırası,
- iade hareketleri,
- miktar birimi,
- döviz kuru,
- üretim sarfları,
- dönemsel maliyetlendirme sırası,
- negatif stok anları.

Maliyet problemi çoğu zaman yalnızca `PRICE` alanına bakılarak çözülemez.

## 9. KDV muafiyet alanı boş

Belirti:

- KDV oranı 0,
- muafiyet sebebi boş,
- e-belge veya kontrol sürecinde hata.

Kontrol:

```text
VAT
VATEXCEPTCODE
VATEXCEPTREASON
```

İş kuralı olarak KDV=0 ise muafiyet kodu/açıklaması zorunlu hale getirilebilir.

## 10. Fatura tarihi değişti ama bağlı kayıtlar değişmedi

Bir fatura tarihinin değiştirilmesi yalnızca `INVOICE.DATE_` güncellemesi değildir.

İlişkili yapılar:

```text
INVOICE
STFICHE
STLINE
CLFLINE
EMFICHE
EMFLINE
```

İşlem kontrollü ve transaction-aware yapılmalıdır.

## 11. SQL Server Error 701

Belirti:

```text
There is insufficient system memory...
```

Kontrol:

- SQL max server memory,
- OS available memory,
- memory grants,
- plan cache,
- büyük sorgular,
- dış istemci davranışları,
- `ASYNC_NETWORK_IO` gibi wait tipleri.

Tek başına toplam RAM yüksek olması problemin olmadığı anlamına gelmez.

## 12. PAGELATCH beklemeleri

Belirti:

```text
PAGELATCH_EX
PAGELATCH_UP
PAGELATCH_SH
```

Özellikle tempdb contention araştırılmalıdır.

Kontrol:

- tempdb dosya sayısı,
- dosya boyutları,
- autogrowth,
- disk gecikmesi,
- yoğun temporary object üretimi.

## 13. ASYNC_NETWORK_IO

Belirti:

SQL sonuç üretmiş olabilir ancak istemci veriyi yavaş tüketiyordur.

Nedenler:

- Excel,
- Power BI / Mashup Engine,
- yavaş uygulama gridleri,
- gereksiz büyük result set,
- istemci tarafı işleme.

Çözüm yalnızca SQL index eklemek değildir.

## 14. Logo Objects login problemi

Kontrol sırası:

- kullanıcı/parola,
- firma/dönem,
- lisans,
- servis hesabı yetkisi,
- COM/Objects kurulumu,
- uygulama bitness,
- çalışma hesabı ve desktop session farkları.

## 15. REST Service yetki problemi

Logo Objects REST servislerinde servis hesabının:

- executable klasörüne,
- config dosyalarına,
- geçici klasörlere,
- gerekli COM bileşenlerine,
- ağ/SQL kaynaklarına

erişimi kontrol edilmelidir.

## 16. Form field type mismatch

Örnek hata:

```text
Form field type does not match with table field type.
```

Muhtemel nedenler:

- form kontrol tipi ile bağlı alan tipi uyuşmuyor,
- combo/text edit yanlış bağlanmış,
- metadata alan türü farklı.

Çözüm, yalnızca script kodunu değil form alan tanımını da incelemektir.

## 17. Trigger kaynaklı görünmeyen veri değişikliği

Belirti:

- kullanıcı bir değer kaydediyor,
- veritabanında farklı değer oluşuyor,
- uygulama davranışı açıklanamıyor.

Kontrol:

```sql
SELECT name, is_disabled
FROM sys.triggers;
```

İlgili tablo trigger'ları ve özel log tabloları incelenmelidir.

## 18. Database Mail profile hatası

Örnek:

```text
profile name is not valid
```

Kontrol:

- gerçek Database Mail profil adı,
- servis/job çalıştıran kullanıcı,
- profil erişimi,
- `sp_send_dbmail` parametreleri.

## 19. Tarih formatı problemi

Entegrasyonlarda tarih string birleştirmek risklidir.

Öneri:

- parametrik SQL,
- `DATE` / `DATETIME` tipleri,
- ISO format gerektiğinde açık format.

## 20. Hata analiz şablonu

Her yeni vaka için şu kayıt formatı kullanılabilir:

```text
Belirti:
Etkilenen modül:
Firma/Dönem:
Belge türü:
İlgili LOGICALREF:
İlgili tablolar:
Beklenen davranış:
Gerçek davranış:
Kök neden:
Çözüm:
Kalıcı önlem:
Doğrulama sorgusu:
```

## Sonuç

Gerçek Logo problemlerinde en güçlü teşhis yöntemi:

```text
Belirtiyi tanımla
    ↓
Doğru modülü belirle
    ↓
Referans zincirini takip et
    ↓
İş kuralını doğrula
    ↓
Veritabanı kaydını karşılaştır
    ↓
Logo Objects / ekran davranışıyla test et
```

Bu katalog, yeni gerçek vakalar geldikçe genişletilmelidir.
