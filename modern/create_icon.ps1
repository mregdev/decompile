# Build a Windows icon from the supplied artwork. Small frames use a prefiltered
# thumbnail so the fine mesh does not shimmer at taskbar sizes.
Add-Type -AssemblyName System.Drawing

$assetDirectory = Join-Path $PSScriptRoot 'DWTFD.Modern'
$pngPath = Join-Path $assetDirectory 'app.png'
$smallPngPath = Join-Path $assetDirectory 'app-small.png'
$mediumPngPath = Join-Path $assetDirectory 'app-medium.png'
$icoPath = Join-Path $assetDirectory 'app.ico'
$source = [System.Drawing.Bitmap]::new($pngPath)
$smallSource = [System.Drawing.Bitmap]::new($smallPngPath)
$mediumSource = [System.Drawing.Bitmap]::new($mediumPngPath)
$frames = @()

foreach ($size in @(16, 24, 32, 48, 64, 128, 256)) {
    $bitmap = [System.Drawing.Bitmap]::new($size, $size, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
    $graphics.Clear([System.Drawing.Color]::Transparent)
    $graphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
    $graphics.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
    if ($size -le 32) {
        $graphics.DrawImage($smallSource, 0, 0, $size, $size)
    } elseif ($size -le 64) {
        $graphics.DrawImage($mediumSource, 0, 0, $size, $size)
    } else {
        $graphics.DrawImage($source, 0, 0, $size, $size)
    }
    $memory = [System.IO.MemoryStream]::new()
    $bitmap.Save($memory, [System.Drawing.Imaging.ImageFormat]::Png)
    $frames += [pscustomobject]@{ Size = $size; Data = $memory.ToArray() }
    $memory.Dispose()
    $graphics.Dispose()
    $bitmap.Dispose()
}
$source.Dispose()
$smallSource.Dispose()
$mediumSource.Dispose()

$stream = [System.IO.File]::Create($icoPath)
$writer = [System.IO.BinaryWriter]::new($stream)
$writer.Write([uint16]0)
$writer.Write([uint16]1)
$writer.Write([uint16]$frames.Count)
$offset = 6 + 16 * $frames.Count
foreach ($frame in $frames) {
    $dimension = [byte]($frame.Size % 256)
    $writer.Write($dimension)
    $writer.Write($dimension)
    $writer.Write([byte]0)
    $writer.Write([byte]0)
    $writer.Write([uint16]1)
    $writer.Write([uint16]32)
    $writer.Write([uint32]$frame.Data.Length)
    $writer.Write([uint32]$offset)
    $offset += $frame.Data.Length
}
foreach ($frame in $frames) { $writer.Write([byte[]]$frame.Data) }
$writer.Dispose()
