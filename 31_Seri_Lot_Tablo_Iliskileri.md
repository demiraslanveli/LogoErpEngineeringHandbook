# 31 — Seri / Lot Tablo İlişkileri

## 1. Amaç

Logo ERP'de seri/lot takibi, yalnızca stok satırındaki miktarı bilmekten daha karmaşık bir ilişki yapısına sahiptir. Özellikle üretim, kalite, SKT, izlenebilirlik ve ambar bazlı lot stoğu analizlerinde hareket satırı ile seri/lot kayıtları arasındaki bağ doğru kurulmalıdır.

Bu bölüm seri/lot yapısını tablo ezberinden çok ilişki mantığı üzerinden açıklar.

## 2. Temel Yaklaşım

Seri/lot analizi yaparken en az üç katman düşünülmelidir:

```text
Malzeme hareket satırı
        ↓
Seri/Lot hareket bağlantısı
        ↓
Seri/Lot kartı / tanımı
```

Bu yapı sayesinde bir malzeme hareketinin hangi lot veya seri numarası ile gerçekleştiği izlenebilir.

## 3. STLINE Başlangıç Noktasıdır

Malzeme hareketinin ana kaydı çoğunlukla `STLINE` üzerindedir.

Önemli alanlar:

```text
LOGICALREF
STOCKREF
STFICHEREF
INVOICEREF
TRCODE
IOCODE
SOURCEINDEX
DESTINDEX
AMOUNT
DATE_
```

Seri/lot araştırması çoğu zaman bu satırın `LOGICALREF` değeri üzerinden başlatılır.

## 4. Seri/Lot Bağlantı Tabloları

Logo sürüm ve ürüne göre tablo isimleri kontrol edilmelidir. Saha ortamlarında seri/lot hareket ilişkilerinde `SLTRANS`, seri/lot tanımlarında ise `SERILOTN` benzeri tablolarla karşılaşılır.

Kesin tablo yapısı kullanılan sürümün gerçek veritabanında doğrulanmalıdır.

Genel ilişki mantığı:

```text
STLINE.LOGICALREF
      ↓
Seri/Lot hareket bağlantısı
      ↓
Seri/Lot kart referansı
      ↓
Lot / seri kodu
```

## 5. Neden STLINE Yeterli Değildir?

Bir `STLINE` satırında 100 adet hareket bulunabilir; ancak bu miktar örneğin:

```text
LOT-A : 40
LOT-B : 35
LOT-C : 25
```

şeklinde üç farklı lota dağılmış olabilir.

Sadece `STLINE.AMOUNT` üzerinden rapor üretmek lot bazlı stok doğruluğunu garanti etmez.

## 6. Lot Bazlı Stok

Lot stok hesabında şu boyutlar birlikte değerlendirilmelidir:

```text
Malzeme
Lot / Seri
Ambar
Hareket yönü
Miktar
Tarih
```

Örnek sonuç modeli:

```text
Malzeme Kodu | Lot No | Ambar | Giriş | Çıkış | Kalan
```

Bu model özellikle ilaç, gıda, veteriner ürünleri ve regülasyona tabi üretimlerde önemlidir.

## 7. SKT ve Üretim Tarihi

Lot kayıtlarında iş ihtiyacına göre şu bilgiler kritik olabilir:

- Lot numarası
- Seri numarası
- Son kullanma tarihi
- Üretim tarihi
- Giriş tarihi
- İlk hareket tarihi
- Son hareket tarihi
- Mevcut kalan miktar

SKT raporunda yalnızca lot kartındaki tarih yeterli olmayabilir; lotun gerçekten stokta kalan miktarı da hesaplanmalıdır.

## 8. FEFO Yaklaşımı

Son kullanma tarihi kullanılan sektörlerde lot çıkış stratejisi çoğu zaman FEFO mantığına göre değerlendirilir:

```text
First Expire, First Out
```

Yani son kullanma tarihi daha yakın olan stok önce tüketilmelidir.

Raporlama tarafında örnek sıralama:

