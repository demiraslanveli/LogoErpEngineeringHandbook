# 76 — Scheduled Job ve Background Worker Mimarisi

## 1. Amaç

Bu bölüm, Logo ERP entegrasyonlarında zamanlanmış görevlerin ve arka plan worker servislerinin güvenilir biçimde nasıl tasarlanması gerektiğini açıklar.

## 2. Kullanım Alanları

Tipik scheduled job örnekleri:

- Borç takip e-postaları
- 7 gün kala uyarıları
- Fatura yasal süre kontrolleri
- Stok kritik seviye kontrolleri
- Entegrasyon retry işlemleri
- Reconciliation kontrolleri
- Gece veri senkronizasyonu
- Rapor üretimi

## 3. Scheduler ile Worker Ayrımı

Scheduler yalnızca "ne zaman çalışacağını" belirlemelidir.

İş mantığı ayrı worker/service katmanında bulunmalıdır.

```text
Scheduler
    ↓
Job Trigger
    ↓
Application Service
    ↓
Logo Objects / SQL
```

## 4. Tek Çalıştırma Garantisi

Aynı job'ın aynı anda iki kez çalışması engellenmelidir.

Yöntemler:

- SQL application lock
- Job lock tablosu
- Distributed lock
- Scheduler concurrency policy

## 5. Job Run Tablosu

Örnek:

```text
JOB_RUN
-------
ID
JOB_NAME
STARTED_AT
FINISHED_AT
STATUS
HOST_NAME
INSTANCE_ID
AFFECTED_ROWS
ERROR_MESSAGE
```

## 6. Idempotent Job

Job tekrar çalıştırıldığında aynı kaydı ikinci kez üretmemelidir.

Örnek mail job:

```text
DOCUMENT_REF + NOTIFICATION_TYPE + NOTIFICATION_DATE
```

benzersiz anahtar olarak kullanılabilir.

## 7. Uzun Süren İşler

Uzun job'lar küçük batch'lere ayrılmalıdır.

Örnek:

```text
500 kayıt seç
    ↓
İşle
    ↓
Checkpoint kaydet
    ↓
Sonraki 500
```

Bu yapı restart sonrası kaldığı yerden devam etmeyi kolaylaştırır.

## 8. Logo Objects Session Yönetimi

Uzun süre açık kalan COM session yerine kontrollü yaşam döngüsü tercih edilmelidir.

Örnek:

```text
Job başladı
    ↓
Login
    ↓
Batch işle
    ↓
Logout / release
```

Çok uzun batch'lerde belirli sayıda işlem sonrası session yeniden oluşturulabilir.

## 9. Retry

Transient hatalar retry edilebilir:

- Geçici SQL bağlantı hatası
- Network timeout
- Servis geçici erişilemiyor

Kalıcı veri hataları retry edilmemelidir.

## 10. Timeout

Her job için maksimum çalışma süresi tanımlanmalıdır.

Sonsuza kadar çalışan job operasyonel risk yaratır.

## 11. Heartbeat

Uzun worker'larda heartbeat tutulabilir:

```text
WORKER_NAME
INSTANCE_ID
LAST_HEARTBEAT
CURRENT_JOB
CURRENT_ITEM
```

Bu sayede worker'ın kilitlenip kilitlenmediği görülebilir.

## 12. Mail Gönderim Kuyruğu

Mail işlemlerinde önerilen yapı:

```text
Business Rule
    ↓
MAIL_QUEUE
    ↓
Mail Worker
    ↓
SMTP / Database Mail
```

İş transaction'ı içinde doğrudan mail gönderilmesi önerilmez.

## 13. SQL Agent ve Windows Service

### SQL Agent

Uygun kullanım:

- SQL ağırlıklı bakım
- Stored procedure çalıştırma
- Basit raporlama

### Windows Service / Worker Service

Uygun kullanım:

- Logo Objects COM
- API çağrıları
- Queue tüketimi
- Karmaşık retry ve orchestration

## 14. İzlenmesi Gereken Metrikler

- Son başarılı çalışma
- Son hata
- Ortalama süre
- İşlenen kayıt sayısı
- Hata sayısı
- Retry sayısı
- Queue backlog

## 15. Sonuç

Scheduled job mimarisinde temel hedef yalnızca görevin çalışması değildir.

Görev:

- Tekil çalışmalı,
- tekrar çalıştırılabilir olmalı,
- idempotent olmalı,
- hatası izlenebilmeli,
- kaldığı yerden devam edebilmeli,
- Logo session kaynaklarını doğru yönetmelidir.
