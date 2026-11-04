# NV-01 — Foundation + pipeline final + DB externa

## Contexto
Casa vacía pero con luz. Base ejecutable una sola vez, ya en forma prod, sin CRUD alguno.
**Depende de**: `NV-00`.

## Requisitos
1. Pipeline final día 1: `SecurityHeadersMiddleware outermost → ExceptionHandlingMiddleware → ForwardedHeaders → HSTS no-Dev → HttpsRedirection → CORS 1 origen → UseRateLimiter antes Auth → UseAuthentication → UseAuthorization → Controllers`.
2. `GET /health liveness 200 siempre + GET /health/ready 200 con SQL externo, 503 si cae + AssemblyIntegrity SHA-256 + JSON genérico sin trazas en prod`.
3. `GET api/v1/ping → {Status:Pong} + versionado estricto sin versión 404 + JSON legacy medido (prefijos minúsculos + resto PascalCase + id, PropertyNamingPolicy=null) + Scalar solo Dev + ErrorResponse{Error,Status,TraceId,Detail solo no-prod} + sondas probe/{timeout,error,forbidden} gateadas no-prod`.
4. Config solo usada: `ConnectionStrings:Default externa por env, UseInMemoryDatabase=false default, Jwt:Key>=32B solo env/Issuer/Audience configurados aunque sin usuarios, PasswordHasher:, RateLimiting: 5 policies, Cors:, DataProtection:KeysPath, Kestrel:Limits 10MB/2min, PORT`.
5. `Program public partial` para `WebApplicationFactory`. `Directory.Build.props All+TreatWarningsAsErrors+NuGetAudit+Sonar + nuget.config mapping + .gitignore`.
6. Layout: `API_SDD + Unit + Integration + Security + Database + Contract`. Sin `Chaos/Performance/Mutation`. CI `build→critic→endpoints→semgrep` verde en vacío.
> Nota: orden vacío acotado a nv-01; canónico completo en `ci-pr.yml` (`AGENTS.md §1` / `constitution.md §8`).

## Diseño
- `Program.cs: AddWebApiServices + UseWebApiPipeline` orden fijo constitucional. `RateLimiter + Headers + JWT` ya configurados aunque sin `AdminPolicy` usada — es base, no CRUD.
- `SecurityHeadersMiddleware`: `nosniff, DENY, strict-origin-when-cross-origin, X-XSS 0, CSP nonce 16B/request en Items[CspNonce], exención scalar/openapi`.
- `ExceptionHandlingMiddleware`: `404/403/409/422/400/408/500`, `HasStarted` relanza, `Detail` nulo en prod.
- `DbContext` futuro lee `IConfiguration` lazy en factory para overrides WAF. Retry EF + timeout 60s. Cero connection string hardcodeada.
- `IMemoryCache` registrado solo para `blacklist:{jti}+attempts 2FA` futuros — deuda single-instance, 0 `cache:{entidad}`.

## Contratos
- `GET /health, GET /health/ready, GET /api/v1/ping, GET /api/v1/probe/* no-prod, /openapi/v1.json + /scalar/v1 solo Dev`.
- Claves tabla canónica recortada `ConnectionStrings:Default, UseInMemoryDatabase, Jwt:Key/Issuer/Audience, Cors:AllowedOrigins, Kestrel:Limits`.

## Tests
- `Unit/DI 3: CORS registrado, sin AllowAnyOrigin, orígenes configurables`.
- `Unit/Middleware 10: headers+CSP+nonce 16B único, 5 exentas sin CSP, set antes _next`.
- `Unit/Errors 14: tabla 8 mapeos + nulos + Detail prod/dev + HasStarted`.
- `Integration/Routing 3: v1 resuelve + PascalCase, sin versión 404, openapi/scalar Dev 200 Prod 404`.
- `Integration/Health 2 + Integration/Errors 8 en Staging + Security/AssemblyIntegrity 4 + Security/Headers 3`.

## Criterios
- `dotnet build -c Release --no-restore → 0 errores 0 warnings`.
- `dotnet test UnitTest/SecurityTest/IntegrationTest -c Release --no-build → 100% verdes en vacío`.
- `curl /health 200 + /health/ready 200 con externa, 503 sin DB`.
- `powershell -File scripts/critic-guardrails.ps1 → PASS exit 0`.
- `powershell -File scripts/check_endpoints.ps1 → OK exit 0` solo `ping/probe/health`.
- `semgrep scan --config=auto --config=.semgrep/semgrep.yaml --error → 0 nuevos`.
> Nota: `Database/Contract` se activan con primera entidad; en vacío `CONTRACT_CAPTURE=0`.

## Límites
Sin entidades, sin `AdminPolicy` usada, sin Redis/SQS/OTel/health-ui/metrics full. JWT configurado pero sin emisión — la emisión es `NV-03`. Sin hosting — `dotnet publish` listo no desplegado.

## Aprobación y Control de Cambios
- **Estado:** ✅ Aprobada
- **Revisores:** Usuario (aceptación expresa en sesión)
- **Fecha:** 2026-10-11
- **Detalle:** implementada y verificada (54/54 tests, build 0/0, cobertura 66.32%, critic/endpoints/sdd OK). Evidencia en `specs/nv-01/checklists/gates.md` + `Memoria.md` fase Fase-1.
