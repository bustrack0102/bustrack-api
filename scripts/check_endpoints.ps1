#Requires -Version 5.1
<#
.SYNOPSIS
  Diff bidireccional rutas código ↔ docs/endpoints.md (constitución §6).
  Código: controllers ([HttpVerb("sub")]) + /health + /ready + openapi/scalar (si existen en Program.cs).
  Plantillas "{id}" truncadas en "{" (igual que en docs).
  Docs: columna Ruta de la tabla (concreta, sin placeholders).
  Exit 0 OK, exit 1 FAIL. PS5.1-safe (sin &&, con LiteralPath).
#>
$ErrorActionPreference = "Stop"
$script:failed = $false

function Fail([string]$message) {
  Write-Output "ENDPOINTS-FAIL: $message"
  $script:failed = $true
}

function Get-CodeRoutes {
  $routes = @("/health", "/health/ready")
  foreach ($f in (Get-ChildItem -LiteralPath "API_SDD/Controllers" -Recurse -Filter "*Controller.cs" -File -ErrorAction SilentlyContinue)) {
    $base = "api/v1/" + $f.BaseName.Replace("Controller", "").ToLower()
    $text = Get-Content -LiteralPath $f.FullName -Raw
    if ([string]::IsNullOrEmpty($text)) { continue }
    if ($text -match '\[Http(Get|Post|Put|Delete|Patch)\]') { $routes += "/$base" }
    foreach ($m in ([regex]::Matches($text, '\[Http(Get|Post|Put|Delete|Patch)\("([^"]*)"\)'))) {
      $sub = $m.Groups[2].Value
      if ([string]::IsNullOrEmpty($sub)) { continue }
      $full = "/$base/$sub"
      $cut = $full.IndexOf('{')
      if ($cut -ge 0) { $full = $full.Substring(0, $cut) }
      $routes += $full
    }
  }
  $program = Get-Content -LiteralPath "API_SDD/Program.cs" -Raw
  if ($program -match "MapOpenApi") { $routes += "/openapi/v1.json" }
  if ($program -match "MapScalarApiReference") { $routes += "/scalar/v1" }
  return ($routes | Sort-Object -Unique)
}

function Get-DocRoutes {
  $routes = @()
  foreach ($line in (Get-Content -LiteralPath "docs/endpoints.md")) {
    if ($line -notmatch "^\| (GET|POST|PUT|DELETE|PATCH) ") { continue }
    $cells = $line.Split("|")
    if ($cells.Count -lt 3) { continue }
    $cell = $cells[2]
    $m = [regex]::Match($cell, "(/[A-Za-z0-9/_\.\-]+)")
    if (-not $m.Success) { continue }
    $path = $m.Groups[1].Value
    if ($path -match "\{" -or $path -match "\[controller\]") { continue }
    $routes += $path
  }
  return ($routes | Sort-Object -Unique)
}

$code = Get-CodeRoutes
$docs = Get-DocRoutes
Write-Output "code: $($code.Count) rutas, docs: $($docs.Count) filas"
foreach ($r in $code) {
  if ($docs -notcontains $r) { Fail "ruta sin fila en docs: $r" }
}
foreach ($r in $docs) {
  if ($code -notcontains $r) { Fail "fila sin ruta en código: $r" }
}

if ($script:failed) { Write-Output "ENDPOINTS-FAIL"; exit 1 }
Write-Output "OK"
