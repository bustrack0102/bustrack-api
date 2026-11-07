# CI/CD — BusTrack API

> Documentación de integración y despliegue continuo. Los workflows ejecutables viven en `../.github/workflows/`.

## 1. Arquitectura del pipeline

`dotnet restore → dotnet build → dotnet test → Semgrep → Azure Login → Azure Container Instance Restart → Smoke Testing`

- **Disparadores CI:** Pull Request y push a `main`.
- **Disparadores CD:** Push a `main` y ejecución manual vía `workflow_dispatch`.
- **Permisos:** `contents: read`.
- **Artefactos:** Cobertura de código Cobertura XML en `./TestResults`.
- **Destino Cloud:** Azure Container Instances (`rg-bustrack-ncus` / `bustrack-api`).

## 2. Etapas

| Etapa | Herramienta | Bloqueante |
|---|---|---|
| Construcción | `dotnet build` (.NET 10) | Sí |
| Pruebas Unitarias | xUnit, FluentAssertions, Moq | Sí (94 pruebas) |
| Cobertura | Coverlet (`XPlat Code Coverage`) | Sí |
| Análisis Estático | Semgrep (`.semgrep/rules/semgrep.yaml`) | Sí |
| Despliegue Cloud | Azure CLI (`az container restart`) | Sí |
| Smoke Testing | Curl Health & Ping Probe | Sí |

## 3. Condiciones de aprobación y despliegue

- [x] Pruebas unitarias en verde (94/94 pruebas superadas).
- [x] Cobertura reportada y subida como artefacto descargable.
- [x] Semgrep sin hallazgos de seguridad.
- [x] Build reproducible en modo Release.
- [x] Despliegue verificado con respuesta HTTP 200 en `/health` y `/api/v1/ping`.
