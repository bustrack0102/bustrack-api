# Pipeline detallado — BusTrack API

## Workflows

- `../.github/workflows/ci-pr.yml`: Integración continua en Pull Requests y ramas `main`.
- `../.github/workflows/cd-azure.yml`: Despliegue continuo automático a Azure Container Instances en push a `main` o ejecución manual con quality gates y smoke testing.

## Etapas y comandos de CI

```text
dotnet restore API_SDD.slnx
dotnet build API_SDD.slnx -c Release --no-restore
dotnet test UnitTest/UnitTest.csproj -c Release --no-build --collect:"XPlat Code Coverage"
```

## Etapas de CD (Despliegue a Azure)

1. **Quality Gate (`verify`):** Ejecución previa de pruebas unitarias y análisis de seguridad estático (Semgrep). Bloquea el despliegue si alguna prueba falla.
2. **Build Release (`deploy-azure`):** Verificación de compilación de producción con `dotnet publish`.
3. **Autenticación Cloud:** Azure Login mediante Service Principal (`secrets.AZURE_CREDENTIALS`).
4. **Reinicio de Contenedor:** Actualización de la instancia de contenedor `bustrack-api` en el Resource Group `rg-bustrack-ncus`.
5. **Smoke Testing & Health Probe:** Sondeo activo automatizado contra `http://bustrack-api-uttt.northcentralus.azurecontainer.io:8000/health` y `/api/v1/ping` para certificar la disponibilidad en vivo antes de dar por finalizado el workflow.
6. **Resumen de Despliegue:** Publicación de tabla de evidencias en el `$GITHUB_STEP_SUMMARY`.

## Análisis estático y seguridad

- Reglas locales en `../.semgrep/rules/semgrep.yaml`.
- Detección de cadenas duras / secretos, prevención de inyecciones SQL y uso de algoritmos criptográficos obsoletos.
- Un fallo de Semgrep bloquea la fusión y el despliegue a producción.

## Dependencias y secretos

- Las credenciales de Azure (`AZURE_CREDENTIALS`) se configuran únicamente en GitHub Secrets del repositorio, nunca en el código fuente.
- Los PRs no acceden a secretos de producción.

## Artefactos

- Reporte de cobertura de código (`coverage.cobertura.xml`) almacenado automáticamente por 14 días en cada corrida de CI.
