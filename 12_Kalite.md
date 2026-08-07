# 12 — Kalite Yönetimi

## 1. Bölümün Amacı

Bu bölüm, Logo Tiger / Tiger Wings Enterprise üzerinde kalite süreçlerinin üretim, satınalma, stok, seri/lot ve maliyet yapılarıyla nasıl birlikte ele alınması gerektiğini açıklar. Amaç yalnızca kalite kontrol kartlarını veya sonuç alanlarını tanımlamak değil; kalite bilgisinin ERP içindeki operasyonel etkisini doğru modellemektir.

Kalite yönetimi özellikle ilaç, medikal, gıda, kimya ve izlenebilirliğin kritik olduğu sektörlerde bağımsız bir modül gibi düşünülmemelidir. Kalite sonucu çoğu zaman malzemenin kullanılabilirliği, lot statüsü, üretime sevk edilebilirliği, bloke edilmesi, yeniden işleme alınması veya imha edilmesi gibi süreçleri doğrudan etkiler.

> Kalite kaydı yalnızca ölçüm sonucu değildir; stok hareketinin kullanılabilirlik kararını belirleyen iş kuralıdır.

---

## 2. Kalite Sürecinin ERP İçindeki Yeri

Tipik akış şu şekildedir:

1. Malzeme satınalma veya üretim yoluyla sisteme girer.
2. Malzeme için seri/lot oluşur.
3. Kalite kontrol gerekliliği belirlenir.
4. Numune veya kontrol kaydı açılır.
5. Parametre bazında ölçümler girilir.
6. Sonuç değerlendirilir.
7. Lot veya stok için kabul/red/bloke kararı oluşur.
8. Sonraki stok ve üretim hareketleri bu karara göre yürütülür.

Bu nedenle kalite entegrasyonu tasarlanırken yalnızca sonuç tablosuna kayıt atmak yeterli değildir.

---

## 3. Kalite Kontrol Planı

Bir kalite kontrol planında genel olarak aşağıdaki bilgiler bulunabilir:

- kontrol edilen malzeme,
- operasyon veya proses,
- kontrol parametresi,
- hedef değer,
- minimum değer,
- maksimum değer,
- ölçü birimi,
- kontrol yöntemi,
- zorunluluk bilgisi,
- numune miktarı,
- kabul kriteri.

Örneğin bir hammadde için:

| Parametre | Min | Max | Birim |
|---|---:|---:|---|
| Yoğunluk | 0.95 | 1.05 | g/ml |
| pH | 6.50 | 7.50 | pH |
| Nem | 0 | 5 | % |

Kalite sonucu yalnızca serbest metin olarak tutulmamalıdır. Mümkün olduğunca parametrik ve karşılaştırılabilir veri modeli tercih edilmelidir.

---

## 4. Seri/Lot ile Kalite İlişkisi

Kalite süreçlerinin en kritik bağlantılarından biri lot ilişkisidir.

Örneğin aynı malzeme koduna ait iki lot bulunabilir:

- LOT-A → kabul edildi,
- LOT-B → bloke edildi.

Malzeme kartı aynı olsa bile kullanılabilirlik lot bazında farklıdır.

Bu nedenle entegrasyonlarda aşağıdaki ilişki korunmalıdır:

```text
Malzeme
   ↓
Stok Hareketi
   ↓
Seri/Lot
   ↓
Kalite Kontrol
   ↓
Kalite Sonucu / Statü
```

Kalite sisteminin yalnızca malzeme kodu ile çalışması, seri/lot bazlı üretim yapan işletmelerde yetersizdir.

---

## 5. Kalite Statüleri

Kuruma göre isimler değişebilmekle birlikte aşağıdaki statüler yaygındır:

- Bekliyor
- Numune Alındı
- Analizde
- Kabul
- Şartlı Kabul
- Red
- Bloke
- Yeniden Kontrol
- İmha

Entegrasyonda kalite statüsünün yalnızca metinsel açıklama değil, iş akışını yöneten kontrollü bir değer olması gerekir.

---

## 6. Bloke Stok Yaklaşımı

Kalite kontrol tamamlanmamış veya reddedilmiş stokların üretimde kullanılmasını önlemek için blokaj mekanizması gerekir.

Bu kontrol farklı mimarilerle yapılabilir:

- özel ambar kullanımı,
- lot statüsü,
- özel kod,
- kalite statüsü,
- ara yazılım kontrolü,
- Logo tarafındaki yetki ve iş kuralları.

En doğru yöntem müşterinin süreç yapısına ve kullanılan Logo sürüm/modüllerine göre belirlenmelidir.

Önemli olan şudur:

> Kullanıcı kalite sonucu oluşmadan malzemeyi üretimde tüketememelidir.

---

## 7. Üretim ile Kalite Entegrasyonu

