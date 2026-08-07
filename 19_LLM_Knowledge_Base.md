# 19 — LLM Knowledge Base Standardı

## 1. Bölümün Amacı

Bu bölüm, `LogoObjectsMasterBook` repository'sinin ChatGPT, Claude ve benzeri büyük dil modelleri tarafından güvenilir bir teknik bilgi tabanı olarak kullanılabilmesi için içerik standardını tanımlar.

Amaç yalnızca insanlar için okunabilir dokümantasyon üretmek değil; yapay zekâ araçlarının Logo ERP, Logo Objects, SQL Server ve üretim entegrasyonları hakkında bağlamı doğru kurmasını sağlamaktır.

> Bir bilgi tabanı LLM için faydalı olacaksa yalnızca doğru değil, aynı zamanda açık, bağlamlı, sürüm farklarına duyarlı ve kaynak türü belli olmalıdır.

---

## 2. Bilgi Türleri Ayrılmalıdır

Repository içindeki teknik bilgiler mümkün olduğunca şu kategorilerden biriyle düşünülmelidir:

### 2.1 Doğrulanmış Bilgi

Resmi dokümantasyon, çalışan kod veya doğrudan test ile teyit edilmiş bilgi.

Örnek:

```text
IData, Logo Objects içinde kart ve fiş işlemleri için kullanılan temel veri nesnesidir.
```

### 2.2 Saha Gözlemi

Gerçek müşteri/veritabanı üzerinde gözlemlenmiş davranış.

### 2.3 Mimari Öneri

Logo'nun zorunlu kıldığı bir kural değil, sürdürülebilir entegrasyon için önerilen yaklaşım.

### 2.4 Sürüm Bağımlı Bilgi

Logo Tiger/Tiger Wings sürümüne göre değişebilecek alan, enum veya servis davranışı.

### 2.5 Kontrol Edilmesi Gereken Bilgi

Kesin doğrulanmamış, araştırma veya test gerektiren bilgi.

---

## 3. Kesin Olmayan Bilgi Kesinmiş Gibi Yazılmamalıdır

LLM'lerin en büyük risklerinden biri eksik bağlamdan makul görünen fakat yanlış teknik detay üretmesidir.

Örneğin bir `DataObjectType` enum değeri doğrulanmamışsa:

```text
DataObjectType = 42
```

şeklinde kesin bilgi verilmemelidir.

Yerine:

```text
DataObjectType değeri kullanılan Logo Objects sürümünün enum tanımından doğrulanmalıdır.
```

yazılmalıdır.

---

## 4. Tablo Alanları Bağlamıyla Yazılmalıdır

Şu ifade yetersizdir:

```text
TRCODE 8 satış faturasıdır.
```

Daha doğru dokümantasyon:

```text
TRCODE değerleri tablo/modül bağlamına göre yorumlanmalıdır. Bir kodun anlamı belirtilirken ilgili tablo ve belge türü de yazılmalıdır.
```

LLM'nin farklı tablolardaki kodları birbirine karıştırması bu şekilde azaltılır.

---

## 5. Firma ve Dönem Placeholder Standardı

Örnek SQL'lerde gerçek firma numarasına bağımlılığı azaltmak için mümkün olduğunca açıklama eklenmelidir.

Örnek:

```sql
SELECT *
FROM LG_040_01_STLINE;
```

kullanılıyorsa:

```text
040 = örnek firma numarası
01  = örnek dönem numarası
```

açıklaması verilmelidir.

Genel ifade:

```text
LG_{FirmaNo}_{DonemNo}_STLINE
```

şeklinde gösterilebilir.

---

## 6. Kod Örnekleri Minimum Ama Çalışma Mantığını Gösteren Yapıda Olmalıdır

Kod örnekleri:

- gereksiz UI detayından arındırılmalı,
- hata kontrolü göstermeli,
- kullanılan nesnenin amacını açıklamalı,
- kritik alanları yorumlamalıdır.

Örnek kavramsal akış:

```csharp
var data = app.NewDataObject(dataObjectType);

if (data.New())
{
    // zorunlu alanlar
    // satırlar
    // validation

    if (!data.Post())
    {
        // hata detaylarını logla
    }
}
```

Enum ve field isimleri sürüme bağlıysa ayrıca not düşülmelidir.

---

## 7. SQL Örneklerinde Veri Güvenliği

LLM'nin ürettiği SQL doğrudan production ortamında çalıştırılabileceği için özellikle `UPDATE`, `DELETE` ve `INSERT` örnekleri güvenli tasarlanmalıdır.

Tercih edilen yapı:

```sql
BEGIN TRANSACTION;

-- Önce kontrol
SELECT ...
WHERE ...;

-- Gerekirse UPDATE
-- UPDATE ...

ROLLBACK;
```

veya açık `@TestModu` yaklaşımıdır.

---

## 8. SQL ile Logo Objects Arasındaki Sınır Açık Yazılmalıdır

LLM'nin "tabloyu biliyorum, o halde INSERT yapabilirim" sonucuna gitmesini engellemek için repository genel prensibi tekrar edilmelidir:

```text
SQL → okuma, raporlama, analiz, kontrollü bakım
Logo Objects → resmi kart ve fiş işlemleri
```

İstisnalar ayrıca gerekçelendirilmelidir.

---

## 9. İlişki Zinciri Yazılmalıdır

Logo'da tek bir belge birçok tabloya dokunur.

LLM'nin yalnızca görünen tabloyu değiştirmemesi için ilişkiler şema olarak yazılmalıdır.

Örnek:

```text
INVOICE
   ├── STLINE
   ├── STFICHE
   ├── CLFLINE
   └── Muhasebe bağlantıları
```

