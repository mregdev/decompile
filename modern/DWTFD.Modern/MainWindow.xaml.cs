using System.Globalization;
using System.Windows;

namespace DWTFD.Modern;

public partial class MainWindow : Window
{
    private ScanResult? _scan;
    private CancellationTokenSource? _operation;
    private bool _busy;

    public MainWindow() => InitializeComponent();

    private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();

    private CleanupArea SelectedAreas()
    {
        var areas = CleanupArea.None;
        if (UserTempToggle.IsChecked == true) areas |= CleanupArea.UserTemp;
        if (WindowsTempToggle.IsChecked == true) areas |= CleanupArea.WindowsTemp;
        if (RecentToggle.IsChecked == true) areas |= CleanupArea.Recent;
        if (PrefetchToggle.IsChecked == true) areas |= CleanupArea.Prefetch;
        return areas;
    }

    private void SelectionChanged(object sender, RoutedEventArgs e)
    {
        if (StatusText is null || _busy) return;
        _scan = null;
        SizeLabel.Text = "TARANAN ALAN";
        FileLabel.Text = "BULUNAN DOSYA";
        SizeValue.Text = FileValue.Text = SkippedValue.Text = "—";
        CleanButton.IsEnabled = false;
        StatusText.Text = "Seçim hazır";
        DetailText.Text = "Güncel sonuçları görmek için tara.";
        DetailText.ToolTip = null;
    }

    private async void ScanButton_Click(object sender, RoutedEventArgs e)
    {
        var areas = SelectedAreas();
        if (areas == CleanupArea.None)
        {
            StatusText.Text = "Alan seçilmedi";
            DetailText.Text = "Taramak istediğin en az bir alanı seç.";
            return;
        }

        SetBusy(true, "Taranıyor…", "Seçilen klasörler inceleniyor.");
        try
        {
            _scan = await Task.Run(() => CleanupService.Scan(areas, _operation!.Token));
            SizeLabel.Text = "TARANAN ALAN";
            FileLabel.Text = "BULUNAN DOSYA";
            SizeValue.Text = FormatBytes(_scan.Bytes);
            FileValue.Text = _scan.Files.Count.ToString("N0", CultureInfo.GetCultureInfo("tr-TR"));
            SkippedValue.Text = _scan.Skipped.ToString("N0", CultureInfo.GetCultureInfo("tr-TR"));
            StatusText.Text = "Tarama tamamlandı";
            DetailText.Text = _scan.Skipped > 0
                ? $"{_scan.Skipped} öğeye erişilemedi. Ayrıntılar için bu metnin üzerine gel."
                : areas == CleanupArea.Recent ? "Son açılanlar listesi temizlenmeye hazır." : "Sonuçları gözden geçirip temizliği başlatabilirsin.";
            DetailText.ToolTip = _scan.Errors.Count > 0 ? string.Join(Environment.NewLine, _scan.Errors) : null;
        }
        catch (OperationCanceledException)
        {
            _scan = null;
            StatusText.Text = "Tarama iptal edildi";
            DetailText.Text = "İstersen yeniden tarayabilirsin.";
        }
        catch (Exception ex)
        {
            _scan = null;
            StatusText.Text = "Tarama başarısız";
            DetailText.Text = ex.Message;
        }
        finally
        {
            SetBusy(false);
        }
    }

    private async void CleanButton_Click(object sender, RoutedEventArgs e)
    {
        if (_scan is null || _scan.Areas != SelectedAreas()) return;
        var scan = _scan;
        var summary = scan.Areas.HasFlag(CleanupArea.Recent)
            ? "Son açılanlar listesi de sıfırlanacak.\n" : string.Empty;
        var answer = MessageBox.Show(
            $"{scan.Files.Count:N0} dosya ve {scan.Directories.Count:N0} klasör temizlenecek.\n{summary}Bu işlem geri alınamaz. Devam edilsin mi?",
            "Temizliği onayla", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No);
        if (answer != MessageBoxResult.Yes) return;

        SetBusy(true, "Temizleniyor…", "Kullanımdaki veya erişilemeyen öğeler atlanır.");
        WorkProgress.IsIndeterminate = false;
        WorkProgress.Minimum = 0;
        WorkProgress.Maximum = Math.Max(1, scan.Files.Count + scan.Directories.Count);
        WorkProgress.Value = 0;
        var progress = new Progress<int>(value => WorkProgress.Value = value);

        try
        {
            var result = await Task.Run(() => CleanupService.Clean(scan, _operation!.Token, progress));
            var recentCleared = false;
            if (!result.Cancelled && scan.Areas.HasFlag(CleanupArea.Recent))
            {
                try
                {
                    CleanupService.ClearRecentDocuments();
                    recentCleared = true;
                }
                catch (Exception ex)
                {
                    result.Skipped++;
                    result.Errors.Add("Son açılanlar listesi: " + ex.Message);
                }
            }

            StatusText.Text = result.Cancelled ? "Temizlik iptal edildi" : "Temizlik tamamlandı";
            DetailText.Text = $"{result.DeletedFiles:N0} dosya silindi · {FormatBytes(result.FreedBytes)} alan açıldı · {result.Skipped:N0} öğe atlandı" +
                              (recentCleared ? " · Son açılanlar sıfırlandı" : "");
            DetailText.ToolTip = result.Errors.Count > 0 ? string.Join(Environment.NewLine, result.Errors.Take(5)) : null;
            _scan = null;
            SizeLabel.Text = "AÇILAN ALAN";
            FileLabel.Text = "SİLİNEN DOSYA";
            SizeValue.Text = FormatBytes(result.FreedBytes);
            FileValue.Text = result.DeletedFiles.ToString("N0", CultureInfo.GetCultureInfo("tr-TR"));
            SkippedValue.Text = result.Skipped.ToString("N0", CultureInfo.GetCultureInfo("tr-TR"));
        }
        catch (Exception ex)
        {
            _scan = null;
            StatusText.Text = "Temizlik durdu";
            DetailText.Text = ex.Message;
        }
        finally
        {
            SetBusy(false);
        }
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        _operation?.Cancel();
        CancelButton.IsEnabled = false;
        StatusText.Text = "İptal ediliyor…";
    }

    private void SetBusy(bool busy, string status = "", string detail = "")
    {
        _busy = busy;
        if (busy)
        {
            _operation = new CancellationTokenSource();
            StatusText.Text = status;
            DetailText.Text = detail;
            DetailText.ToolTip = null;
            WorkProgress.Visibility = Visibility.Visible;
            WorkProgress.IsIndeterminate = true;
            CancelButton.Visibility = Visibility.Visible;
            CancelButton.IsEnabled = true;
        }
        else
        {
            _operation?.Dispose();
            _operation = null;
            WorkProgress.Visibility = Visibility.Collapsed;
            CancelButton.Visibility = Visibility.Collapsed;
        }
        ScanButton.IsEnabled = !busy;
        CleanButton.IsEnabled = !busy && _scan is not null;
        UserTempToggle.IsEnabled = WindowsTempToggle.IsEnabled = RecentToggle.IsEnabled = PrefetchToggle.IsEnabled = !busy;
    }

    private static string FormatBytes(long bytes)
    {
        string[] units = ["B", "KB", "MB", "GB", "TB"];
        var value = (double)bytes;
        var index = 0;
        while (value >= 1024 && index < units.Length - 1) { value /= 1024; index++; }
        return value.ToString(index == 0 ? "N0" : "N1", CultureInfo.GetCultureInfo("tr-TR")) + " " + units[index];
    }
}
