$ErrorActionPreference = "Stop"

Write-Host "1. Linting y formato (.NET)..."
dotnet format API_SDD.slnx --verify-no-changes --no-restore

Write-Host "2. Compilacion en Release..."
dotnet build API_SDD.slnx -c Release --no-restore

Write-Host "3. Pruebas unitarias + Concurrencia + Snapshot + Contrato + Cobertura..."
dotnet test UnitTest/UnitTest.csproj -c Release --no-build --collect:"XPlat Code Coverage" --results-directory ./TestResults

Write-Host "4. Verificacion de dependencias vulnerables o sin usar..."
dotnet list API_SDD.slnx package --vulnerable

Write-Host "5. SDD & Critic Guardrails..."
powershell -File scripts/check_sdd.ps1
powershell -File scripts/critic-guardrails.ps1

Write-Host "QUALITY-GATE-OK"
