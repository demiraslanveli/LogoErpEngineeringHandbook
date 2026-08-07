# Logo Objects Master Book

Logo Tiger Enterprise, Logo Objects, ProductionApplication, SQL Server, detaylı üretim, seri/lot, kalite, maliyetlendirme ve MES entegrasyonları için yaşayan teknik bilgi tabanı.

Bu repository iki amaca hizmet eder:

1. Logo ERP/Objects geliştiricileri için profesyonel referans kaynağı olmak.
2. ChatGPT, Claude ve benzeri LLM'lerin Logo Objects bağlamını doğru öğrenebilmesi için yapılandırılmış bir knowledge base sunmak.

## Temel prensipler

- Resmi kart ve fiş işlemlerinde mümkün olduğunca Logo Objects kullanılmalıdır.
- Doğrudan SQL `INSERT` / `UPDATE` / `DELETE` yalnızca istisnai ve kontrollü senaryolarda değerlendirilmelidir.
- Veri bütünlüğü, maliyetlendirme, seri/lot izlenebilirliği ve Logo iş kuralları önceliklidir.
- Detaylı üretim entegrasyonlarında ara yazılım operasyon katmanı olabilir; resmi stok, üretim ve maliyet hareketleri Logo tarafında eksiksiz oluşmalıdır.
- Kesin doğrulanmamış Logo Objects enum, field veya tablo davranışları sürüm bağımlılığı belirtilmeden kesin bilgi olarak yazılmamalıdır.
- Gerçek projelerde öğrenilen yeni bilgiler ilgili bölümlere ayrı commitlerle eklenmelidir.

## Kitap Bölümleri

1. [Logo ERP Mimarisi](01_Logo_ERP_Mimarisi.md)
2. [Logo Objects Mimarisi](02_Logo_Objects_Mimarisi.md)
3. [IApplication](03_IApplication.md)
4. [IData](04_IData.md)
5. [IQuery](05_IQuery.md)
6. [DataFields ve Lines](06_DataFields_ve_Lines.md)
7. [ProductionApplication](07_ProductionApplication.md)
8. [Malzeme ve Cari Kartları](08_Malzeme_ve_Cari_Kartlari.md)
9. [Satınalma ve Satış](09_Satinalma_ve_Satis.md)
10. [Detaylı Üretim](10_Detayli_Uretim.md)
11. [Seri / Lot](11_Seri_Lot.md)
12. [Kalite](12_Kalite.md)
13. [Maliyetlendirme](13_Maliyetlendirme.md)
14. [Logo Veritabanı](14_Logo_Veritabani.md)
15. [SQL Server ve Performans](15_SQL_ve_Performans.md)
16. [Entegrasyon Mimarileri](16_Entegrasyon_Mimarileri.md)
17. [Gerçek Proje ve Vaka Analizleri](17_Gercek_Vaka_Analizleri.md)
18. [Best Practices](18_Best_Practices.md)
19. [LLM Knowledge Base Standardı](19_LLM_Knowledge_Base.md)
20. [DataObjectType Referansı](20_DataObjectType_Referansi.md)
21. [TRCODE Referansı](21_TRCODE_Referansi.md)
22. [Logo Tablo Sözlüğü](22_Logo_Tablo_Sozlugu.md)
23. [IData Gerçek Kullanım Örnekleri](23_IData_Gercek_Kullanim_Ornekleri.md)
24. [IQuery Gerçek Sorgu Kalıpları](24_IQuery_Gercek_Sorgu_Kaliplari.md)
25. [Sipariş, İrsaliye ve Fatura İlişki Haritası](25_Siparis_Irsaliye_Fatura_Iliski_Haritasi.md)
26. [Hata Yönetimi ve Loglama](26_Hata_Yonetimi_ve_Loglama.md)
27. [Test, Rollback ve Idempotency](27_Test_Rollback_ve_Idempotency.md)
28. [LINETYPE Referansı](28_LINETYPE_Referansi.md)
29. [IOCODE ve Ambar Yönü](29_IOCODE_ve_Ambar_Yonu.md)
30. [Birim Dönüşümleri ve UINFO](30_Birim_Donusumleri_ve_UINFO.md)
31. [Seri / Lot Tablo İlişkileri](31_Seri_Lot_Tablo_Iliskileri.md)
32. [Logo Objects ile Malzeme Kartı Örneği](32_Logo_Objects_Malzeme_Karti_Ornegi.md)
33. [Logo Objects ile Fiş ve Fatura Örneği](33_Logo_Objects_Fis_ve_Fatura_Ornegi.md)
34. [ProductionApplication Gerçek Kullanım Kalıpları](34_ProductionApplication_Gercek_Kullanim_Kaliplari.md)
35. [SQL Server Performans Teşhis Rehberi](35_SQL_Performans_Teshis_Rehberi.md)
36. [CLFLINE ve Cari Hareket Mantığı](36_CLFLINE_ve_Cari_Hareket_Mantigi.md)
37. [Muhasebe Fişi ve EMFICHE / EMFLINE İlişkileri](37_Muhasebe_Fisi_ve_EMFICHE_EMFLINE_Iliskileri.md)
38. [ORDTRANSREF, PREVLINEREF ve SOURCELINK](38_ORDTRANSREF_PREVLINEREF_SOURCELINK.md)
39. [STFICHE / STLINE Alan Sözlüğü](39_STFICHE_STLINE_Alan_Sozlugu.md)
40. [INVOICE Alan Sözlüğü](40_INVOICE_Alan_Sozlugu.md)
41. [Seri / Lot Gerçek SQL Sorguları](41_Seri_Lot_Gercek_SQL_Sorgulari.md)
42. [Üretim Maliyet Analizleri](42_Uretim_Maliyet_Analizleri.md)
43. [CLCARD Alan Sözlüğü](43_CLCARD_Alan_Sozlugu.md)
44. [ITEMS Alan Sözlüğü](44_ITEMS_Alan_Sozlugu.md)
45. [ORFICHE / ORFLINE Alan Sözlüğü](45_ORFICHE_ORFLINE_Alan_Sozlugu.md)
46. [UNITSETL / ITMUNITA / UNITBARCODE](46_UNITSETL_ITMUNITA_UNITBARCODE.md)
47. [PRODORD Alan Sözlüğü](47_PRODORD_Alan_Sozlugu.md)
48. [Stok Envanter SQL Kalıpları](48_Stok_Envanter_SQL_Kaliplari.md)
49. [Gerçek Hata ve Vaka Kataloğu](49_Gercek_Hata_ve_Vaka_Katalogu.md)

