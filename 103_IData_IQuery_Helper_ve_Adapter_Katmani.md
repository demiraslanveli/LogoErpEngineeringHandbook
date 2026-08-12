# 103 — IData / IQuery Helper ve Adapter Katmanı

Bu bölüm Logo Objects erişimini uygulama servislerinden izole eden adapter/helper katmanını tanımlar.

## Amaç

Uygulama servisleri doğrudan her yerde `IData`, `IQuery`, `DataFields`, `Lines`, `SetSqlText`, `ExecuteDirect` gibi Logo API detaylarıyla uğraşmamalıdır.

Tercih edilen ayrım:

```text
Application Service
      ↓
Logo Adapter
      ↓
IData / IQuery
      ↓
Logo Objects
```

## IData Adapter

```csharp
public interface ILogoDataAdapter
{
    LogoOperationResult Create(
        ILogoSession session,
        int dataObjectType,
        Action<object> map);

    LogoOperationResult Update(
        ILogoSession session,
        int dataObjectType,
        int logicalRef,
        Action<object> map);
}
```

`dataObjectType` ve gerçek COM tipleri kullanılan Logo Objects sürümüne göre doğrulanmalıdır.

## IData İşlem Akışı

```text
NewDataObject
    ↓
New / Read
    ↓
DataFields doldur
    ↓
Lines doldur
    ↓
Validation
    ↓
Post
    ↓
ErrorInfo / Result
```

## Mapping'i Ayrı Tutmak

Yanlış yaklaşım:

```csharp
public void CreateMaterial(MaterialDto dto)
{
    // login
    // NewDataObject
    // field mapping
    // Post
    // log
    // error parse
}
```

Bu yapı tüm sorumlulukları tek metoda toplar.

Daha sürdürülebilir yaklaşım:

```text
MaterialService
   ↓
MaterialMapper
   ↓
IData Adapter
   ↓
Logo Objects
```

## Field Helper

Logo field erişimleri için yardımcı metotlar kullanılabilir.

```csharp
public static class LogoFieldHelper
{
    public static void SetString(
        dynamic data,
        string fieldName,
        string value)
    {
        data.DataFields.FieldByName(fieldName).Value = value ?? string.Empty;
    }

    public static void SetNumber(
        dynamic data,
        string fieldName,
        double value)
    {
        data.DataFields.FieldByName(fieldName).Value = value;
    }
}
```

`dynamic` yalnızca örnek basitleştirmesidir. Üretim kodunda interop tipleri biliniyorsa strongly typed kullanım tercih edilmelidir.

## IQuery Adapter

IQuery çoğunlukla şu amaçlarla kullanılmalıdır:

- referans lookup
- varlık kontrolü
- raporlama
- entegrasyon ön kontrolü
- mapping tablolarını okuma

```csharp
public interface ILogoQueryAdapter
{
    T ExecuteScalar<T>(
        ILogoSession session,
        string sql,
        Func<object, T> mapper);
}
```

## SQL DML Sınırı

IQuery üzerinden SQL `INSERT`, `UPDATE`, `DELETE` teknik olarak mümkün olsa bile ERP nesneleri üzerinde birincil yazma yolu olarak kullanılmamalıdır.

Ana kural:

```text
ERP nesnesi değişikliği → IData
Kontrol / lookup / rapor → IQuery veya SQL read
```

## LogicalRef Lookup

Sık kullanılan kontrol:

```text
External code
    ↓
IQuery lookup
    ↓
LOGICALREF
    ↓
IData Read / Update
```

Örneğin malzeme kodundan `ITEMS.LOGICALREF` bulunabilir; fakat malzeme kartı değişikliği yine IData üzerinden yapılmalıdır.

## Lines Helper

Fiş ve faturalar için satır ekleme operasyonu tekrar eden bir desendir.

```csharp
public static dynamic AppendLine(dynamic lines)
{
    lines.AppendLine();
    return lines[lines.Count - 1];
}
```

Gerçek Logo Objects API'sindeki Lines erişim biçimi kullanılan sürüm ile doğrulanmalıdır.

## Adapter'ın Yapmaması Gerekenler

Adapter katmanı:

- ticari iş kuralı üretmemeli
- DTO doğrulamasının tamamını üstlenmemeli
- firma/dönem seçmemeli
- kullanıcı ekranı mesajı üretmemeli

Bunlar üst katman sorumluluklarıdır.

## Temel Kural

> IData ve IQuery uygulama servislerinin içinde dağılmamalı; Logo'ya özgü erişim adapter katmanında merkezileştirilmelidir.
