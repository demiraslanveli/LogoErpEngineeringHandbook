# 11 — Seri / Lot Takibi

## 1. Bölümün Amacı

Bu bölüm, Logo ERP'de seri ve lot izlenebilirliğinin mantığını, üretim ve stok hareketleriyle ilişkisini ve entegrasyon sırasında dikkat edilmesi gereken temel prensipleri açıklar.

Seri/lot takibi özellikle aşağıdaki sektörlerde kritik öneme sahiptir:

- İlaç
- Gıda
- Kimya
- Medikal
- Otomotiv
- Elektronik

Temel prensip:

> Seri/lot bilgisi yalnızca açıklama değildir; stok hareketinin izlenebilirlik boyutudur ve ilgili stok satırıyla doğru bağlantı içinde tutulmalıdır.

---

## 2. Seri ve Lot Arasındaki Fark

### Seri

Tekil ürün kimliği için kullanılır.

Örnek:

```text
SN-000001
SN-000002
SN-000003
```

Her fiziksel ürün ayrı bir seri numarasıyla izlenebilir.

### Lot

Aynı üretim/tedarik grubundaki ürünlerin toplu kimliğidir.

Örnek:

```text
LOT-260807-A
```

Aynı lotta yüzlerce veya binlerce birim bulunabilir.

---

## 3. Kart Seviyesinde Takip Ayarı

Bir malzemenin seri/lotla takip edilip edilmeyeceği kart seviyesinde belirlenir.

Bu ayar fiş davranışını etkiler.

Takipli bir üründe stok hareketinin yalnızca miktar satırıyla oluşturulması yeterli olmayabilir; dağıtım detaylarının da girilmesi gerekir.

---

## 4. Miktar ile Lot Dağıtımı Uyumlu Olmalıdır

Örnek:

```text
Satır miktarı: 100 KG
```

Lot dağıtımı:

```text
LOT-A = 60 KG
LOT-B = 40 KG
```

Toplam:

```text
60 + 40 = 100 KG
```

olmalıdır.

Eksik veya fazla dağıtım Objects validasyonunda hata üretebilir veya süreç bütünlüğünü bozabilir.

---

## 5. Seri Takibinde Miktar

Seri bazlı takipte çoğu senaryoda her seri tekil miktarı temsil eder.

Örnek:

```text
Satır miktarı: 3 ADET

SN001 → 1
SN002 → 1
SN003 → 1
```

Entegrasyon, aynı seri numarasını yanlışlıkla iki kez kullanmamalıdır.

---

## 6. Satınalmada Lot Girişi

Lot takipli bir hammadde satın alındığında tipik olarak şu bilgiler önemlidir:

- Tedarikçi
- Malzeme
- Lot no
- Miktar
- Üretim tarihi
- Son kullanma tarihi
- Ambar
- Stok yeri

Bu bilgi daha sonra üretimde sarf izlenebilirliğinin temelini oluşturur.

---

## 7. Üretimde Lot Sarfı

Örnek:

```text
Üretim Emri: UE-000125

Hammadde A:
LOT-A1 → 300 KG
LOT-A2 → 200 KG
```

Toplam 500 KG sarf edilmiştir.

Mamul lotu:

```text
MAMUL-260807-01
```

oluştuğunda hammadde lotlarıyla bağlantı korunmalıdır.

---

## 8. İleri İzlenebilirlik

Bir hammadde lotunun hangi mamullerde kullanıldığını bulma işlemidir.

Soru:

> LOT-A1 hangi üretimlerde kullanıldı?

Bu analiz geri çağırma süreçlerinde çok önemlidir.

---

## 9. Geri İzlenebilirlik

Bir mamul lotunun hangi hammaddelerden üretildiğini bulma işlemidir.

Soru:

> MAMUL-260807-01 hangi hammadde lotlarını içeriyor?

Kalite şikâyetlerinde en kritik analizlerden biridir.

---

## 10. Stok Yeri ile Birlikte İzleme

Lot takibi tek başına yeterli olmayabilir.

Örnek:

```text
LOT-A
Ambar: 4
Stok Yeri: RAF-A03
Miktar: 120 KG
```

Aynı lot farklı stok yerlerinde bulunabilir.

Bu nedenle lot + ambar + stok yeri birlikte değerlendirilmelidir.

---

## 11. Karantina ve Serbest Stok

Kalite kontrollü firmalarda aynı lot farklı statülerde bulunabilir.

Örnek:

```text
LOT-A
- Karantina: 100 KG
- Serbest: 400 KG
```

Toplam stok 500 KG olsa bile üretimde kullanılabilir stok yalnızca 400 KG olabilir.

Entegrasyon yalnızca toplam lot bakiyesine bakmamalıdır.

---

## 12. Son Kullanma Tarihi

Lot bazında SKT yönetimi kritik olabilir.

