# Plan técnico — 12-migracion-de-persistencia-a-cosmos-db — Conexión a Azure DocumentDB (BusTrackDB)

> Spec: `spec.md` · Repositorio: BusTrack API · Estado: Completado · Fecha: 2026-11-23.

## 1. Estrategia

Sustituir la configuración de conexión hacia MongoDB Atlas por Azure DocumentDB (vCore) preservando íntegramente `MongoDB.Driver 3.5.1` y la totalidad de repositorios, entidades y endpoints. La base de datos objetivo es `BusTrackDB`, migrada al 100% con 34 colecciones mediante Azure Database Migration Service (DMS).

## 2. Arquitectura y componentes

- **Cluster destino:** `bustrack-db.global.mongocluster.cosmos.azure.com` (Azure Cosmos DB for MongoDB vCore / Azure DocumentDB).
- **Base de datos:** `BusTrackDB` (34 colecciones migradas, 0 errores).
- **Autenticación y Cifrado:** SCRAM-SHA-256, TLS 1.2+, `retrywrites=false`.
- **Driver:** `MongoDB.Driver 3.5.1` con `MongoClientSettings.FromConnectionString`.
- **Contexto:** `MongoDbContext` como servicio Singleton con pool de conexiones compartido.
- **Verificación:** `MongoDbHealthCheck` ejecutando comando BSON `{ ping: 1 }`.

## 3. Archivos y estructura

```text
API_SDD/Data/MongoDbContext.cs                  # Soporte dual de configuración y default BusTrackDB
API_SDD/appsettings.Production.json             # Actualización de Database: BusTrackDB
.github/workflows/cd-azure.yml                  # Variables MongoDb__Database=BusTrackDB
infra/terraform/main.tf                         # Parámetros MongoDb__Database=BusTrackDB
infra/terraform/variables.tf                    # Descripción actualizada a Azure DocumentDB
specs/12-migracion-de-persistencia-a-cosmos-db/ # Especificación, plan, tareas y diagramas SDD
Memoria.md                                      # Bitácora viva de arquitectura y migración
```

## 4. Seguridad y Gestión de Secretos

- **Zero Secrets in Code:** Ninguna credencial ni usuario/password de Azure DocumentDB se versiona en Git.
- **Mecanismos seguros de inyección:**
  1. **Desarrollo local:** .NET User Secrets (`dotnet user-secrets set "MongoDb:ConnectionString" "..."`) o archivo git-ignored `appsettings.Local.json`.
  2. **CI/CD:** GitHub Secrets (`${{ secrets.MONGODB_CONNECTION_STRING }}`).
  3. **Producción Azure:** Azure Container Apps / App Service Application Settings (`MongoDb__ConnectionString` mapeado a Azure Key Vault secretref).
- **Sanitización:** `ConfigurationSanitizer.HasUsableValue` previene arranque con valores vacíos o placeholders (`REEMPLAZAR`, `EXTERNA`).

## 5. Riesgos y mitigaciones

| Riesgo | Mitigación |
|---|---|
| Incompatibilidad de operaciones NoSQL | Driver `MongoDB.Driver 3.5.1` compatible con MongoDB vCore; migración validada con 0 errores en 34 colecciones. |
| Timeouts en conexión TLS | Ajuste de timeouts en `MongoClientSettings` (`ServerSelectionTimeout = 5s`, `ConnectTimeout = 5s`). |
| Exposición accidental de credenciales | Validación de critic-guardrails (`scripts/critic-guardrails.ps1` regla B1) y exclusión en `.gitignore`. |

## 6. Estrategia de pruebas

- **Compilación Release:** Validación de 0 errores y 0 warnings en `API_SDD.csproj`.
- **Health Check:** `MongoDbHealthCheck` BSON ping para validar conectividad y credenciales.
- **Guardrails SDD:** Verificación de aprobación SDD y consistencia en `endpoints.md`.
