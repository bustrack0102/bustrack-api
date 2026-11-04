# BusTrack API — BusTrack

> Sistema de consulta y planificación de transporte público — Universidad Tecnológica de Tula-Tepeji.
> SDD + DevSecOps. Este README describe el **estado real** y la **arquitectura objetivo**.

## Estado actual

- **Tecnología real:** ASP.NET Core .NET 10, EF Core + SQL Server, JWT HS256, Argon2id, FluentValidation, Serilog, Scalar/OpenAPI, xUnit.
- **Tecnología objetivo:** ASP.NET Core .NET 10 + Azure Cosmos DB for NoSQL, conservando JWT HS256, Argon2id y FluentValidation.
- **Implementado:** ver lista en `Memoria.md`.
- **No implementado:** ver pendientes en `Memoria.md`.
- **Recursos Azure:** no desplegados; propuesta en `azure/`.

## Qué hace este repositorio

Expone REST a Flutter y Angular; consume ML/GenAI por HTTP interno y es la fuente autorizada de datos.

## Estructura

```text
API_SDD/{Common,Controllers/V1,Data,Dtos,Health,Hosting,Logging,Middleware,OpenApi,Security,Services,Validators}; pruebas en UnitTest, IntegrationTest, SecurityTest, ContractTest y DatabaseTest
```

## Gobernanza SDD

| Documento | Propósito |
|---|---|
| `constitution.md` | Principios inalterables del repositorio. |
| `AGENTS.md` | Instrucciones para agentes de IA. |
| `Memoria.md` | Estado real, decisiones y deuda técnica. |
| `CHECKLIST_PR.md` | Controles obligatorios de PR. |
| `specs/` | Specs canónicas (`spec.md`, `plan.md`, `task.md`, `chart.md`). |
| `docs/contratos-integracion.md` | Contratos con los demás repositorios. |
| `.opencode/` | Agentes y skills específicos. |
| `.semgrep/` | Reglas SAST reales. |
| `.github/workflows/` | CI por stack. |

## Specs por tarea

- `specs/01-configuracion-inicial-del-proyecto/` — Configuración inicial del proyecto (Implementado).
- `specs/02-arquitectura-base-y-pipeline-http/` — Arquitectura base y pipeline HTTP (Implementado).
- `specs/03-configuracion-de-seguridad/` — Configuración de seguridad (Parcial).
- `specs/04-gestion-de-usuarios/` — Gestión de usuarios (Implementado).
- `specs/05-autenticacion-jwt/` — Autenticación con JWT (Implementado).
- `specs/06-gestion-de-sesiones-y-logout/` — Gestión de sesiones y logout (Implementado).
- `specs/07-modelo-de-dominio-de-transporte/` — Modelo de dominio de transporte (Pendiente).
- `specs/08-consulta-de-empresas-y-asociaciones/` — Consulta de empresas y asociaciones (Pendiente).
- `specs/09-consulta-de-rutas-y-paradas/` — Consulta de rutas y paradas (Pendiente).
- `specs/10-consulta-de-horarios-y-tarifas/` — Consulta de horarios y tarifas (Pendiente).
- `specs/11-gestion-de-calificaciones/` — Gestión de calificaciones (Pendiente).
- `specs/12-migracion-de-persistencia-a-cosmos-db/` — Migración de persistencia a Cosmos DB for NoSQL (Pendiente).

## Specs de endpoints

- `specs-endpoints/13-endpoint-login/` — `POST /api/v1/auth/login` (Implementado).
- `specs-endpoints/14-endpoint-logout/` — `POST /api/v1/auth/logout` (Implementado).
- `specs-endpoints/15-endpoint-health/` — `GET /health y /health/ready` (Implementado).
- `specs-endpoints/16-endpoint-usuarios-coleccion/` — `GET/POST /api/v1/usuarios` (Implementado).
- `specs-endpoints/17-endpoint-usuario-individual/` — `GET/PUT/DELETE /api/v1/usuarios/{id}` (Implementado).
- `specs-endpoints/18-endpoint-empresas/` — `GET /api/v1/empresas` (Propuesto).
- `specs-endpoints/19-endpoint-rutas/` — `GET /api/v1/rutas` (Propuesto).
- `specs-endpoints/20-endpoint-paradas/` — `GET /api/v1/rutas/{id}/paradas` (Propuesto).
- `specs-endpoints/21-endpoint-horarios/` — `GET /api/v1/rutas/{id}/horarios` (Propuesto).
- `specs-endpoints/22-endpoint-tarifas/` — `GET /api/v1/rutas/{id}/tarifas` (Propuesto).
- `specs-endpoints/23-endpoint-calificaciones/` — `GET/POST /api/v1/calificaciones` (Propuesto).

## CI/CD y Azure

- `cicd/` — arquitectura del pipeline y gates.
- `azure/` — arquitectura final de despliegue (propuesta).

El historial de la organización anterior está en `docs/archivo-sdd/specs-anteriores/`.

## Comandos

```text
dotnet restore API_SDD.slnx
dotnet build API_SDD.slnx -c Release
dotnet test UnitTest/UnitTest.csproj -c Release
```

## Limitaciones y advertencias

- No se declara funcionalidad terminada sin código y pruebas.
- El árbol está anidado `X-main/X-main`; la raíz real es la carpeta interior.
- No hay repositorio Git local, por lo que tags/releases no son verificables.
- No hay despliegue Azure ni recursos provisionados.

## Equipo

Fátima Ximena García González, Diego Parra Cruz, José de Jesús López Islas, Melanie Joliee Bonilla Domínguez.
