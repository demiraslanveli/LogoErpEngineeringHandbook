# 109 — Configuration Encryption ve Secret Yönetimi

Logo entegrasyon servislerinde connection string, Logo kullanıcı bilgisi, servis hesabı ve API anahtarları açık metin tutulmamalıdır.

## Korunması Gereken Bilgiler

- SQL Server connection string
- Logo Objects kullanıcı adı/parola
- servis kullanıcı bilgileri
- SMTP kimlik bilgileri
- API key / token
- dış sistem servis şifreleri

## Temel İlke

```text
Configuration != Secret Storage
```

`App.config` uygulama ayarlarını taşıyabilir ancak hassas bilgi için tek başına güvenli depolama değildir.

## .NET Framework 4.8 İçin Yaklaşımlar

### Protected Configuration

`connectionStrings` veya belirli config section'ları Windows seviyesinde şifrelenebilir.

Örnek kullanım yaklaşımı:

```text
App.config
   ↓
ProtectedConfigurationProvider
   ↓
Encrypted section
```

### DPAPI

Windows Data Protection API özellikle servis hesabına veya makineye bağlı secret koruması için kullanılabilir.

İki yaygın scope:

```text
CurrentUser
LocalMachine
```

Windows Service senaryosunda hangi hesabın decrypt edeceği tasarım aşamasında belirlenmelidir.

## Secret Provider Soyutlaması

```csharp
public interface ISecretProvider
{
    string GetSecret(string key);
}
```

Uygulama katmanı secret'ın nereden geldiğini bilmemelidir.

Olası implementasyonlar:

```text
DpapiSecretProvider
ProtectedConfigSecretProvider
EnvironmentSecretProvider
EnterpriseVaultSecretProvider
```

## Configuration Modeli

```csharp
public sealed class LogoEnvironmentOptions
{
    public int CompanyId { get; set; }
    public int PeriodId { get; set; }
    public string SqlConnectionName { get; set; }
    public string LogoCredentialKey { get; set; }
}
```

Burada doğrudan parola değil, secret'a erişim anahtarı tutulması tercih edilir.

## Log Güvenliği

Aşağıdaki bilgiler loglanmamalıdır:

- password
- token
- tam connection string
- SMTP password
- authentication header

Structured logging sırasında hassas alanlar maskelenmelidir.

Örnek:

```text
Server=ERPDB;Database=LOGO;User=logo_service;Password=***
```

## Secret Rotation

Uzun ömürlü entegrasyon servislerinde parola değişimi servis kodu değişikliği gerektirmemelidir.

İdeal süreç:

```text
Secret değişir
   ↓
Secret store güncellenir
   ↓
Servis kontrollü restart/reload
   ↓
Yeni credential kullanılır
```

## Servis Hesabı Bağlantısı

Windows Service mümkünse özel servis hesabıyla çalıştırılmalıdır.

Bu hesabın:

- yalnızca gerekli dosyalara
- gerekli SQL kaynaklarına
- gerekli Logo servislerine
- gerekli secret store alanlarına

erişimi olmalıdır.

## Production Kontrol Listesi

- config dosyasında açık parola var mı?
- source control içinde secret var mı?
- loglarda credential görünüyor mu?
- servis hesabının yetkileri gereğinden geniş mi?
- test ve production secret'ları ayrılmış mı?
- credential rotation prosedürü var mı?

> Secret yönetiminin amacı yalnızca şifreyi gizlemek değildir. Credential yaşam döngüsünü uygulama kodundan ayırmaktır.
