# 52 — L_CAPIFIRM / L_CAPIPERIOD Firma ve Dönem Yapısı

## Amaç

Logo ERP veritabanında firma ve dönem mantığı, tablo isimlerinin ve işlem kapsamının temelini oluşturur. Dinamik SQL, servis ve entegrasyon geliştirirken firma numarası ile dönem numarasının doğru yönetilmesi kritik önemdedir.

## Temel Sistem Tabloları

Firma ve dönem tanımlarını incelemek için sistem tabloları kullanılır:

```text
L_CAPIFIRM
L_CAPIPERIOD
```

Bu tablolar `LG_XXX_...` iş tablolarından farklı olarak sistem seviyesinde firma/dönem metadata bilgisini taşır.

## Firma Mantığı

Logo iş tablolarının önemli bölümü firma numarası ile adlandırılır:

```text
LG_{FIRMA}_ITEMS
LG_{FIRMA}_CLCARD
LG_{FIRMA}_PRCLIST
```

Örnek firma 040 için:

```text
LG_040_ITEMS
LG_040_CLCARD
LG_040_PRCLIST
```

## Dönem Mantığı

Hareket tablolarının önemli bölümü firma + dönem numarası içerir:

```text
LG_{FIRMA}_{DONEM}_STLINE
LG_{FIRMA}_{DONEM}_STFICHE
LG_{FIRMA}_{DONEM}_INVOICE
LG_{FIRMA}_{DONEM}_CLFLINE
```

Örnek:

```text
LG_040_01_STLINE
LG_040_01_INVOICE
```

## L_CAPIFIRM

Bu tabloda firma kartlarına ilişkin temel metadata bulunur. Sık ihtiyaçlar:

- firma numarası
- firma adı
- aktif firma listesi
- Logo Objects veya servis tarafında firma eşlemesi

Alan isimleri Logo sürümüne göre kontrol edilmelidir.

## L_CAPIPERIOD

Firma dönemleri burada takip edilir. Tipik ihtiyaçlar:

- dönem numarası
- başlangıç tarihi
- bitiş tarihi
- bağlı firma
- aktif dönem tespiti

## Dinamik Tablo Adı

Özellikle SQL raporlarında firma ve dönem parametrelerinden tablo adı üretilebilir.

Örnek mantık:

```sql
DECLARE @FirmaNo INT = 40;
DECLARE @DonemNo INT = 1;

DECLARE @Firma CHAR(3) = RIGHT('000' + CAST(@FirmaNo AS VARCHAR(3)), 3);
DECLARE @Donem CHAR(2) = RIGHT('00' + CAST(@DonemNo AS VARCHAR(2)), 2);

SELECT @Firma, @Donem;
```

Sonuç:

```text
040
01
```

## Güvenlik

Dinamik tablo adı parametre ile oluşturulurken kullanıcıdan gelen serbest metin doğrudan SQL'e eklenmemelidir. Firma ve dönem parametreleri sayısal olarak doğrulanmalı, ardından kontrollü formatlanmalıdır.

## Dönem Sabitleme

Bazı kurumlarda dönem sürekli `01` olabilir. Bu durumda prosedürde dönem parametresi vermek yerine açıkça sabitlenmesi kodu sadeleştirebilir.

Ancak bu karar yalnızca o kurumun operasyon modeline ait olmalıdır. Genel amaçlı kütüphanelerde dönem parametrik tutulmalıdır.

## Logo Objects ile Firma Seçimi

Logo Objects tarafında işlem yapılmadan önce uygulama oturumu doğru firma ve dönem bağlamına geçirilmelidir. Yanlış firma/dönem seçimi, aynı kodlu kartların farklı firma kayıtları üzerinde işlem görmesine yol açabilir.

## Kontrol Sorgusu Yaklaşımı

Bir işlem başlamadan önce:

1. Firma mevcut mu?
2. Dönem mevcut mu?
3. İşlem tarihi dönem aralığında mı?
4. Hedef tablo gerçekten var mı?
5. Firma/dönem kodu doğru formatlandı mı?

kontrol edilmelidir.

## Yaygın Hata

En sık hatalardan biri firma `40` için tabloyu `LG_40_ITEMS` şeklinde üretmektir.

Doğrusu:

```text
LG_040_ITEMS
```

Dönem için de aynı şekilde:

```text
1 → 01
```

## Mimari İlke

Firma/dönem üretme kodu uygulamanın farklı yerlerinde tekrar yazılmamalıdır. Merkezi bir yardımcı fonksiyon kullanılmalıdır:

```text
FormatFirmNo(40)   → 040
FormatPeriodNo(1)  → 01
```

Bu küçük standart, dinamik Logo SQL kodlarında önemli miktarda hata önler.
