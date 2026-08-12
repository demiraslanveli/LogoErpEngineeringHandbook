# 95 — HA/DR ve Always On Mantığı

## Amaç

Logo ERP ortamında yüksek erişilebilirlik ile felaket kurtarma aynı kavram değildir.

- **HA (High Availability)**: Servis kesintisini azaltmayı hedefler.
- **DR (Disaster Recovery)**: Büyük arıza veya lokasyon kaybı sonrası sistemi geri getirmeyi hedefler.

Always On Availability Groups bu iki hedefin bir bölümünü karşılayabilir; ancak tek başına backup ve DR planının yerine geçmez.

---

## Temel mimari

Basitleştirilmiş yapı:

```text
Application / Logo / Services
          ↓
       Listener
          ↓
  Primary SQL Instance
          ⇅
 Secondary SQL Instance
```

Uygulama doğrudan node adına değil listener adına bağlanabiliyorsa failover yönetimi kolaylaşır.

---

## Synchronous ve asynchronous commit

### Synchronous commit

Primary transaction commit olurken secondary'nin log harden etmesi beklenebilir.

Avantaj:
- Daha düşük veri kaybı riski

Dezavantaj:
- Ağ gecikmesi transaction latency'ye yansıyabilir

### Asynchronous commit

Primary secondary'yi beklemeden commit eder.

Avantaj:
- Uzak DR lokasyonu için uygundur

Dezavantaj:
- Failover anında veri kaybı ihtimali vardır

---

## Automatic failover

Automatic failover için topoloji, synchronization state ve cluster quorum doğru tasarlanmalıdır.

Failover yalnızca SQL seviyesinde düşünülmemelidir.

Kontrol edilmesi gerekenler:

- Logo uygulama connection string'i
- Objects / REST Service bağlantısı
- SQL Agent job sahipliği
- Database Mail
- Linked Server
- Credential / proxy
- File share yolları
- Entegrasyon servisleri

---

## Availability Group backup tercihi

Backup'ların secondary üzerinde alınması primary yükünü azaltabilir.

Ancak backup preference tanımlanmış olsa bile job tasarımı bu tercihi uygulamalıdır.

---

## Login ve server-level objeler

Availability Group database seviyesinde veri taşır.

Aşağıdaki server-level nesneler otomatik olarak senkronize olmayabilir:

- SQL Login
- SQL Agent Job
- Linked Server
- Credential
- Database Mail konfigurasyonu
- Server-level permission

Bu nedenle ayrı senkronizasyon/runbook gerekir.

---

## Logo ERP açısından dikkat

Failover sonrası kontrol:

```text
SQL erişimi
    ↓
Logo login
    ↓
Firma/dönem erişimi
    ↓
Objects login
    ↓
REST Service
    ↓
Scheduled jobs
    ↓
Mail / integration workers
```

Sadece SQL listener cevap veriyor diye sistem sağlıklı kabul edilmemelidir.

---

## RPO / RTO ilişkisi

HA tasarımı gerçek iş ihtiyacından türemelidir.

Örnek:

```text
RPO: 0-5 dakika
RTO: 15 dakika
```

Bu hedefler kullanılacak commit mode, lokasyon, network ve failover prosedürünü belirler.

---

## Always On backup değildir

Secondary replica üzerindeki veri bozulması veya kullanıcı hatası replike olabilir.

Bu nedenle:

```text
Always On ≠ Backup
```

Mutlaka bağımsız backup zinciri bulunmalıdır.

---

## Sonuç

Logo ERP için HA/DR tasarımı SQL Server, Logo servisleri, job'lar ve entegrasyon worker'ları birlikte ele alınarak yapılmalıdır.
