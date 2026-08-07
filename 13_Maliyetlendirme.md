# 13 — Maliyetlendirme

## 1. Bölümün Amacı

Bu bölüm, Logo Tiger / Tiger Wings Enterprise ortamında maliyetlendirme mantığını geliştirici ve entegrasyon mimarisi açısından ele alır. Amaç tek tek maliyet ekranlarını açıklamak değil; stok hareketleri, üretim tüketimleri, üretim çıktıları, satınalma fiyatları, kur bilgileri, ek maliyetler ve muhasebe kayıtlarının maliyet sonucuna nasıl etki ettiğini doğru anlamaktır.

> Maliyet sonucu, tek bir tablodaki tek bir alan değildir. Birçok operasyonel hareketin tarihsel ve ilişkisel sonucudur.

---

## 2. Maliyet Neden Entegrasyon Açısından Kritiktir?

Bir entegrasyon stok miktarını doğru oluşturabilir fakat maliyet zincirini bozabilir.

Örneğin:

- tüketim fişi eksik oluşmuş olabilir,
- yanlış birim kullanılmış olabilir,
- hareket tarihi hatalı olabilir,
- kur bilgisi eksik olabilir,
- üretim emri ile stok hareketi bağı kopmuş olabilir,
- ek maliyetler yanlış dağıtılmış olabilir,
- seri/lot hareketi ile stok miktarı uyuşmayabilir.

Bu durumda fiziksel stok doğru görünse bile mali tablolar yanlış olabilir.

---

## 3. Temel Maliyet Kaynakları

Maliyet yapısını etkileyen başlıca kaynaklar şunlardır:

- satınalma faturaları,
- satınalma irsaliyeleri,
- üretim tüketimleri,
- üretim çıktıları,
- sarf hareketleri,
- fireler,
- depolar arası transferler,
- iade hareketleri,
- ek maliyetler,
- döviz kuru,
- işçilik,
- genel üretim giderleri,
- operasyon maliyetleri.

Kuruma ve aktif Logo modüllerine göre kapsam değişebilir.

---

## 4. Hareket Tarihi ve Maliyet

Maliyetlendirme tarih sırasına duyarlıdır.

Örneğin bir malzeme için:

```text
01.08  Satınalma  +100 adet
03.08  Üretim      -50 adet
05.08  Satınalma  +100 adet
```

ile

```text
01.08  Satınalma  +100 adet
05.08  Satınalma  +100 adet
06.08  Üretim      -50 adet
```

aynı miktarlara sahip olsa bile kullanılan maliyet yöntemine göre farklı sonuçlar üretebilir.

Bu nedenle geçmiş tarihli kayıt ekleme veya tarih değiştirme işlemleri maliyet açısından mutlaka değerlendirilmelidir.

---

## 5. Negatif Stok ve Maliyet

Maliyet sistemlerindeki en riskli durumlardan biri negatif stoktur.

Örneğin:

```text
Mevcut stok : 10
Çıkış       : 25
Sonuç       : -15
```

Daha sonra geriye dönük giriş yapıldığında maliyet hesaplarının yeniden değerlendirilmesi gerekebilir.

Negatif stok özellikle:

- üretim tüketimi,
- hızlı sevkiyat,
- geç gelen satınalma faturası,
- entegrasyon gecikmesi,
- hatalı hareket tarihi

nedeniyle oluşabilir.

Maliyet kontrol raporlarında negatif stok seviyesi ayrıca takip edilmelidir.

---

## 6. Üretim Maliyeti

Üretim maliyeti genel olarak aşağıdaki bileşenlerden oluşabilir:

```text
Hammadde
+ Yardımcı Malzeme
+ Ambalaj
+ İşçilik
+ Makine / Operasyon
+ Genel Üretim Gideri
+ Diğer Dağıtılan Giderler
= Mamul Maliyeti
```

Logo detaylı üretim kullanılan projelerde üretim emri, iş emirleri, operasyonlar ve stok fişleri arasındaki ilişkilerin korunması kritik önem taşır.

Bir üretim emrinin yalnızca mamul girişini oluşturmak doğru maliyet için yeterli değildir.

---

## 7. Sarf ve Üretim Çıktısı İlişkisi

Üretim entegrasyonunda aşağıdaki bağ kurulmalıdır:

```text
Üretim Emri
   ├── Hammadde Tüketimleri
   ├── Fireler
   ├── Operasyonlar
   └── Mamul / Yarı Mamul Çıktıları
```

Maliyet hesabı bu hareketler arasındaki bağlantılara dayanabilir.

Doğrudan SQL ile oluşturulan ve üretim emri ilişkisi eksik bırakılan stok hareketleri operasyonel olarak görünse bile maliyetlendirme tarafında eksik sonuç üretebilir.

---

## 8. Birim Dönüşümleri ve Maliyet

Maliyet hesaplarında birim dönüşümü kritik konudur.

Örnek:

```text
Ana Birim    : KG
İkinci Birim : ADET
1 ADET       : 0.250 KG
```

