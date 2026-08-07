# 53 — İşyeri, Fabrika ve Ambar Organizasyon Yapısı

## Amaç

Logo ERP’de işyeri, fabrika ve ambar kavramları aynı şey değildir. Stok, üretim, satınalma, satış ve maliyet raporlarında bu organizasyon boyutlarının doğru ayrılması gerekir.

## Temel Kavramlar

```text
İşyeri   = operasyonel / idari organizasyon birimi
Fabrika  = üretim organizasyon birimi
Ambar    = fiziksel veya lojistik stok alanı
```

Bir işlem satırında veya fiş başlığında bu boyutların farklı alanlarla temsil edilmesi mümkündür.

## Ambar

Stok hareketlerinde sık görülen alanlar:

```text
SOURCEINDEX
DESTINDEX
```

Anlamları işlem tipine göre değişmekle birlikte genel olarak kaynak ve hedef ambar bilgisini temsil eder.

Tek yönlü giriş/çıkış işlemlerinde çoğu zaman yalnızca ilgili yön anlamlıdır. Ambar transferlerinde iki taraf da önemlidir.

## Fabrika

Üretim ve bazı stok hareketlerinde `FACTORYNR` alanı üretim organizasyonunun izlenmesinde kullanılır.

```text
STLINE.FACTORYNR
```

Üretim maliyeti ve üretim emri analizlerinde fabrika filtresi ayrıca değerlendirilmelidir.

## İşyeri

Belge başlıklarında işyeri bilgisi rapor ve yetki ayrımında kritik olabilir. Kullanılan tabloya göre alan adı ve bağlama şekli değişebilir; sahadaki sürümden doğrulanmalıdır.

## SOURCEINDEX ve DESTINDEX

Ambar transferinde temel mantık:

```text
Kaynak ambar  → SOURCEINDEX
Hedef ambar   → DESTINDEX
```

Ancak TRCODE / IOCODE ile birlikte değerlendirilmelidir. Tek başına alan değerine bakılarak hareket yönü yorumlanmamalıdır.

## Başlık ve Satır Ambarı

Bazı işlemlerde fiş başlığındaki ambar ile satırdaki ambar birbirinden farklı olabilir veya satır bazlı ambar kullanımı bulunabilir.

Bu nedenle kontrol raporlarında:

```text
STFICHE kaynak/hedef ambarı
STLINE kaynak/hedef ambarı
```

karşılaştırılmalıdır.

## Örnek Tutarsızlık Kontrolü

```sql
SELECT
    F.LOGICALREF AS FIS_REF,
    F.FICHENO,
    L.LOGICALREF AS SATIR_REF,
    L.SOURCEINDEX AS SATIR_KAYNAK_AMBAR,
    L.DESTINDEX AS SATIR_HEDEF_AMBAR
FROM LG_040_01_STFICHE F
INNER JOIN LG_040_01_STLINE L
    ON L.STFICHEREF = F.LOGICALREF;
```

Başlık alanları kullanılan sürüme göre ayrıca eklenmelidir.

## Üretim Senaryosu

Detaylı üretimde aşağıdaki boyutlar birlikte düşünülmelidir:

```text
Firma
  ↓
İşyeri
  ↓
Fabrika
  ↓
Ambar
  ↓
Üretim emri / iş emri / operasyon
```

Yanlış fabrika veya ambar seçimi üretimin stok ve maliyet sonuçlarını doğrudan etkileyebilir.

## Entegrasyon Kontrol Listesi

Bir dış sistem Logo’ya hareket aktaracaksa:

1. İşyeri doğru mu?
2. Fabrika doğru mu?
3. Kaynak ambar doğru mu?
4. Hedef ambar gerekli mi?
5. IOCODE hareket yönüyle uyumlu mu?
6. Başlık ve satır ambarları tutarlı mı?
7. Seri/lot seçilen ambarda gerçekten mevcut mu?

## Mimari İlke

Entegrasyonda ambar numarasını serbest sayı olarak taşımak yerine eşleme tablosu kullanmak daha güvenlidir:

```text
DIS_SISTEM_AMBAR_KODU
LOGO_AMBAR_NO
FIRMA_NO
ISYERI_NO
FABRIKA_NO
AKTIF
```

Bu yaklaşım aynı dış sistem kodunun farklı Logo firmalarında farklı ambarlara karşılık gelmesini yönetilebilir hale getirir.
