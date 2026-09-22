using DWTFD.Modern;

var work = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../obj/fixtures"));
Directory.CreateDirectory(work);
var root = Path.Combine(work, "isolated-temp");
var outside = Path.Combine(work, "outside.txt");
if (!Path.GetFullPath(root).StartsWith(work + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
    throw new InvalidOperationException("Test folder escaped workspace.");

Directory.CreateDirectory(root);
Directory.CreateDirectory(Path.Combine(root, "nested"));
File.WriteAllText(Path.Combine(root, "a.txt"), "abc");
File.WriteAllText(Path.Combine(root, "nested", "b.txt"), "12345");
File.WriteAllText(outside, "keep");

var scan = CleanupService.ScanRoots([root], CleanupArea.UserTemp, CancellationToken.None);
if (scan.Files.Count != 2 || scan.Bytes != 8 || scan.Directories.Count != 1)
    throw new Exception($"Unexpected scan: {scan.Files.Count} files, {scan.Bytes} bytes, {scan.Directories.Count} folders");

using var canceled = new CancellationTokenSource();
canceled.Cancel();
var canceledResult = CleanupService.Clean(scan, canceled.Token);
if (!canceledResult.Cancelled || !File.Exists(Path.Combine(root, "a.txt")))
    throw new Exception("Cancellation removed a file.");

var result = CleanupService.Clean(scan, CancellationToken.None);
if (result.DeletedFiles != 2 || result.FreedBytes != 8 || result.DeletedDirectories != 1 || result.Skipped != 0)
    throw new Exception($"Unexpected clean: {result.DeletedFiles} files, {result.FreedBytes} bytes, {result.DeletedDirectories} folders, {result.Skipped} skipped");
if (!Directory.Exists(root) || Directory.EnumerateFileSystemEntries(root).Any() || !File.Exists(outside))
    throw new Exception("Cleanup changed the root or an outside file.");

Console.WriteLine("PASS: scan, cancellation, cleanup, root retention, outside file retention");