Detaylı üretim süreçlerinde kalite kontrolleri farklı aşamalarda yapılabilir:

### 7.1 Girdi kalite kontrolü

Satın alınan hammaddenin üretime alınmadan önce kontrol edilmesidir.

### 7.2 Proses kalite kontrolü

Üretim operasyonları sırasında yapılan ölçümlerdir.

Örnek:

- sıcaklık,
- basınç,
- karıştırma süresi,
- pH,
- dolum ağırlığı.

### 7.3 Final kalite kontrolü

Üretim tamamlandıktan sonra mamul lotunun serbest bırakılmadan önce kontrol edilmesidir.

Bu üç kalite noktası birbirinden ayrılmalıdır.

---

## 8. MES / LIMS Entegrasyonu

Gerçek projelerde kalite verisinin kaynağı Logo olmayabilir.

Örneğin:

```text
Üretim Makinesi / Laboratuvar
            ↓
        MES / LIMS
            ↓
      Ara Entegrasyon
            ↓
          Logo ERP
```

Burada temel mimari karar şudur:

- ölçüm verisinin ana kaynağı hangi sistemdir?
- kalite onayının ana kaynağı hangi sistemdir?
- Logo’ya hangi özet veya sonuç aktarılacaktır?

Her sistemin aynı verinin master'ı olması veri tutarsızlığına neden olur.

---

## 9. Entegrasyonda Idempotency

Kalite sonucu dış sistemden Logo’ya aktarılıyorsa aynı kayıt iki kez işlenmemelidir.

Önerilen entegrasyon kaydı:

```text
ExternalQualityId
MaterialRef
LotRef
ResultStatus
TransferDate
LogoReference
```

Aktarım öncesinde `ExternalQualityId` kontrol edilmelidir.

---

## 10. Hata Yönetimi

Kalite entegrasyonlarında aşağıdaki hatalar özellikle kritik kabul edilmelidir:

- lot bulunamadı,
- malzeme bulunamadı,
- lot ve malzeme eşleşmiyor,
- kalite planı bulunamadı,
- zorunlu parametre eksik,
- ölçüm değeri geçersiz,
- aynı sonuç daha önce aktarılmış,
- red statüsündeki lot kullanılmaya çalışılıyor.

Bu hataların yalnızca kullanıcı ekranında gösterilmesi yeterli değildir; merkezi log yapısına yazılması gerekir.

---

## 11. Doğrudan SQL Kullanımı

Kalite tablolarına doğrudan `INSERT`, `UPDATE` veya `DELETE` yapılmadan önce bağlı kayıt yapısı mutlaka analiz edilmelidir.

Bir kalite kaydı:

- stok hareketine,
- seri/lot kaydına,
- üretim emrine,
- operasyon kaydına,
- kalite parametrelerine,
- kullanıcı/onay bilgilerine

bağlı olabilir.

Bu nedenle desteklenen Logo Objects veya uygulama servisleri mevcutsa öncelik bunlara verilmelidir.

---

## 12. Raporlama

Kalite tarafında faydalı rapor örnekleri:

- kabul/red oranı,
- tedarikçi bazlı red oranı,
- malzeme bazlı uygunsuzluk,
- lot bazlı kalite geçmişi,
- ortalama analiz süresi,
- bekleyen kalite kayıtları,
- bloke stok miktarı,
- parametre bazlı trend analizi.

Bu raporlar operasyonel kalite yönetiminin yanı sıra satınalma performansını da ölçmek için kullanılabilir.

---

## 13. Tasarım Prensipleri

1. Kalite sonucu lot ile ilişkilendirilebiliyorsa lot ilişkisi zorunlu tutulmalıdır.
2. Kabul/red kararları serbest metin yerine kodlanmış statülerle yönetilmelidir.
3. Kalite sonucu oluşmamış stokların kullanımı kontrol edilmelidir.
4. Dış sistem entegrasyonlarında idempotency uygulanmalıdır.
5. Master veri sahibi sistem açıkça belirlenmelidir.
6. Kalite kayıtları silinmek yerine mümkün olduğunca durum değişikliği ve iz kaydı ile yönetilmelidir.
7. Audit trail kritik kabul edilmelidir.

---

## 14. Sonuç

Logo ERP içindeki kalite yönetimi, üretimden bağımsız bir laboratuvar kayıt sistemi değildir. Kalite sonucu stokun kullanılabilirliğini ve üretim akışını etkileyen temel operasyonel veridir.

Sağlam bir mimaride kalite; malzeme, stok hareketi, seri/lot, üretim emri ve gerektiğinde MES/LIMS sistemleri ile açık ilişkiler üzerinden yönetilir.

Bir sonraki bölümde üretim ve stok süreçlerinin finansal sonucunu oluşturan **maliyetlendirme mimarisi** ele alınacaktır.
