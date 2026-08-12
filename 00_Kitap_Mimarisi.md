# Logo ERP Engineering Handbook — Kitap Mimarisi

Bu repository artık yalnızca Logo Objects API kullanımını anlatan bir kitap değildir. İçerik; Logo ERP mimarisi, Logo Objects, ProductionApplication, SQL Server, üretim, seri/lot, kalite, maliyetlendirme, finans, entegrasyon, operasyon ve güvenlik konularını kapsayan geniş bir mühendislik el kitabına dönüşmüştür.

Bu nedenle ana çalışma adı:

# Logo ERP Engineering Handbook

Alt başlık:

**Logo ERP, Logo Objects, ProductionApplication, SQL Server ve Kurumsal Entegrasyonlar için Teknik Referans**

## Bölüm Grupları

1. **Logo ERP Core ve Veri Modeli**
2. **Logo Objects SDK ve Uygulama Geliştirme**
3. **Üretim, Seri/Lot, Kalite ve Maliyetlendirme**
4. **SQL Server, Veri Tabanı ve Performans**
5. **Entegrasyon Mimarileri ve Servisler**
6. **Finans, Muhasebe ve Elektronik Belgeler**
7. **Operasyon, Güvenlik, Backup ve DR**
8. **Geliştirici Kılavuzu ve Referans Uygulama Mimarisi**

## Neden Dosyalar Fiziksel Olarak Taşınmıyor?

Mevcut 99 bölüm repository içinde uzun süredir doğrudan bağlantılarla kullanılıyor. Dosyaları klasörlere taşımak eski bağlantıları, commit referanslarını ve dışarıdan verilen linkleri kırabilir.

Bu nedenle mevcut bölüm dosyaları kök dizinde korunur. Yeni yapı, bölüm indeks dosyaları ve README üzerinden mantıksal olarak ayrılır.

Yeni bölümler de numaralandırmaya devam eder; ancak hangi ana kitap bölümüne ait oldukları README ve ilgili PART dosyalarında belirtilir.

## Temel Yayın Kuralı

Her yeni teknik konu için:

```text
Gerçek problem / ihtiyaç
    ↓
Doğrulanmış teknik analiz
    ↓
Genelleştirilebilir bilgi
    ↓
Uygun ana bölüm altında yeni chapter
    ↓
Ayrı Git commit
    ↓
README ve PART indeks güncellemesi
```

## Bilgi Güven Seviyeleri

- **Doğrulanmış Bilgi:** Resmi dokümantasyon, çalışan kod veya tekrar test ile doğrulanmış bilgi.
- **Saha Gözlemi:** Gerçek Logo ortamında gözlemlenmiş davranış.
- **Mimari Öneri:** Sürdürülebilir çözüm için önerilen mühendislik yaklaşımı.
- **Sürüm Bağımlı:** Logo / Objects sürümüne göre değişebilecek bilgi.
- **Kontrol Edilmeli:** Henüz kesin doğrulanmamış bilgi.

> Amaç sadece bilgi toplamak değil; gerçek Logo ERP projelerinde tekrar kullanılabilir bir mühendislik referansı oluşturmaktır.
