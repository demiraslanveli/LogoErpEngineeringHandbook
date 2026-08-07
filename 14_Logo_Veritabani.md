# 14 — Logo Veritabanı Mimarisi

## 1. Bölümün Amacı

Bu bölüm, Logo Tiger / Tiger Wings Enterprise veritabanı yapısını geliştirici açısından açıklar. Amaç tablo isimlerini ezberlemek değil; firma, dönem, kart, fiş, satır, referans ve hareket ilişkilerinin nasıl kurgulandığını anlamaktır.

Logo veritabanını doğru anlamak; raporlama, entegrasyon, hata analizi ve performans çalışmaları için zorunludur. Ancak bu bilgi doğrudan veri yazmayı meşrulaştırmaz.

> Logo veritabanını okumak için tablo yapısını bilmek gerekir; kayıt oluşturmak için ise mümkün olduğunca Logo iş katmanı kullanılmalıdır.

---

## 2. Firma ve Dönem Yapısı

Logo tablolarının önemli bir bölümü firma ve dönem numarasını tablo adında taşır.

Genel örnek:

```text
LG_040_ITEMS
LG_040_CLCARD
LG_040_01_STFICHE
LG_040_01_STLINE
LG_040_01_INVOICE
```

Burada:

- `040` firma numarası,
- `01` dönem numarasıdır.

Kart tablolarının önemli bir kısmı firma bazlıdır; hareket tabloları ise çoğunlukla firma + dönem bazlıdır.

---

## 3. Firma Numarasını Dinamik Kullanmak

Entegrasyon veya ortak rapor geliştirirken firma numarası kod içine sabit gömülmemelidir.

Örnek mantık:

```text
FirmaNo = 40
Tablo = LG_040_ITEMS
```

Üç haneli format gerektiğinde sıfırla doldurma uygulanır.

Örnek:

```text
40  → 040
102 → 102
803 → 803
```

Form script veya Logo tarafında `Application.CompanyId` benzeri kaynaklardan aktif firma bilgisi alınabilir.

---

## 4. Dönem Numarası

Hareket tablolarında dönem bilgisi kritik önemdedir.

Örnek:

```text
LG_102_01_STLINE
LG_102_02_STLINE
```

aynı firmanın farklı dönemlerine ait farklı fiziksel tablolardır.

Bu nedenle bir sorguda yalnızca firma numarasını dinamik yapmak yeterli değildir; çözümün dönem stratejisi de açıkça belirlenmelidir.

---

## 5. LOGICALREF

Logo tablolarındaki temel ilişki alanlarından biri `LOGICALREF` alanıdır.

Genel olarak kayıtların Logo içindeki benzersiz referansıdır.

Örneğin:

```text
LG_040_ITEMS.LOGICALREF
LG_040_CLCARD.LOGICALREF
LG_040_01_STFICHE.LOGICALREF
LG_040_01_STLINE.LOGICALREF
```

Ancak farklı tablolardaki `LOGICALREF` değerlerinin aynı olması aynı nesneyi ifade etmez. Referans her tablo bağlamında değerlendirilmelidir.

---

## 6. Referans İlişkileri

Logo veritabanında foreign key constraint her ilişkide fiziksel olarak tanımlı olmayabilir; ilişkiler referans alanları üzerinden kurulur.

Örnekler:

```text
STLINE.STOCKREF      → ITEMS.LOGICALREF
STLINE.CLIENTREF     → CLCARD.LOGICALREF
STLINE.STFICHEREF    → STFICHE.LOGICALREF
STLINE.INVOICEREF    → INVOICE.LOGICALREF
```

Bu nedenle bir referans alanının anlamı tablo bağlamından okunmalıdır.

---

## 7. Kart ve Hareket Ayrımı

### Kart tabloları

Uzun ömürlü master data kayıtlarıdır.

Örnek:

- `ITEMS` — malzemeler,
- `CLCARD` — cari hesaplar,
- birim setleri,
- fiyat kartları,
- proje kartları.

### Hareket tabloları

Belge ve işlem geçmişini tutar.

Örnek:

- `STFICHE`,
- `STLINE`,
- `INVOICE`,
- `CLFLINE`,
- `EMFICHE`,
- `EMFLINE`.

