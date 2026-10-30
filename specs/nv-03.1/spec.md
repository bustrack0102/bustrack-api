# NV-03.1 — Blindaje de solución (slnx canónico + deuda diferida ratificada)

> Deriva de: `.specify/memory/constitution.md §7-§8` + `AGENTS.md §1/§4/§7` + `NV-03` (puerta advisory, sin bloqueo por mutación) + `Memoria.md` (fase recorte base + hardening `.gitignore`).
> Predecesoras: `nv-00…nv-03` aprobadas y firmadas 2026-10-11. No las modifica. `nv-03` sigue dueño de la puerta delta+Stryker; `nv-03.1` solo blinda el `slnx`.

## 1. Objetivo
Fijar el `slnx` canónico de 6 proyectos (`API_SDD + Unit + Integration + Security + Database + Contract`) y añadir un gate que falle si el `slnx` referencia algún `.csproj` inexistente en disco (regresión tipo IDE que re-añade `ChaosTest/MutationTest/PerformanceTest`, plantillas eliminadas a propósito). Ratificar Chaos/Perf/Mutación-proyecto como deuda diferida, no alcance.

## 2. Alcance / No-alcance
In: gate `slnx` (cada `<Project Path>` existe en disco; FAIL exit 1 si falta) integrado en el step existente de `critic` (orden CI intacto) + `Memoria.md` (fase nv-03.1) + negativo verificado con el diff revertido del 2026-10-11.
Out: re-añadir `Chaos/Performance/MutationTest`; enmienda constitucional; NBomber/Chaos/Pact-real/perf; cambiar 45% o bloqueo por mutación (exige enmienda §7, dueño `nv-03`); reorden del pipeline; endpoints; entidades/migraciones.

## 3. Contratos API
Sin cambio de contrato. `docs/endpoints.md` intacto. Puerta de infraestructura, no añade rutas. Convención legacy según `nv-02`.

## 4. Modelo de datos
Sin modelo. Sin `DbSet`/entidad/migración. Regla `CA1707` sin `_` sigue vigente para futuros CRUDs.

## 5. Seguridad
Sin superficie nueva. `B1` (sin secretos, `launchSettings.json` TRACKED limpio), `B2` (sin `token` en logs), `B3` (pipeline y orden intactos: el gate vive dentro del step `critic`), `B4` (`endpoints.md` intacto). El chequeo solo lee XML y hace `Test-Path`; no ejecuta código ni toca red. Inglés en código.

## 6. Tests / Gates
- Positivo: `slnx` HEAD (6 paths existentes) → `SLNX-OK` + `dotnet build API_SDD.slnx -c Release` 0/0.
- Negativo (obligatorio, estilo `critic B1` con `Local.json`): `slnx` con ref fantasma (`ChaosTest/ChaosTest.csproj`, el diff revertido) → `SLNX-FAIL` exit 1 con la ruta faltante en el mensaje.
- Gates docs: `sdd-lint SDD-OK + critic PASS + endpoints OK` (los `.md` no alteran gates).

## 7. Plan de entrega
Sin `publish`, sin tag, sin endpoint. Entrega: 4 documentos en Borrador en `specs/nv-03.1/` + `scripts/check_slnx.ps1` cableado en el step `critic` (T2 ejecutado). Tras Stryker siempre `dotnet build` antes de `--no-build` (no aplica aquí: sin cambios de código).

## Aprobación
- [x] Revisor humano — Usuario (aceptación expresa en sesión)
- [x] Fecha: 2026-10-12
- [x] CHECKLIST_PR.md OK
- [x] Sin conflicto con constitución
- **Estado:** ✅ Aprobada