Satınalma fiyatı ADET üzerinden, stok ana birimi KG üzerinden tutuluyorsa fiyat karşılaştırmalarında dönüşüm doğru uygulanmalıdır.

Yanlış `UINFO1 / UINFO2`, dönüşüm faktörü veya birim seçimi:

- stok miktarını,
- birim maliyeti,
- satınalma fiyat karşılaştırmasını,
- üretim tüketim maliyetini

bozabilir.

---

## 9. Dövizli İşlemler

Dövizli satınalmalarda fiyatın tek başına saklanması yeterli değildir.

Aşağıdaki bilgiler birlikte değerlendirilmelidir:

- işlem dövizi,
- döviz kuru,
- raporlama dövizi,
- yerel para birimi karşılığı,
- belge tarihi,
- kur tarihi.

Örneğin USD son alış fiyatı hesaplanırken hareketin hangi fiyat alanının ve hangi kur alanının kullanıldığı açıkça tanımlanmalıdır.

---

## 10. Son Alış Fiyatı ile Maliyet Aynı Şey Değildir

Bu iki kavram sık karıştırılır.

**Son alış fiyatı:** En son uygun satınalma hareketindeki fiyat bilgisidir.

**Maliyet:** Seçilen maliyet yöntemine ve hareket geçmişine göre hesaplanan değerdir.

Dolayısıyla:

```text
Son Alış Fiyatı ≠ Stok Maliyeti
```

Her rapor hangi değeri kullandığını açıkça belirtmelidir.

---

## 11. Maliyet Kontrol Raporları

Faydalı kontrol raporları:

- negatif stok oluşan malzemeler,
- maliyeti sıfır olan stok hareketleri,
- son alış fiyatına göre anormal sapmalar,
- üretim sarfı olup mamul çıktısı olmayan emirler,
- mamul çıktısı olup sarfı olmayan emirler,
- yanlış birim fiyatlı hareketler,
- geçmiş tarihli yeni kayıtlar,
- yüksek maliyet sapması bulunan lotlar.

---

## 12. Entegrasyonlarda Maliyet Güvenliği

Entegrasyon tasarımında aşağıdaki kontroller önerilir:

1. Hareket tarihi doğrulanmalı.
2. Malzeme ve birim eşleşmesi doğrulanmalı.
3. Miktar sıfır veya anlamsız olmamalı.
4. Dövizli işlemde kur bilgisi doğrulanmalı.
5. Üretim hareketinde üretim emri bağlantısı korunmalı.
6. Seri/lot miktarı stok satırı miktarıyla uyumlu olmalı.
7. Aynı hareket ikinci kez aktarılmamalı.
8. Negatif stok riski önceden kontrol edilmeli.

---

## 13. Doğrudan SQL Güncellemelerinin Riski

Maliyet alanlarını doğrudan güncellemek genellikle doğru yaklaşım değildir.

Çünkü hesaplanan değerler:

- başka hareketlerden,
- dönemsel maliyet işlemlerinden,
- stok seviyelerinden,
- döviz bilgilerinden,
- üretim ilişkilerinden

türetilebilir.

Bir alanı manuel değiştirmek sonraki maliyet çalıştırmasında ezilebilir veya diğer tablolarla tutarsızlık oluşturabilir.

---

## 14. Performans Perspektifi

Maliyet raporları büyük `STLINE` tablolarında ağır çalışabilir.

Dikkat edilmesi gerekenler:

- tarih filtresi,
- firma/dönem filtresi,
- `STOCKREF`, `INVOICEREF`, `STFICHEREF`, `PROJECTREF` gibi bağlantı alanları,
- uygun indeksler,
- gereksiz `SELECT *` kullanımından kaçınma,
- aynı büyük tabloyu tekrar tekrar taramama,
- rapor amaçlı özetleme stratejileri.

Performans iyileştirmesi yapılırken Logo'nun standart indekslerinin körlemesine değiştirilmemesi gerekir.

---

## 15. Maliyet Problemi Analiz Sırası

Bir maliyet problemi geldiğinde şu sıra faydalıdır:

```text
1. Malzeme
2. Tarih
3. Ambar
4. Hareket sırası
5. Miktar
6. Birim
7. Fiyat
8. Kur
9. Üretim bağlantısı
10. Seri/Lot bağlantısı
11. Negatif stok
12. Maliyet çalıştırma zamanı
```

Bu sıra sorunun yalnızca sonuç alanına bakılarak analiz edilmesini engeller.

---

## 16. Sonuç

Logo maliyetlendirme sistemi, stok hareketlerinin finansal izdüşümüdür. Doğru maliyet için yalnızca fiyat değil, hareket sırası, tarih, miktar, birim, kur, üretim ilişkileri ve stok seviyeleri birlikte doğru olmalıdır.

Entegrasyon geliştirirken temel hedef yalnızca "fiş kaydedildi" olmamalıdır. Doğru soru şudur:

> Bu hareket Logo'nun stok, üretim ve maliyet zincirine eksiksiz olarak bağlandı mı?

Bir sonraki bölümde bu ilişkilerin fiziksel olarak tutulduğu **Logo veritabanı mimarisi** ele alınacaktır.
