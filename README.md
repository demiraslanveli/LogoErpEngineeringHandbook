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

## Kapsam

Bu bilgi tabanı zaman içinde özellikle aşağıdaki alanlarda derinleştirilecektir:

- Logo Objects `DataObjectType` referansları
- Kart ve fiş bazında çalışan kod örnekleri
- `IData` field sözlüğü
- `ProductionApplication` gerçek kullanım örnekleri
- Üretim emri, iş emri ve operasyon ilişkileri
- Seri/lot tablo ve nesne ilişkileri
- Logo tablo/veri sözlüğü
- TRCODE ve LINETYPE referans tabloları
- SQL performans vaka analizleri
- MES / LIMS / WMS entegrasyon örnekleri
- Gerçek hata mesajları ve çözüm notları
- Sürüm bazlı davranış farkları

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
