# 89 — ASYNC_NETWORK_IO ve İstemci Tüketim Problemleri

## 1. Amaç

`ASYNC_NETWORK_IO`, SQL Server'ın sonucu üretmiş olmasına rağmen istemcinin satırları yeterince hızlı tüketmemesi durumunda görülebilen wait type'lardan biridir.

Logo ERP, Excel, raporlama araçları veya özel entegrasyon servislerinde sık görülebilir.

## 2. Her Zaman Ağ Problemi Değildir

İsim yanıltıcı olabilir.

Olası nedenler:

- istemci satırları yavaş işliyor
- uygulama result set'i tek tek ve gecikmeli okuyor
- Excel / Office tarafı veriyi yavaş render ediyor
- gereğinden fazla kolon dönüyor
- milyonlarca satır istemciye gönderiliyor
- gerçek network darboğazı var

## 3. Teşhis

İlk soru:

> SQL Server sorguyu mu yavaş çalıştırıyor, yoksa sonucu tüketen uygulama mı yavaş?

Kontrol edilmesi gerekenler:

- elapsed time
- CPU time
- logical reads
- result row count
- network packet transfer süresi
- istemci uygulama davranışı

## 4. Büyük Result Set

Örneğin:

```sql
SELECT *
FROM LG_040_01_STLINE;
```

gibi sorgular uygulamaya çok büyük veri taşıyabilir.

Sorun SQL planından bağımsız olarak istemci tarafında ortaya çıkabilir.

## 5. Excel Senaryosu

Excel üzerinden ODBC/OLEDB ile çok büyük sonuç çekildiğinde:

- SQL sorgusu tamamlanmış gibi görünür
- session `suspended` olabilir
- wait type `ASYNC_NETWORK_IO` görülebilir

Bu durumda sorguyu sadece SQL Server tarafında optimize etmek yeterli olmayabilir.

## 6. Uygulama Tarafı İyileştirmeleri

- gereksiz kolonları kaldır
- filtreyi SQL'e taşı
- paging kullan
- result set'i streaming mantığıyla tüket
- UI thread üzerinde satır satır ağır işlem yapma
- büyük export işlemlerini kontrollü batch'lere böl

## 7. Rapor Tasarımı

Kullanıcıya 5 milyon satır göstermek çoğu zaman doğru rapor tasarımı değildir.

Önce aggregate veya özet sonuç sunulmalı, drill-down ile detay açılmalıdır.

## 8. Network Gerçekten Problemse

Kontrol edilebilir:

- network latency
- packet loss
- NIC saturation
- VM network katmanı
- uzak lokasyon bağlantısı

Ancak bu teşhis SQL wait type isminden tek başına çıkarılmamalıdır.

## 9. Logo Entegrasyon Servislerinde

API çağrısı sonucu binlerce satırı JSON/XML olarak dönmek:

- SQL süresini
- serialize süresini
- network transferini
- istemci parse süresini

birlikte etkiler.

Bu nedenle uçtan uca ölçüm gerekir.

## 10. Temel Prensip

> `ASYNC_NETWORK_IO` görüldüğünde yalnızca ağı değil, sonuç setinin boyutunu ve istemcinin tüketim hızını da incelemek gerekir.
