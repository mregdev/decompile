Add-Type -AssemblyName System.Drawing

$bitmap = [System.Drawing.Bitmap]::new(128, 128, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
$graphics = [System.Drawing.Graphics]::FromImage($bitmap)
$graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
$graphics.Clear([System.Drawing.Color]::Transparent)

$teal = [System.Drawing.SolidBrush]::new([System.Drawing.Color]::FromArgb(98, 228, 208))
$shadow = [System.Drawing.SolidBrush]::new([System.Drawing.Color]::FromArgb(24, 76, 84))
$outline = [System.Drawing.Pen]::new([System.Drawing.Color]::FromArgb(30, 111, 113), 3)

$body = [System.Drawing.Point[]]@(
    [System.Drawing.Point]::new(30, 45),
    [System.Drawing.Point]::new(98, 45),
    [System.Drawing.Point]::new(91, 108),
    [System.Drawing.Point]::new(37, 108)
)
$graphics.FillPolygon($teal, $body)
$graphics.DrawPolygon($outline, $body)
$graphics.FillRectangle($teal, 22, 34, 84, 12)
$graphics.FillRectangle($teal, 48, 23, 32, 12)

foreach ($x in @(49, 63, 77)) {
    $graphics.FillRectangle($shadow, $x, 61, 5, 31)
}

$pngPath = Join-Path $PSScriptRoot 'DWTFD.Modern/app.png'
$icoPath = Join-Path $PSScriptRoot 'DWTFD.Modern/app.ico'
$bitmap.Save($pngPath, [System.Drawing.Imaging.ImageFormat]::Png)
$handle = $bitmap.GetHicon()
$icon = [System.Drawing.Icon]::FromHandle($handle)
$stream = [System.IO.File]::Create($icoPath)
$icon.Save($stream)

$stream.Dispose()
$icon.Dispose()
$outline.Dispose()
$shadow.Dispose()
$teal.Dispose()
$graphics.Dispose()
$bitmap.Dispose()
