# plan.md — NV-01 Foundation + pipeline + DB externa

1. Mapeo
- Modificar: `API_SDD/Program.cs` con `AddWebApiServices + UseWebApiPipeline`, `Directory.Build.props`, `nuget.config` mapping `Microsoft.*/System.*/SonarAnalyzer.*/xunit*/coverlet* → nuget.org`, `appsettings.Example.json`, `appsettings.json`, `.gitignore` con `appsettings.Local.json + bin/obj/.vs/coverage`, `.github/workflows/ci-pr.yml`.
- Crear: `Middleware/SecurityHeadersMiddleware.cs`, `Middleware/ExceptionHandlingMiddleware.cs`, `Controllers/V1/PingController.cs`, `Controllers/V1/ProbeController.cs`, `Services/AssemblyIntegrityCheck.cs`, `docs/endpoints.md` inicial con `ping/probe/health`, `scripts/critic-guardrails.ps1`, `scripts/check_endpoints.ps1`, `.semgrep/semgrep.yaml`.
- Registrar: `IMemoryCache` solo para `blacklist:{jti}+attempts 2FA` futuros, 0 `cache:{entidad}`.

2. Guardarraíles
- Orden constitucional fijo: `SecurityHeaders outermost absoluta antes DeveloperExceptionPage → ExceptionHandling → ForwardedHeaders → HSTS no-Dev → HttpsRedirection → CORS → RateLimiter antes Auth → Auth JWT configurado → Authorization`.
- `SecurityHeaders`: `nosniff + DENY + strict-origin-when-cross-origin + X-XSS 0 + CSP nonce 16B/request Base64 en Items[CspNonce] + exención scalar/openapi sin CSP`.
- `ExceptionHandling`: `404/403/409/422/400/408/500 + Detail nulo prod + HasStarted relanza + sin try/catch en controllers`.
- `AddHsts MaxAge 365d` solo no-Dev vía `AddHsts`, sin overload `UseHsts(Action)`.
- `JwtBearer` configurado `key≥32B env + ClockSkew=Zero + ValidAlgorithms=[HS256] + Issuer/Audience` aunque sin emisión — emisión es NV-03.
- `RateLimiter` 5 policies `SlidingWindow QueueLimit=0 + ConcurrentWrites` + `OnRejected 429 ErrorResponse + Retry-After`, lazy `IOptionsMonitor` para WAF.

3. Pruebas
- `Unit/DI 3`: CORS registrado, sin `AllowAnyOrigin`, orígenes configurables.
- `Unit/Middleware 10`: 2 nulos + headers+CSP+nonce + nonce único + Theory 5 exentas + set antes `_next`.
- `Unit/Errors 14`: tabla 8 + nulos + passthrough + Detail prod/dev + HasStarted.
- `Integration/Routing 3`: v1 resuelve + PascalCase, sin versión 404, openapi/scalar Dev 200 Prod 404 en `Production` con `UseInMemoryDatabase` override si aplica.
- `Integration/Health 2`: `/health 200 + /health/ready 200 con externa, 503 sin DB`.
- `Integration/Errors 8` en `Staging`: `404/403/409/422 reales + 408/500 por sonda + Detail ausente prod + sonda deshabilitada prod`.
- `Security/AssemblyIntegrity 4`: correcto Healthy, incorrecto Unhealthy Prod/Degraded Dev, ausente Degraded Dev/Healthy Prod.
- `Security/Headers 3`: `/health` anónimo con headers+CSP + `/probe/forbidden` 403 con headers + `HstsOptions MaxAge 365d` en factory Production.

4. Secuencia
1. `Directory.Build.props + nuget.config + Example.json + .gitignore + semgrep.yaml`.
2. `Program.cs` pipeline + `SecurityHeadersMiddleware` outermost.
3. `ExceptionHandlingMiddleware + ErrorResponse + ProbeController` gateado.
4. `Routing v1 + PingController + Scalar solo Dev`.
5. `Health + AssemblyIntegrity + RateLimiter + JwtBearer configurado + CORS + HSTS`.
6. `docs/endpoints.md + ci-pr.yml + scripts` y gates en vacío.

5. Verificación
- `dotnet restore; dotnet build -c Release --no-restore → 0/0`
- `dotnet test UnitTest/SecurityTest/IntegrationTest -c Release --no-build → 100% verdes vacío`
- `powershell -File scripts/critic-guardrails.ps1 → PASS exit 0`
- `powershell -File scripts/check_endpoints.ps1 → OK exit 0`
- `semgrep scan --config=auto --config=.semgrep/semgrep.yaml --error → 0 nuevos`
- `curl /health 200 + /health/ready 200/503 + /api/v1/ping Pong`