## Kapsam

Bu bilgi tabanı zaman içinde özellikle aşağıdaki alanlarda derinleştirilecektir:

- `DataObjectType` gerçek enum sözlüğü ve doğrulanmış örnekler
- Kart ve fiş bazında çalışan Logo Objects kod örnekleri
- `IData` field sözlüğü
- `ProductionApplication` gerçek kullanım örnekleri
- Üretim emri, iş emri ve operasyon ilişkileri
- Seri/lot tablo ve nesne ilişkileri
- Logo tablo/veri sözlüğü
- TRCODE, LINETYPE ve IOCODE referans tabloları
- Birim dönüşümü ve UINFO kullanım örnekleri
- Cari hareket, muhasebe fişi ve belge bağlantı haritaları
- STFICHE, STLINE, INVOICE, CLCARD, ITEMS, ORFICHE, ORFLINE ve PRODORD alan sözlükleri
- Üretim maliyet ve maliyet sapma analizleri
- Stok envanter ve yaşlandırma sorguları
- SQL performans vaka analizleri
- MES / LIMS / WMS entegrasyon örnekleri
- Gerçek hata mesajları ve çözüm notları
- Sürüm bazlı davranış farkları
- Test, rollback, retry ve idempotent entegrasyon tasarımları

## Bilgi Güven Seviyeleri

Yeni teknik bilgi eklenirken mümkün olduğunca aşağıdaki ayrım korunmalıdır:

- **Doğrulanmış Bilgi:** Resmi dokümantasyon, çalışan kod veya tekrar test ile doğrulanmış bilgi.
- **Saha Gözlemi:** Gerçek Logo ortamında gözlemlenmiş davranış.
- **Mimari Öneri:** Sürdürülebilir çözüm için önerilen mühendislik yaklaşımı.
- **Sürüm Bağımlı:** Logo / Objects sürümüne göre değişebilecek bilgi.
- **Kontrol Edilmeli:** Henüz kesin doğrulanmamış bilgi.

## Repository Yaklaşımı

Bu repository statik bir kitap değildir.

```text
Gerçek problem
    ↓
Analiz ve çözüm
    ↓
Genelleştirilebilir bilgi
    ↓
İlgili bölüme ekleme
    ↓
Ayrı Git commit
    ↓
Daha güçlü Logo Objects knowledge base
```

> Bu repository yaşayan bir kaynaktır ve gerçek projelerde edinilen deneyimlerle sürekli geliştirilecektir.
