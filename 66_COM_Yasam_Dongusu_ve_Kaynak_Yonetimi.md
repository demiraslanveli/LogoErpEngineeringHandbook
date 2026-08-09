# 66 — COM Yaşam Döngüsü ve Kaynak Yönetimi

## Amaç

Logo Objects entegrasyonlarında yalnızca doğru metodu çağırmak yeterli değildir. COM tabanlı nesnelerin yaşam döngüsü doğru yönetilmezse zaman içinde memory leak, kilitlenme, session birikmesi veya beklenmeyen servis davranışları oluşabilir.

## Temel İlke

Logo Objects nesneleri uzun süre kontrolsüz biçimde global/static tutulmamalıdır.

Önerilen kapsam:

```text
Request / Job
  ↓
Logo session aç
  ↓
IData / IQuery işlemleri
  ↓
Nesneleri serbest bırak
  ↓
Logout
```

## IApplication Yaşam Döngüsü

`IApplication` uygulamanın giriş noktasıdır.

Kontrol edilmesi gerekenler:

- Login başarılı mı?
- Firma/dönem seçimi doğru mu?
- Aynı instance farklı thread'lerde paylaşılıyor mu?
- Logout garanti altında mı?

## try/finally Kullanımı

Kaynak temizliği exception durumunda da çalışmalıdır.

C# yaklaşımı:

```csharp
UnityApplication app = null;
try
{
    app = new UnityApplication();
    // login ve işlemler
}
finally
{
    if (app != null)
    {
        try
        {
            app.UserLogout();
        }
        catch
        {
        }

        if (System.Runtime.InteropServices.Marshal.IsComObject(app))
            System.Runtime.InteropServices.Marshal.FinalReleaseComObject(app);
    }
}
```

> Metot adları ve sınıf isimleri kullanılan Logo Objects sürümüne göre doğrulanmalıdır.

## IData / IQuery Nesneleri

Her iş için yeni nesne oluşturulması çoğu senaryoda daha güvenli bir yaklaşımdır.

```text
IApplication
 ├── IData #1
 ├── IData #2
 └── IQuery #1
```

İş bittikten sonra bu alt COM nesneleri de serbest bırakılmalıdır.

## FinalReleaseComObject

COM nesnelerinde .NET GC'nin ne zaman çalışacağı garanti değildir.

Uzun ömürlü servislerde kontrollü release önem kazanır.

Ancak yanlış kullanım da sorun yaratabilir:

- Başka bir kod aynı RCW nesnesini kullanıyorsa erken release crash yaratabilir.
- Aynı COM instance farklı katmanlarda paylaşılıyorsa ownership belirsizleşir.

Bu nedenle ownership tek bir katmanda olmalıdır.

## GC.Collect Kullanılmalı mı?

Her request sonunda `GC.Collect()` çağırmak doğru çözüm değildir.

Bu yaklaşım:

- performansı düşürür,
- gerçek yaşam döngüsü hatasını gizler,
- latency spike oluşturabilir.

Öncelik COM referanslarını doğru release etmektir.

## Static Singleton Riski

Şu yaklaşım risklidir:

```csharp
public static UnityApplication App = new UnityApplication();
```

Özellikle web servisinde bu nesnenin:

- thread-safe olduğu varsayılır,
- session state karışabilir,
- farklı firma/dönem talepleri birbirini etkileyebilir.

## Session Pool

Performans ihtiyacı nedeniyle her request'te login maliyetli oluyorsa kontrollü session pool tasarlanabilir.

Ancak pool entry şu bilgileri taşımalıdır:

```text
SessionId
CompanyNr
PeriodNr
UserId
CreatedAt
LastUsedAt
InUse
Healthy
```

Pool'daki bir session aynı anda iki iş tarafından kullanılmamalıdır.

## Windows Service Senaryosu

Uzun ömürlü worker servislerinde düzenli health kontrolü gerekir.

Şüpheli durumlarda session yeniden oluşturulabilir:

```text
Logo call failed
→ session unhealthy işaretle
→ instance release
→ yeni login
→ retry policy uygunsa tekrar dene
```

## Apartment State

COM bileşenlerinin threading modeli önemlidir.

Bazı COM bileşenleri STA beklentisine sahip olabilir. Bu nedenle uygulamanın kullandığı Logo Objects sürümü ve COM threading davranışı gerçek ortamda test edilmelidir.

## Sonuç

Logo Objects servislerinde stabiliteyi belirleyen kritik konulardan biri COM yaşam döngüsüdür. Nesnelerin sahipliği, login/logout sınırı ve thread kullanımı açık biçimde tasarlanmalıdır.