Benzer şemalar üretim ve seri/lot için de kullanılmalıdır.

---

## 10. Anti-Pattern'ler Açıkça Belgelenmelidir

LLM için yalnızca "doğru yöntem" değil, yanlış yöntem de önemlidir.

Örnek anti-pattern:

```text
❌ Fatura tarihini yalnızca INVOICE tablosunda update etmek
```

Doğru yaklaşım:

```text
✅ Bağlı stok, cari ve muhasebe hareketlerini birlikte analiz etmek
```

---

## 11. Gerçek Hata Mesajları Değerlidir

Gerçek hata mesajları knowledge base için yüksek değer taşır.

Örnek format:

```text
Error:
Form field type does not match with table field type.

Context:
frmSatisSiparisi / LComboBox1

Meaning:
Form alanının tipi, bağlı tablo alanının veri tipiyle uyumsuz olabilir.
```

Bu yapı LLM'nin hata mesajından bağlam çıkarmasını kolaylaştırır.

---

## 12. Vaka Analizi Şablonu

Yeni gerçek vaka repository'ye eklenirken şu şablon kullanılabilir:

```markdown
# Vaka — Başlık

## Belirti

## Ortam

## İlgili Tablolar / Nesneler

## Analiz

## Kök Neden

## Çözüm

## Kontrol Sorgusu

## Önleyici Tedbir
```

---

## 13. Kod Parçası Şablonu

Yeni Logo Objects örneği için:

```markdown
## Amaç

## Gereksinimler

## Kullanılan Nesneler

## Kod

## Alan Açıklamaları

## Hata Yönetimi

## Sürüm Notu
```

---

## 14. SQL Sorgusu Şablonu

```markdown
## Amaç

## Firma / Dönem

## Parametreler

## SQL

## Sonuç Kolonları

## Performans Notu

## Veri Güvenliği Notu
```

---

## 15. Sürüm Bilgisi

Logo davranışı sürüme bağlıysa bölüm veya örnek içinde sürüm bilgisi tutulmalıdır.

Önerilen alanlar:

```text
Logo Product
Logo Version
Objects Version
SQL Server Version
Test Date
```

Böylece eski bilginin yeni sürüme yanlış uygulanma riski azalır.

---

## 16. Kaynak Önceliği

Bilgi çeliştiğinde önerilen güven sırası:

```text
1. Resmi Logo dokümantasyonu
2. Çalışan ve tekrar test edilmiş kod
3. Gerçek veritabanı gözlemi
4. Güvenilir saha notu
5. Varsayım / yorum
```

Varsayım mutlaka işaretlenmelidir.

---

## 17. LLM İçin Terminoloji Tutarlılığı

Aynı kavram farklı bölümlerde farklı adlarla yazılmamalıdır.

Önerilen temel terminoloji:

```text
Firma
Dönem
Malzeme
Cari Hesap
Stok Fişi
Stok Satırı
Fatura
Üretim Emri
İş Emri
Operasyon
Seri/Lot
Kalite Kontrol
Maliyetlendirme
Logo Objects
ProductionApplication
```

---

## 18. Hassas Bilgiler Repository'ye Eklenmemelidir

Knowledge base içine şunlar yazılmamalıdır:

- gerçek SQL kullanıcı şifreleri,
- SMTP şifreleri,
- API key,
- access token,
- gerçek müşteri gizli bilgileri,
- kişisel veri.

Örneklerde placeholder kullanılmalıdır:

```text
<SQL_SERVER>
<DB_NAME>
<USERNAME>
<PASSWORD>
```

---

## 19. Yaşayan Dokümantasyon

Bu repository statik kitap olarak düşünülmemelidir.

Yeni bir saha problemi çözüldüğünde:

```text
Problem çözüldü
↓
Bilgi genelleştirildi
↓
İlgili bölüme eklendi
↓
Ayrı commit atıldı
↓
Knowledge base güncellendi
```

Bu yöntem zamanla kişisel ve kurumsal Logo uzmanlığını kalıcı hale getirir.

---

## 20. Commit Standardı

Her anlamlı dokümantasyon değişikliği ayrı commit olmalıdır.

Örnek mesajlar:

```text
Add IData chapter
Document purchase invoice date update case
Add tempdb performance notes
Clarify TRCODE context rules
```

Böylece bilginin tarihsel gelişimi Git üzerinden izlenebilir.

---

## 21. Repository'nin LLM Tarafından Kullanım Prensibi

Bir yapay zekâ bu repository'yi kullanırken şu sırayı izlemelidir:

```text
1. İlgili bölümü bul
2. Kesin bilgi / saha gözlemi ayrımını yap
3. Firma ve dönem bağlamını belirle
4. Nesne mi SQL mi kullanılacağını seç
5. Bağlı kayıtları değerlendir
6. Veri güvenliği kontrolü yap
7. Sürüm bağımlılığını kontrol et
8. Sonra çözüm üret
```

---

## 22. Sonuç

`LogoObjectsMasterBook`, yalnızca bir teknik doküman koleksiyonu değil; insan ve yapay zekâ tarafından ortak kullanılabilecek yaşayan bir Logo ERP bilgi tabanı olarak tasarlanmalıdır.

Başarılı bir knowledge base'in temel özellikleri:

```text
Doğru
Bağlamlı
İlişkisel
Sürüm Bilinçli
Güvenli
Örnekli
İzlenebilir
Sürekli Güncellenen
```

Bu standart, repository'ye bundan sonra eklenecek tüm yeni Logo Objects, SQL, üretim ve entegrasyon bilgilerinin temel dokümantasyon çerçevesidir.
