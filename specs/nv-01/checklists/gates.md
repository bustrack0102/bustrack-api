# Checklist — nv-01 Foundation + pipeline + DB externa

> Trazabilidad: `spec R1-R6 ↔ plan §3 ↔ task T1`. Estado: Borrador con evidencia.

- [x] Pipeline orden fijo (R1): SecurityHeaders → ExceptionHandling → ForwardedHeaders → HSTS → HttpsRedirect (solo Prod) → CORS → RateLimiter → Auth → AuthZ
- [x] `/health` 200 + `/health/ready` 200/503 + AssemblyIntegrity + JSON sin trazas (R2)
- [x] `GET api/v1/ping {status:Pong}` + versionado estricto 404 + JSON legacy + Scalar Dev + ErrorResponse + sondas (R3)
- [x] Config solo usada + `Program partial` + Build.props + nuget mapping + .gitignore (R4-R5)
- [x] Layout `API_SDD + 5 suites`, sin Chaos/Performance/Mutation en CI (R6)
- [x] Middlewares + `Ping/Probe` + `IntegrityCheck` + `docs/endpoints.md` inicial
- [x] Tests 47: Unit DI 3 + Middleware 10 + Errors 14, Integration Routing 3 + Health 2 + Errors 8, Security Integrity 4 + Headers 3
- [x] Gates: build 0/0 + Unit/Security/Integration 100% + critic PASS + endpoints OK + semgrep 0 + /ready 200/503
- [x] `docs/endpoints.md` reescrito a nv-01 (login/CRUD vuelven en nv-02)
- [x] Aprobación firmada: Usuario (aceptación expresa), 2026-10-11
