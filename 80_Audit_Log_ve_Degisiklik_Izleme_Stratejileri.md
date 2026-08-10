# 80 — Audit Log ve Değişiklik İzleme Stratejileri

Logo ERP üzerinde özel geliştirme yapılırken yalnızca hatayı çözmek değil, hatanın nasıl oluştuğunu sonradan kanıtlayabilmek de önemlidir. Bu nedenle audit/log tasarımı entegrasyon mimarisinin temel parçası olmalıdır.

## Audit neden gereklidir?

Örnek saha soruları:

- Ambar numarası kim tarafından değiştirildi?
- Fatura tarihi ne zaman güncellendi?
- Bir satır hangi servis tarafından oluşturuldu?
- Aynı belge neden iki kez işlendi?
- Trigger mı, kullanıcı mı, entegrasyon servisi mi değişiklik yaptı?

## Log tablosunda tutulması önerilen alanlar

```text
ID
KAYIT_TARIHI
FIRMA_NO
DONEM_NO
TABLO_ADI
ISLEM_TIPI
LOGICALREF
BELGE_REF
ESKI_DEGER
YENI_DEGER
LOGIN_ADI
HOST_ADI
PROGRAM_ADI
SESSION_ID
CORRELATION_ID
REQUEST_ID
ACIKLAMA
```

## Kimlik bilgileri

SQL Server tarafında yararlı fonksiyonlar:

```sql
SELECT
    ORIGINAL_LOGIN() AS LoginAdi,
    HOST_NAME() AS HostAdi,
    APP_NAME() AS ProgramAdi,
    @@SPID AS SessionId;
```

## Correlation ID

Servis tabanlı entegrasyonlarda her işleme benzersiz bir `CorrelationId` verilmesi önerilir.

```text
MES Request
   ↓ correlation-id
Integration Queue
   ↓ correlation-id
Logo Objects Worker
   ↓ correlation-id
Logo Record
   ↓
Audit Log
```

Bu sayede tek bir iş akışı uçtan uca izlenebilir.

## Snapshot mı, alan bazlı log mu?

İki yaklaşım vardır:

### Alan bazlı

Sadece değişen alan tutulur.

Avantajı:
- küçük log hacmi,
- kolay raporlama.

### Snapshot

İşlem öncesi/sonrası JSON veya XML tutulur.

Avantajı:
- daha güçlü hata analizi,
- tam yeniden oluşturma imkanı.

Dezavantajı:
- yüksek disk kullanımı.

## Audit ile business log ayrımı

Audit log:
- kim neyi değiştirdi?

Business log:
- süreçte ne oldu?

Örnek:

```text
Audit: SOURCEINDEX 801 → 4
Business: İade fişi merkez ambara yönlendirildi
```

İki kayıt tipi ayrı tutulabilir.

## Log büyümesi

Audit tabloları zamanla çok büyür. Bu nedenle:

- tarih alanında index,
- partition/arşiv stratejisi,
- retention süresi,
- eski kayıtların sıkıştırılması veya taşınması

planlanmalıdır.

## Kritik prensip

Audit mekanizması ana Logo işlemini yavaşlatmamalıdır. Büyük payload veya uzak servis çağrısı gerekiyorsa outbox/queue yaklaşımı tercih edilmelidir.

> İyi audit sistemi, “ne oldu?” sorusunu tahminle değil veriyle cevaplamayı sağlar.
