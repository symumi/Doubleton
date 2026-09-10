# Render UI SVG sources into PNG files under Assets/Resources/Texture/UI.
# Requires Google Chrome (headless screenshot). No Unity needed.
# Usage:
#   powershell -NoProfile -File docs/design/ui_source/render_png.ps1          # only missing files
#   powershell -NoProfile -File docs/design/ui_source/render_png.ps1 -Force   # re-render all
param(
    [string]$SvgRoot = (Join-Path $PSScriptRoot 'svg'),
    [string]$OutRoot = 'E:\WorkSpace\BalartroLike\Assets\Resources\Texture\UI',
    [switch]$Force
)

$ErrorActionPreference = 'Stop'
$chrome = 'C:\Program Files\Google\Chrome\Application\chrome.exe'
if (-not (Test-Path $chrome)) { throw "Chrome not found: $chrome" }

$profile = Join-Path $env:TEMP 'dsh_ui_render_profile'
New-Item -ItemType Directory -Force -Path $profile | Out-Null

$total = 0
$done = 0
$failed = @()

Get-ChildItem $SvgRoot -Recurse -Filter *.svg | Sort-Object FullName | ForEach-Object {
    $total++
    $rel = $_.FullName.Substring($SvgRoot.Length + 1)
    $png = Join-Path $OutRoot ($rel -replace '\.svg$', '.png')
    New-Item -ItemType Directory -Force -Path (Split-Path $png) | Out-Null

    if ((Test-Path $png) -and -not $Force) { $done++; return }

    $head = Get-Content $_.FullName -TotalCount 1
    if ($head -match 'width="(\d+)"\s+height="(\d+)"') {
        $w = $Matches[1]
        $h = $Matches[2]
    }
    else {
        $failed += "$rel (cannot parse size)"
        return
    }

    $url = 'file:///' + ($_.FullName -replace '\\', '/')
    $argv = @(
        '--headless=new', '--disable-gpu', '--no-first-run', '--no-default-browser-check',
        '--hide-scrollbars', '--force-device-scale-factor=1', '--default-background-color=00000000',
        "--user-data-dir=$profile", "--screenshot=$png", "--window-size=$w,$h", $url
    )
    Start-Process -FilePath $chrome -ArgumentList $argv -Wait -WindowStyle Hidden | Out-Null

    if (Test-Path $png) { $done++ } else { $failed += $rel }
}

"rendered $done / $total -> $OutRoot"
if ($failed.Count) {
    'failed:'
    $failed
}
