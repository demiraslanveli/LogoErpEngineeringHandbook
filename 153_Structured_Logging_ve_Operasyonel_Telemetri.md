# 153 — Structured Logging ve Operasyonel Telemetri

Bu bölüm referans uygulamanın production logging standardını tanımlar.

## Amaç

Log satırları insan tarafından okunabilir olduğu kadar makine tarafından sorgulanabilir de olmalıdır. Serbest metin log yerine alan bazlı structured logging tercih edilir.

## Zorunlu Alanlar

Her kritik işlem için en az:

```text
Timestamp
Level
Application
Environment
Company
Period
CorrelationId
Operation
EntityType
EntityKey
DurationMs
Success
ErrorCode
Message
```

Logo işlemlerinde mümkünse ayrıca:

```text
LogoLogicalRef
LogoObjectType
LogoErrorCode
LogoErrorDescription
```

## CorrelationId

Bir dış sistem mesajı Logo'ya kadar aynı correlation id ile izlenebilmelidir.

```text
Source Event
   ↓ same CorrelationId
Queue
   ↓
Application Service
   ↓
Logo Adapter
   ↓
Reconciliation
```

## Secret Masking

Şunlar loglanmamalıdır:

- Logo parolası,
- SQL parola içeren connection string,
- token/secret,
- hassas kişisel veri gereksizse.

## Süre Ölçümü

Özellikle şu işlemler ayrı sürelenmelidir:

- session login,
- SQL lookup,
- IData create/post,
- ProductionApplication operation,
- reconciliation,
- queue processing.

## Log Seviyeleri

```text
DEBUG → geliştirme ayrıntısı
INFO  → başarılı operasyon
WARN  → retry edilebilir veya dikkat gerektiren durum
ERROR → işlem başarısız
FATAL → host çalışmasını sürdüremiyor
```

## Sink Stratejisi

Log sink değiştirilebilir olmalıdır. Dosya, SQL veya merkezi log platformu application katmanına sızmamalıdır.

## Operasyonel Metrikler

Loglardan en az şu KPI'lar üretilebilmelidir:

- dakika başına işlem,
- başarı oranı,
- ortalama/p95 süre,
- Logo login hata oranı,
- retry sayısı,
- dead-letter sayısı,
- queue backlog,
- reconciliation mismatch.

> İyi logging yalnızca hata olduğunda değil, sistem normal çalışırken de ne yaptığını kanıtlayabilmelidir.
