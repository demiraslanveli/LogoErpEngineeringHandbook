# 98 — Logo Servis Hesapları ve Windows Service Çalışma Modeli

## Amaç

Logo Objects, REST servisleri, background worker'lar ve entegrasyon servisleri Windows Service olarak çalıştırıldığında sorunların önemli bir bölümü koddan değil servis hesabı, dosya sistemi, ağ erişimi veya oturum bağlamından kaynaklanır.

Bu bölüm servis hesabı tasarımını üretim ortamı perspektifiyle ele alır.

---

## Servis hesabı neden önemlidir?

Windows Service çalışırken kullanıcı masaüstü oturumundan farklı bir güvenlik bağlamında çalışır.

Bu nedenle şu durum sık görülür:

```text
Console uygulamasında çalışıyor
Windows Service olarak çalışmıyor
```

Muhtemel farklar:

- Dosya erişim yetkisi
- Registry erişimi
- Network share erişimi
- SQL authentication / integrated security
- COM registration
- Temp klasörü
- Working directory
- Environment variable

---

## Hesap seçenekleri

Windows servisleri şu tür hesaplarla çalışabilir:

- LocalSystem
- NetworkService
- LocalService
- Local user
- Domain service account
- gMSA

Kurumsal ortamda ihtiyaç uygunsa domain service account veya gMSA tercih edilebilir.

---

## LocalSystem riski

LocalSystem çok yüksek yerel yetkiye sahiptir.

Bir uygulamanın çalışması için LocalSystem vermek kalıcı çözüm değildir.

Doğru yaklaşım eksik permission'ı belirleyip yalnızca gerekli yetkiyi vermektir.

---

## Dosya sistemi izinleri

Servis aşağıdaki klasörlere ihtiyaç duyabilir:

```text
Application directory
Log directory
Temp directory
Export / import directory
Attachment directory
XML directory
```

Her klasör için gereken permission ayrı düşünülmelidir.

Örnek:

```text
Read
Write
Modify
```

ihtiyaca göre verilmelidir.

---

## Network share

`C:\...` ile çalışan süreç `\\server\share\...` ile çalışmayabilir.

Network share erişiminde servis hesabının uzak sunucuda da permission'ı olmalıdır.

Mapped drive kullanımı Windows Service için güvenilir değildir.

Tercih:

```text
\\server\share\folder
```

UNC path kullanılmalıdır.

---

## SQL erişimi

Integrated Security kullanılıyorsa SQL Server servis hesabını değil uygulamanın Windows Service hesabını görür.

Bu nedenle login mapping doğru yapılmalıdır.

Kontrol:

```text
Windows Service account
    ↓
SQL Login
    ↓
Database User
    ↓
Role / Permission
```

---

## COM / Logo Objects

Logo Objects COM bileşenleri kullanılıyorsa:

- doğru bitness
- COM registration
- servis hesabı yetkileri
- uygulama çalışma dizini
- ilgili DLL bağımlılıkları

doğrulanmalıdır.

32-bit/64-bit uyumsuzluğu ayrı bir hata kaynağıdır.

---

## Servis başlangıç sırası

Entegrasyon worker'ı SQL Server veya Logo bağımlı servisten önce başlayabilir.

Bu nedenle startup sırasında dependency kontrolü yapılmalıdır.

Örnek:

```text
Service starts
↓
Configuration check
↓
SQL connectivity check
↓
Logo Objects availability check
↓
Worker loop
```

Bağımlılık hazır değilse servis crash loop'a girmemeli, kontrollü retry yapmalıdır.

---

## Loglama

Windows Service mutlaka ayrı log üretmelidir.

Önerilen alanlar:

```text
Timestamp
Service name
Machine name
Process id
Correlation id
Company
Period
Operation
Result
Elapsed time
Error
```

---

## Service recovery

Windows Service Recovery seçenekleri değerlendirilebilir:

- First failure → Restart service
- Second failure → Restart service
- Subsequent failure → Restart veya alert

Ancak sürekli crash eden servis restart döngüsüne bırakılmamalıdır.

---

## Logo REST Service özel kontrol

`LogoObjects.RestServiceWS.exe` gibi servislerde permission hatasında kontrol sırası:

```text
Service account
↓
Executable/DLL folder permission
↓
Config permission
↓
SQL connection
↓
Logo Objects dependencies
↓
Port binding
↓
Firewall
↓
Event Viewer
```

---

## Sonuç

Servis hesabı bir deployment detayı değil, uygulama mimarisinin güvenlik ve çalışma zamanı bileşenidir.
