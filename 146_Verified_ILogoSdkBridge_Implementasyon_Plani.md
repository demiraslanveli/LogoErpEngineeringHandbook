# 146 — Verified ILogoSdkBridge Implementasyon Planı

Bu bölüm, referans uygulamadaki `ILogoSdkBridge` sözleşmesinin gerçek Logo Objects / UnityApplication COM nesnesine bağlanması için production standardını tanımlar.

## Amaç

`LogoSessionAdapter` doğrudan COM API detaylarını bilmemelidir. Gerçek SDK bağlantısı yalnızca `VerifiedLogoSdkBridge` içinde bulunmalıdır.

```text
LogoSessionAdapter
      ↓
ILogoSdkBridge
      ↓
VerifiedLogoSdkBridge
      ↓
UnityApplication / Logo Objects
```

## Zorunlu Sorumluluklar

`VerifiedLogoSdkBridge` aşağıdaki görevleri üstlenmelidir:

- doğrulanmış Logo COM tipini oluşturmak,
- kullanıcı bilgileri ile login olmak,
- firma ve dönem context'ini uygulamak,
- login sonucunu gerçekten doğrulamak,
- SDK sürümünü `LogoSdkBindingManifest` ile karşılaştırmak,
- logout yapmak,
- COM nesnesini deterministik olarak bırakmak,
- exception ve Logo hata bilgisini normalize etmek.

## Fail-Fast İlkesi

Binding manifest doğrulanmamışsa bridge session açmamalıdır.

```text
Manifest doğrulanmış mı?
        ↓
Hayır → LOGO_SDK_BINDING_NOT_VERIFIED
Evet  → COM oluştur → login dene
```

## Önerilen Constructor

```csharp
public VerifiedLogoSdkBridge(
    LogoErpOptions options,
    LogoSdkBindingManifest manifest)
```

Bridge kendi içine hard-coded firma/dönem veya kullanıcı almamalıdır.

## Login Akışı

```text
Create COM Application
    ↓
Verify Binding Manifest
    ↓
Login
    ↓
Select Company / Period
    ↓
Verify Session State
    ↓
Expose IsLoggedIn = true
```

Gerçek metot adları hedef Logo Objects sürümünde Object Browser veya çalışan örnek üzerinden doğrulanmadan handbook'a kesin API olarak yazılmaz.

## Logout Akışı

Logout idempotent olmalıdır. Birden fazla çağrıda exception üretmemelidir.

```text
if (!IsLoggedIn)
    return;

Try Logout
Finally Release COM
```

## COM Scope

Uzun yaşayan worker süreçlerinde COM application nesnesi global static tutulmamalıdır. Session sahibi açıkça belirlenmelidir.

Önerilen model:

```text
Worker Iteration / Scoped Operation
          ↓
LogoSessionAdapter
          ↓
VerifiedLogoSdkBridge
          ↓
COM Application
```

## Thread Kuralı

Logo Objects COM bileşeninin thread-safe olduğu doğrulanmadan aynı application/session nesnesi paralel thread'lerde paylaşılmamalıdır.

## Hata Kodları

Referans adapter seviyesinde önerilen hata kodları:

```text
LOGO_SDK_BINDING_NOT_VERIFIED
LOGO_COM_CREATE_FAILED
LOGO_LOGIN_FAILED
LOGO_COMPANY_CONTEXT_FAILED
LOGO_PERIOD_CONTEXT_FAILED
LOGO_SESSION_STATE_INVALID
LOGO_LOGOUT_FAILED
```

## Production Kabul Kriteri

Verified bridge ancak aşağıdakiler test edildikten sonra production'a alınmalıdır:

- doğru kullanıcı ile başarılı login,
- yanlış parola ile kontrollü hata,
- geçersiz firma ile kontrollü hata,
- geçersiz dönem ile kontrollü hata,
- logout sonrası session state false,
- COM process/resource sızıntısı olmaması,
- worker restart sonrası tekrar login başarısı.

> Bu sınıf Logo ERP entegrasyonunun güven sınırıdır. Session gerçekten doğrulanmadan application katmanına başarı sinyali verilmemelidir.
