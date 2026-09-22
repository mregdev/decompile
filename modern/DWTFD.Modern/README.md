# DWTFD

Üniversite döneminde yazılan [orijinal DWTFD](https://github.com/mregdev/decompile) uygulamasının güncel Windows sürümü. Kaynak kod, WPF ve .NET 10 ile yeniden yazıldı.

Pencere 700 × 590 piksel sabittir. Başlıkta yalnızca kapatma düğmesi ve saydam arka planlı çöp kutusu simgesi bulunur.

## Kullanım

1. Temizlenecek alanları seçin.
2. **Tara** ile dosya sayısını ve tahmini boyutu görün.
3. **Temizle** ile onay verip işlemi başlatın.

Dört alan da açılışta seçilidir. **Son açılanlar listesi** Windows kabuk geçmişini sıfırlar; gerçek belgeleri silmez. **Prefetch** önbelleğini boşaltmak uygulama açılışını geçici olarak yavaşlatabilir; istemiyorsanız taramadan önce bu seçeneği kapatın.

Temizlik yönetici yetkisi istemeden çalışır. Erişilemeyen veya kullanımdaki dosyalar atlanır. Klasör kökleri ve sembolik bağlantılar silinmez. İşlemin ilerleyişi ve atlanan öğeler arayüzde gösterilir.

## Derleme

Windows üzerinde .NET 10 SDK ile:

```powershell
dotnet build DWTFD.Modern.csproj -c Release
```

Tek dosyalık, .NET kurulumu gerektirmeyen paket için:

```powershell
dotnet publish DWTFD.Modern.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true
```