Örnek:

```text
Lot: HM260701
Üretim Tarihi: 01.07.2026
SKT: 01.07.2028
```

Sarf seçimlerinde FEFO yaklaşımı uygulanabilir:

```text
First Expired, First Out
```

Yani önce son kullanma tarihi yaklaşan lot tüketilir.

---

## 13. FIFO ve FEFO Ayrımı

### FIFO

İlk giren stok ilk çıkar.

### FEFO

Son kullanma tarihi önce dolacak stok önce çıkar.

İlaç ve gıda sektöründe FEFO çoğu zaman operasyonel olarak daha anlamlıdır.

Logo’daki seçim davranışı ve firma süreçleri ayrıca doğrulanmalıdır.

---

## 14. Lot Stok Hesabı

Lot stoğu hareket bazlı hesaplanmalıdır.

Basit mantık:

```text
Girişler - Çıkışlar = Lot Stoku
```

Ancak gerçek sorguda:

- IOCODE
- Belge tipi
- İptal durumu
- Ambar
- Stok yeri
- Dönem

kriterleri dikkate alınmalıdır.

---

## 15. REMAMOUNT Kullanımı

Bazı seri/lot tablolarında kalan miktarı temsil eden `REMAMOUNT` benzeri alanlar bulunabilir.

Bu alan stok yaşlandırma ve mevcut lot listelerinde yararlı olabilir.

Ancak alanın hangi seviyede tutulduğu ve güncellenme mantığı sürüme göre doğrulanmalıdır.

---

## 16. Seri/Lot ile SQL Raporlama

SQL şu amaçlarda güçlüdür:

- Mevcut lot stokları
- SKT yaklaşan lotlar
- Lot yaşlandırma
- İlk giriş tarihi
- Son hareket tarihi
- Lotun bulunduğu stok yeri
- İleri/geri izlenebilirlik raporları

Ancak seri/lot hareketi üretmek için doğrudan SQL önerilmez.

---

## 17. Doğrudan SQL Riski

Şu yaklaşım yüksek risklidir:

```text
STLINE oluştur
→ Lot tablosuna manuel kayıt ekle
```

Çünkü dağıtım ilişkileri, referanslar ve toplamlar eksik kalabilir.

Seri/lot hareketleri Logo Objects’in beklediği alt satır yapılarıyla oluşturulmalıdır.

---

## 18. Duplicate Lot Kontrolü

Lot numarasının benzersizlik kuralı firma süreçlerine göre değişebilir.

Örneğin aynı lot no:

- Farklı malzemelerde kullanılabilir mi?
- Aynı malzemede tekrar açılabilir mi?
- Tedarikçi lotu ile iç lot ayrı mı?

başta belirlenmelidir.

---

## 19. Tedarikçi Lotu ve İç Lot

Bazı firmalarda iki ayrı lot bilgisi kullanılır:

```text
Supplier Lot: SUP-845792
Internal Lot: HM-260807-001
```

Entegrasyonda bu iki bilgi karıştırılmamalıdır.

İzlenebilirlik için eşleme saklanmalıdır.

---

## 20. Lot Bölme

Bir lotun farklı stok yerlerine veya süreçlere bölünmesi mümkündür.

Örnek:

```text
LOT-A = 500 KG

RAF-A01 = 200 KG
RAF-A02 = 150 KG
KARANTINA = 150 KG
```

Lot kimliği aynı kalırken dağıtım konumu değişebilir.

---

## 21. Lot Birleştirme Konusu

Fiziksel olarak farklı lotların tek lot gibi ele alınması izlenebilirliği bozabilir.

Özellikle üretimde:

```text
LOT-A + LOT-B → Mamul LOT-X
```

ise iki giriş lotunun da ayrı ayrı bağlantısı korunmalıdır.

---

## 22. Sayım ve Lot

Stok sayımında toplam malzeme miktarını düzeltmek yeterli değildir.

Lot takipli malzemede sayım şu seviyede yapılmalıdır:

```text
Malzeme
+ Ambar
+ Stok Yeri
+ Lot
```

Aksi halde toplam stok doğru görünürken lot stokları yanlış kalabilir.

---

## 23. Lot Transferi

Ambar veya stok yeri transferlerinde lot kimliği korunmalıdır.

Örnek:

```text
LOT-A
Ambar 1 → Ambar 4
```

Transfer sonrası yeni lot oluşturmak yerine mevcut lotun hareket zinciri devam etmelidir; firma kuralı ve Logo davranışı doğrulanmalıdır.

---

## 24. Lot İptal ve Geri Alma

Bir seri/lot hareketinin iptali yalnızca stok satırının silinmesi değildir.

Dağıtım ve bağlı hareketlerin de tutarlı biçimde geri alınması gerekir.

