# 18 — Best Practices

## 1. Bölümün Amacı

Bu bölüm, Logo ERP, Logo Objects, SQL Server ve entegrasyon geliştirmelerinde uygulanması önerilen temel mühendislik prensiplerini tek yerde toplar.

Amaç yalnızca çalışan kod üretmek değil; veri bütünlüğünü koruyan, izlenebilir, performanslı ve sürdürülebilir çözümler geliştirmektir.

---

## 2. Kayıt İşlemlerinde Logo Objects Önceliği

Kart ve fiş işlemlerinde mümkün olduğunca:

- `IApplication`,
- `IData`,
- `DataFields`,
- `Lines`,
- `ProductionApplication`

kullanılmalıdır.

Doğrudan SQL `INSERT`, `UPDATE`, `DELETE` yalnızca istisnai ve kontrollü senaryolarda değerlendirilmelidir.

---

## 3. SQL'i Okuma ve Analiz İçin Güçlü Kullan

SQL özellikle:

- raporlama,
- kontrol,
- veri karşılaştırma,
- reconciliation,
- hata analizi,
- performans teşhisi

için uygundur.

Önerilen hibrit yaklaşım:

```text
Okuma / Raporlama → SQL
Resmi Kayıt       → Logo Objects
```

---

## 4. Firma ve Dönemi Sabitleme

Ortak çözüm geliştirirken aşağıdaki gibi sabit tablo adı kullanmaktan kaçının:

```sql
LG_040_01_STLINE
```

Firma ve gerekiyorsa dönem dinamik oluşturulmalıdır.

Ancak dinamik SQL kullanırken parametre güvenliği ve tablo adı doğrulaması yapılmalıdır.

---

## 5. LOGICALREF'i Doğru Yorumla

`LOGICALREF`, ait olduğu tablo bağlamında anlamlıdır.

Aynı sayısal `LOGICALREF` farklı tablolarda farklı kayıtları temsil edebilir.

Her referans alanı hangi tabloya bağlandığıyla birlikte dokümante edilmelidir.

---

## 6. Belgeyi Tek Tablo Olarak Görme

Bir fatura yalnızca `INVOICE`, stok fişi yalnızca `STFICHE` değildir.

Belge zinciri analiz edilmelidir:

```text
Başlık
↓
Satırlar
↓
Cari
↓
Seri/Lot
↓
Muhasebe
↓
Üretim / Sipariş Bağları
```

---

## 7. TRCODE'u Tablo Bağlamıyla Kullan

`TRCODE = 8` gibi bir değer tek başına dokümantasyon değildir.

Her zaman:

```text
Tablo + TRCODE + İşlem Anlamı
```

şeklinde belirtilmelidir.

---

## 8. LINETYPE Filtrelerini Unutma

Stok satırlarında yalnızca malzeme satırları analiz edilecekse uygun `LINETYPE` filtresi kullanılmalıdır.

Aksi halde indirim, hizmet veya başka satır tipleri hesaba yanlış dahil edilebilir.

---

## 9. Veri Değişikliğinde Test Modu

Toplu düzeltme procedure'lerinde mümkünse:

```text
@TestModu = 1
```

benzeri dry-run özelliği bulunmalıdır.

Test modu:

- etkilenecek kayıtları,
- bulunamayan kayıtları,
- olası bağlantı problemlerini

göstermeli; veri değiştirmemelidir.

---

## 10. Transaction Kullan

Birden fazla ilişkili kayıt değiştiriliyorsa işlem atomik olmalıdır.

```sql
BEGIN TRY
    BEGIN TRANSACTION;

    -- işlemler

    COMMIT;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK;

    THROW;
END CATCH;
```

Ancak uzun transaction oluşturulmamalıdır.

---

## 11. Hata Mesajını Kaybetme

Logo Objects veya SQL hata mesajları loglanmalıdır.

Minimum:

```text
Date
Company
Period
Operation
Reference
ErrorCode
ErrorMessage
```

Gerekirse stack trace ve payload da eklenmelidir.

---

## 12. Idempotency Uygula

Entegrasyonlarda aynı işlem iki kez gönderilebilir.

