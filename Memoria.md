# Memoria técnica — BusTrack API

> Registro vivo. No se borra historial. Última actualización: 2026-11-11.

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
- **EP-AU-01 (registro):** `POST /api/v1/auth/registro` — alta de usuario con hash BCrypt, rol 'Usuario' por defecto, par JWT + Refresh Token.
- **EP-AU-02 (login unificado):** `POST /api/v1/auth/login` — compatible con formato legacy (`strNombreUsuario`/`strPWD`) y con el nuevo formato email/password; devuelve accessToken + refreshToken.
- **EP-AU-03 (refresh):** `POST /api/v1/auth/refresh` — rotación segura del Refresh Token con revocación de token anterior.
- **EP-AU-04 (logout):** `POST /api/v1/auth/logout` — revocación de sesión activa o de todos los tokens del usuario.
- **EP-AU-05 (recuperar password):** `POST /api/v1/auth/recuperar-password` — generación de token de recuperación (SHA-256), almacenado en `tokens_recuperacion`.
- **EP-AU-06 (restablecer password):** `POST /api/v1/auth/restablecer-password` — validación del token + reestablecimiento del hash de contraseña.
- **EP-AU-07 (perfil me):** `GET /api/v1/usuarios/me` — retorna perfil del usuario autenticado.
- **EP-AU-08 (editar perfil):** `PUT /api/v1/usuarios/me` — actualización de nombre y/o contraseña.
- **EP-AU-09 (baja cuenta):** `DELETE /api/v1/usuarios/me` — baja lógica (activo = false) del propio usuario.
- **EP-AU-10 (listado admin):** `GET /api/v1/usuarios` — colección paginada con filtros (solo Admin).
- **EP-EM-01 (lista empresas):** `GET /api/v1/empresas` — listado paginado con filtros multicriterio (q, medioTransporteId, municipio, calificacionMin, ordenarPor).
- **EP-EM-02 (autocompletar empresas):** `GET /api/v1/empresas/autocompletar` — hasta 10 sugerencias ultraligeras con índice de texto.
- **EP-EM-03 (detalle empresa):** `GET /api/v1/empresas/{id}` — ficha completa con rutas, medios de transporte y destinos.
- **EP-EM-04 (rutas de empresa):** `GET /api/v1/empresas/{id}/rutas` — rutas filtradas por medio de transporte, parada o día.
- **EP-EM-05 (comparar empresas):** `GET /api/v1/empresas/comparar` — comparativa lado a lado de 2–4 empresas (placeholder documentado como deuda).
- **EP-EM-06 (crear empresa):** `POST /api/v1/empresas` — alta de empresa (Admin/JWT).
- **EP-EM-07 (editar empresa):** `PUT /api/v1/empresas/{id}` — actualización de empresa (Admin/JWT).
- **EP-EM-08 (baja empresa):** `DELETE /api/v1/empresas/{id}` — baja lógica de empresa (Admin/JWT).
- **EP-EM-09 (lista medios de transporte):** `GET /api/v1/medios-transporte` — catálogo completo con filtro activo.
- **EP-EM-10 (crear medio de transporte):** `POST /api/v1/medios-transporte` — alta de medio (Admin/JWT).
- **EP-EM-11 (editar medio de transporte):** `PUT /api/v1/medios-transporte/{id}` — actualización de medio (Admin/JWT).
- **EP-EM-12 (baja medio de transporte):** `DELETE /api/v1/medios-transporte/{id}` — baja lógica de medio; impide eliminación si tiene rutas activas (Admin/JWT).
- Seis suites de prueba; 112 unitarios verificados en verde (incluye 16 nuevos: `PasswordPolicyTests`).

## 3. Funcionalidades pendientes

