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
