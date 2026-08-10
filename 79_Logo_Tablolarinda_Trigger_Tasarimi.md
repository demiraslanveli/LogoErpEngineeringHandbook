# 79 — Logo Tablolarında Trigger Tasarımı

Logo ERP tablolarında trigger kullanımı güçlü fakat riskli bir müdahaledir. Trigger, Logo Objects iş kurallarının yerine geçmemeli; mümkünse yalnızca kontrol, loglama veya dış sistem senkronizasyonu gibi sınırlı görevler için kullanılmalıdır.

## Temel riskler

- Logo işlem süresini uzatabilir.
- Deadlock ihtimalini artırabilir.
- Çok satırlı INSERT/UPDATE senaryolarında hatalı çalışabilir.
- Ana işlemi geri alabilir.
- Sürüm yükseltmelerinde beklenmeyen yan etkiler oluşturabilir.

## En kritik kural

Trigger hiçbir zaman `inserted` veya `deleted` tablolarında yalnızca tek satır varmış gibi yazılmamalıdır.

Kötü yaklaşım:

```sql
SELECT @LogicalRef = LOGICALREF FROM inserted;
```

Bu kod çok satırlı işlemde tek bir değeri rastgele seçebilir.

Doğru yaklaşım set-based çalışmaktır.

## Örnek log trigger kalıbı

```sql
INSERT INTO dbo.Z_LOGO_HAREKET_LOG
(
    LOGICALREF,
    ISLEM_TIPI,
    KAYIT_TARIHI
)
SELECT
    I.LOGICALREF,
    'INSERT',
    GETDATE()
FROM inserted I
LEFT JOIN deleted D
    ON D.LOGICALREF = I.LOGICALREF
WHERE D.LOGICALREF IS NULL;
```

## UPDATE senaryosu

Değişen alanı belirlemek için `inserted` ve `deleted` karşılaştırılmalıdır.

```sql
SELECT
    D.LOGICALREF,
    D.SOURCEINDEX AS ESKI_AMBAR,
    I.SOURCEINDEX AS YENI_AMBAR
FROM deleted D
JOIN inserted I
    ON I.LOGICALREF = D.LOGICALREF
WHERE ISNULL(D.SOURCEINDEX,-1) <> ISNULL(I.SOURCEINDEX,-1);
```

## Logo işlemini engelleyen trigger

Bir trigger `RAISERROR` / `THROW` üreterek ana Logo transaction'ını geri alabilir. Bu ancak iş kuralı gerçekten veritabanı seviyesinde zorunluysa kullanılmalıdır.

Tercih sırası:

1. Logo ekranı / form kontrolü
2. Logo Objects validasyonu
3. Servis katmanı validasyonu
4. En son çare olarak trigger

## Transaction süresi

Trigger içinde şu işlemlerden kaçınılmalıdır:

- uzak sunucu çağrısı,
- HTTP çağrısı,
- uzun rapor sorgusu,
- mail gönderimi,
- büyük cursor işlemleri,
- uzun süren MERGE/UPDATE.

Bunun yerine queue/outbox tablosuna kısa kayıt atılmalıdır.

## Trigger loglama alanları

Saha projelerinde faydalı alanlar:

- kayıt tarihi,
- `LOGICALREF`,
- belge ref,
- `TRCODE`,
- eski/yeni değer,
- `ORIGINAL_LOGIN()`,
- `HOST_NAME()`,
- `APP_NAME()`,
- `@@SPID`,
- işlem tipi.

## Örnek alan kullanım senaryosu

Ambar değişikliğini izlemek için:

- `SOURCEINDEX`
- `DESTINDEX`
- `SOURCELINK`
- `PREVLINEREF`
- `ORDTRANSREF`

gibi alanlar birlikte loglanabilir.

## Trigger yönetim standardı

Her özel trigger için aşağıdaki bilgiler repository veya operasyon dokümanında tutulmalıdır:

- hangi tablo üzerinde,
- hangi olayda,
- neden oluşturulduğu,
- hangi iş kuralını uyguladığı,
- hangi log tablosuna yazdığı,
- nasıl devre dışı bırakılacağı,
- nasıl test edildiği.

> Logo tablolarında trigger yazmak mümkündür; ancak trigger ne kadar az sorumluluk taşırsa sistem o kadar güvenli kalır.