- Pruebas de integración para los nuevos 22 endpoints (EP-AU-01..10 y EP-EM-01..12).
- Aplicar la migración EF Core `AddAuthAndTransportTables` (creada el 2026-11-11, ver historial) en base de datos SQL Server de producción/staging.
- Dominio de transporte restante: rutas (CRUD admin), paradas, destinos, horarios, tarifas y calificaciones.
- Bootstrap del primer Admin y 2FA/MFA (nv-07).
- Eliminar o reconectar `EmpresaService` (MongoDB) ahora que `EmpresasController` usa `IEmpresaSqlService`; decisión pendiente.
- Migración de EF Core/SQL Server a Azure Cosmos DB for NoSQL (objetivo arquitectónico).
- Reglas Semgrep reales y gates CI efectivos.
- Deuda EP-EM-05 (comparar): placeholder sin datos reales de calificación calculada en tiempo real.

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
- **2026-11-11:** análisis contra el Excel `BusTrack_Componentes_Backend_Movil_Azure.xlsx` (épicas EP-01 Auth/Usuarios y EP-02 Empresas/Medios) y corrección de fallos reales: 
  1. **Rutas rotas (crítico):** los 12 endpoints EP-EM-01..12 (`EmpresasController`, `MediosTransporteController`) devolvían 404: usaban rutas literales `api/v1/...` incompatibles con el `UrlSegmentApiVersionReader` del pipeline. Corregido a `[ApiVersion("1.0")]` + plantilla `api/v{version:apiVersion}/...` (mismo patrón que el resto). Evidencia: nueva suite `IntegrationTest/EpRoutingTests.cs` pasó de 12 fallos a 22/22 en verde.
  2. **Migración faltante (crítico):** creada `AddAuthAndTransportTables` (7 tablas TB-AU-01..04 + TB-EM-01/02 + `rutas`, índices, FKs y seed de roles). Las 7 tablas nuevas no tenían migración y los endpoints EP-AU/EP-EM habrían fallado contra una BD migrada. El snapshot `AppDbContextModelSnapshot.cs` ahora refleja el modelo completo (antes solo las 2 tablas legacy).
     Nota técnica: los IDs de migración de este repo NO llevan timestamp (patrón manual desde `AddInitialSDDUsuario`), lo que rompe `dotnet-ef migrations list/add`; la migración se generó con el tool (respaldando/restaurando las legacy) y se recortó su `Up()/Down()` a las tablas nuevas. Verificado con script idempotente (`dotnet ef migrations script --idempotent`).
  3. **Robustez EP-AU:** `AuthService.RegistroAsync` valida nombre/correo/formato (null → 400, antes NRE → 500); `UsuarioPerfilService.UpdateMeAsync` valida fuerza de contraseña (letras+números) y formato de correo; `DeleteMeAsync` ahora anonimiza (nombre, correo `anon-{id}@baja.bustrack.local`, hash aleatorio, consentimiento off) y elimina tokens de recuperación pendientes, según spec EP-AU-09. Política compartida extraída a `Security/PasswordPolicy.cs` (`EnsureStrongPassword`, `IsValidEmail`).
  4. **Robustez EP-EM:** `EmpresaSqlService` Create/Update devuelven `EmpresaResponseDto` (antes exponían la entidad EF completa), invalidación de caché por versión en escrituras admin (spec EP-EM-06/07), `CompararAsync` filtra empresas por trayecto (parada origen/destino/destino turístico) y limita a 4 empresas; null guards en Updates.
  Archivos afectados: `API_SDD/Controllers/V1/{EmpresasController,MediosTransporteController}.cs`, `API_SDD/Data/Migrations/*`, `API_SDD/Security/PasswordPolicy.cs`, `API_SDD/Services/{AuthService,UsuarioPerfilService}.cs`, `API_SDD/Services/Empresa/{EmpresaSqlService,IEmpresaSqlService}.cs`, `API_SDD/Services/MedioTransporte/MedioTransporteService.cs`, `API_SDD/Dtos/Empresa/EmpresaAdminDtos.cs`, `IntegrationTest/EpRoutingTests.cs`, `UnitTest/Auth/PasswordPolicyTests.cs`.
  Estado: Implementado. Validación local: `dotnet build API_SDD.slnx -c Release` (0 errores/0 advertencias), UnitTest 112/112, IntegrationTest 27/27 (Routing 5 + EpRouting 22; el resto requiere Docker), SecurityTest 7/7 (Headers+Integrity), ContractTest 2/2 (Ping).
  Decisiones del equipo (2026-11-11): (a) conservar `EmpresaService`/`MongoDbContext` (MongoDB) aunque sigue desconectado de los controladores — deuda y decisión pendiente; (b) las pruebas de integración de los 22 endpoints EP-AU/EP-EM quedan pendientes (requieren Docker, solo verificables en CI).
  Riesgo: la migración existe pero NO está aplicada a ninguna BD real; BE-35 (envío de correo de recuperación) no existe — el token se genera y persiste pero no se envía; el lockout de BD sigue siendo solo del login legacy (login V2 cubierto por rate limiting HTTP, conforme a la spec EP-AU-02).
  Relación con otros repos: ninguna por ahora (Flutter/Angular aún no consumen estos endpoints).
