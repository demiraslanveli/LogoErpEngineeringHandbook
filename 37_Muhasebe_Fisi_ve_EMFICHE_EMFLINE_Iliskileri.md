# 37 — Muhasebe Fişi ve EMFICHE / EMFLINE İlişkileri

## Amaç

Bu bölüm Logo ERP’de ticari işlemlerin muhasebe fişi ayağını anlamak için `EMFICHE` ve `EMFLINE` tablolarının rolünü açıklar.

Logo’da satış, satınalma, banka, kasa ve benzeri işlemler muhasebeleştirildiğinde oluşan fişler yalnızca finansal raporlama için değil, ticari belge ile genel muhasebe arasındaki izlenebilirlik için de önemlidir.

## 1. Temel tablolar

Dönemsel muhasebe fişi tabloları tipik olarak:

```text
LG_XXX_YY_EMFICHE
LG_XXX_YY_EMFLINE
```

şeklindedir.

Örnek:

```text
LG_102_01_EMFICHE
LG_102_01_EMFLINE
```

Genel yapı:

```text
EMFICHE
  └── EMFLINE
```

Fiş üst bilgisi `EMFICHE`, muhasebe satırları ise `EMFLINE` üzerinde tutulur.

## 2. Fiş–satır ilişkisi

Temel ilişki mantığı:

```text
EMFICHE.LOGICALREF
       ↓
EMFLINE.ACCFICHEREF
```

Analiz sorgusu:

```sql
SELECT
    F.LOGICALREF AS FIS_REF,
    F.FICHENO,
    F.DATE_,
    L.*
FROM LG_102_01_EMFICHE F
INNER JOIN LG_102_01_EMFLINE L
    ON L.ACCFICHEREF = F.LOGICALREF
WHERE F.LOGICALREF = @EmficheRef;
```

> Alan isimleri sürüm kontrolü gerektirir; gerçek ortamda tablo metadata’sı üzerinden doğrulanmalıdır.

## 3. Ticari belge ile muhasebe fişi ilişkisi

Bir faturanın veya başka ticari belgenin muhasebe fişine bağlanması doğrudan veya ara referans alanları üzerinden olabilir.

Analizde aşağıdaki bilgi grupları birlikte değerlendirilmelidir:

- ticari belgenin `LOGICALREF` değeri,
- muhasebe fişinin `LOGICALREF` değeri,
- kaynak modül bilgisi,
- kaynak fiş referansı,
- muhasebeleştirme bağlantı alanları,
- işlem tarihi,
- fiş numarası,
- TRCODE / fiş türü.

Bu nedenle ilişkiyi yalnızca aynı tarih veya aynı açıklamaya göre kurmak güvenilir değildir.

## 4. Fatura tarih güncelleme senaryosu

Bir faturanın tarihi değiştirildiğinde yalnızca `INVOICE.DATE_` alanını güncellemek yeterli değildir.

Belge zinciri aşağıdaki gibi olabilir:

```text
INVOICE
  ├── STFICHE
  ├── STLINE
  ├── CLFLINE
  └── EMFICHE
        └── EMFLINE
```

Bu nedenle tarih düzeltme gibi özel operasyonlarda bağlantılı kayıtların tamamı analiz edilmelidir.

Örnek kontrol listesi:

```text
[ ] Fatura tarihi
[ ] İrsaliye tarihi
[ ] Stok satır tarihleri
[ ] Cari hareket tarihi
[ ] Muhasebe fişi tarihi
[ ] Muhasebe satır tarihleri
```

## 5. Muhasebe fişi her zaman olmak zorunda mı?

Hayır.

Bir ticari belgenin muhasebe fişi bulunmaması şu nedenlerden kaynaklanabilir:

- belge henüz muhasebeleştirilmemiştir,
- muhasebeleştirme farklı bir süreçte çalışıyordur,
- işlem türü muhasebe fişi üretmiyordur,
- fiş sonradan silinmiş veya yeniden oluşturulmuş olabilir,
- özel uygulama akışı söz konusu olabilir.

Bu nedenle entegrasyonlarda şu mantık tercih edilmelidir:

```text
Muhasebe fişi varsa güncelle / kontrol et
Muhasebe fişi yoksa diğer geçerli kayıtları işlemeye devam et
```

Muhasebe fişi bulunamadığı için tüm batch işlemini durdurmak çoğu bakım senaryosunda doğru değildir.

## 6. Denge kontrolü

Muhasebe fişlerinde temel kontrol toplam borç ve toplam alacak dengesidir.

Örnek analiz yaklaşımı:

```sql
SELECT
    ACCFICHEREF,
    SUM(CASE WHEN SIGN = 0 THEN DEBIT ELSE 0 END) AS BORC,
    SUM(CASE WHEN SIGN = 1 THEN CREDIT ELSE 0 END) AS ALACAK
FROM LG_102_01_EMFLINE
WHERE ACCFICHEREF = @EmficheRef
GROUP BY ACCFICHEREF;
```

Bu örnek alan adları kavramsaldır. Gerçek sürümde borç/alacak tutar alanlarını metadata’dan doğrulayın.

Kontrol mantığı:

```text
Toplam Borç = Toplam Alacak
```

olmalıdır.

## 7. Doğrudan SQL UPDATE riski

Muhasebe fişi tabloları yüksek riskli tablolardır.

Özellikle şu tür doğrudan müdahaleler dikkat gerektirir:

- tarih değişikliği,
- hesap referansı değişikliği,
- satır tutarı değişikliği,
- fiş referansı değişikliği,
- kaynak belge bağlantısı değişikliği.

Bu alanlardaki hatalar şunlara yol açabilir:

- mizan bozulması,
- ticari sistem ile muhasebe uyuşmazlığı,
- yeniden muhasebeleştirme sorunları,
- rapor farkları,
- kaynak belge zincirinin kopması.

## 8. Teşhis sorgusu yaklaşımı

Bir belgeye bağlı muhasebe fişini bulmak için önce ticari belgenin kendi muhasebe referans alanı incelenmelidir.

Ardından:

```sql
SELECT *
FROM LG_102_01_EMFICHE
WHERE LOGICALREF = @AccountingRef;

SELECT *
FROM LG_102_01_EMFLINE
WHERE ACCFICHEREF = @AccountingRef;
```

çalıştırılabilir.

Kaynak referans alanının adı ürün/sürüm bazında doğrulanmalıdır.

## 9. Toplu düzeltmelerde güvenli yaklaşım

Önerilen işlem modeli:

```text
1. Hedef belgeleri listele
2. Bağlantılı muhasebe fişlerini tespit et
3. Eksik bağlantıları raporla
4. Test modunda sonucu göster
5. Transaction aç
6. İlgili kayıtları güncelle
7. Etkilenen satır sayılarını doğrula
8. Commit / rollback
9. Audit log yaz
```

## 10. Sonuç

`EMFICHE` ve `EMFLINE`, Logo’nun ticari operasyonlarını genel muhasebe ile bağlayan kritik tablolardır.

Temel prensip:

> Ticari belge üzerinde yapılan bir düzeltmenin muhasebe etkisini göz ardı etme; fakat muhasebe fişinin her belgede zorunlu olarak var olduğunu da varsayma.