```sql
ORDER BY EXPDATE, LOTNO
```

Ancak gerçek çıkış davranışı Logo stok/lot seçim kuralları ve operasyon süreciyle birlikte değerlendirilmelidir.

## 9. Ambar Bazlı Lot Stoğu

Aynı lot farklı ambarlarda bulunabilir.

Bu nedenle:

```text
Malzeme + Lot
```

tek başına yeterli anahtar değildir.

Raporlama çoğu zaman:

```text
Malzeme + Lot + Ambar
```

seviyesinde yapılmalıdır.

## 10. Üretimde Seri/Lot

Üretim senaryosunda iki ayrı izlenebilirlik zinciri vardır:

```text
Tüketilen hammadde lotları
        ↓
Üretim emri / operasyon
        ↓
Üretilen mamul lotu
```

İyi tasarlanmış bir entegrasyon şu soruyu cevaplayabilmelidir:

> Bu mamul lotu üretilirken hangi hammadde lotları kullanıldı?

Ve ters yönde:

> Bu hammadde lotu hangi mamul lotlarında kullanıldı?

Bu ilişki recall/geri çağırma süreçleri için kritiktir.

## 11. Seri/Lot ve Ambar Değişikliği

Bir `STLINE` satırının ambarını SQL ile değiştirirken seri/lot ilişkileri güncellenmezse şu tutarsızlık oluşabilir:

```text
STLINE → Ambar 4
Lot hareketi → Ambar 801
```

Bu durumda standart stok ile seri/lot stok raporu farklı sonuç verebilir.

Bu nedenle seri/lot takipli hareketlerde doğrudan SQL update çok yüksek risklidir.

## 12. Tutarlılık Kontrolü

Bir seri/lot raporu hazırlanırken aşağıdaki kontroller yapılmalıdır:

1. Hareket satırı mevcut mu?
2. Seri/lot bağlantısı mevcut mu?
3. Bağlantı miktarı `STLINE.AMOUNT` ile uyumlu mu?
4. Ambar bilgisi tutarlı mı?
5. İptal edilmiş belge dahil mi?
6. Giriş/çıkış yönü doğru hesaplandı mı?
7. Aynı lot birden fazla ambarda mı?

## 13. Performans

Seri/lot tabloları yüksek hareket hacminde büyüyebilir. Sorgularda özellikle:

- hareket referansı,
- stok referansı,
- lot referansı,
- tarih,
- ambar

alanlarında kullanılan join ve filtreler execution plan üzerinden incelenmelidir.

## 14. Entegrasyon Tasarımı

MES veya WMS sistemi Logo'ya lotlu hareket gönderiyorsa payload seviyesinde en az şu bilgiler bulunmalıdır:

```text
Malzeme kodu/ref
Miktar
Birim
Ambar
Lot/seri numarası
Üretim tarihi
SKT (varsa)
Belge referansı / dış sistem anahtarı
```

Aynı işlem tekrar gönderildiğinde duplicate lot hareketi oluşmaması için idempotency anahtarı kullanılmalıdır.

## 15. Best Practice

- Lot stok hesabını sadece `STLINE` üzerinden yapma.
- Seri/lot bağlantı miktarlarını hareket miktarıyla karşılaştır.
- Ambarı lot seviyesinde de doğrula.
- Üretimde hammadde lotundan mamul lotuna traceability kur.
- SQL update ile hareket ambarı değiştirirken seri/lot ilişkilerini göz ardı etme.
- SKT raporunda sadece tarih değil kalan stok miktarını da kontrol et.
- Seri/lot entegrasyonlarında duplicate koruması kullan.

## 16. Özet

Seri/lot takibi Logo ERP'nin en hassas veri bütünlüğü alanlarından biridir. Doğru analiz için hareket satırı, lot bağlantısı, lot tanımı, ambar ve hareket yönü birlikte değerlendirilmelidir. Özellikle üretim entegrasyonlarında bu zincir eksiksiz tutulmalıdır.
