# 139 — Logo SDK Binding ve Session Doğrulama Standardı

Logo ERP entegrasyonunda en riskli alanlardan biri, hedef ortamda kullanılan Logo Objects / UnityApplication sürümüne ait COM tiplerini ve login davranışını doğrulamadan production koduna sabitlemektir.

Bu bölüm, referans uygulamadaki `LogoSessionAdapter` ve `ILogoSdkBridge` yapısının neden oluşturulduğunu ve gerçek SDK bağlantısının hangi kontrollerden geçirilmesi gerektiğini tanımlar.

## Temel Kural

`Application`, `Core`, `Infrastructure` ve `Worker` projeleri doğrudan UnityApplication COM tiplerini bilmemelidir.

```text
Worker / Application
        ↓
LogoSessionAdapter
        ↓
ILogoSdkBridge
        ↓
Verified Logo Objects Binding
        ↓
UnityApplication / Logo Objects
```

## Session State Kuralı

Session yalnızca gerçek SDK bridge login işlemi başarılı olduğunda açık kabul edilir.

Yanlış yaklaşım:

```text
Login çağrısı yapılmadı
        ↓
_isOpen = true
```

Doğru yaklaşım:

```text
Bridge.Login(...)
        ↓
Success kontrolü
        ↓
Bridge.IsLoggedIn kontrolü
        ↓
Session Open
```

## Doğrulanması Gereken Noktalar

Hedef Logo kurulumunda aşağıdaki bilgiler test edilmeden production adapter yazılmamalıdır:

- kullanılan COM assembly / interop referansı
- UnityApplication nesnesinin oluşturulma biçimi
- login metodu ve parametre sırası
- firma seçimi
- dönem seçimi
- logout davranışı
- login sonrası session state kontrolü
- hata kodu / hata açıklaması alma yöntemi
- COM nesnesi release davranışı
- aynı process içindeki çoklu session davranışı
- servis hesabı altında COM aktivasyonu
- 32-bit / 64-bit bağımlılıkları

## Fail-Fast Yaklaşımı

Referans uygulamada doğrulanmış SDK binding yoksa:

```text
UnconfiguredLogoSdkBridge
```

kullanılır ve sistem bilinçli olarak başarılı login raporlamaz.

Örnek hata kodu:

```text
LOGO_SDK_NOT_CONFIGURED
```

Bu davranışın amacı, entegrasyonun sessiz biçimde yanlış çalışmasını engellemektir.

## Health Check

Session açıldıktan sonra yalnızca `IsLoggedIn` kontrolü yeterli değildir. Adapter seviyesinde hafif bir `Ping` mekanizması bulunmalıdır.

```text
LogoSessionAdapter.CheckHealth()
        ↓
ILogoSdkBridge.Ping()
```

Ping implementasyonu sürüme göre değişebilir. Amaç ağır ERP işlemi yapmak değil, session'ın halen kullanılabilir olduğunu doğrulamaktır.

## COM Yaşam Döngüsü

COM nesnelerinde yaşam döngüsü deterministik yönetilmelidir.

```text
Create
Login
Use
Logout
Release COM references
Dispose
```

Özellikle Windows Service senaryosunda session nesnelerinin belirsiz süre boyunca GC'ye bırakılması önerilmez.

## Threading

Logo Objects COM nesnelerinin thread davranışı sürüm ve kurulum bazında doğrulanmalıdır.

Doğrulanmadıkça aşağıdaki varsayım güvenlidir:

```text
Bir Logo session
    ↓
Tek kontrollü execution scope
```

Aynı COM session nesnesini rastgele paralel worker thread'leri arasında paylaşmak önerilmez.

## Production Onay Checklist

- [ ] SDK assembly sürümü kayıt altına alındı
- [ ] Platform target doğrulandı
- [ ] Login test edildi
- [ ] Firma/dönem doğru açılıyor
- [ ] Yanlış kullanıcı test edildi
- [ ] Yanlış firma/dönem test edildi
- [ ] Logout test edildi
- [ ] COM release test edildi
- [ ] Windows Service hesabı ile login test edildi
- [ ] Session drop sonrası davranış test edildi
- [ ] Health check test edildi

> Session katmanında tahmine dayalı başarı durumu üretmek, Logo entegrasyonlarında veri hatasından önce gelen en tehlikeli mimari hatalardan biridir.
