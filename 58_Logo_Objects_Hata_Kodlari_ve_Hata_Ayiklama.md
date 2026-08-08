# 58 — Logo Objects Hata Kodları ve Hata Ayıklama

## Amaç

Bu bölüm Logo Objects işlemlerinde başarısız kayıtların sistematik biçimde teşhis edilmesini ele alır. Amaç yalnızca hata mesajını göstermek değil; hatayı tekrar üretilebilir, loglanabilir ve sınıflandırılabilir hale getirmektir.

## Hata katmanları

Logo Objects entegrasyonlarında hata en az dört farklı katmanda ortaya çıkabilir:

1. Uygulama / bağlantı hatası
2. Nesne oluşturma veya login hatası
3. Business rule / validation hatası
4. Veritabanı / transaction kaynaklı hata

## Temel hata yakalama kalıbı

```text
İşlem Başlat
  ↓
Parametreleri logla
  ↓
IData / IQuery işlemini çalıştır
  ↓
Başarılı mı?
 ├─ Evet -> LOGICALREF ve sonuç logu
 └─ Hayır -> ErrorCode + ErrorMessage + context
```

## Hata kaydında tutulması gereken bağlam

- CompanyId
- FirmNr
- PeriodNr
- UserId
- DataObjectType
- işlem tipi: INSERT / UPDATE / DELETE / QUERY
- document number
- source system ID
- line number
- error code
- error description
- timestamp

## Validation hataları

Bir fiş `Post()` aşamasında başarısız oluyorsa ilk bakılması gerekenler:

- zorunlu üst alanlar
- cari referansı
- malzeme referansı
- birim referansı
- miktar / fiyat
- ambar bilgisi
- satır tipi
- TRCODE uyumu
- seri/lot zorunluluğu
- proje veya merkez zorunluluğu

## Hata mesajını kaybetmemek

Kullanıcıya yalnızca `Kayıt başarısız` mesajı gösterilmemelidir. Logo Objects'in döndürdüğü hata kodu ve açıklama loglanmalı, mümkünse teknik kullanıcı ekranında gösterilmelidir.

## Tekrar deneme politikası

Her hata retry edilmemelidir.

### Retry edilebilecek örnekler

- geçici bağlantı problemi
- timeout
- servis erişim problemi

### Retry edilmemesi gereken örnekler

- geçersiz cari kodu
- zorunlu alan eksikliği
- yanlış DataObjectType
- hatalı birim
- business rule ihlali

## Saha yaklaşımı

Bir hata analiz edilirken yalnızca uygulama kodu değil, aynı işlemin Logo arayüzünden manuel olarak yapılabilir olup olmadığı da test edilmelidir. Manuel işlem de başarısızsa sorun entegrasyon kodundan önce Logo business rule veya master data tarafındadır.

## Bilgi güven seviyesi

Bu bölümdeki hata ayıklama yaklaşımı **mimari ve saha pratiği olarak doğrulanmıştır**. Spesifik hata kodlarının numerik karşılıkları kullanılan Logo Objects sürümünden doğrulanmalıdır.
