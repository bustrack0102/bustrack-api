$ErrorActionPreference = "Stop"

Write-Host "=== 1/13 SAST (Roslyn Analyzers en Release) ==="
dotnet build API_SDD.slnx -c Release --no-restore
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

Write-Host "=== 2/13 SCA (Dependencias vulnerables directas y transitivas) ==="
dotnet list API_SDD.slnx package --vulnerable --include-transitive
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

Write-Host "=== 3/13 Escaneo de secretos en archivos e historial Git ==="
$secretPattern = '(AKIA[0-9A-Z]{16}|-----BEGIN (RSA|EC|OPENSSH) PRIVATE KEY-----|ghp_[A-Za-z0-9]{30,}|xoxb-[0-9A-Za-z-]+)'
$hits = Get-ChildItem -Recurse -File -Exclude *.dll,*.exe,*.pdb,*.png,*.jpg |
  Where-Object { $_.FullName -notmatch "\\(bin|obj|\.git|TestResults)\\" } |
  Select-String -Pattern $secretPattern
if ($hits) {
  $hits | Format-Table Path, LineNumber
  throw "Se detectaron posibles secretos expuestos en archivos."
}
$gitHits = git log -n 20 -p | Select-String -Pattern $secretPattern
if ($gitHits) {
  throw "Se detectaron posibles secretos en historial reciente de Git."
}
Write-Host "Escaneo de secretos limpio."

Write-Host "=== 5/13 Seguridad de Contenedores (Dockerfile) ==="
$df = Get-Content Dockerfile -Raw
if ($df -match "(?i)(password|secret|token)=") {
  throw "Posible secreto en Dockerfile."
}
Write-Host "Dockerfile verificado sin secretos embebidos."

Write-Host "=== 6/13 Seguridad IaC (Terraform) ==="
if (Test-Path "infra/terraform") {
  $tfHits = Get-ChildItem "infra/terraform" -Filter *.tf | Select-String -Pattern '(?i)(password|secret_key|connection_string)\s*=\s*"[^$]'
  if ($tfHits) {
    throw "Posible secreto hardcoded en Terraform."
  }
  Write-Host "Terraform verificado sin secretos hardcoded."
}

Write-Host "=== 7/13, 8/13, 12/13, 13/13 Unitarias + Fuzzing + Seguridad + Auth + Rate Limit + Crypto ==="
dotnet test UnitTest/UnitTest.csproj -c Release --no-build --verbosity minimal
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
dotnet test SecurityTest/SecurityTest.csproj -c Release --no-build --filter "FullyQualifiedName~HeadersTests|FullyQualifiedName~AssemblyIntegrityTests|FullyQualifiedName~NoSqlSecurityTests|FullyQualifiedName~OperationalSecurityTests|FullyQualifiedName~IntelligenceSecurityTests" --verbosity minimal
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

Write-Host "=== 9/13 Supply Chain (Workflows con permissions acotados) ==="
Get-ChildItem ".github/workflows" -Filter *.yml | ForEach-Object {
  $raw = Get-Content $_.FullName -Raw
  if ($raw -notmatch "permissions:") {
    Write-Warning "$($_.Name) no declara permissions explicitos."
  }
}

Write-Host "SECURITY-REPERTOIRE-API-OK"
