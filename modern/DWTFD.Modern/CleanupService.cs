using System.IO;
using System.Runtime.InteropServices;

namespace DWTFD.Modern;

[Flags]
public enum CleanupArea
{
    None = 0,
    UserTemp = 1,
    WindowsTemp = 2,
    Recent = 4,
    Prefetch = 8
}

public sealed record ScannedFile(string Path, string Root, long Bytes);
public sealed record ScannedDirectory(string Path, string Root);

public sealed class ScanResult
{
    public required CleanupArea Areas { get; init; }
    public List<ScannedFile> Files { get; } = [];
    public List<ScannedDirectory> Directories { get; } = [];
    public List<string> Errors { get; } = [];
    public long Bytes => Files.Sum(file => file.Bytes);
    public int Skipped { get; set; }
}

public sealed class CleanResult
{
    public bool Cancelled { get; set; }
    public long FreedBytes { get; set; }
    public int DeletedFiles { get; set; }
    public int DeletedDirectories { get; set; }
    public int Skipped { get; set; }
    public List<string> Errors { get; } = [];
}

public static class CleanupService
{
    public static ScanResult Scan(CleanupArea areas, CancellationToken cancellationToken)
    {
        var roots = new List<string>();
        if (areas.HasFlag(CleanupArea.UserTemp)) roots.Add(Path.GetTempPath());
        if (areas.HasFlag(CleanupArea.WindowsTemp))
            roots.Add(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Temp"));
        if (areas.HasFlag(CleanupArea.Prefetch))
            roots.Add(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Prefetch"));
        return ScanRoots(roots, areas, cancellationToken);
    }

    // Public so the file operations can be checked with an isolated test folder.
    public static ScanResult ScanRoots(IEnumerable<string> paths, CleanupArea areas, CancellationToken cancellationToken)
    {
        var result = new ScanResult { Areas = areas };
        var roots = paths.Select(path => Path.TrimEndingDirectorySeparator(Path.GetFullPath(path)))
            .Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(path => path.Length).ToList();
        var processed = new List<string>();

        foreach (var root in roots)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (processed.Any(parent => root.StartsWith(parent + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)))
                continue;
            processed.Add(root);

            if (!Directory.Exists(root))
            {
                AddError(result, root, "Klasör bulunamadı veya erişilemiyor.");
                continue;
            }

            var pending = new Stack<string>();
            pending.Push(root);
            while (pending.Count > 0)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var directory = pending.Pop();
                try
                {
                    if ((File.GetAttributes(directory) & FileAttributes.ReparsePoint) != 0)
                    {
                        result.Skipped++;
                        continue;
                    }
                    foreach (var entry in Directory.EnumerateFileSystemEntries(directory))
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        try
                        {
                            var attributes = File.GetAttributes(entry);
                            if ((attributes & FileAttributes.ReparsePoint) != 0)
                            {
                                result.Skipped++;
                                continue;
                            }
                            if ((attributes & FileAttributes.Directory) != 0)
                            {
                                result.Directories.Add(new ScannedDirectory(entry, root));
                                pending.Push(entry);
                            }
                            else
                            {
                                result.Files.Add(new ScannedFile(entry, root, new FileInfo(entry).Length));
                            }
                        }
                        catch (Exception ex) when (IsFileSystemError(ex))
                        {
                            AddError(result, entry, ex.Message);
                        }
                    }
                }
                catch (Exception ex) when (IsFileSystemError(ex))
                {
                    AddError(result, directory, ex.Message);
                }
            }
        }
        return result;
    }

    public static CleanResult Clean(ScanResult scan, CancellationToken cancellationToken, IProgress<int>? progress = null)
    {
        var result = new CleanResult();
        var done = 0;
        foreach (var file in scan.Files)
        {
            if (cancellationToken.IsCancellationRequested) { result.Cancelled = true; return result; }
            try
            {
                if (!IsInsideUnlinkedRoot(file.Path, file.Root))
                    throw new IOException("Yol değişti veya bağlantı içeriyor.");
                File.Delete(file.Path);
                result.DeletedFiles++;
                result.FreedBytes += file.Bytes;
            }
            catch (Exception ex) when (IsFileSystemError(ex))
            {
                AddError(result, file.Path, ex.Message);
            }
            progress?.Report(++done);
        }

        foreach (var directory in scan.Directories.OrderByDescending(item => item.Path.Length))
        {
            if (cancellationToken.IsCancellationRequested) { result.Cancelled = true; return result; }
            try
            {
                if (!IsInsideUnlinkedRoot(directory.Path, directory.Root))
                    throw new IOException("Yol değişti veya bağlantı içeriyor.");
                Directory.Delete(directory.Path, false);
                result.DeletedDirectories++;
            }
            catch (Exception ex) when (IsFileSystemError(ex))
            {
                AddError(result, directory.Path, ex.Message);
            }
            progress?.Report(++done);
        }
        return result;
    }

    public static void ClearRecentDocuments() => SHAddToRecentDocs(1, IntPtr.Zero);

    private static bool IsInsideUnlinkedRoot(string path, string root)
    {
        var full = Path.GetFullPath(path);
        if (!full.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)) return false;
        for (var current = full; current is not null; current = Path.GetDirectoryName(current))
        {
            if ((File.GetAttributes(current) & FileAttributes.ReparsePoint) != 0) return false;
            if (string.Equals(current, root, StringComparison.OrdinalIgnoreCase)) return true;
        }
        return false;
    }

    private static bool IsFileSystemError(Exception exception) =>
        exception is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException;

    private static void AddError(ScanResult result, string path, string message)
    {
        result.Skipped++;
        if (result.Errors.Count < 5) result.Errors.Add($"{path}: {message}");
    }

    private static void AddError(CleanResult result, string path, string message)
    {
        result.Skipped++;
        if (result.Errors.Count < 5) result.Errors.Add($"{path}: {message}");
    }

    [DllImport("shell32.dll")]
    private static extern void SHAddToRecentDocs(uint flags, IntPtr value);
}
