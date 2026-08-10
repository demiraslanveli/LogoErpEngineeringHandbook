# 81 — Deadlock, Blocking ve Lock Analizi

Logo ERP ve entegrasyon servislerinde performans problemi her zaman yavaş sorgu değildir. Özellikle yoğun fatura, stok ve üretim hareketlerinde blocking ve deadlock ciddi kullanıcı şikayetlerine neden olabilir.

## Blocking ve deadlock farkı

Blocking:
- bir session diğerinin lock bırakmasını bekler.

Deadlock:
- iki veya daha fazla session birbirinin tuttuğu kaynağı bekler,
- SQL Server zinciri kırmak için bir session'ı victim seçer.

## İlk teşhis sorgusu

```sql
SELECT
    r.session_id,
    r.status,
    r.command,
    r.wait_type,
    r.wait_time,
    r.blocking_session_id,
    DB_NAME(r.database_id) AS Veritabani,
    s.host_name,
    s.program_name,
    s.login_name,
    t.text AS Sorgu
FROM sys.dm_exec_requests r
JOIN sys.dm_exec_sessions s
    ON s.session_id = r.session_id
CROSS APPLY sys.dm_exec_sql_text(r.sql_handle) t
WHERE r.session_id <> @@SPID
ORDER BY r.blocking_session_id DESC, r.session_id;
```

## Blocking zinciri

Ana blocker bulunmadan yalnızca bekleyen session'ları sonlandırmak problemi çözmez.

Kontrol edilmesi gerekenler:

- `blocking_session_id`
- transaction ne zamandır açık?
- sorgu hangi tabloyu güncelliyor?
- kullanıcı işlemi mi, trigger mı, servis mi?
- uzak bağlantı nedeniyle transaction açık mı kalıyor?

## Uzun açık transaction kontrolü

```sql
DBCC OPENTRAN;
```

ve DMV'ler birlikte değerlendirilmelidir.

## Deadlock kaynakları

Sık nedenler:

- aynı tabloların farklı sırada güncellenmesi,
- trigger içinde ek update işlemleri,
- gereksiz büyük transaction,
- uygun index olmaması nedeniyle fazla satır lock'lanması,
- batch işlem ile kullanıcı işleminin aynı kayıtları eşzamanlı değiştirmesi.

## Tutarlı erişim sırası

Örneğin bir servis belge işlemlerinde her zaman şu sırayı kullanabilir:

```text
Header
  ↓
Lines
  ↓
Auxiliary records
  ↓
Integration log
```

Farklı servislerin aynı kaynaklara ters sırada erişmesi deadlock riskini yükseltir.

## NOLOCK çözüm değildir

`WITH (NOLOCK)` blocking'i azaltabilir gibi görünse de dirty read, phantom read ve eksik/çift kayıt okuma riski oluşturur. Finansal ve stok raporlarında bilinçsiz kullanılmamalıdır.

## Deadlock analizi

Tercih edilen yöntemlerden biri Extended Events üzerinden `xml_deadlock_report` toplamaktır.

Analizde şu sorular cevaplanmalıdır:

1. Hangi session victim oldu?
2. Hangi tablolar/indexler kilitlendi?
3. Hangi sorgular kaynakları hangi sırada aldı?
4. Trigger veya uzun transaction var mı?
5. Index iyileştirmesi lock kapsamını küçültür mü?

## Entegrasyon servisleri için öneriler

- transaction kapsamını küçük tut,
- kullanıcı beklerken transaction açma,
- Logo Objects çağrısı öncesi uzun SQL okuması yapma,
- batch boyutunu sınırlı tut,
- transient deadlock hatalarında kontrollü retry uygula,
- idempotency olmadan kör retry yapma.

> Deadlock çözümü çoğu zaman “timeout artırmak” değil, erişim sırasını ve transaction tasarımını düzeltmektir.