Bu nedenle Objects üzerinden işlem yapmak önemlidir.

---

## 25. Üretim Sonrası Lot Mutabakatı

Kontrol:

```text
Mamul üretim miktarı
=
Mamul lot dağıtım toplamı
```

Ayrıca:

```text
Sarf satırı miktarı
=
Sarf lot dağıtım toplamı
```

olmalıdır.

---

## 26. Entegrasyon Payload Örneği

```json
{
  "itemCode": "150.HM.001",
  "warehouse": 4,
  "quantity": 500,
  "lots": [
    {
      "lotNo": "HM260801",
      "quantity": 300
    },
    {
      "lotNo": "HM260805",
      "quantity": 200
    }
  ]
}
```

Servis tarafında toplam lot miktarı ana miktarla karşılaştırılmalıdır.

---

## 27. Validasyon Kuralları

Kayıt öncesinde:

- Malzeme lot takipli mi?
- Lot mevcut mu?
- Lot kullanılabilir durumda mı?
- Lot stoğu yeterli mi?
- Ambar doğru mu?
- Stok yeri doğru mu?
- SKT geçmiş mi?
- Dağıtım toplamı satır miktarına eşit mi?

kontrol edilmelidir.

---

## 28. Negatif Lot Stoku

Toplam malzeme stoğu yeterli olsa bile seçilen lotun stoğu yetersiz olabilir.

Örnek:

```text
Toplam stok: 1000 KG
LOT-A: 20 KG
İstenen LOT-A sarfı: 50 KG
```

Toplam stok kontrolü bu hatayı yakalayamaz.

Lot bazlı yeterlilik kontrolü gerekir.

---

## 29. Lot Yaşlandırma

Lot yaşlandırma raporu için faydalı bilgiler:

- Lot no
- Mevcut miktar
- İlk giriş tarihi
- Son hareket tarihi
- Üretim tarihi
- SKT
- Gün yaşı
- Ambar
- Stok yeri

Bu rapor stok optimizasyonu ve FEFO planlamasında kullanılır.

---

## 30. Hareketsiz Lotlar

Örnek analiz:

```text
REMAMOUNT > 0
AND son hareket < bugün - 365 gün
```

Bu lotlar stokta kalmış ancak uzun süredir hareket görmemiş olabilir.

İmha, kalite kontrol veya stok optimizasyonu açısından değerlendirilir.

---

## 31. Geri Çağırma Senaryosu

Bir hammadde lotunda kalite problemi tespit edildiğinde sistem şu zinciri hızlıca üretebilmelidir:

```text
Hammadde LOT-A
   ↓
Üretim Emirleri
   ↓
Mamul Lotları
   ↓
Satış Sevkiyatları
   ↓
Müşteriler
```

Bu raporlamanın mümkün olması, üretim ve satış boyunca lot bağlantılarının korunmasına bağlıdır.

---

## 32. Audit Log

Entegrasyon kaynaklı lot işlemlerinde şu bilgiler loglanabilir:

```text
ExternalId
ItemCode
LotNo
Warehouse
Location
Quantity
TransactionType
LogoRef
CreatedAt
```

Bu log, Logo kayıtlarının yerine geçmez; entegrasyon izini tutar.

---

## 33. Best Practices

### Yapılması önerilenler

- Kart seviyesinde doğru takip yöntemini tanımla.
- Satır miktarı ile lot dağıtım toplamını eşitle.
- Lot bazında stok yeterliliği kontrol et.
- Ambar ve stok yerini lotla birlikte değerlendir.
- SKT kontrolü yap.
- Üretimde ileri ve geri izlenebilirliği koru.
- Tedarikçi lotu ile iç lotu ayır.
- Sayımları lot seviyesinde yap.
- Geri çağırma raporlarını önceden test et.

### Kaçınılması gerekenler

- Lot bilgisini açıklama alanında tutmak.
- Toplam stok yeterliyse lot stoğunu kontrol etmemek.
- Seri/lot tablolarına doğrudan INSERT yapmak.
- Farklı lotları iz bırakmadan birleştirmek.
- Mamul lotunu hammadde lotlarından koparmak.
- SKT geçmiş lotların otomatik sarfına izin vermek.

---

## 34. Sonuç

Seri/lot takibi, Logo ERP'de stok hareketlerinin izlenebilirlik katmanıdır.

Doğru zincir:

```text
Malzeme
  ↓
Seri / Lot
  ↓
Ambar / Stok Yeri
  ↓
Stok Hareketi
  ↓
Üretim / Satış / Satınalma
  ↓
İleri ve Geri İzlenebilirlik
```

şeklinde ele alınmalıdır.

Seri/lot entegrasyonunda amaç yalnızca lot numarasını kaydetmek değil, **lotun bütün yaşam döngüsünü hareketlerle bağlantılı şekilde korumaktır.**
