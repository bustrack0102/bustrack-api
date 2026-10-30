---
name: 10-testing-strategy
description: Qué suite cubre qué sin perf/caos (lite)
metadata:
  scope: lite
  variant: lite
  opencode/slash: "true"
---
# 10-testing-strategy MVP (lite)

## Incluye
- `Unit lógica + Integration/Security WAF SQL real + Database migraciones + Contract captura + FsCheck seeds + maxParallelThreads:4 + --blame-crash`.
- Puerta incremental advisory nv-03: `check_delta_coverage.py --min 80 --advisory` (delta `merge-base main`, higiene `Migrations/Program/Dtos` fuera vía `ExcludeByFile`) + Stryker PR `since:main` sin break. Bloqueo real sigue 45% global (§7).

## Excluye
- NBomber, Chaos, Pact-real, perf/caos.

## Verify
- `dotnet test <csproj> -c Release --no-build --blame-crash` 5 suites verdes + cobertura ≥45%.
