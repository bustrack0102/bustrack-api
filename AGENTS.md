# Agents.md — CrudDevSecOpsMVP

> Rector: `.specify/memory/constitution.md` manda ante conflicto. Espejo: `./constitution.md`.
> Este archivo dice **cómo** trabajar. La constitución dice **qué** no se negocia.

## 1. Forma de trabajo
> Stack: ver `constitution.md §3`. Nada fuera sin excepción documentada.
- Setup: `API_SDD/appsettings.Example.json → API_SDD/appsettings.json`; `ConnectionStrings__Default` apunta a **DB externa**; `UseInMemoryDatabase=false`; `dotnet restore`; Docker solo para tests.
- Comandos: `dotnet restore`, `dotnet build -c Release --no-restore`, `dotnet test <csproj> -c Release --no-build` (Unit, Integration, Security, Contract, Database), `dotnet run --project API_SDD/API_SDD.csproj`, `dotnet publish -c Release -o ./publish`.
- CI `ci-pr.yml` (ubuntu-latest, Testcontainers exige Linux): restore → build → unit → integration → security → database → contract → critic → endpoints → sdd → coverage → delta-check → stryker → semgrep. Sin deploy.
- PR = 1 slice CRUD; `CHECKLIST_PR.md` + 1 revisor + `Memoria.md` al día.
- Naming: inglés código; DB `Prefijo* + str/int/dec/dte/bln`; JSON legacy medido, NO PascalCase puro.
- Sin secretos, sin TODO sin issue, sin usings muertos, sin código muerto.

## 2. Memoria
- Plantilla canónica `.specify/templates/spec-template.md` (alias `specs/_template.md`), DoD por CRUD/auth en `nv-04…nv-06`.
- `AGENTS.md`: manual operativo vivo.
- `Memoria.md`: estado, decisiones, lecciones aprendidas por fase y metodología de diagnóstico de chaos. Actualizar al cerrar cada fase.
- Al empezar, lee `Memoria.md` para conocer el estado del proyecto y las decisiones tomadas.
- Al terminar una tarea, actualízalo: estado actual, decisiones importantes (con su porqué) y errores a evitar.
- Al cerrar una fase, concilia su `spec.md`/`plan.md` con lo implementado (rutas de tests, criterios, nombres reales de archivos) y resuelve el bloque de aprobación: Borrador con evidencia, o firmado solo si el usuario indicó revisor.
- Mantenlo breve (máximo ~10000 líneas): resume o elimina lo que ya no aporte.
- Si algo se convierte en una regla permanente, propón moverlo a AGENTS.md en lugar de dejarlo en la memoria.
- No guardes nunca datos sensibles (claves, tokens, datos personales).

## 3. Límites — no hacer
- No InMemory para negocio; solo 1 test dummy `byte[]{1}`.
- No `TokenBlacklist` estático sin `Reset()`.
- No hardcodear connection strings/keys.
- Secretos locales solo en `appsettings.Local.json` (git-ignored) con carga opt-in `UseLocalSettings` + `CopyToOutput Never` en csproj; WAF nunca debe verlos.
- No Argon2id débil; BCrypt solo migración.
- No tocar pipeline sin orden §4 constitución.
- No relajar rate en prod; sin `PERF_*` en este MVP.
- No config eager en DI con overrides WAF — `IOptionsMonitor` lazy.
- No duplicar tablas vivas — canónico en `docs/`.
- No estados saga — no existe `06-04` aquí.
- No `Invoke-WebRequest` sin `-UseBasicParsing` en PS5.1.

## 4. Verificación por CRUD
- `build Release 0/0` antes de test.
- `Unit/Integration/Security/Contract/Database --no-build` 100% verdes.
- `critic-guardrails.ps1 → PASS`, `check_slnx.ps1 → PASS`, `check_endpoints.ps1 → OK`, `check_coverage.py ≥45%`, `check_delta_coverage.py --min 80 --advisory` informa el delta (no bloquea), `semgrep scan --config=auto --config=.semgrep/semgrep.yaml --error` 0 nuevos.
- `Stryker` PR advisory (`since:main`, `break:0`, `continue-on-error`) + solo nightly opcional; tras él siempre `build` antes de `--no-build`.
- Contract `CONTRACT_CAPTURE=1` para regenerar, sin var solo valida.
- `curl /health/ready` 200 con DB externa, 503 si cae.

