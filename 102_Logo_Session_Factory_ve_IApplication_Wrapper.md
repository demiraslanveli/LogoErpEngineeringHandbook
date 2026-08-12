# 102 — Logo Session Factory ve IApplication Wrapper

Bu bölüm Logo Objects oturumlarının kontrollü şekilde oluşturulması, firma/dönem context'i ile ilişkilendirilmesi ve uygulama servislerinden izole edilmesini ele alır.

## Amaç

Application katmanının doğrudan her yerde `UnityApplication` veya `IApplication` üretmesi yerine tek sorumluluklu bir session factory kullanılmalıdır.

```text
Application Service
       ↓
ILogоSessionFactory
       ↓
LogoSession
       ↓
IApplication / UnityApplication
```

## Session Abstraction

```csharp
public interface ILogoSession : IDisposable
{
    LogoCompanyContext Context { get; }

    object Application { get; }

    bool IsLoggedIn { get; }
}
```

`Application` tipi gerçek projede kullanılan Logo Objects interop tipine göre somutlaştırılmalıdır.

## Factory

```csharp
public interface ILogoSessionFactory
{
    ILogoSession Create(LogoCompanyContext context);
}
```

Örnek iskelet:

```csharp
public sealed class LogoSessionFactory : ILogoSessionFactory
{
    public ILogoSession Create(LogoCompanyContext context)
    {
        if (context == null)
            throw new ArgumentNullException(nameof(context));

        return new LogoSession(context);
    }
}
```

## LogoSession Sorumlulukları

Session şu işleri üstlenmelidir:

- Logo application nesnesini oluşturmak
- login yapmak
- firma/dönem context'ini uygulamak
- login sonucunu doğrulamak
- hata bilgisini üst katmana aktarmak
- logout yapmak
- COM nesnelerini güvenli şekilde serbest bırakmak

## Neden Wrapper?

Uygulama kodunun her noktasında doğrudan Logo API çağrısı yapılırsa:

- login/logout tekrarları oluşur
- session leak riski artar
- test etmek zorlaşır
- hata işleme dağılır
- çoklu firma/dönem yönetimi zorlaşır
- servis katmanı Logo COM detaylarına bağımlı hale gelir

Wrapper bu bağımlılığı tek noktaya toplar.

## Kullanım

```csharp
public sealed class MaterialService
{
    private readonly ILogoSessionFactory _sessionFactory;

    public MaterialService(ILogoSessionFactory sessionFactory)
    {
        _sessionFactory = sessionFactory;
    }

    public void Execute(
        LogoCompanyContext context)
    {
        using (var session = _sessionFactory.Create(context))
        {
            // Logo işlemleri
        }
    }
}
```

## Session Yaşam Süresi

Bir session'ın yaşam süresi sınırsız olmamalıdır.

Tercih edilen model:

```text
1 logical integration operation
        ↓
1 Logo session scope
        ↓
Dispose
```

Batch senaryolarında session yeniden kullanımı performans sağlayabilir ancak kontrollü olmalıdır.

Örnek:

```text
Batch = 100 kayıt
Session aç
  kayıt 1
  kayıt 2
  ...
  kayıt 100
Session kapat
```

Her kayıt için yeni login yapmak gereksiz maliyet oluşturabilir.

## Thread Safety

Bir `IApplication` instance'ı farklı worker thread'leri arasında paylaşılmamalıdır.

```text
Worker 1 → Session A
Worker 2 → Session B
Worker 3 → Session C
```

Aynı session üzerinde paralel Logo Objects işlemi yapılması güvenli kabul edilmemelidir.

## Failure Model

Session factory hata durumunda açık bir exception veya result üretmelidir.

Örnek hata türleri:

- application oluşturulamadı
- login başarısız
- firma geçersiz
- dönem geçersiz
- lisans problemi
- COM registration problemi
- Logo service erişim problemi

## COM Kaynak Yönetimi

Session dispose edilirken:

1. açık Logo nesneleri kapatılmalı
2. logout uygulanmalı
3. COM referansları bırakılmalı
4. managed referanslar temizlenmeli

COM serbest bırakma implementasyonu kullanılan Logo Objects sürümüne ve interop tiplerine göre doğrulanmalıdır.

## Temel Kural

> Uygulama servisleri Logo oturumunun nasıl açıldığını bilmemeli; yalnızca `ILogoSessionFactory` üzerinden geçerli bir session istemelidir.
