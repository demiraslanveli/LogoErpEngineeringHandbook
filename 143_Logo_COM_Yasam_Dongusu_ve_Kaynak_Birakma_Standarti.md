# 143 — Logo COM Yaşam Döngüsü ve Kaynak Bırakma Standardı

Logo Objects tabanlı uzun yaşayan servislerde yalnızca doğru `IData` kullanmak yeterli değildir. COM nesnelerinin yaşam döngüsü de deterministik yönetilmelidir.

## Temel Kural

Logo SDK / COM nesnesi hangi scope içinde oluşturulduysa aynı scope içinde bırakılmalıdır.

```text
Worker Iteration / Operation Scope
        ↓
Logo Session
        ↓
IData / Query / ProductionApplication
        ↓
Operation Complete
        ↓
COM Release
        ↓
Session Close
```

## Neden Önemli?

Yanlış COM yaşam döngüsü uzun çalışan Windows Service süreçlerinde aşağıdaki problemlere dönüşebilir:

- process memory artışı,
- RCW birikmesi,
- stale session,
- logout edilmemiş Logo oturumları,
- servis restart ihtiyacının artması,
- aynı COM nesnesinin farklı thread'lerde yanlış kullanımı.

## Referans Helper

Referans uygulamada:

```text
src/LogoErp.Reference.LogoAdapter/Interop/ComReleaseHelper.cs
```

oluşturulmuştur.

Helper yalnızca gerçekten COM nesnesi olan instance'ları bırakır ve `FinalReleaseComObject` sonrası referansı temizleme modelini destekler.

## Scope İlkesi

COM nesnesini static singleton yapmak varsayılan yaklaşım değildir.

Önerilen model:

```text
Open Session
  ↓
Create IData
  ↓
Set Fields
  ↓
Post
  ↓
Read Error / Result
  ↓
Release IData
  ↓
Close Session
```

SDK açıkça thread-safe garanti vermediği sürece bir session'ın birden fazla worker thread tarafından ortak kullanıldığı kabul edilmemelidir.

## GC Bir Kaynak Yönetim Stratejisi Değildir

`GC.Collect()` çağırmak COM yaşam döngüsünün yerine geçmez. Öncelik explicit release ve doğru scope tasarımıdır.

## Üretim Kontrol Listesi

- Logo session owner bellidir.
- IData owner bellidir.
- ProductionApplication owner bellidir.
- Close/logout her hata yolunda çağrılabilir durumdadır.
- Exception halinde cleanup yapılır.
- Released COM nesnesi tekrar kullanılmaz.
- Thread ownership dokümante edilir.

> Logo entegrasyon servislerinde kaynak yönetimi bir performans optimizasyonu değil, doğruluk şartıdır.