---

## 8. Fiş Başlık ve Satır Yapısı

Logo'da birçok belge başlık + satır modeline sahiptir.

Örneğin stok fişi:

```text
STFICHE
   ↓ 1:N
STLINE
```

Bağlantı genel olarak:

```sql
STLINE.STFICHEREF = STFICHE.LOGICALREF
```

şeklindedir.

Bir fişin yalnızca başlığını incelemek çoğu zaman yeterli değildir; gerçek malzeme hareketi satırlardadır.

---

## 9. Fatura İlişkileri

Bir faturanın etkisi tek tabloda değildir.

Tipik olarak ilişki zinciri şunları kapsayabilir:

```text
INVOICE
   ├── STLINE
   ├── STFICHE
   ├── CLFLINE
   └── Muhasebe bağlantıları
```

Belgenin türüne ve entegrasyon durumuna göre tüm ilişkiler aynı şekilde oluşmayabilir.

Bu yüzden fatura tarih değiştirme, silme veya analiz işlemlerinde bağlı kayıtlar birlikte değerlendirilmelidir.

---

## 10. TRCODE

`TRCODE`, Logo hareket tablolarında işlemin türünü belirleyen temel alanlardan biridir.

Ancak aynı sayı farklı tablo veya modül bağlamında farklı anlamlara gelebilir.

Bu nedenle:

> `TRCODE = X` bilgisini tablo adından bağımsız yorumlamak hatalıdır.

Kod listeleri belge/modül bağlamıyla birlikte dokümante edilmelidir.

---

## 11. LINETYPE

`STLINE` gibi satır tablolarında `LINETYPE`, satırın niteliğini ayırmak için kritik alanlardan biridir.

Bir belgede yalnızca malzeme satırları değil;

- indirim,
- masraf,
- hizmet,
- promosyon,
- açıklama veya farklı satır tipleri

bulunabilir.

Bu nedenle stok/malzeme analizi yaparken `LINETYPE` filtresi çoğu zaman zorunludur.

---

## 12. IOCODE

Stok hareketinin giriş/çıkış yönünü yorumlamada `IOCODE` önemli alanlardan biridir.

Ancak stok hesabı yazarken yalnızca miktarı toplayıp çıkarmak yerine işlem türü, satır tipi ve hareket yönü birlikte değerlendirilmelidir.

Hazır Logo view veya güvenilir envanter mantığı mevcutsa referans alınması yararlıdır.

---

## 13. Ambar Alanları

Stok hareketlerinde ambar bilgisi başlık veya satır düzeyinde bulunabilir.

Önemli alan örnekleri:

```text
SOURCEINDEX
DESTINDEX
```

Transfer ve ambar hareketlerinde kaynak ve hedef kavramları doğru yorumlanmalıdır.

Başlık ambarı ile satır ambarının farklı olduğu hatalı kayıtlar özellikle entegrasyonlarda kontrol edilmelidir.

---

## 14. Sipariş ve Hareket Bağlantıları

Siparişten irsaliyeye veya faturaya dönüşümde satır ilişkilerini taşıyan referans alanları bulunur.

Örnek alanlar:

```text
ORDTRANSREF
ORDFICHEREF
PREVLINEREF
SOURCELINK
```

Bu alanların anlamı kullanılan belge akışına göre analiz edilmelidir.

Doğrudan bir stok satırı oluşturup bu bağlantıları atlamak sipariş karşılama miktarlarını ve belge izlenebilirliğini bozabilir.

---

## 15. Seri/Lot Tabloları

Seri/lot kullanılan sistemlerde stok satırı tek başına fiziksel izlenebilirliği açıklamaz.

Stok satırının seri/lot hareketleriyle bağlantısı ayrıca incelenmelidir.

Kontrol edilmesi gereken temel tutarlılık:

```text
Stok satırı miktarı
        ↕
Seri/Lot dağılım toplamı
```

Özellikle üretim, transfer ve sevkiyat işlemlerinde bu ilişki kritiktir.

---

## 16. Muhasebe Bağlantıları

ERP hareketlerinin bir kısmı muhasebeleştirildiğinde muhasebe fiş ve satırlarıyla ilişki kurar.

