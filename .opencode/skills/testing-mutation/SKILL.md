---
name: testing-mutation
description: Stryker PR advisory + nightly opcional (lite)
metadata:
  scope: lite
  variant: advisory-plus-nightly
  opencode/slash: "true"
---
# testing-mutation MVP (advisory + optional-nightly)

## Incluye
- `stryker-config.json` PR advisory (`since.target=main`, `mutate` sin `Migrations`, `thresholds.break=0`, `continue-on-error`) + `stryker-{entidad}.json` acotado nightly + `test-case-filter UnitTest` + piloto medido + `ignore` equivalentes documentado.
- Piloto nv-02: 23 mutantes Standard en ~3min, score 45.83% (bloqueo al 80% hoy fallaría: advisory correcto).

## Excluye
- Gate release/bloqueo por mutation score (exige enmienda §7). Corre PR advisory + nightly/manual + `dotnet build` restaurativo después.

## Verify
- `dotnet-stryker --config-file stryker-config.json` exit 0 en PR. Tras Stryker siempre `dotnet build` antes de `--no-build`.
