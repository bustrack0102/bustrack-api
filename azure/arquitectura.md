# Detalle de arquitectura Azure — BusTrack API

> **Estado:** Implementado y Verificado en Producción (Azure Container Instances).

## 1. Red y exposición

- **API Pública:** Expuesta en Azure Container Instances (`bustrack-api`) bajo el FQDN `http://bustrack-api-uttt.northcentralus.azurecontainer.io:8000`.
- **Headers de seguridad activos:** HSTS, CSP, `X-Frame-Options: DENY`, `X-Content-Type-Options: nosniff`, `Referrer-Policy: strict-origin-when-cross-origin`.
- **CORS:** Restringido a orígenes aprobados definidos en configuración.

## 2. Persistencia y configuración

- **Persistencia:** Compatible con SQL Server / Azure SQL y Cosmos DB via Entity Framework Core.
- **Inyección de secretos:** Cadenas de conexión inyectadas mediante variables de entorno (`ConnectionStrings__DefaultConnection`) sin credenciales duras en el repositorio.

## 3. Observabilidad

- **Correlation ID:** Generado e inyectado en cada petición HTTP por `CorrelationIdMiddleware` (`X-Correlation-Id`).
- **Logs estructurados:** Serilog JSON con sanitización activa de credenciales (`SecretScrubbingEnricher`).
- **Health Checks:**
  - `GET /health`: Liveness probe activo (`200 OK`).
  - `GET /health/ready`: Readiness probe acotado con timeout de 5 segundos para verificar conectividad a base de datos.
  - `GET /api/v1/ping`: Endpoint funcional REST (`{"status":"Pong"}`).

## 4. CI/CD y Automatización

- **Quality Gate:** Pipeline `ci-pr.yml` ejecuta compilación Release, 94 pruebas unitarias xUnit, Semgrep y resguardo de reporte de cobertura de código.
- **Continuous Deployment:** Pipeline `cd-azure.yml` revalida pruebas y seguridad, realiza login en Azure y reinicia la instancia con *Smoke Testing* activo automatizado.
