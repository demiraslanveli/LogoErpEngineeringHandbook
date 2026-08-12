# Logo ERP Engineering Handbook

**Logo ERP, Logo Objects, ProductionApplication, SQL Server ve Kurumsal Entegrasyonlar için Teknik Referans**

Bu repository artık yalnızca Logo Objects kullanımını anlatan bir kitap değildir. İçerik; Logo ERP veri modeli, SDK geliştirme, detaylı üretim, seri/lot, kalite, maliyetlendirme, SQL Server, finans, entegrasyon, operasyon ve güvenlik başlıklarını kapsayan yaşayan bir mühendislik el kitabıdır.

## Kitap Mimarisi

Genel yapı ve yayın prensipleri:

- [Kitap Mimarisi](00_Kitap_Mimarisi.md)

## Ana Bölümler

1. [Logo ERP Core ve Veri Modeli](PART_01_Logo_ERP_Core_ve_Veri_Modeli.md)
2. [Logo Objects SDK ve Uygulama Geliştirme](PART_02_Logo_Objects_SDK_ve_Uygulama_Gelistirme.md)
3. [Üretim, Seri/Lot, Kalite ve Maliyet](PART_03_Uretim_SeriLot_Kalite_ve_Maliyet.md)
4. [SQL Server, Veritabanı ve Performans](PART_04_SQL_Server_Veritabanı_ve_Performans.md)
5. [Entegrasyon Mimarileri ve Servisler](PART_05_Entegrasyon_Mimarileri_ve_Servisler.md)
6. [Finans, Muhasebe ve Elektronik Belgeler](PART_06_Finans_Muhasebe_ve_EBelge.md)
7. [Operasyon, Güvenlik, Backup ve DR](PART_07_Operasyon_Guvenlik_Backup_ve_DR.md)
8. [Geliştirici Kılavuzu ve Referans Uygulama](PART_08_Gelistirici_Kilavuzu_ve_Referans_Uygulama.md)

## Güncel Uygulamalı Seri

100. bölümden itibaren kitap, mevcut teorik ve saha bilgisini çalışan bir referans uygulama mimarisine dönüştürmektedir.

- [100 — Referans .NET Çözüm Mimarisi](100_Referans_DotNet_Cozum_Mimarisi.md)

## Temel Prensipler

- Resmi kart ve fiş işlemlerinde mümkün olduğunca Logo Objects kullanılmalıdır.
- Doğrudan SQL `INSERT` / `UPDATE` / `DELETE` yalnızca istisnai ve kontrollü senaryolarda değerlendirilmelidir.
- Logo veritabanı, yalnızca bağımsız tablolar bütünü değil; ERP iş kurallarının ürettiği ilişkili veri modelidir.
- Veri bütünlüğü, maliyetlendirme, seri/lot izlenebilirliği ve muhasebe ilişkileri birlikte ele alınmalıdır.
- Detaylı üretim entegrasyonlarında dış sistem operasyon katmanı olabilir; resmi ERP hareketleri Logo tarafında eksiksiz oluşmalıdır.
- Entegrasyonlarda idempotency, retry, reconciliation, correlation id ve loglama standart kabul edilir.
- Kesin doğrulanmamış Logo enum, field veya tablo davranışı sürüm bağımlılığı belirtilmeden kesin bilgi olarak yazılmaz.

## Bilgi Güven Seviyeleri

- **Doğrulanmış Bilgi:** Resmi dokümantasyon, çalışan kod veya tekrar test ile doğrulanmış bilgi.
- **Saha Gözlemi:** Gerçek Logo ortamında gözlemlenmiş davranış.
- **Mimari Öneri:** Sürdürülebilir çözüm için önerilen mühendislik yaklaşımı.
- **Sürüm Bağımlı:** Logo / Objects sürümüne göre değişebilecek bilgi.
- **Kontrol Edilmeli:** Henüz kesin doğrulanmamış bilgi.

## Repository Yaklaşımı

Mevcut 1–99 bölüm dosyaları bağlantı geçmişini korumak için kök dizinde bırakılmıştır. İçerik artık `PART_01`–`PART_08` indeksleri üzerinden alan bazında okunur.

```text
Gerçek problem
    ↓
Analiz ve doğrulama
    ↓
Genelleştirilebilir teknik bilgi
    ↓
Uygun ana bölüm
    ↓
Ayrı chapter
    ↓
Ayrı Git commit
    ↓
Yaşayan mühendislik el kitabı
```

> Amaç sadece Logo Objects dokümantasyonu oluşturmak değil; Logo ERP üzerinde geliştirme, entegrasyon ve operasyon yapan ekipler için tekrar kullanılabilir bir mühendislik referansı oluşturmaktır.
