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
49. [Gerçek Hata ve Vaka Kataloğu](49_Gercek_Hata_ve_Vaka_KATALOGu.md)
50. [PRCLIST Fiyat Kartları ve Fiyat Mantığı](50_PRCLIST_Fiyat_Kartlari_ve_Fiyat_Mantigi.md)
51. [PAYPLANS Ödeme Planı Mantığı](51_PAYPLANS_Odeme_Plani_Mantigi.md)
52. [L_CAPIFIRM / L_CAPIPERIOD Firma ve Dönem Yapısı](52_L_CAPIFIRM_L_CAPIPERIOD_Firma_Donem_Yapisi.md)
53. [İşyeri, Fabrika ve Ambar Organizasyon Yapısı](53_Isyeri_Fabrika_Ambar_Organizasyon_Yapisi.md)
54. [PROJECT Proje Kartları ve PROJECTREF](54_PROJECT_Proje_Kartlari_ve_ProjectRef.md)
55. [Üretim Emri, İş Emri ve Operasyon İlişkileri](55_Uretim_Emri_Is_Emri_Operasyon_Iliskileri.md)
56. [Logo Objects Tam CRUD Örnekleri](56_Logo_Objects_Tam_CRUD_Ornekleri.md)
57. [XML Import / Export ve Veri Aktarımı](57_XML_Import_Export_ve_Veri_Aktarimi.md)
58. [Logo Objects Hata Kodları ve Hata Ayıklama](58_Logo_Objects_Hata_Kodlari_ve_Hata_Ayiklama.md)
59. [Dispatch / Invoice Transaction İlişkileri](59_Dispatch_Invoice_Transaction_Iliskileri.md)
60. [Döviz Alanları ve Kur Mantığı](60_Doviz_Alanlari_ve_Kur_Mantigi.md)
61. [KDV, İstisna ve Muafiyet Alanları](61_KDV_Istisna_ve_Muafiyet_Alanlari.md)
62. [Cari Yaşlandırma ve FIFO Kapama Mantığı](62_Cari_Yaslandirma_ve_FIFO_Kapama_Mantigi.md)
63. [Maliyet Alanları ve Maliyetlendirme Verisi](63_Maliyet_Alanlari_ve_Maliyetlendirme_Verisi.md)
64. [ProductionApplication Operasyon Kod Örnekleri](64_ProductionApplication_Operasyon_Kod_Ornekleri.md)
65. [Logo Objects REST Service Mimarisi](65_Logo_Objects_REST_Service_Mimarisi.md)
66. [COM Yaşam Döngüsü ve Kaynak Yönetimi](66_COM_Yasam_Dongusu_ve_Kaynak_Yonetimi.md)
67. [Çoklu Firma / Dönem Servis Mimarisi](67_Coklu_Firma_Donem_Servis_Mimarisi.md)
68. [Thread, Concurrency ve Session İzolasyonu](68_Thread_Concurrency_ve_Session_Izolasyonu.md)
69. [Batch İşlemler, Retry ve Backoff](69_Batch_Islemler_Retry_ve_Backoff.md)
70. [Entegrasyon Log, Queue ve Reconciliation Modeli](70_Entegrasyon_Log_Queue_ve_Reconciliation_Modeli.md)
71. [MES → Logo Uçtan Uca Referans Mimari](71_MES_Logo_Uctan_Uca_Referans_Mimari.md)
72. [LIMS ve WMS Entegrasyon Mimarisi](72_LIMS_WMS_Entegrasyon_Mimarisi.md)
73. [Outbox / Inbox Pattern ve Event-Driven Entegrasyon](73_Outbox_Inbox_Pattern_ve_Event_Driven_Entegrasyon.md)
74. [e-Fatura ve e-İrsaliye Entegrasyon Bağlantıları](74_EFatura_EIrsaliye_Entegrasyon_Baglantilari.md)
75. [Muhasebe Entegrasyon Hataları ve Kontrol Listesi](75_Muhasebe_Entegrasyon_Hatalari_ve_Kontrol_Listesi.md)
76. [Scheduled Job ve Background Worker Mimarisi](76_Scheduled_Job_ve_Background_Worker_Mimarisi.md)
77. [Monitoring, Observability ve Operasyon Runbook](77_Monitoring_Observability_ve_Operasyon_Runbook.md)

## Kapsam

Bu bilgi tabanı zaman içinde özellikle aşağıdaki alanlarda derinleştirilecektir:

- `DataObjectType` gerçek enum sözlüğü ve doğrulanmış örnekler
- Kart ve fiş bazında çalışan Logo Objects kod örnekleri
- `IData` field sözlüğü ve tam CRUD kalıpları
- XML import/export ve dış sistem veri eşleştirme kalıpları
- `ProductionApplication` gerçek kullanım ve operasyon örnekleri
- Üretim emri, iş emri ve operasyon ilişkileri
- Seri/lot tablo ve nesne ilişkileri
- Logo tablo/veri sözlüğü
- TRCODE, LINETYPE ve IOCODE referans tabloları
- Birim dönüşümü ve UINFO kullanım örnekleri
- Cari hareket, muhasebe fişi ve belge bağlantı haritaları
- STFICHE, STLINE, INVOICE, CLCARD, ITEMS, ORFICHE, ORFLINE ve PRODORD alan sözlükleri
- PRCLIST fiyat ve PAYPLANS ödeme planı mantığı
- Firma/dönem ve işyeri/fabrika/ambar organizasyon yapısı
- PROJECT / PROJECTREF kullanımı
- Döviz, KDV istisna/muafiyet ve finansal alanlar
- Cari yaşlandırma ve FIFO kapama mantığı
- Üretim maliyet ve maliyet sapma analizleri
- Stok envanter ve yaşlandırma sorguları
- SQL performans vaka analizleri
- Logo Objects REST servisleri ve COM yaşam döngüsü
- Çoklu firma/dönem servis mimarileri
- Thread/concurrency ve session izolasyonu
- Batch, retry, backoff ve dead-letter yaklaşımları
- Entegrasyon log, queue ve reconciliation modelleri
- MES / LIMS / WMS entegrasyon örnekleri
- Outbox/Inbox ve event-driven entegrasyon tasarımları
- e-Fatura/e-İrsaliye bağlantıları ve elektronik belge reconciliation
- Muhasebe entegrasyon hata kontrolleri
- Scheduled job ve background worker mimarileri
- Monitoring, observability ve operasyon runbook yaklaşımı
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