Her kaynak işlem için benzersiz ID kullanın.

```text
ExternalId UNIQUE
```

Bu kontrol özellikle timeout ve retry senaryolarında zorunludur.

---

## 13. Retry Hatalarını Sınıflandır

Her hatayı tekrar denemeyin.

Geçici altyapı hataları retry edilebilir; iş kuralı hataları veri düzeltilmeden retry edilmemelidir.

---

## 14. Mapping'i Kod İçine Gömmeyin

Örneğin:

```text
MES ambar RAW-01 = Logo ambar 4
```

bilgisi kod içinde sabit tutulmamalıdır.

Mapping tablosu veya konfigürasyon kullanılmalıdır.

---

## 15. Seri/Lot Miktarını Doğrula

Seri/lot takipli hareketlerde:

```text
STLINE miktarı = Seri/Lot dağılım toplamı
```

olmalıdır.

Bu kontrol üretim ve transfer işlemlerinde kritik önemdedir.

---

## 16. Negatif Stok Kontrolü

Üretim veya sevkiyat entegrasyonlarında hareket öncesinde negatif stok riski kontrol edilmelidir.

Negatif stok yalnızca stok problemi değil, maliyet problemi de oluşturabilir.

---

## 17. Birim Dönüşümünü Merkezi Yönet

Ana birim, ikinci birim ve satınalma/satış birimleri için dönüşüm mantığı farklı yerlerde tekrar yazılmamalıdır.

Tek bir güvenilir dönüşüm servisi/fonksiyonu kullanılmalıdır.

---

## 18. Son Alış Fiyatını Açık Tanımla

"Son alış fiyatı" raporunda şu kriterler açık olmalıdır:

- hangi belge türleri,
- iadeler dahil mi,
- iptal kayıtları dahil mi,
- hangi birim,
- hangi döviz,
- hangi tarih,
- mevcut belge hariç mi.

---

## 19. Trigger İçinde Ağır İş Yapma

Trigger:

- mail göndermemeli,
- web service çağırmamalı,
- uzun cursor çalıştırmamalı,
- büyük rapor sorgusu çalıştırmamalıdır.

Gerekirse queue tablosuna kısa kayıt bırakmalıdır.

---

## 20. Cursor Yerine Set-Based Yaklaşım

SQL Server'da toplu işlemlerde mümkün olduğunca set-based sorgular kullanılmalıdır.

Cursor yalnızca gerçekten satır bazlı durum gerektiren özel senaryolarda kullanılmalıdır.

---

## 21. SELECT * Kullanmayın

Kalıcı servis ve rapor sorgularında yalnızca gereken alanları seçin.

Bunun faydaları:

- daha az network,
- daha az memory,
- daha okunabilir kod,
- tablo değişikliklerine daha yüksek dayanıklılık.

---

## 22. İndeksi Ölçmeden Eklemeyin

İndeks önerisi şu üç veriyle desteklenmelidir:

- actual execution plan,
- logical reads,
- gerçek workload.

Missing index önerisini otomatik uygulamayın.

---

## 23. NOLOCK'u Varsayılan Yapmayın

`NOLOCK` veri doğruluğu pahasına concurrency sağlayabilir.

Finansal, stok ve maliyet raporlarında yanlış sonuç üretme riski değerlendirilmelidir.

---

## 24. tempdb'yi İzleyin

Özellikle yoğun Logo sistemlerinde:

- file sayısı,
- boyut,
- autogrowth,
- disk latency,
- `PAGELATCH`

izlenmelidir.

---

## 25. SQL Server Belleğini OS'den Bağımsız Düşünmeyin

`max server memory` ayarlanırken:

- Windows,
- antivirüs,
- backup agent,
- Logo servisleri,
- diğer uygulamalar

için bellek bırakılmalıdır.

---

## 26. Performans Probleminde Wait Type ile Başlayın

Önce darboğaz türünü belirleyin:

```text
CPU?
Disk?
Lock?
Memory?
Network?
tempdb?
```

Sonra çözüm uygulayın.

---

## 27. Queue Kullanımını Tercih Edin