## 5. Convenciones técnicas
- Rutas `api/v{version}/[controller]`; DTOs + FluentValidation; `RowVersion base64` en Update/Delete.
- Health `/health,/health/ready`; `/scalar` solo Dev. Sin `health-ui/metrics` full.
- Keys IMemory solo `blacklist:{jti}` (TTL = vida remanente, `nv-06`), `attempts:{user},lockout:{user}` 2FA TTL 120s. Password nunca en caché.
- Migrations: `dotnet ef migrations add --project API_SDD`.
- Auth `nv-05` da `AdminPolicy`; `nv-04` y demás CRUDs la consumen. `AuthController` único por slice auth (login+logout); Sonar S6960 suprimido con justificación (`#pragma` por archivo como S101) — dividirlo rompería la convención de rutas que verifica `check_endpoints`. 2FA TOTP `Otp.NET ±1 + DataProtection` llega en `nv-07`.
- Molde nv-02: `Common/PagedResult<T>` + validators espejo auto-registrados por reflexión (`ValidatorRegistration`, sin paquete DI extra); `Dtos/PaginationQuery` demo sin entidad.
- Nombres `SDD*` eximidos de Sonar S101 con `#pragma` por archivo (el prefijo lo manda constitución §5).
- OpenAPI de un solo v1: transformer `v{version}` → `v1` (sin Swagger/ApiExplorer).

## 6. Seguridad DevSecOps
- JWT HS256 real `UseJwtBearer=true` siempre (sin flag legacy), `OnTokenValidated` blacklist.
- Headers outermost + HSTS365d no-Dev + CORS 1 origen.
- Rate 5 policies + 429 uniforme + `UseRateLimiter` antes Auth.
- `Assembly integrity + timeout 60s + Serilog filtro secretos`.
- SAST Semgrep bloqueante; Sonar informativo; `Dependabot` activo. `Trivy/ZAP/RESTler` diferidos documentados, no exigidos.

## 7. Testing quirks
- `xUnit+FluentAssertions+Moq+FsCheck`; WAF para Integration/Security.
- `SecurityTest` reuse `UnitTest/Common/TestDataFactory`.
- `Testcontainers.MsSql` puerto fijo, suites con contenedor nunca en paralelo local (colisionan). `NBomber borra reportes` — no aplica aquí, pero separa `reports/` si lo usas futuro.
- Apphost local se mata por nombre `APISDD`, no `dotnet.exe` (`dotnet run` lanza el exe).
- `Contract IsConventional` legacy, no PascalCase puro.

## 8. Observabilidad mínima
- Serilog JSON + consola, sin secretos. CorrelationId por request.
- Sin OTel/Prometheus/Grafana por ahora.

## 9. Operación manual — sin hosting aún
- DB externa: migraciones aplicadas por ops + backup documentado fuera de repo (sin seed: bootstrap del primer Admin por token auto-firmado).
- Publish listo `dotnet publish`, no desplegado. Cuando haya hosting se añade runbook con `env` + `KeysPath` + verificación post-subida.
- Sin Redis: single-instance documentado. Sin AWS/SQS.

## 10. Lecciones
- Fuente: `Memoria.md`. Si se vuelve regla, mover a AGENTS.md.

## 11. Sub-agentes
- `slice-scaffolder` real-first sin Fase fake; `security-reviewer` gate sin editar; `traceability-clerk` reporta drift no edita; `readme-keeper` edita solo `**/*.md` (`bash: deny`).
- En CI solo `critic` ligero bloqueantes 1-4.

## Solution structure
| Project | Path | Type |
|---|---|---|
| `API_SDD` | `API_SDD/` | Web API `Program.cs` |
| `UnitTest` | `UnitTest/` | xUnit |
| `IntegrationTest` | `IntegrationTest/` | WAF + Testcontainers |
| `SecurityTest` | `SecurityTest/` | WAF |
| `DatabaseTest` | `DatabaseTest/` | Testcontainers migraciones/seed |
| `ContractTest` | `ContractTest/` | captura + convención |
| `deploy/` | `deploy/` | compose local SQL opcional (futuro, ver `deploy/README.md`) |
| `scripts/` | `scripts/` | critic, slnx, endpoints, coverage, delta |
| `stryker-config.json` | `./` | Stryker advisory nv-03 (`since:main`, sin break) |
| `docs/` | `docs/` | endpoints, rate-matrix, asvs |
