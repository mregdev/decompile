# DWTFD

Üniversite döneminde yazılan [orijinal DWTFD](https://github.com/mregdev/decompile) uygulamasının güncel Windows sürümü. Kaynak kod WPF ile yeniden yazıldı. Dağıtım dosyası yaklaşık 360 KB'tır; .NET Framework 4.8 kullandığı için çalışma ortamı EXE içine eklenmez.

Pencere 700 × 590 piksel sabittir. Başlıkta yalnızca kapatma düğmesi ve saydam arka planlı çöp kutusu simgesi bulunur.

## Kullanım

1. Temizlenecek alanları seçin.
2. **Temizle** düğmesine basın. Uygulama önce tarar, dosya sayısını ve tahmini boyutu 1,5 saniye gösterir, ardından dosyaları temizler.

Ek onay penceresi açılmaz. Bu 1,5 saniyelik aralıkta **İptal** düğmesiyle işlemi durdurabilirsiniz.

Dört alan da açılışta seçilidir. **Son açılanlar listesi** Windows kabuk geçmişini sıfırlar; gerçek belgeleri silmez. **Prefetch** önbelleğini boşaltmak uygulama açılışını geçici olarak yavaşlatabilir; istemiyorsanız Temizle'ye basmadan önce bu seçeneği kapatın.

Temizlik yönetici yetkisi istemeden çalışır. Erişilemeyen veya kullanımdaki dosyalar atlanır. Klasör kökleri ve sembolik bağlantılar silinmez. İşlemin ilerleyişi ve atlanan öğeler arayüzde gösterilir.

## Derleme

Windows üzerinde .NET 10 SDK ve .NET Framework 4.8 hedefleme paketiyle:

```powershell
dotnet build DWTFD.Modern.csproj -c Release
```

Dağıtılacak tek dosya `bin/Release/net48/DWTFD.exe` konumundadır. Windows 11 ve güncel Windows 10 sürümlerinde .NET Framework 4.8 sistemle birlikte gelir. Daha eski sistemlerde 4.8 kurulumu gerekebilir.
