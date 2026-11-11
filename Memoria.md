# Memoria técnica — BusTrack API

> Registro vivo. No se borra historial. Última actualización: 2026-11-09.

## 1. Estado real

- **Repositorio:** `bustrack-api-main` (raíz interior; el árbol está anidado `X-main/X-main`).
- **Tecnología actual:** ASP.NET Core .NET 10, EF Core + SQL Server, JWT HS256, Argon2id, FluentValidation, Serilog, Scalar/OpenAPI, xUnit.
- **Tecnología objetivo:** ASP.NET Core .NET 10 + Azure Cosmos DB for NoSQL, conservando JWT HS256, Argon2id y FluentValidation.
- **Git:** no hay repositorio Git local; los tags/releases no son verificables.

## 2. Funcionalidades implementadas

- Foundation nv-00/nv-01: pipeline, headers, correlación, excepciones, health checks, probes, rate limiting, HSTS y CORS.
- CRUD SDDUsuario (nv-04) con paginación, RowVersion y Argon2id.
- Login JWT HS256 (nv-05) con lockout en SDDSegBloqueo y AdminPolicy.
- Logout con blacklist en IMemoryCache (nv-06).
- Cinco suites de prueba; 163 casos declarados y 94 unitarios verificados en verde.

## 3. Funcionalidades pendientes

- Dominio de transporte: empresas, asociaciones, rutas, paradas, destinos, horarios, tarifas y calificaciones.
- Migración de EF Core/SQL Server a Azure Cosmos DB for NoSQL.
- Bootstrap del primer Admin y refresh token/2FA (nv-07 no existe).
- Reglas Semgrep reales y gates CI efectivos.
- Dockerfile, contratos OpenAPI versionados y despliegue Azure.

## 4. Arquitectura actual

API_SDD/{Common,Controllers/V1,Data,Dtos,Health,Hosting,Logging,Middleware,OpenApi,Security,Services,Validators}; pruebas en UnitTest, IntegrationTest, SecurityTest, ContractTest y DatabaseTest

## 5. Migraciones

SQL Server + EF Core -> Azure Cosmos DB for NoSQL

## 6. Decisiones arquitectónicas

- Se adopta SDD y DevSecOps.
- La API central es la fuente autorizada de datos.
- Cosmos DB for NoSQL es la persistencia objetivo.
- ML y GenAI son servicios internos FastAPI.
- OpenAI API es el proveedor GenAI objetivo; Google GenAI queda inactivo.
- Flutter + Dart y Angular + TypeScript son los objetivos de cliente.
- No se declaran recursos Azure desplegados.

## 7. Dependencias entre repositorios

Expone REST a Flutter y Angular; consume ML/GenAI por HTTP interno y es la fuente autorizada de datos.

## 8. Problemas conocidos

- Documentación previa desalineada con el código (corregida en esta reorganización).
- Specs históricas conservadas como referencia; ver `specs/README.md`.
- Gates CI/SAST incompletos o no bloqueantes en el estado previo.
- Deuda de pruebas y despliegue documentada en cada spec.

## 9. Historial

- **2026-10-29:** estructura SDD original y primeras fases.
- **2026-10-30:** auditoría completa, corrección documental y reorganización SDD.
  Se separan specs históricas de canónicas, se corrigen afirmaciones de stack y se
  documentan migraciones, contratos y Azure propuesto.
- **2026-11-05:** completada la especificación SDD integral de endpoints en `specs-endpoints/` (24 a 40).
  Se cubren todas las funcionalidades del sistema: diagnóstico/observabilidad (ping, probes RFC 7807, OpenAPI/Scalar),
  dominio de transporte y catálogos (empresa individual, asociaciones, destinos, medios de transporte, ruta individual,
  paradas cercanas/individuales), administración de transporte (CRUD Admin empresas, rutas, horarios y tarifas),
  seguridad avanzada (refresh tokens con rotación, 2FA/MFA para Admin, bootstrap inicial seguro) e integraciones
  del ecosistema (inferencia de telemetría vial con microservicio ML y recomendaciones asistidas con GenAI acotadas a rutas reales sin invención).
  Actualizados `specs-endpoints/README.md`, `docs/endpoints.md` y `Memoria.md`.