- **2026-11-11 (Rutas, Paradas, Moderación, Inferencia, Resiliencia y Puerta SDD):**
  - Implementados `RutasController`, `ParadasController`, `ModeracionController` (`47-endpoint-moderacion`), `InferenciaController` (`/telemetria/inferencia`, `/recomendaciones`) y degradación resiliente en `EmpresaSqlService`, `MedioTransporteService` y `MongoDbHealthCheck`.
  - Agregada suite `UnitTest/Controllers/V1EndpointsTests.cs` verificando rutas, Haversine, moderación (prompt injection / PII / vocabulario / URIs), inferencia inercial, recomendaciones y fallback sin SQL externo.
  - Reparada puerta SDD (`scripts/check_sdd.ps1`, `docs/endpoints.md`, `specs*/**/spec.md` con bloque `## Aprobación`, `specs/*/checklists/pr.md`) e integrada en `.github/workflows/ci-pr.yml`.
- **2026-11-11 (Etapa 0 — v0.2.0 SemVer, Saneamiento Critic, Brotli/SourceGen JSON y MongoDB Atlas Ready):**
  - Definida matriz SemVer previa al desarrollo en `docs/versionamiento.md` (Req. 11).
  - Eliminados secretos literales de `API_SDD/appsettings.Production.json`, corregida falsa alarma `TODO`/`todos` y rutas kebab-case/`[Route]` en `scripts/critic-guardrails.ps1` (`PASS`).
  - Reparado ciclo de vida de `MongoClient` en `API_SDD/Data/MongoDbContext.cs` (eliminado `using var` prematuro; implementa `IDisposable`), agregados `MongoIndexInitializer` (índices `unique`, texto, compuesto y `2dsphere`), `BusTrackJsonContext` (Source-Gen JSON) y compresión **Brotli + Gzip** (`AddResponseCompression` / `UseResponseCompression`).
  - Creado script entregable idempotente `db/mongodb/init-bustrack-nosql.js` con colecciones, `$jsonSchema`, índices y catálogo semilla real Tula–Tepeji.
  - Actualizado `.github/workflows/ci-pr.yml` y `cd-azure.yml` con `--secure-environment-variables` (`MongoDb__ConnectionString`, `Jwt__Key`, `InternalService__Key`, `Persistence__Provider=MongoDB`) y smoke gate `/health/ready` (`200 Healthy`).
  - Verificación local: `dotnet build API_SDD.slnx -c Release` (0 errores/0 warnings), `UnitTest` 127/127, `IntegrationTest` no-Docker 37/37, `SecurityTest` no-Docker 7/7, `check_sdd.ps1` (`SDD-OK`), `critic-guardrails.ps1` (`PASS`), `node --check db/mongodb/init-bustrack-nosql.js` ok.
- **2026-11-12 (Etapa 1 — v0.4.0 Persistencia NoSQL Unificada, Caché Distribuido Redis, Correo SMTP/Webhook y Core MVP EP1–EP5):**
  - Implementada capa NoSQL unificada `IBusTrackNoSqlStore` (`MongoBusTrackStore` + `InMemoryBusTrackStore` + `BusTrackSeedData` alineado con `db/mongodb/init-bustrack-nosql.js`) e implementaciones Mongo para `MongoAuthService`, `MongoUsuarioPerfilService`, `MongoEmpresaService`, `MongoMedioTransporteService` y `TransportCatalogService`.
  - Sanitización de URI `MongoDbContext.SanitizeConnectionString` (elimina caracteres invisibles/control Unicode para prevenir `SaslPrepHelper` al conectar con Atlas).
  - Blacklist distribuida JWT (`blacklist:{jti}`) y caché distribuido por versión en `TokenRevocationService` (`IDistributedCache` + `StackExchangeRedis` / fallback `DistributedMemoryCache`), `RedisHealthCheck` y `IEmailNotificationService`.
  - Controladores completos Core MVP (`EP1–EP5` + `EP-AD-01`): `RutasController`, `ParadasController`, `MapaController` (`48-endpoint-mapa-elementos`), `HorariosController`, `TarifasController`, `DestinosController` y `CatalogosController` (`49-endpoint-catalogos-estado`), actualizando `docs/endpoints.md` y `specs-endpoints/`.
  - Agregadas suites `TokenRevocationDistributedTests`, `CoreEndpointsIntegrationTests` y `NoSqlSecurityTests`.
  - Verificación local tras la edición final: `dotnet build API_SDD.slnx -c Release` (0 errores/0 advertencias), `UnitTest` 129/129, `IntegrationTest` no-Docker 62/62, `SecurityTest` no-Docker 9/9, `check_sdd.ps1` (`SDD-OK`), `critic-guardrails.ps1` (`PASS`), `node --check db/mongodb/init-bustrack-nosql.js` ok.
