#Requires -Version 5.1
<#
.SYNOPSIS
  Gate NV-03.1: el slnx solo referencia .csproj existentes en disco.
  FAIL (exit 1) si alguna entrada <Project Path="..."/> no existe.
  Se ejecuta dentro del step `critic` de ci-pr.yml (orden intacto, B3).
  PS5.1-safe (sin &&, con LiteralPath).
#>
$ErrorActionPreference = "Stop"
$failed = $false

$slnx = Join-Path "." "API_SDD.slnx"
if (-not (Test-Path -LiteralPath $slnx)) {
  Write-Output "SLNX-FAIL: falta API_SDD.slnx"
  exit 1
}
[xml]$x = Get-Content -LiteralPath $slnx -Raw
$projects = $x.SelectNodes("//Project")
if (-not $projects -or $projects.Count -eq 0) {
  Write-Output "SLNX-FAIL: sin entradas <Project> en API_SDD.slnx"
  exit 1
}
foreach ($p in $projects) {
  $rel = $p.GetAttribute("Path")
  if ([string]::IsNullOrWhiteSpace($rel)) {
    Write-Output "SLNX-FAIL: entrada <Project> sin atributo Path"
    $failed = $true
    continue
  }
  $full = Join-Path "." ($rel -replace "/", [System.IO.Path]::DirectorySeparatorChar)
  if (-not (Test-Path -LiteralPath $full)) {
    Write-Output "SLNX-FAIL: no existe en disco: $rel"
    $failed = $true
  }
}

if ($failed) { exit 1 }
Write-Output "SLNX-OK ($($projects.Count) proyectos)"
