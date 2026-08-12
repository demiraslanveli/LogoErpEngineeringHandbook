# 110 — Batch Processor ve Toplu İşlem Mimarisi

Logo entegrasyonlarında toplu veri aktarımı, tek kaydı işlemekten farklı tasarlanmalıdır. Amaç yalnızca hızlı çalışmak değil; kısmi hata, tekrar deneme, izlenebilirlik ve veri bütünlüğünü yönetmektir.

## Temel Akış

```text
Input Batch
   ↓
Normalize
   ↓
Validate
   ↓
Partition
   ↓
Process Item
   ↓
Persist Result
   ↓
Retry / Dead Letter
   ↓
Reconciliation
```

## Batch Item Modeli

```csharp
public sealed class BatchItem<T>
{
    public string BatchId { get; set; }
    public string ItemId { get; set; }
    public T Payload { get; set; }
    public int Attempt { get; set; }
}
```

## Sonuç Modeli

```csharp
public sealed class BatchItemResult
{
    public string ItemId { get; set; }
    public bool Success { get; set; }
    public string LogoLogicalRef { get; set; }
    public string ErrorCode { get; set; }
    public string ErrorMessage { get; set; }
}
```

## Tüm Batch Tek Transaction Olmamalı

Binlerce kaydı tek transaction içinde tutmak:

- lock süresini uzatır
- rollback maliyetini artırır
- log büyümesine neden olabilir
- bir kaydın hatasının tüm batch'i düşürmesine yol açar

Logo Objects tarafında çoğu entegrasyon için item bazlı transaction daha güvenlidir.

## Chunking

Örnek:

```text
10.000 kayıt
   ↓
100 kayıtlık chunk
   ↓
her chunk kontrollü işlenir
```

Chunk boyutu sabit bir doğru değildir. Belge tipi, satır sayısı, seri/lot yoğunluğu ve Logo performansına göre ölçülmelidir.

## Paralellik

Logo session nesneleri güvenli şekilde paylaşılmamalıdır.

Yanlış:

```text
10 thread
   ↓
aynı IApplication instance
```

Daha güvenli yaklaşım:

```text
Worker 1 → Session 1
Worker 2 → Session 2
Worker 3 → Session 3
```

Paralellik mutlaka yük testi ile sınırlandırılmalıdır.

## Retry

Her hata retry edilmemelidir.

Retry edilebilir örnekler:

- geçici network sorunu
- SQL timeout
- servis geçici olarak erişilemiyor

Retry edilmemesi gereken örnekler:

- malzeme bulunamadı
- cari bulunamadı
- zorunlu alan eksik
- KDV kuralı hatalı
- belge zaten mevcut

## Dead Letter

Belirli deneme sayısından sonra kayıt ayrı kuyruğa alınmalıdır.

```text
Pending
  ↓
Processing
  ↓
Retrying
  ↓
DeadLetter
```

Dead-letter kayıtları manuel veya otomatik reconciliation sürecine girmelidir.

## Idempotency

Her batch item dış sistemde benzersiz bir anahtar taşımalıdır.

Örnek:

```text
SourceSystem + DocumentType + SourceDocumentId
```

Bu anahtar Logo'ya aynı belgenin iki kez yazılmasını önlemek için kullanılabilir.

## Önerilen Batch Processor Arayüzü

```csharp
public interface IBatchProcessor<T>
{
    BatchResult Process(IEnumerable<T> items, LogoContext context);
}
```

## Ölçülmesi Gereken Metrikler

- toplam kayıt
- başarılı kayıt
- hatalı kayıt
- retry sayısı
- dead-letter sayısı
- ortalama işlem süresi
- p95 işlem süresi
- Logo session hata sayısı

> Toplu entegrasyonda başarı kriteri yalnızca throughput değildir. Her kaydın sonucunun açıklanabilir ve tekrar işlenebilir olmasıdır.
