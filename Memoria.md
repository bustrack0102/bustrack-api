# Memoria técnica — BusTrack API

> Registro vivo. No se borra historial. Última actualización: 2026-10-30.

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
