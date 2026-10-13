# verify-headless-adhoc.ps1
# Build + headless REAL load de MainMenu y Level en Godot 4.7.1 (embedded).
# Filtra el ruido benigno de shutdown (RID leaks / Unreferenced static string).
$ErrorActionPreference = "Stop"
$proj = (Resolve-Path (Join-Path $PSScriptRoot ".."))
$godot = if ($env:GODOT_MONO_BIN) { $env:GODOT_MONO_BIN } else { "godot" }

function Run-Scene {
    param([string]$scene, [string]$out)
    Write-Host "=== Headless REAL: $scene ==="
    # swap main_scene, run REAL (no --check-only), then restore.
    Copy-Item (Join-Path $proj "project.godot") "$env:TEMP\pg.bak"
    (Get-Content (Join-Path $proj "project.godot")) -replace 'run/main_scene="[^"]*"', "run/main_scene=`"res://scenes/$scene`"" | Set-Content (Join-Path $proj "project.godot")
    # Godot no se auto-termina en headless -> envolver en timeout y matar al final.
    $job = Start-Process -FilePath $godot -ArgumentList "--headless","--path",$proj -NoNewWindow -RedirectStandardOutput $out -PassThru
    Start-Sleep -Seconds 16
    if (-not $job.HasExited) { $job.Kill() }
    Copy-Item "$env:TEMP\pg.bak" (Join-Path $proj "project.godot") -Force
    return $job.ExitCode
}

# Build
Write-Host "=== dotnet build ==="
Push-Location $proj
dotnet build -clp:ErrorsOnly 2>&1 | tail -3
Pop-Location

Run-Scene "MainMenu.tscn" "$env:TEMP\hm.log"
Run-Scene "Level.tscn"     "$env:TEMP\lv.log"

# Filtrar fallos reales (excluir ruido de shutdown).
$failRe = "script error|nullreference|parse error|node not found|referenced non-existent|couldn't load"
$hf = (Select-String -Path "$env:TEMP\hm.log" -Pattern $failRe -Quiet)
$lf = (Select-String -Path "$env:TEMP\lv.log" -Pattern $failRe -Quiet)
Write-Host "=== RESULTADOS ==="
Write-Host ("MainMenu fallos reales: " + $hf)
Write-Host ("Level    fallos reales: " + $lf)
if (-not $hf -and -not $lf) { Write-Host "VERIFICACION: OK (carga + _Ready/_Process sin errores de juego)" }
else { Write-Host "VERIFICACION: FALLO" }
