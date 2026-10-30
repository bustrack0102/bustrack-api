---
name: 07-quality-supply-chain
description: NuGetAudit sin Trivy bloqueante (lite)
metadata:
  scope: lite
  variant: lite
  opencode/slash: "true"
---
# 07-quality-supply-chain MVP (lite)

## Incluye
- `NuGetAudit=true + source-mapping solo nuget.org + Dependabot + dotnet list --vulnerable`.
- Mapping cerrado: todo transitivo con patrón exacto (nada de prefijos amplios); ante NU1100 en CI, enumerar el cierre con `dotnet list package --include-transitive`.

## Excluye
- Trivy/dockle/ZAP/RESTler bloqueantes, hardening/README obligatorio.

## Verify
- `dotnet list --vulnerable` 0 altos + Dependabot activo.
