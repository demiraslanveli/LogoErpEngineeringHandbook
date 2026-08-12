# Part 02 — Logo Objects SDK ve Uygulama Geliştirme

Bu bölüm Logo Objects ile ERP nesnelerine güvenli erişim, CRUD işlemleri, IApplication/IData/IQuery kullanımı, XML aktarımı, hata yönetimi ve SDK tabanlı uygulama geliştirme konularını kapsar.

## İlgili Bölümler

- 02 Logo Objects Mimarisi
- 03 IApplication
- 04 IData
- 05 IQuery
- 06 DataFields ve Lines
- 19 LLM Knowledge Base Standardı
- 23 IData Gerçek Kullanım Örnekleri
- 24 IQuery Gerçek Sorgu Kalıpları
- 26 Hata Yönetimi ve Loglama
- 27 Test, Rollback ve Idempotency
- 32 Logo Objects ile Malzeme Kartı Örneği
- 33 Logo Objects ile Fiş ve Fatura Örneği
- 56 Logo Objects Tam CRUD Örnekleri
- 57 XML Import / Export ve Veri Aktarımı
- 58 Logo Objects Hata Kodları ve Hata Ayıklama
- 65 Logo Objects REST Service Mimarisi
- 66 COM Yaşam Döngüsü ve Kaynak Yönetimi
- 67 Çoklu Firma / Dönem Servis Mimarisi
- 68 Thread, Concurrency ve Session İzolasyonu

## Ana Prensip

```text
ERP nesnesi değiştirilecekse -> Logo Objects / IData
SQL ile okunacaksa         -> IQuery veya kontrollü read-only SQL
```

Doğrudan SQL DML, Logo'nun iş kurallarını ve bağlı kayıt üretimini atlayabileceği için istisnai durum olarak değerlendirilir.
