# NV-01 — Foundation + pipeline + DB externa

## T1 — Casa vacía con luz, sin CRUD
- **Modificar**: `Program.cs` orden constitucional, `Directory.Build.props`, `nuget.config`, `appsettings.Example.json` solo claves usadas, `.gitignore`, `ci-pr.yml` con `restore→build→unit→integration→security→critic→endpoints→contract→semgrep`.
- **Crear**: `SecurityHeadersMiddleware.cs` con `NonceItemKey=CspNonce + GenerateNonce 16B + IsExempt scalar/openapi`, `ExceptionHandlingMiddleware.cs` con `WriteErrorAsync + JsonIgnore null Detail`, `PingController.cs`, `ProbeController.cs` con `EnableProviderStates+no-prod`, `AssemblyIntegrityCheck.cs`, `docs/endpoints.md` con `ping/probe/health`, `.semgrep/semgrep.yaml`, `scripts/critic-guardrails.ps1 + check_endpoints.ps1`.
- **Crear tests**: `Unit/DI 3 + Middleware 10 + Errors 14 + Integration/Routing 3 + Health 2 + Errors 8 + Security/Integrity 4 + Headers 3`.
- **Verificar**: `build 0/0 + Unit/Security/Integration 100% vacío + critic PASS + endpoints OK + semgrep 0 + /ready 200/503 externa`.
- **No hacer**: entidades, `AdminPolicy` usada, emisión JWT, TOTP, lockout, caché entidades, SQS, OTel, hosting.
