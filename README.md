# DWTFD

Bu depo, ilk DWTFD uygulamasının decompile edilmiş kaynaklarını ve yeniden yazılmış Windows sürümünü içerir. İlk sürümün dosyaları depo kökünde korunur.

## Yeni sürüm

[WPF uygulaması](modern/DWTFD.Modern/) .NET 10 ile geliştirilmiştir. Geçici dosyaları tarayıp temizler; Windows'un son açılanlar listesini de sıfırlayabilir. Dört alan açılışta seçilidir. Temizle düğmesi tarama sonucunu ve boyutu 1,5 saniye gösterip işlemi başlatır.

Windows üzerinde derlemek için:

```powershell
dotnet build modern/DWTFD.Modern/DWTFD.Modern.csproj -c Release
```

Tek dosyalık, kendi .NET çalışma zamanını içeren uygulamayı üretmek için:

```powershell
dotnet publish modern/DWTFD.Modern/DWTFD.Modern.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true
```

İzole klasörde dosya işlemlerini doğrulayan kontroller:

```powershell
dotnet run --project modern/DWTFD.Checks/DWTFD.Checks.csproj -c Release
```

Kullanım ve seçenekler için [modern sürümün README dosyasına](modern/DWTFD.Modern/README.md) bakın.
