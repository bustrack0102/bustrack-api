# NV-00 — Constitución MVP hosting-ready

## Contexto
Filosofía del proyecto madre aplicada a CRUDs liberables por slice, pero en esta
etapa solo bases sin CRUD alguno. **Depende de**: —.

## Requisitos
1. Fijar stack recortado sin `SQS/MassTransit, Redis distribuido, OTel/Grafana full, NBomber/Chaos, AWS, hosting/CD`.
2. Exigir `spec-first + DoD por slice + trazabilidad spec↔código↔test + 1 revisor + Memoria.md`.
3. Fijar NFR MVP verificables por comando: `timeout 60s, Kestrel 10MB/2min, build 0/0, cobertura >=45%`.
4. Registrar reglas no negociables: `sin secretos en Git, sin TODO sin issue, sin código muerto, inglés en código, IOptionsMonitor lazy para WAF, real-first sin fakes`.

## Diseño
- Principios: `stateless 1-instancia, seguridad primero, fail-fast, empírico-antes-teórico, documentar límites`.
- Stack:
  | Capa | Tecnología |
  |---|---|
  | Runtime | .NET 10 / ASP.NET Core 10 |
  | Persistencia | EF Core + SQL Server externo, migraciones |
  | Auth futuro | JWT HS256 + Argon2id + TOTP Otp.NET + DataProtection |
  | Validación | FluentValidation |
  | Tests | xUnit + FluentAssertions + Moq + FsCheck determinista + WebApplicationFactory + Testcontainers.MsSql |
  | Logs | Serilog JSON + CorrelationId |
  | SAST | Semgrep + SonarAnalyzer.CSharp + NuGetAudit |
  | CI | GitHub Actions solo `ci-pr.yml`, sin deploy |
- Naming: tablas/columnas `Prefijo* + str/int/dec/dte/bln + RowVersion`, JSON legacy medido `prefijos minúsculos + resto PascalCase + id`, NO PascalCase puro.
- `AGENTS.md §1` recortado cita este stack. `Memoria.md` inicial creada.

## Contratos
N/A — constitucional. Referencia normativa en `AGENTS.md §1`.

## Tests
N/A — verificación manual de estructura + `AppSettingsTests 3`: `Example válido + sin secretos + sin claves futuras`.

## Criterios
- `AGENTS.md + Memoria.md` existen y citan stack recortado.
- Cada NFR tiene comando: `dotnet build -c Release --no-restore → 0/0`, `python scripts/check_coverage.py → >=45%`.
- `powershell -File scripts/critic-guardrails.ps1 → PASS` en vacío.

## Límites
Sin `p95, mutation gate, ZAP/RESTler bloqueantes, Trivy imagen, /metrics full, Pact real` — van como deuda documentada, no verde falso. Sin saga, sin Redis, sin hosting.

## Aprobación y Control de Cambios
- **Estado:** ✅ Aprobada
- **Revisores:** Usuario (aceptación expresa en sesión)
- **Fecha:** 2026-10-11
- **Detalle:** implementada y verificada (AppSettingsTests 3/3, build 0/0, critic PASS). Evidencia en `specs/nv-00/checklists/gates.md` + `Memoria.md` fase Fase-1.