- **2026-11-12 (Etapa 2 — v0.6.0 Operación, Moderación Cloud Texto/Imagen/Video, Blob CDN, Push FCM y Admin CRUD EP6/EP7/EP10):**
  - Implementado `IContentModerationService` (Azure AI Content Safety `F0` + motor local determinista: Prompt Injection, XSS, PII, firmas mágicas JPEG/PNG/WebP/MP4/WebM y evaluación de fotograma clave) conectado obligatoriamente previo al almacenamiento (`422` si falla).
  - Implementados `BlobMediaService` + `MediaController` (`POST /api/v1/media/upload` con `Cache-Control: public, max-age=31536000, immutable`), `PushNotificationService` + `DispositivosController` (FCM HTTP v1 con firma RS256 nativa y degradación segura sin credenciales expuestas).
  - Implementados `CalificacionesController` (`EP-CA-01..06`), `ViajesController` (`EP-VG-01..05`), `AdminController` (`EP-AD-02..07` fuentes de datos e importaciones) y CRUD administrativo en `ParadasController` (`EP-RP-11..13`), `RutasController` (`EP-RP-14..16`), `HorariosController` (`EP-HT-03..05`), `TarifasController` (`EP-HT-08..10`) y `DestinosController` (`EP-DE-06..08`).
  - Documentadas nuevas specs `50..53` y filas en `docs/endpoints.md`, agregadas suites `ContentModerationAndMediaTests`, `OperationalEndpointsIntegrationTests` y `OperationalSecurityTests` (IDOR 403, Admin RBAC 403 y MIME falsificado 422).
  - Verificación local tras la edición final de código: `dotnet build API_SDD.slnx -c Release` (0 errores/0 advertencias), `UnitTest` 132/132, `IntegrationTest` no-Docker 64/64, `SecurityTest` no-Docker 11/11, `check_sdd.ps1` (`SDD-OK`), `critic-guardrails.ps1` (`PASS`).
- **2026-11-12 (Etapa 3 — v0.8.0 Orquestación ML RandomForest y GenAI Grounded E2E EP8/EP9/EP-AD-08):**
  - Implementados `MlServiceClient` y `GenAiServiceClient` con `IHttpClientFactory` + políticas de resiliencia `Microsoft.Extensions.Http.Resilience` (Polly) y cabecera `X-Internal-Key`.
  - Implementados `ConsentimientosController` (`EP-ML-01..03`), `SensoresController` (`EP-ML-04, EP-ML-06`), `CandidatosParadaController` (`EP-ML-07` y `PATCH /api/v1/admin/candidatos-parada/{id}/validar` `EP-AD-08` que promueve a `paradas` oficiales e invoca Push FCM).
  - Actualizado `InferenciaController` (`POST /telemetria/inferencia`, `POST /recomendaciones` `EP-IA-01` con validador anti-alucinación BE-34, `GET /recomendaciones/historial` `EP-IA-02` y `POST /recomendaciones/{id}/retroalimentacion` `EP-IA-03`).
  - Documentadas specs `54..55` y agregadas suites `IntelligenceUnitTests`, `IntelligenceEndpointsIntegrationTests` e `IntelligenceSecurityTests`.
  - Verificación local tras la edición final de código: `dotnet build API_SDD.slnx -c Release` (0 errores/0 advertencias), `UnitTest` 133/133, `IntegrationTest` no-Docker 66/66, `SecurityTest` no-Docker 12/12, `check_sdd.ps1` (`SDD-OK`), `critic-guardrails.ps1` (`PASS`).
