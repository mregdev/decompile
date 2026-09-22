Add-Type -AssemblyName System.Drawing

$large = [System.Drawing.Bitmap]::new(256, 256, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
$graphics = [System.Drawing.Graphics]::FromImage($large)
$graphics.Clear([System.Drawing.Color]::Transparent)
$graphics.TextRenderingHint = [System.Drawing.Text.TextRenderingHint]::AntiAliasGridFit
$teal = [System.Drawing.SolidBrush]::new([System.Drawing.Color]::FromArgb(98, 228, 208))
# Windows'un Segoe Fluent Icons yazı tipindeki Delete simgesi.
$font = [System.Drawing.Font]::new('Segoe Fluent Icons', 198, [System.Drawing.FontStyle]::Regular, [System.Drawing.GraphicsUnit]::Pixel)
$alignment = [System.Drawing.StringFormat]::new()
$alignment.Alignment = [System.Drawing.StringAlignment]::Center
$alignment.LineAlignment = [System.Drawing.StringAlignment]::Center
$graphics.DrawString([char]0xE74D, $font, $teal, [System.Drawing.RectangleF]::new(0, -2, 256, 256), $alignment)

$assetDirectory = Join-Path $PSScriptRoot 'DWTFD.Modern'
$large.Save((Join-Path $assetDirectory 'app.png'), [System.Drawing.Imaging.ImageFormat]::Png)

$small = [System.Drawing.Bitmap]::new(64, 64, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
$smallGraphics = [System.Drawing.Graphics]::FromImage($small)
$smallGraphics.Clear([System.Drawing.Color]::Transparent)
$smallGraphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
$smallGraphics.DrawImage($large, 0, 0, 64, 64)
$icon = [System.Drawing.Icon]::FromHandle($small.GetHicon())
$stream = [System.IO.File]::Create((Join-Path $assetDirectory 'app.ico'))
$icon.Save($stream)

$stream.Dispose()
$icon.Dispose()
$smallGraphics.Dispose()
$small.Dispose()
$alignment.Dispose()
$font.Dispose()
$teal.Dispose()
$graphics.Dispose()
$large.Dispose()
