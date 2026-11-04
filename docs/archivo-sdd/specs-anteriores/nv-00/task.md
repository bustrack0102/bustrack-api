# NV-00 — Constitución MVP

## T1 — Reglas y stack sin CRUD
- **Crear**: `specs/nv-00/spec.md`, `plan.md`, `task.md`, `AGENTS.md §1 recortado`, `Memoria.md inicial`, `appsettings.Example.json mínimo` con `ConnectionStrings:Default=__SQL_EXTERNA__, UseInMemoryDatabase=false, Jwt:Key=PLACEHOLDER_HS256_KEY_MIN_32_BYTES, Cors:AllowedOrigins=[]`.
- **Crear**: `UnitTest/Common/AppSettingsTests.cs (3)` + `Directory.Build.props` con `All + TreatWarningsAsErrors + NuGetAudit + SonarAnalyzer.CSharp`.
- **Verificar**: `dotnet build -c Release 0/0 + AppSettingsTests 3/3 + critic PASS + NFR cada fila con comando`.
- **No hacer**: saga, Redis-dist, SQS, OTel, NBomber/Chaos, AWS, hosting, entidades, endpoints negocio.
