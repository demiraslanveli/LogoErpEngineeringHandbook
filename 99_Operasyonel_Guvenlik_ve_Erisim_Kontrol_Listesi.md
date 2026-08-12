# 99 — Operasyonel Güvenlik ve Erişim Kontrol Listesi

## Amaç

Logo ERP, SQL Server, Objects servisleri ve entegrasyon worker'larının birlikte çalıştığı ortamlarda güvenlik yalnızca kullanıcı parolası konusu değildir.

Gerçek operasyonel güvenlik aşağıdaki katmanların birlikte kontrol edilmesiyle sağlanır:

```text
Windows
SQL Server
Logo ERP
Logo Objects
Windows Services
Network
File Shares
Scheduled Jobs
Secrets
Audit
```

---

## 1. SQL yetkileri

Kontrol:

- Gereksiz `sysadmin` var mı?
- Servis hesapları `db_owner` mı?
- Read-only servislerin DML yetkisi var mı?
- Kritik stored procedure'ler kontrollü EXECUTE permission ile mi çalışıyor?
- Eski kullanıcı/login'ler kaldırılmış mı?

---

## 2. Logo kullanıcıları

Kontrol:

- Kullanılmayan Logo kullanıcıları aktif mi?
- Entegrasyon için kullanılan Logo kullanıcıları kişisel kullanıcılarla aynı mı?
- Yetki grupları iş ihtiyacına uygun mu?
- Firma/dönem erişimleri gereğinden geniş mi?

Entegrasyon kullanıcısının ayrı tanımlanması audit açısından faydalıdır.

---

## 3. Service account

Kontrol:

- Servis LocalSystem ile mi çalışıyor?
- Domain service account kullanılabiliyor mu?
- Hesap interaktif login için gereksiz yere açık mı?
- Password expiration servis kesintisine yol açabilir mi?
- Dosya ve share yetkileri minimum mu?

---

## 4. Connection string

Kontrol:

- Repository içinde parola var mı?
- `app.config` / `web.config` düz metin secret içeriyor mu?
- Log içine connection string yazılıyor mu?
- Test ortamı credential'ı production'da kullanılıyor mu?

---

## 5. Network

Kontrol:

- SQL portu gereksiz subnet'lere açık mı?
- REST Service tüm network'e mi yayın yapıyor?
- Firewall rule'ları belgeli mi?
- SMB share erişimi sınırlandırılmış mı?
- Eski VPN / remote access hesapları aktif mi?

---

## 6. Dosya paylaşımı

Logo entegrasyonlarında XML, Excel, PDF, e-belge ve attachment klasörleri bulunabilir.

Kontrol:

```text
Who can read?
Who can write?
Who can delete?
Who can change ACL?
```

Entegrasyon klasöründe `Everyone: Full Control` kullanılmamalıdır.

---

## 7. SQL Agent

Kontrol:

- Job owner geçerli mi?
- Job sysadmin kullanıcıya bağımlı mı?
- Credential/proxy gereksiz yetkili mi?
- Job step içinde düz metin parola var mı?
- Failed job alarmı var mı?

---

## 8. Database Mail

Kontrol:

- Mail profiline kimler erişebiliyor?
- SMTP credential nerede tutuluyor?
- Mail ile hassas ticari veri gönderiliyor mu?
- Recipient listeleri kontrol altında mı?
- Test adresi production'da kalmış mı?

---

## 9. Backup güvenliği

Kontrol:

- `.bak` dosyalarını kim okuyabilir?
- Backup farklı lokasyona kopyalanıyor mu?
- Eski backup dosyaları kontrolsüz tutuluyor mu?
- Backup encryption değerlendirildi mi?
- Restore yetkisi sınırlı mı?

---

## 10. Audit

Kritik operasyonlarda izlenmesi gereken minimum alanlar:

```text
Who
When
Where
Company
Period
Object
Operation
Before/After veya result
Correlation Id
```

Özellikle doğrudan SQL DML kullanılan istisnai işlemler audit edilmelidir.

---

## 11. Deployment

Kontrol:

- Production config source control'a yanlışlıkla commit edilmiş mi?
- DLL değişikliği kim tarafından yapıldı?
- Rollback paketi var mı?
- Release versiyonu log'da görünüyor mu?
- Servis restart prosedürü belgeli mi?

---

## 12. Erişim gözden geçirme

Yetkiler yalnızca kullanıcı oluşturulurken kontrol edilmemelidir.

Periyodik olarak:

```text
SQL logins
Windows service accounts
Logo users
VPN users
File share ACLs
Repository permissions
```

gözden geçirilmelidir.

---

## Sonuç

Operasyonel güvenlik tek bir ürün ayarı değildir. Logo ERP ekosistemindeki tüm çalışma katmanlarının minimum yetki, izlenebilirlik ve kontrollü değişiklik prensibiyle yönetilmesidir.