Mail, uzun entegrasyon veya harici servis çağrıları ana ERP transaction'ından ayrılmalıdır.

```text
Transaction
↓
Queue
↓
Worker
```

---

## 28. Audit Trail Tutun

Özellikle veri düzeltme ve entegrasyonlarda:

- eski değer,
- yeni değer,
- kullanıcı,
- tarih,
- host,
- program,
- session,
- işlem kaynağı

tutulmalıdır.

---

## 29. Manuel SQL Düzeltmelerini Script Olarak Saklayın

Production ortamında yapılan kritik düzeltme:

- kim tarafından,
- ne zaman,
- neden,
- hangi script ile

yapıldığı belli olmalıdır.

Mümkünse repository'de versiyonlanmalıdır.

---

## 30. Önce SELECT, Sonra UPDATE

Toplu SQL değişikliğinde önce aynı `WHERE` koşuluyla `SELECT` çalıştırın.

Örnek:

```sql
SELECT *
FROM ...
WHERE ...;
```

Sonuç doğrulandıktan sonra update uygulanmalıdır.

---

## 31. Backup Olmadan Kritik Müdahale Yapmayın

Geniş veri düzeltmelerinde geri dönüş planı bulunmalıdır.

Tercihen:

- backup,
- transaction,
- audit table,
- test mode

birlikte kullanılmalıdır.

---

## 32. Kodda Magic Number Kullanımını Azaltın

Örneğin:

```csharp
if (trCode == 8)
```

yerine anlamı ifade eden enum veya sabit kullanın.

```csharp
InvoiceType.SalesInvoice
```

Bu yaklaşım Logo entegrasyon kodunu daha okunabilir yapar.

---

## 33. Logo Objects Nesnelerini Kontrollü Yaşatın

COM tabanlı nesnelerde yaşam döngüsü önemlidir.

- gereksiz nesne üretmeyin,
- kullanılmayan referansları serbest bırakın,
- uzun çalışan servislerde memory kullanımını izleyin,
- login/logout yaşam döngüsünü doğru yönetin.

---

## 34. İş Kuralını UI Koduna Gömmeyin

Form script içinde gereken kısa kontroller yapılabilir; ancak kritik ve tekrar kullanılan iş kuralları servis/procedure/library katmanına taşınmalıdır.

Böylece aynı kontrol farklı ekranlarda tekrar yazılmaz.

---

## 35. Reconciliation Raporı Oluşturun

Entegrasyonlarda günlük veya periyodik kontrol:

```text
Kaynak sistem kayıt sayısı
Logo kayıt sayısı
Fark
Hatalı kayıtlar
Bekleyen kayıtlar
```

raporlanmalıdır.

---

## 36. Dokümantasyonu Kodla Birlikte Güncelleyin

Logo sürümü, tablo davranışı, `DataObjectType`, field adı veya iş kuralı doğrulandığında bilgi repository'ye eklenmelidir.

Bu kitap bu amaçla yaşayan bir knowledge base olarak tutulmaktadır.

---

## 37. Doğrulanmış Bilgi ile Varsayımı Ayırın

Logo Objects dokümantasyonunda kesin doğrulanmamış bir alan veya enum değeri varsa bunu kesin bilgi gibi yazmayın.

Dokümantasyonda mümkünse şu etiketler kullanılabilir:

```text
Doğrulandı
Saha Gözlemi
Sürüm Bağımlı
Kontrol Edilmeli
```

---

## 38. Sonuç

Logo ERP geliştirmelerinde en önemli prensip veri bütünlüğüdür. İkinci sırada izlenebilirlik, üçüncü sırada performans gelir.

Çalışan fakat veri ilişkilerini bozan kod başarılı entegrasyon değildir.

İyi bir Logo çözümü:

```text
Doğru veri
+ Doğru ilişki
+ Doğru iş kuralı
+ İzlenebilir işlem
+ Ölçülebilir performans
= Sürdürülebilir ERP geliştirmesi
```

Bir sonraki bölümde bu repository'nin yapay zekâ araçları tarafından doğru kullanılabilmesi için **LLM Knowledge Base standartları** tanımlanacaktır.
