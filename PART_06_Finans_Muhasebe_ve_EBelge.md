# Part 06 — Finans, Muhasebe ve Elektronik Belgeler

Bu bölüm cari hareketler, muhasebe fişleri, döviz, KDV/istisna, yaşlandırma, e-Fatura/e-İrsaliye ve finansal belge zincirlerini kapsar.

## İlgili Bölümler

- 25 Sipariş / İrsaliye / Fatura İlişki Haritası
- 36 CLFLINE ve Cari Hareket Mantığı
- 37 Muhasebe Fişi ve EMFICHE / EMFLINE İlişkileri
- 40 INVOICE Alan Sözlüğü
- 50 PRCLIST Fiyat Kartları
- 51 PAYPLANS Ödeme Planları
- 59 Dispatch / Invoice Transaction İlişkileri
- 60 Döviz Alanları ve Kur Mantığı
- 61 KDV, İstisna ve Muafiyet Alanları
- 62 Cari Yaşlandırma ve FIFO Kapama Mantığı
- 74 e-Fatura ve e-İrsaliye Entegrasyon Bağlantıları
- 75 Muhasebe Entegrasyon Hataları ve Kontrol Listesi

## Temel Amaç

Bir ticari belgenin yalnızca faturadan ibaret olmadığını; stok, cari, muhasebe, vergi ve elektronik belge katmanlarıyla birlikte ele alınması gerektiğini göstermek.

## Tipik Zincir

```text
Sipariş
  ↓
İrsaliye / Stok Hareketi
  ↓
Fatura
  ↓
Cari Hareket
  ↓
Muhasebe Fişi
  ↓
e-Fatura / e-İrsaliye
  ↓
Reconciliation
```
