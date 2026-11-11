#Requires -Version 5.1
<#
.SYNOPSIS
  Puerta SDD: specs + specs-endpoints con Aprobación + contratos con spec viva + plan/task/chart/checklists.
  Sin cambio de contrato sin spec (AGENTS.md §2). Exit 1 ante cualquier drift.
#>
$ErrorActionPreference = "Stop"
$failed = $false

function Fail([string]$message) {
  Write-Output "SDD-FAIL: $message"
  $script:failed = $true
}

# B1: toda spec en specs/ tiene spec.md con Aprobación, plan.md, task.md, chart.md y checklists/.
$specs = Get-ChildItem -LiteralPath "specs" -Directory | Where-Object { $_.Name -notmatch '^(_template|templates)' }
if (-not $specs) { Fail "sin specs en specs/" }
foreach ($spec in $specs) {
  $specFile = Join-Path $spec.FullName "spec.md"
  if (-not (Test-Path -LiteralPath $specFile)) { Fail "$($spec.Name) sin spec.md"; continue }
  $text = Get-Content -LiteralPath $specFile -Raw
  if ($text -notmatch "## Aprobaci") { Fail "$($spec.Name)/spec.md sin bloque de Aprobación" }
  foreach ($req in @("plan.md", "task.md", "chart.md")) {
    if (-not (Test-Path -LiteralPath (Join-Path $spec.FullName $req))) { Fail "$($spec.Name) sin $req" }
  }
  if (-not (Test-Path -LiteralPath (Join-Path $spec.FullName "checklists"))) { Fail "$($spec.Name) sin checklists/" }
}

$endpointSpecs = Get-ChildItem -LiteralPath "specs-endpoints" -Directory -ErrorAction SilentlyContinue
if (-not $endpointSpecs) { Fail "sin specs en specs-endpoints/" }
foreach ($es in $endpointSpecs) {
  $esFile = Join-Path $es.FullName "spec.md"
  if (-not (Test-Path -LiteralPath $esFile)) { Fail "specs-endpoints/$($es.Name) sin spec.md"; continue }
  $esText = Get-Content -LiteralPath $esFile -Raw
  if ($esText -notmatch "## Aprobaci") { Fail "specs-endpoints/$($es.Name)/spec.md sin bloque de Aprobación" }
}

# B2: toda fila de endpoints.md referencia una spec viva en specs/ o specs-endpoints/.
$endpoints = Get-Content -LiteralPath "docs/endpoints.md" -Raw
$live = @($specs | ForEach-Object { $_.Name }) + @($endpointSpecs | ForEach-Object { $_.Name })
foreach ($row in ($endpoints -split "`n" | Where-Object { $_ -match "^\| (GET|POST|PUT|DELETE|PATCH)" })) {
  $hit = $false
  foreach ($name in $live) { if ($row -match [regex]::Escape($name)) { $hit = $true } }
  if (-not $hit) { Fail "fila sin spec viva: $($row.Trim())" }
}

# B3: sin rutas Api/ legacy en specs (proyecto real API_SDD/).
foreach ($file in Get-ChildItem -LiteralPath "specs" -Recurse -Filter "*.md") {
  $hits = Select-String -LiteralPath $file.FullName -Pattern "Api/Program\.cs" -SimpleMatch
  if ($hits) { Fail "$($file.Name) referencia Api/ en vez de API_SDD/" }
}

if ($failed) { Write-Output "SDD-FAIL"; exit 1 }
Write-Output "SDD-OK"
