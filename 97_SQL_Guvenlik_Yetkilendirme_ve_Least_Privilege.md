# 97 — SQL Güvenlik, Yetkilendirme ve Least Privilege

## Amaç

Logo ERP ve çevre servislerinde kullanılan SQL hesapları çoğu zaman gereğinden geniş yetkilerle tanımlanır. Bu yaklaşım kısa vadede kolay görünür ancak güvenlik, denetim ve hata etkisi açısından risklidir.

Temel prensip:

```text
Bir hesap yalnızca ihtiyaç duyduğu yetkilere sahip olmalıdır.
```

---

## Hesap türlerini ayır

Aşağıdaki roller mümkünse aynı SQL hesabını paylaşmamalıdır:

- Logo uygulama hesabı
- Logo Objects / REST servis hesabı
- Raporlama hesabı
- ETL / entegrasyon hesabı
- SQL Agent proxy / job hesabı
- DBA hesabı
- Read-only monitoring hesabı

---

## Raporlama hesabı

Raporlama kullanıcıları için ideal yaklaşım read-only yetkidir.

Örnek:

```sql
CREATE USER ReportUser FOR LOGIN ReportLogin;
ALTER ROLE db_datareader ADD MEMBER ReportUser;
```

Ancak geniş `db_datareader` yerine yalnızca belirli view veya schema'lara `SELECT` vermek daha kontrollü olabilir.

---

## DML yetkileri

Entegrasyon hesabına doğrudan `db_owner` vermek yerine yalnızca gerekli stored procedure execute yetkileri tercih edilmelidir.

Örnek:

```sql
GRANT EXECUTE ON dbo.SP_LOGO_FATURA_TARIH_GUNCELLE TO IntegrationUser;
```

Bu yaklaşım doğrudan tablo `UPDATE` yetkisinden daha güvenlidir.

---

## Schema bazlı yetki

Özel entegrasyon objelerini ayrı schema altında toplamak yönetimi kolaylaştırır.

Örnek:

```text
logo_core
integration
reporting
audit
```

Böylece permission yönetimi obje bazında dağılmaz.

---

## SQL Login mi Windows Account mu?

Kurumsal ortamda mümkün olduğunda Windows/AD tabanlı servis hesabı tercih edilir.

Avantajları:

- Merkezi parola politikası
- Hesap yaşam döngüsü yönetimi
- Audit kolaylığı
- Secret dağıtım ihtiyacının azalması

Ancak kullanılan Logo bileşeninin desteklediği authentication modeli sürüme göre teyit edilmelidir.

---

## Secret yönetimi

Connection string içinde düz metin kullanıcı/parola bırakmak risklidir.

Tercih sırası ortam imkanına göre:

```text
Managed identity / integrated auth
↓
Secret store
↓
Encrypted configuration
↓
Düz metin config — kaçınılmalı
```

---

## Yetki değişikliklerini logla

Aşağıdaki değişiklikler kontrollü change sürecinden geçmelidir:

- sysadmin verme
- db_owner verme
- ALTER/CONTROL permission
- Linked Server security değişiklikleri
- Credential değişiklikleri
- SQL Agent proxy değişiklikleri

---

## Login başarısızlıkları

Servis bağlantı sorunlarında yalnızca uygulama loguna bakılmamalıdır.

Kontrol:

- SQL Error Log
- Windows Event Log
- Login disabled mı?
- Default database online mı?
- Password expired mı?
- Network protocol / port erişimi var mı?

---

## Audit yaklaşımı

Kritik servis hesapları için en azından şu bilgiler izlenmelidir:

```text
Hangi hesap
Hangi uygulamadan
Hangi sunucudan
Hangi veritabanına
Ne zaman bağlandı
Hangi kritik procedure'leri çalıştırdı
```

---

## Logo ERP açısından önemli kural

Entegrasyon hesabının `UPDATE` yetkisi olması, Logo tablolarına doğrudan yazılması gerektiği anlamına gelmez.

Veri bütünlüğü için resmi transaction üretiminde Objects/ERP iş kuralları tercih edilmelidir.

---

## Sonuç

Yetkilendirme tasarımı yalnızca güvenlik konusu değildir. Hatalı bir servis hesabının etkisini sınırlandıran temel operasyon kontrolüdür.
