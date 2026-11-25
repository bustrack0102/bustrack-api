# 12-migracion-de-persistencia-a-cosmos-db — Migración de persistencia a Azure DocumentDB (vCore) / BusTrackDB_Import

> Repositorio: BusTrack API · Estado: Completado · Fecha: 2026-11-25.

## 1. Objetivo

Configurar y verificar la conexión e integración completa de la API hacia Azure Cosmos DB for MongoDB vCore (Azure DocumentDB) utilizando la base de datos central de integración `BusTrackDB_Import` (que alberga 23,850 documentos de transporte y concentrará todas las colecciones de usuarios, autenticación, tokens y administración), conservando intacta la base original `BusTrackDB`.

## 2. Contexto

La migración de datos desde MongoDB Atlas hacia Azure DocumentDB fue completada al 100% por Azure DMS:
- **Estado:** Completed (0 errores reportados).
- **Colecciones migradas:** 34 colecciones.
- **Clúster destino:** `bustrack-db` (`bustrack-db.global.mongocluster.cosmos.azure.com`).
- **Base de datos:** `BusTrackDB`.
- **Protocolo:** MongoDB compatible (MongoDB.Driver 3.5.1 / SCRAM-SHA-256 / TLS habilitado).

## 3. Alcance

**Incluye:**
- Actualización de `MongoDbContext` en `API_SDD/Data/MongoDbContext.cs` para resolver cadenas de conexión de forma dual (`MongoDb:ConnectionString` o `ConnectionStrings:MongoDb`) y fallback por defecto a la base de datos `BusTrackDB`.
- Soporte transparente para autenticación SCRAM-SHA-256, TLS y parámetro de compatibilidad `retrywrites=false` a través de `MongoClientSettings.FromConnectionString`.
- Configuración de entornos (`appsettings.Production.json`, CI/CD `.github/workflows/cd-azure.yml`, Terraform `infra/terraform/`).
- Documentación de mecanismos seguros para inyección de credenciales (.NET User Secrets, variables de entorno `ConnectionStrings__MongoDb` / `MongoDb__ConnectionString`, Azure App Settings / GitHub Secrets).
- Pruebas de salud (`MongoDbHealthCheck`) mediante comando `ping` BSON contra Azure DocumentDB.

**No incluye:**
- Modificación de arquitectura modular monolítica, controladores, servicios, repositorios o contratos REST.
- Reemplazo de `MongoDB.Driver` por otro SDK.
- Migraciones adicionales de datos o alteración de colecciones.
- Almacenamiento de credenciales reales en código, Git o archivos versionados.

## 4. Requisitos funcionales

1. La API debe conectarse a Azure DocumentDB vCore utilizando el cluster `bustrack-db` y la base de datos `BusTrackDB`.
2. Las 34 colecciones migradas deben ser accesibles bajo sus nombres originales sin alterar repositorios ni DTOs.
3. El endpoint de salud `/health` y `MongoDbHealthCheck` deben verificar conectividad ejecutando un comando de `ping`.
4. Soporte prioritario de variables de entorno estándar de producción sin fallar si no se inyecta en local/test.

## 5. Requisitos no funcionales

1. Autenticación segura mediante SCRAM-SHA-256 y canal cifrado TLS 1.2+.
2. Sanitización estricta de cadenas de conexión para rechazar placeholders (`REEMPLAZAR`, `EXTERNA`).
3. Cero secretos o credenciales expuestas en Git o archivos versionados.
4. Trazabilidad completa en SDD y `Memoria.md`.

## 6. Reglas de negocio

- La API es la única fuente autorizada de información de transporte y persistencia.
- Ningún cliente frontend (Flutter/Angular) accede directamente a Azure DocumentDB; todo pasa por la API.
- Azure DocumentDB (vCore) `BusTrackDB` funge como repositorio de persistencia NoSQL de la solución.

## 7. Criterios de aceptación

- [x] Conexión a `bustrack-db.global.mongocluster.cosmos.azure.com` / `BusTrackDB` configurada en `MongoDbContext`.
- [x] Fallback por defecto a base de datos `BusTrackDB` en lugar del antiguo placeholder.
- [x] Soporte dual de configuración: `MongoDb:ConnectionString` y `ConnectionStrings:MongoDb`.
- [x] Sin credenciales reales en Git, archivos JSON versionados ni `Memoria.md`.
- [x] Compilación limpia (`dotnet build -c Release`) con 0 errores y 0 warnings.
- [x] Reglas de guardrails (`critic-guardrails.ps1`) y SDD (`check_sdd.ps1`) en verde.

## 8. Dependencias

- `constitution.md`
- `docs/contratos-integracion.md`
- `API_SDD/Data/MongoDbContext.cs`
- `API_SDD/Features/Health/MongoDbHealthCheck.cs`

## 9. Trazabilidad

- Plan: `plan.md` · Tareas: `task.md` · Diagramas: `chart.md`.
- Estado real: `../../Memoria.md`.
- Contratos: `../../docs/contratos-integracion.md`.

## Aprobación

- [x] Revisión de arquitectura y contrato completada — fecha: 2026-11-23
- [x] CHECKLIST_PR.md validado
- [x] Sin conflicto con constitution.md
