# 158 — End-to-End Integration Test Senaryoları

Bu bölüm referans uygulamanın uçtan uca kabul testlerini tanımlar.

## Amaç

Unit test yalnızca application logic'i doğrular. End-to-end test ise dış event'ten Logo ERP kayıt ilişkilerine kadar tüm zinciri doğrular.

## Temel Senaryolar

### Malzeme

```text
Create Material
  ↓
Logo Objects Post
  ↓
Read Back ITEMS / Unit Relations
  ↓
Assert LogicalRef + Code + Unit Data
```

### Cari

```text
Create Customer
  ↓
Read Back CLCARD
  ↓
Assert Code / Title / Tax Data
```

### Sipariş → İrsaliye → Fatura

En kritik belge zinciri:

```text
Sales Order
   ↓
Dispatch
   ↓
Invoice
   ↓
Stock + Current Account + Accounting
```

Doğrulanacak ilişkiler:

- belge logicalref'leri,
- kaynak satır bağlantıları,
- miktar aktarımı,
- kalan miktar,
- stok hareketleri,
- cari hareket,
- muhasebe fişi varsa bağlantısı.

## Duplicate / Idempotency

Aynı external event iki kez işlendiğinde ikinci ERP kaydı oluşmamalıdır.

## Retry

Geçici SQL/network hatasında retry sonrası tek sonuç oluştuğu doğrulanmalıdır.

## Session Failure

Logo session düşmesi veya login başarısızlığı kontrollü sonuç üretmeli ve işlem yarım ERP kaydı bırakmamalıdır.

## Invalid Data

- geçersiz malzeme,
- geçersiz cari,
- sıfır/negatif miktar,
- hatalı ambar,
- zorunlu field eksikliği.

## Production

Üretim emri testinde üretim, sarf ve gerekiyorsa seri/lot bağlantıları read-back ile kontrol edilir.

## Test İzolasyonu

- production DB kullanılmaz,
- test firma/dönem belirlenir,
- test data prefix'i kullanılır,
- cleanup stratejisi kontrollüdür,
- Logo iş kurallarını bozacak doğrudan SQL DELETE yapılmaz.

## Test Sonucu

Her test correlation id, oluşturulan logicalref'ler ve cleanup bilgisi ile raporlanmalıdır.

> End-to-end test, adapter'ın yalnızca çağrı yapabildiğini değil, Logo ERP'nin beklenen ilişkili sonucu gerçekten ürettiğini kanıtlar.