- **2026-11-12 (Etapa 4 — v0.9.0 Terraform IaC en `rg-bustrack-ncus`, Observabilidad, Container Apps HTTPS ≥2 réplicas y Auditoría 86/86):**
  - Creada infraestructura declarativa completa en `infra/terraform/` (`providers.tf`, `variables.tf`, `main.tf`, `outputs.tf`) + workflow `.github/workflows/infra-lifecycle.yml` (`plan`, `apply`, `destroy` y retiro automático de ACI costosos).
  - Recursos gestionados en `northcentralus`: `Log Analytics` + `Application Insights`, `Azure Storage Account` (`media` Blob CDN), `Cognitive Services ContentSafety F0`, `Container App Environment` (Envoy HTTPS LB), `bustrack-api` (`min_replicas = 2`, HTTPS), `bustrack-landing` (HTTPS) y microservicios internos `bustrack-ml` / `bustrack-genai` (`external_enabled = false`).
  - Documentados `docs/auditoria-rubrica.md` (trazabilidad de los 20 requisitos docentes y 86/86 endpoints `EP-*`, registrando pausa explícita de `bustrack-mobile` Etapa 5) y `scripts/k6-load-test.js`.
- **2026-11-25 (Corrección de verificación de salud en Production y resolución de 500 a 503 en `/health/ready`):**
  - **Causa raíz:** En entorno `Production`, si MongoDB ni Redis están configurados, `IDistributedCache` lanza intencionalmente `InvalidOperationException` (prevención de fail-open a memoria volátil). Sin embargo, `RedisHealthCheck` inyectaba directamente `IDistributedCache` en su constructor, lo que provocaba que la instanciación del health check por el contenedor DI de ASP.NET Core lanzara una excepción no capturada fuera del pipeline de health checks, escalando a `HTTP 500` con `{"error":"internal","status":500}` en lugar del esperado `HTTP 503 Service Unavailable` estructurado en `/health/ready`.
  - **Remediación:** En [RedisHealthCheck.cs](file:///c:/Users/ximeg/source/repos/BusTrack/Repos/bustrack-api-main/bustrack-api-main/API_SDD/Health/RedisHealthCheck.cs), se inyectó `IServiceProvider` para resolver `IDistributedCache` de forma segura durante `CheckHealthAsync` dentro de un bloque `try-catch`, retornando `HealthCheckResult.Unhealthy("distributed cache service unavailable in production.", ex)` ante fallos de configuración.
  - **Resultado:** La prueba `UnitTest.Middleware.CompressionAndJsonTests.ProductionConPersistenceMongoSinCadenaRetorna503SinTrazas` y toda la suite `UnitTest` (145/145 pruebas) pasaron limpiamente en verde con código HTTP 503.
- **2026-11-25 (Diagnóstico y corrección integral de CORS entre BusTrack Flutter Web y ASP.NET Core API):**
  - **Causa raíz identificada:** En `ServiceCollectionExtensions.cs`, la política CORS `SingleOrigin` configuraba `.WithOrigins(origins)` leyendo exclusivamente `Cors:AllowedOrigins` desde configuración. En entornos locales o de desarrollo sin dicha sección configurada, `origins` era un array vacío `[]`. Adicionalmente, no se habilitaban explícitamente las cabeceras preflight requeridas por Dio en Flutter Web (`X-API-Version`, `Authorization`, `Accept`, `Content-Type`), ni se exponían cabeceras críticas como `Retry-After` o `api-supported-versions`.
  - **Corrección en API (`ServiceCollectionExtensions.cs`):**
    - Configuración dinámica segura: si existen orígenes en `Cors:AllowedOrigins`, se configuran directamente. Si está en entorno `Development` y la lista está vacía, se activa un fallback seguro vía `SetIsOriginAllowed` que autoriza automáticamente cualquier puerto local (`localhost` o `127.0.0.1`), permitiendo a Flutter Web depurar sin fricción en Edge, Chrome o emuladores web.
    - Encabezados permitidos explícitos: `Content-Type`, `Accept`, `Authorization`, `X-API-Version`, `X-Requested-With`, `Origin`.
    - Métodos HTTP permitidos explícitos: `GET`, `POST`, `PUT`, `PATCH`, `DELETE`, `OPTIONS`.
    - Encabezados expuestos: `Retry-After`, `X-Correlation-ID`, `api-supported-versions`.
  - **Configuraciones de entorno:**
    - `appsettings.Development.json`: Añadidos orígenes de desarrollo habituales (`http://localhost:5000`, `http://localhost:5001`, `http://localhost:8080`, `http://localhost:3000`, `http://localhost:5173`, `http://127.0.0.1:*`).
    - `appsettings.Production.json`: Añadidos dominios oficiales autorizados de BusTrack (`https://bustrack.lat`, `https://www.bustrack.lat`, `https://app.bustrack.lat`).
  - **Alineación en Cliente Flutter (`bustrack-mobile`):**
    - `auth_interceptor.dart`: Actualizada la lista `publicEndpoints` para incluir `healthLive` (`/health`), `healthReady` (`/health/ready`), `verificarCorreo` y `reenviarVerificacion`, evitando que solicitudes públicas lleven tokens inválidos y asegurando preflight limpio.
  - **Pruebas automatizadas de integración CORS:**
    - Agregadas pruebas en `DevSecOpsSecurityTests.cs`: `PreflightOptionsDevuelveHeadersCorsParaFlutterWeb`, `SolicitudPublicaConOriginRetornaHeadersCors` y `SolicitudProtegidaConBearerTokenFuncionaConCors`.
    - Verificación exitosa en `SecurityTest`: 7/7 pruebas superadas, incluyendo OPTIONS preflight (204 NoContent + Access-Control-Allow-Origin/Methods/Headers).
- **2026-11-25 (Implementación y ejecución de pruebas de seguridad DevSecOps y actualización CI/CD):**
  - **Suites de Seguridad Implementadas (`DevSecOpsSecurityTests`):**
    - *Detección de Reuso de Refresh Token (OAuth 2.0 Threat Model):* Verificación de invalidación de toda la familia de tokens de sesión ante intentos de replay attack.
    - *Mitigación de Inyección NoSQL:* Validación estricta y desinfección de operadores BSON y caracteres maliciosos en credenciales y parámetros de búsqueda.
    - *Seguridad en Verificación de Correo:* Validación criptográfica de tokens y rechazo de valores nulos o inválidos.
    - *Cabeceras de Seguridad HTTP:* Validación de `X-Content-Type-Options: nosniff`, `X-Frame-Options` y HSTS.
  - **Actualización de CI/CD:** Incorporada la suite `DevSecOpsSecurityTests` en el pipeline `.github/workflows/ci-pr.yml` como gate de seguridad obligatorio.
  - **Resultados de ejecución:** `SecurityTest` superó 16/16 pruebas de seguridad (0 errores, 0 fallos), `check_sdd.ps1` (`SDD-OK`), `critic-guardrails.ps1` (`PASS`).
- **2026-11-25 (Integración completa con Azure Cosmos DB for MongoDB vCore — BusTrackDB_Import):**
  - **Objetivo y alcance:** Consolidación de todos los módulos del backend (transporte, usuarios, autenticación, tokens, calificaciones, administración, inteligencia y notificaciones) en una única base de datos centralizada: `BusTrackDB_Import`. La base original `BusTrackDB` permanece intacta con sus 18 documentos originales.
  - **Datos de transporte intactos:** Se verificó la salvaguarda de no sobreescritura de los 23,850 documentos en `BusTrackDB_Import` (16 empresas, 8 mediosTransporte, 5 fuentes_datos, 12,813 paradas, 14 destinos, 439 rutas, 10,517 horarios, 38 tarifas). Se implementó validación estricta en `MongoBusTrackStore.EnsureSeedCollectionAsync` que verifica `CountDocumentsAsync(Empty) > 0` antes de cualquier intento de sembrado, garantizando 0 modificaciones o reinserciones sobre los datos existentes.
  - **Compatibilidad BSON:** Todos los modelos NoSQL (`ParadaDoc`, `RutaDoc`, `HorarioDoc`, `TarifaDoc`, `EmpresaDoc`, etc.) operan con `GuidRepresentation.Standard` (RFC 4122, subtype 4), tipos numéricos alineados (int/double/decimal), soporte para formatos horarios extendidos (24:xx/25:xx) vía strings nativos BSON e índices GeoJSON `2dsphere` para coordenadas `[longitud, latitud]`.
  - **Módulos y colecciones integradas:** La base `BusTrackDB_Import` concentra tanto las 8 colecciones de transporte como las colecciones operativas y de seguridad (`usuarios`, `refresh_tokens`, `tokens_recuperacion`, `tokens_verificacion_correo`, `seg_bloqueo`, `roles`, `calificaciones`, `viajes_guardados`, `consentimientos_sensores`, `lotes_sensores`, `candidatos_parada`, `solicitudes_recomendacion`, `importaciones`, `dispositivos_fcm` y `app_cache_entries`).
  - **Seguridad y secretos:** Cadena de conexión parametrizada hacia `BusTrackDB_Import` sin credenciales en Git, validada con `critic-guardrails.ps1` (`PASS`).
  - **Compilación y SDD:** `dotnet build API_SDD/API_SDD.csproj -c Release` (0 errores, 0 warnings), `check_sdd.ps1` (`SDD-OK`).
- **2026-11-23 (Migración de conexión a Azure DocumentDB vCore — BusTrackDB):**
  - **Objetivo y alcance:** Sustitución de la conexión de persistencia de MongoDB Atlas a Azure DocumentDB (vCore) en cluster `bustrack-db` (`bustrack-db.global.mongocluster.cosmos.azure.com`), apuntando a la base de datos `BusTrackDB`.
  - **Estado de migración de datos:** Migración completada exitosamente al 100% mediante Azure Database Migration Service (DMS): 34 colecciones migradas, 0 errores reportados.
  - **Protocolo y seguridad:** Compatible con protocolo MongoDB vía `MongoDB.Driver 3.5.1`. Autenticación SCRAM-SHA-256 sobre canal seguro TLS 1.2+ con soporte automático para flags de compatibilidad (`retrywrites=false`).
  - **Componentes y archivos adaptados:**
    - `API_SDD/Data/MongoDbContext.cs`: Soporte dual para lectura de cadena de conexión (`MongoDb:ConnectionString` y `ConnectionStrings:MongoDb`) y fallback por defecto a la base de datos `BusTrackDB`.
    - `API_SDD/appsettings.Production.json`: Actualizado `"Database": "BusTrackDB"`.
    - `.github/workflows/cd-azure.yml`: Actualizado `MongoDb__Database=BusTrackDB` en pasos de despliegue a Container Apps y ACI.
    - `infra/terraform/main.tf` y `infra/terraform/variables.tf`: Actualizadas definiciones de base de datos a `BusTrackDB`.
    - `specs/12-migracion-de-persistencia-a-cosmos-db/`: Actualizados `spec.md`, `plan.md`, `task.md` y `chart.md` documentando la migración completada a Azure DocumentDB vCore.
  - **Gestión segura de credenciales (Zero Secrets in Git):** Ninguna credencial ni cadena con credenciales reales fue versionada en Git, `appsettings.json` ni en `Memoria.md`. Se documentan los canales de inyección recomendados: .NET User Secrets en desarrollo local, GitHub Secrets en CI/CD y Azure Key Vault / App Settings en producción.
  - **Verificación y calidad:** Compilación limpia con 0 errores y 0 advertencias (`dotnet build API_SDD/API_SDD.csproj -c Release`), verificación de SDD en verde (`check_sdd.ps1` -> `SDD-OK`) y guardrails de seguridad superados (`critic-guardrails.ps1` -> `PASS`).
  - **1. Rate Limiting particionado por cliente/IP:** Reemplazo de fixed window limiters estáticos de bucket único global por `RateLimitPartition` por dirección remota cliente (`ClientPartitionKey`), evitando que llamadas concurrentes compartan un único cupo y mitigando DoS por agotamiento de tasa.
  - **2. Cobertura de Rate Limiting en catálogos anónimos:** Decorados los endpoints públicos de `EmpresasController` (`/empresas`, `/autocompletar`, `/comparar`, `/{id}`, `/{id}/rutas`) con política `[EnableRateLimiting("Global")]`.
  - **3. Validación estricta de Jwt:Key:** Validación mediante `ConfigurationSanitizer.HasUsableValue(key)` impidiendo que valores placeholder como `REEMPLAZAR_ENV_JWT_KEY_MIN_32_BYTES` sean aceptados en el arranque aun cumpliendo longitud mínima de 32 bytes.
  - **4. Protección contra fuerza bruta en LoginV2 (SQL):** Integrado bloqueo progresivo (`SDDSegBloqueo`) con clave hash determinista en `AuthService.LoginV2Async`, homologando la protección de lockout presente en login legacy y Mongo.
  - **5. Detección de reuso de Refresh Token (OAuth 2.0 Threat Model):** Si se presenta un token ya revocado en `AuthService.RefreshAsync` o `MongoAuthService.RefreshAsync`, se invalida automáticamente la familia completa de tokens del usuario (`RevokeAllActiveRefreshTokensAsync` / `RevokeAllRefreshTokensForUserAsync`).
  - **6. Prevención de Fail-Open en producción:** En entorno `Production`, se lanza excepción explícita si la base de datos o el caché distribuido configurados son inaccesibles o usan placeholders, evitando caer silenciosamente en almacenes volátiles en memoria sin persistencia compartida.
  - **7. Sincronización de expiración de tokens:** Homologado `expiraEnSegundos` en respuestas de autenticación y refresh con `Jwt:AccessTokenMinutes` configurable (`JwtOptions.ResolveAccessTokenMinutes`).
  - **8. Determinismo temporal:** Estandarizado el uso de `TimeProvider` (`_time.GetUtcNow().UtcDateTime`) en lugar de `DateTime.UtcNow` en rutas de autenticación.
  - **9. Forwarded Headers configurados:** Limpieza de `KnownIPNetworks` y `KnownProxies` en `UseForwardedHeaders` para soportar apropiadamente proxies inversos/ingress en Azure Container Apps.
  - **10. Verificación de correo versátil y segura:** Soporte de `POST /api/v1/auth/verificar-correo` con body `{ "token": "..." }` manteniendo compatibilidad con `GET` para enlaces de email, evitando exponer tokens en query strings en clientes API.
  - **11. Normalización MIME en Storage:** Detección de MIME canónico a partir de firmas binarias reales (PNG, JPEG, WebP, MP4, WebM) antes de persistir metadatos en Azure Blob Storage.
  - **12. Índices TTL en Mongo:** Añadidos índices de expiración automática (`ExpireAfter = TimeSpan.Zero` en `fechaExpiracion`) para colecciones de tokens (`refresh_tokens`, `tokens_recuperacion`, `tokens_verificacion_correo`).
  - **13. Manejo resiliente de desconexión:** Captura segura de `OperationCanceledException` cuando `RequestAborted.IsCancellationRequested` sin intentar escribir en socket cerrado.
  - **14. Validaciones:** Compilación de toda la solución `Release` con 0 errores y 0 advertencias, pruebas SDD (`SDD-OK`) y guardrails critic (`PASS`).
- **2026-11-26 (Auditoría Técnica, Validación de Integración Cosmos DB for MongoDB y Preparación Flutter):**
  - **Objetivo y alcance:** Auditoría técnica integral de la conexión con Azure Cosmos DB for MongoDB (vCore), verificación de consultas sobre las 8 colecciones principales de transporte (23,850 documentos) y colecciones auxiliares, resolución de preflight CORS (`OPTIONS`) para Flutter Web y corrección de sondas de disponibilidad.
  - **Componentes y archivos adaptados:**
    - `API_SDD/Data/MongoDbContext.cs`: Fallback por defecto de base de datos homologado a `BusTrackDB`.
    - `API_SDD/appsettings.Production.json`: Configurado `"Database": "BusTrackDB"`.
    - `API_SDD/Health/RedisHealthCheck.cs`: Resolución diferida de `IDistributedCache` con tolerancia a degradación; evita 500 no controlado cuando el caché distribuido no está inicializado, reportando `503 Degraded` ordenadamente.
    - `UnitTest/Middleware/CompressionAndJsonTests.cs`: Corrección de aserción en `ProductionConPersistenceMongoSinCadenaRetorna503SinTrazas` tras la estabilización de `RedisHealthCheck`.
    - `API_SDD/Program.cs`: Orden estricto del pipeline HTTP (`UseRouting` -> `UseCors` -> `UseAuthentication` -> `UseAuthorization`), permitiendo preflights `OPTIONS` y cabecera `X-API-Version` requerida por el cliente Flutter Web.
    - `.github/workflows/cd-azure.yml`: Integración de la suite de pruebas DevSecOps (`DevSecOpsSecurityTests`) en el flujo de integración continua.
  - **Resultados de validación:**
    - Unit Tests: 145/145 aprobados (0 fallos).
    - Integration Tests (CoreEndpoints): 25/25 aprobados (0 fallos).
    - Security Tests (DevSecOps): 7/7 aprobados (0 fallos).
    - SDD Guardrails: `check_sdd.ps1` -> `SDD-OK`, `critic-guardrails.ps1` -> `PASS`.
