# Verify rendered UI PNGs: size vs manifest, alpha coverage, file size.
# Usage: powershell -NoProfile -File docs/design/ui_source/verify_png.ps1
param(
    [string]$PngRoot = 'E:\WorkSpace\BalartroLike\Assets\Resources\Texture\UI',
    [string]$Manifest = (Join-Path $PSScriptRoot 'manifest_all.json')
)

Add-Type -AssemblyName System.Drawing

$items = Get-Content $Manifest -Raw -Encoding UTF8 | ConvertFrom-Json
$bad = @()
$stats = @()

foreach ($it in $items) {
    $path = Join-Path (Split-Path $PngRoot) ($it.file -replace '^Assets/Resources/Texture/', '')
    if (-not (Test-Path $path)) { $bad += "MISSING $($it.name)"; continue }

    $bmp = [System.Drawing.Bitmap]::FromFile($path)
    $stepX = [Math]::Max(1, [int][Math]::Floor($bmp.Width / 34))
    $stepY = [Math]::Max(1, [int][Math]::Floor($bmp.Height / 34))
    $opaque = 0
    $total = 0
    for ($y = 0; $y -lt $bmp.Height; $y += $stepY) {
        for ($x = 0; $x -lt $bmp.Width; $x += $stepX) {
            $total++
            if ($bmp.GetPixel($x, $y).A -gt 12) { $opaque++ }
        }
    }
    $pct = [int](100 * $opaque / $total)
    if ($bmp.Width -ne $it.width -or $bmp.Height -ne $it.height) {
        $bad += "SIZE $($it.name): $($bmp.Width)x$($bmp.Height) expected $($it.width)x$($it.height)"
    }
    if ($opaque -eq 0) { $bad += "EMPTY $($it.name)" }
    $stats += [pscustomobject]@{
        name   = $it.name
        cat    = $it.category
        size   = "$($bmp.Width)x$($bmp.Height)"
        kb     = [int]((Get-Item $path).Length / 1024)
        opaque = $pct
    }
    $bmp.Dispose()
}

$stats | Group-Object cat | ForEach-Object {
    $g = $_.Group
    "{0,-12} count={1,-3} opaque(min/avg/max)={2}/{3}/{4} kb(avg)={5}" -f $_.Name, $g.Count,
        ($g | Measure-Object opaque -Minimum).Minimum,
        [int](($g | Measure-Object opaque -Average).Average),
        ($g | Measure-Object opaque -Maximum).Maximum,
        [int](($g | Measure-Object kb -Average).Average)
}

"files: $($stats.Count) / $($items.Count)"
if ($bad.Count) { "PROBLEMS:"; $bad } else { "no problems" }