Genel yapı:

```text
Operasyonel Belge
       ↓
Muhasebe Fişi
       ↓
Muhasebe Satırları
```

Bir operasyonel belgenin tarihini veya temel finansal alanlarını değiştirirken muhasebe bağlantısı olup olmadığı kontrol edilmelidir.

---

## 17. Silme İşlemleri

Doğrudan SQL `DELETE` özellikle risklidir.

Bir başlık kaydını silip bağlı kayıtları bırakmak:

- yetim satır,
- kopuk seri/lot,
- hatalı cari hareket,
- eksik muhasebe bağı,
- üretim ilişki bozukluğu

oluşturabilir.

Mümkün olduğunda `IData.Delete()` veya desteklenen Logo iş katmanı kullanılmalıdır.

---

## 18. Update İşlemleri

Doğrudan `UPDATE` de her zaman masum değildir.

Örneğin yalnızca `INVOICE.DATE_` değiştirmek, bağlı:

- stok fişi,
- stok satırı,
- cari hareket,
- muhasebe fişi,
- muhasebe satırı

tarihlerini eski bırakabilir.

Bu nedenle toplu düzeltmeler ilişki haritası çıkarıldıktan sonra yapılmalıdır.

---

## 19. Okuma İçin SQL Kullanımı

SQL özellikle şu işler için çok uygundur:

- raporlama,
- kontrol sorguları,
- performans analizi,
- veri karşılaştırma,
- entegrasyon ön doğrulaması,
- hata kök neden analizi.

Örnek:

```sql
SELECT
    I.CODE,
    I.NAME,
    S.AMOUNT,
    S.DATE_
FROM LG_040_01_STLINE S
INNER JOIN LG_040_ITEMS I
    ON I.LOGICALREF = S.STOCKREF
WHERE S.LINETYPE = 0;
```

Üretim ortamında tarih, firma, dönem ve iş kapsamı filtreleri eklenmelidir.

---

## 20. SELECT * Kullanımı

Logo tabloları geniştir. Özellikle `STLINE` gibi tablolarda `SELECT *`:

- gereksiz network trafiği,
- daha yüksek I/O,
- istemci tarafında fazla veri,
- bakım zorluğu

oluşturabilir.

Kalıcı rapor ve servislerde yalnızca gereken kolonlar seçilmelidir.

---

## 21. NOLOCK Konusu

`WITH (NOLOCK)` performans çözümü olarak otomatik kullanılmamalıdır.

Kirli okuma, eksik/fazla satır görme ve tutarsız rapor sonuçları üretme ihtimali vardır.

Finansal ve stok raporlarında doğruluk ihtiyacı değerlendirilmeden `NOLOCK` kullanılmamalıdır.

---

## 22. Veri Sözlüğü Oluşturma

Kurumsal Logo projelerinde kendi veri sözlüğünüzü oluşturmak çok değerlidir.

Önerilen yapı:

| Tablo | Alan | Anlam | İlişki | Not |
|---|---|---|---|---|
| ITEMS | LOGICALREF | Malzeme ref | PK mantığı | Firma bazlı |
| STLINE | STOCKREF | Malzeme ref | ITEMS | Hareket satırı |
| STLINE | STFICHEREF | Fiş ref | STFICHE | Başlık bağlantısı |

Bu repository'nin ilerleyen bölümlerinde bu sözlük genişletilecektir.

---

## 23. Sonuç

Logo veritabanı, birbirine referans alanlarıyla bağlı kart ve hareket tablolarından oluşan geniş bir ERP veri modelidir. Sağlıklı analiz için tablo adı, firma, dönem, `LOGICALREF`, `TRCODE`, `LINETYPE` ve belge ilişkileri birlikte değerlendirilmelidir.

SQL bu yapıyı anlamak ve okumak için güçlü bir araçtır; fakat resmi kayıt oluşturma ve değiştirme işlemlerinde veri bütünlüğü nedeniyle Logo Objects öncelikli yaklaşım olmalıdır.

Bir sonraki bölümde büyük Logo veritabanlarında **SQL Server performansı, indeksler, wait istatistikleri, tempdb, bellek ve disk analizi** ele alınacaktır.
