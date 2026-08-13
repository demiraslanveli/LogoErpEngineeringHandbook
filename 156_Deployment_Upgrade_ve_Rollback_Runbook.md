# 156 — Deployment, Upgrade ve Rollback Runbook

Bu bölüm referans uygulamanın güvenli production deployment standardını tanımlar.

## Deployment Öncesi

Kontrol listesi:

```text
Release version belli mi?
Logo SDK compatibility doğrulandı mı?
DB migration planı hazır mı?
Backup alındı mı?
Rollback paketi hazır mı?
Service account yetkileri doğru mu?
Config/secret doğrulandı mı?
```

## Deployment Akışı

```text
Stop Service
   ↓
Backup Current Artifact
   ↓
Run DB Migration
   ↓
Deploy Binaries
   ↓
Validate Configuration
   ↓
Start Service
   ↓
Run Health Check
   ↓
Smoke Test
```

## Migration İlkesi

Migration yalnızca application-owned tabloları değiştirmelidir. Logo standart tabloları deployment migration'ı ile alter edilmemelidir.

## Rollback Trigger

Aşağıdaki durumlarda rollback değerlendirilir:

- service start olamıyor,
- SDK compatibility başarısız,
- kritik health check unhealthy,
- schema migration sonrası uygulama çalışamıyor,
- business işlem hata oranı kabul eşiğinin üstünde,
- reconciliation sistematik mismatch üretiyor.

## Rollback Akışı

```text
Stop New Version
   ↓
Restore Previous Binaries
   ↓
Rollback Compatible App Migration
   ↓
Restore Config if changed
   ↓
Start Previous Version
   ↓
Health + Smoke Test
```

Her migration geri alınabilir olmayabilir. Destructive schema değişikliği yerine expand/contract yaklaşımı tercih edilmelidir.

## Deployment Log

Her release için:

```text
Version
Commit SHA
DeployedAt
DeployedBy
Host
DB Schema Version
Logo Version
Objects Version
Result
RollbackPerformed
```

## PowerShell

`deploy/` klasöründe install, upgrade, rollback ve validation scriptleri ayrı tutulmalıdır. Scriptler idempotent tasarlanmalıdır.

> Deployment'ın başarı ölçütü dosyaların kopyalanması değil, yeni sürümün health ve smoke testlerden geçerek iş yapabildiğinin doğrulanmasıdır.