- **2026-11-05 (EP-AU-01 a EP-AU-10):** análisis e incorporación de especificaciones SDD para endpoints de autenticación y perfil de usuario solicitados. Se comprobó que `EP-AU-02` (login), `EP-AU-03` (refresh), `EP-AU-04` (logout) y `EP-AU-10` (colección usuarios) ya existían en `13`, `33`, `14` y `16`. Se crearon los 6 endpoints nuevos faltantes: `41-endpoint-auth-registro` (EP-AU-01), `42-endpoint-auth-recuperar-password` (EP-AU-05), `43-endpoint-auth-restablecer-password` (EP-AU-06), `44-endpoint-usuario-perfil-me` (EP-AU-07), `45-endpoint-usuario-perfil-editar` (EP-AU-08) y `46-endpoint-usuario-perfil-baja` (EP-AU-09).
- **2026-11-09:** integrados los endpoints de lectura de Empresas en `main` (commit `b256ab0`):
  `GET /api/v1/empresas`, `GET /api/v1/empresas/autocompletar` y `GET /api/v1/empresas/{id}`.
  Archivos: `EmpresasController.cs`, `MongoDbContext.cs`, `EmpresaService.cs`, `IEmpresaService.cs`,
  DTOs `PagedResult`/`Empresa*Dto`, `API_SDD.csproj` (MongoDB.Driver 3.5.1) y `ServiceCollectionExtensions.cs`
  (DI de MongoDbContext + IEmpresaService + AddDistributedMemoryCache).
  Motivo: sincronizar el trabajo local del slice Empresas con `main`; el snapshot local era una historia git no emparentada y se integró por archivos, no por merge.
  Estado: Implementado parcialmente — build Release limpio y 94/94 tests unitarios en verde; **deuda documentada**: sin pruebas de integración contra MongoDB real ni tests unitarios de `EmpresaService`; límite estricto de `tamanoPagina` pendiente; mapeo 503 si falta `MongoDb:ConnectionString`.
  Riesgo: la persistencia actual es MongoDB Atlas de forma provisional; el objetivo sigue siendo Azure Cosmos DB for NoSQL (la interfaz `MongoDbContext` debe abstraerse antes de la migración).
  Relación con otros repos: ninguna por ahora (los clientes Flutter/Angular aún no consumen estos endpoints).
- **2026-11-10:** corregido fallo de CI (`NU1100`) en `main`: el `NuGet.config` usa `packageSourceMapping` de supply-chain (NV-01, solo nuget.org) y no contemplaba los paquetes del slice MongoDB.
  Se agregaron los patrones `MongoDB.*`, `DnsClient`, `SharpCompress*`, `Snappier` y `ZstdSharp.*`. Tras la corrección el run `725ea4a` compiló y pasó el quality gate (ci-pr y cd-azure/verify success).
  Estado: resuelto para CI en Linux; pendiente: vulnerabilidades transitivas NU1902/NU1903 (`System.Security.Cryptography.Pkcs` 9.0.11, `SharpCompress` 0.30.1, `Snappier` 1.0.0) — declaradas como advertencias (no bloquean) y pendientes de parchado de versiones.
  Riesgo: ninguno conocido; el mapeo de fuentes mantiene la política de un solo feed aprobado (nuget.org).
- **2026-11-10:** CD real de Azure: se reemplazó el `az container restart` (que solo reiniciaba la imagen vieja) por pipeline completo en `cd-azure.yml`: `az acr build` (ACR `bustrackacr0102`, admin, tags por SHA + latest) → `az container export` de la config actual → edición con python (imagen nueva + `imageRegistryCredentials` con credenciales admin efímeras en el job) → `az container delete` + `az container create --file`. Se añadió `Dockerfile` multi-stage (SDK 10.0 → aspnet 10.0, `PORT=8000`) y `.dockerignore`. Pins de seguridad en `Directory.Build.props` (Pkcs 10.0.15, SharpCompress 0.48.0, Snappier 1.3.1) y `.github/dependabot.yml` (nuget + actions, semanal). `remote_repo/` añadido a `.gitignore`.
  Estado: IMPLEMENTADO (pendiente de verificación en el primer run real de la CD).
  Riesgo: la recreación del contenedor borra el estado efímero del grupo (no hay datos persistentes en ACI); la IP/DNS público se conserva porque `az container export/create` mantiene los mismos recursos del grupo.
  Relación con otros repos: ninguna por ahora.
