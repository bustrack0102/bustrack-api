#Requires -Version 5.1
<#
.SYNOPSIS
  Critic ligero bloqueantes B1-B4 (constitución §8, AGENTS.md §4).
  B1 secretos en repo + S1135 TODO + CA1707 migraciones con _.
  B2 token en logs (excepción: blacklist:{jti}).
  B3 pipeline en orden constitucional.
  B4 todo controller con fila en docs/endpoints.md.
  Exit 0 PASS, exit 1 FAIL. PS5.1-safe (sin &&, con LiteralPath).
#>
$ErrorActionPreference = "Stop"
$script:failed = $false

function Fail([string]$message) {
  Write-Output "CRITIC-FAIL: $message"
  $script:failed = $true
}

function Get-SourceFiles([string]$filter) {
  $skip = @("\obj\", "\bin\", "\.git\", "\.vs\", "\TestResults\", "\logs\")
  $all = Get-ChildItem -LiteralPath "." -Recurse -Filter $filter -File -ErrorAction SilentlyContinue
  $out = @()
  foreach ($f in $all) {
    $drop = $false
    foreach ($s in $skip) { if ($f.FullName.Contains($s)) { $drop = $true } }
    if (-not $drop) { $out += $f }
  }
  return $out
}

# B1a: archivos prohibidos presentes en el árbol (aunque estén git-ignored, no deben existir aquí).
# appsettings.Local.json NO está prohibido: es el lugar sancionado para secretos locales
# (git-ignored) y B1b lo exime; aquí solo caen .env/secrets.json no sancionados.
$forbidden = @(".env", "secrets.json")
foreach ($name in $forbidden) {
  $hits = Get-ChildItem -LiteralPath "." -Recurse -Filter $name -File -ErrorAction SilentlyContinue |
    Where-Object { $_.FullName -notmatch "\\(\.git|obj|bin|\.vs)\\" }
  foreach ($h in $hits) { Fail "B1 archivo prohibido presente: $($h.FullName)" }
}
foreach ($dir in @("keys", "publish")) {
  if (Test-Path -LiteralPath $dir) { Fail "B1 directorio prohibido presente: $dir/" }
}

# B1b: valores secretos reales en config/código (placeholders REEMPLAZAR/EXTERNA e Invalid#1 de tests permitidos).
# Se exime appsettings.Local.json: lugar sancionado y git-ignored para secretos locales.
foreach ($f in (Get-SourceFiles "*.json")) {
  if ($f.Name -like "*appsettings.Local.json") { continue }
  $text = Get-Content -LiteralPath $f.FullName -Raw
  if ([string]::IsNullOrEmpty($text)) { continue }
  foreach ($m in ([regex]::Matches($text, '(?i)"(Key|Password|Secret|Token)"\s*:\s*"([^"]+)"'))) {
    $value = $m.Groups[2].Value
    if ($value -notmatch "REEMPLAZAR|EXTERNA|PLACEHOLDER" -and $value.Length -ge 8) {
      Fail "B1 posible secreto en $($f.Name): $($m.Groups[1].Value)"
    }
  }
  foreach ($m in ([regex]::Matches($text, '(?i)Password=([^;"]{6,})'))) {
    $value = $m.Groups[1].Value
    if ($value -notmatch "REEMPLAZAR|EXTERNA|Invalid#1") {
      Fail "B1 password real en $($f.Name)"
    }
  }
}

# B1c: S1135 TODO/FIXME/HACK en *.cs.
foreach ($f in (Get-SourceFiles "*.cs")) {
  $hits = Select-String -LiteralPath $f.FullName -Pattern "TODO|FIXME|HACK" -SimpleMatch:$false
  foreach ($h in $hits) { Fail "B1 $($f.Name):$($h.LineNumber) $($h.Line.Trim())" }
}

# B1d: CA1707 migraciones con _.
$migs = Get-ChildItem -LiteralPath "API_SDD" -Recurse -Filter "Migrations" -Directory -ErrorAction SilentlyContinue
foreach ($d in $migs) {
  foreach ($f in (Get-ChildItem -LiteralPath $d.FullName -Filter "*.cs" -File)) {
    if ($f.BaseName.Contains("_")) { Fail "B1 migración con _: $($f.Name)" }
  }
}

# B2: token en logs (salvo blacklist:{jti}).
foreach ($f in (Get-SourceFiles "*.cs")) {
  if ($f.FullName -match "\\(UnitTest|IntegrationTest|SecurityTest|DatabaseTest|ContractTest)\\") { continue }
  $lines = Get-Content -LiteralPath $f.FullName
  for ($i = 0; $i -lt $lines.Count; $i++) {
    $line = $lines[$i]
    if ($line -match "_logger\.Log\w+\(" -and $line -match "[Tt]oken" -and $line -notmatch "blacklist") {
      Fail "B2 token en log $($f.Name):$($i + 1)"
    }
  }
}

# B3: pipeline en orden constitucional (adiciones documentadas entre pasos: OK).
$program = Get-Content -LiteralPath "API_SDD/Program.cs" -Raw
$order = @(
  "SecurityHeadersMiddleware",
  "ExceptionHandlingMiddleware",
  "ForwardedHeaders",
  "UseHsts",
  "UseHttpsRedirection",
  "UseCors",
  "UseRateLimiter",
  "UseAuthentication",
  "UseAuthorization",
  "MapControllers"
)
$last = -1
foreach ($step in $order) {
  $at = $program.IndexOf($step, [System.StringComparison]::Ordinal)
  if ($at -lt 0) { Fail "B3 paso ausente en Program.cs: $step"; continue }
  if ($at -le $last) { Fail "B3 orden roto en Program.cs antes de: $step" }
  $last = $at
}

# B4: todo controller con fila en endpoints.md.
$endpoints = Get-Content -LiteralPath "docs/endpoints.md" -Raw
foreach ($f in (Get-SourceFiles "*Controller.cs")) {
  $base = $f.BaseName.Replace("Controller", "").ToLower()
  if ($endpoints -notmatch [regex]::Escape("/api/v1/$base")) {
    Fail "B4 $($f.Name) sin fila en docs/endpoints.md"
  }
}
foreach ($probe in @("/health", "/health/ready")) {
  if ($endpoints -notmatch [regex]::Escape($probe)) { Fail "B4 $probe sin fila en docs/endpoints.md" }
}

if ($script:failed) { Write-Output "CRITIC-FAIL"; exit 1 }
Write-Output "PASS"
