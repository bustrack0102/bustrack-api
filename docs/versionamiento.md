# Matriz de Versionamiento SemVer — BusTrack (Requisito 11 y 15)

> Definida previa al desarrollo de nuevas funcionalidades y aplicada en cada despliegue CI/CD, tags Git, contenedores ACR (`bustrackacr0102`) y cabecera/ruta HTTP.

## 1. Formato Estándar

`MAJOR.MINOR.PATCH` (+ `+{BUILD}` en aplicación móvil `.aab` / Google Play).

- **MAJOR (`X.0.0`)**: Cambios incompatibles en contratos públicos `/api/v{X}/` o liberación Release `1.0.0` de evaluación final.
- **MINOR (`0.X.0`)**: Incorporación de módulos/endpoints compatibles hacia atrás (Etapas 1–4).
- **PATCH (`0.0.X`)**: Correcciones internas, parches de seguridad, endurecimiento DevSecOps o optimizaciones sin cambio de contrato.

## 2. Matriz por Hito del Ecosistema

| Etapa / Hito | Versión SemVer | Tag Git | Imagen Docker (`bustrackacr0102`) | Flutter `pubspec.yaml` | Contrato API | Alcance |
|---|---|---|---|---|---|---|
| Línea Base | `0.1.0` | `v0.1.0` | `0.1.0-{sha}` | `0.1.0+1` | `/api/v1` (`X-API-Version: 1.0`) | SDD base, guardrails, Health/Auth inicial. |
| **Etapa 0** | `0.2.0` | `v0.2.0` | `0.2.0-{sha}` | `0.1.1+2` | `/api/v1` (`X-API-Version: 1.0`) | `critic-guardrails PASS`, MongoDB saludable, Brotli/Gzip, Source-Gen JSON, imágenes ACR reales. |
| **Etapa 1** | `0.4.0` | `v0.4.0` | `0.4.0-{sha}` | `0.2.0+3` | `/api/v1` (`X-API-Version: 1.0`) | Persistencia unificada MongoDB Atlas NoSQL + Redis distribuido (`blacklist:{jti}` + caché) + Core MVP (`EP1–EP5`, `EP-AD-01`). |
| **Etapa 2** | `0.6.0` | `v0.6.0` | `0.6.0-{sha}` | `0.4.0+4` | `/api/v1` (`X-API-Version: 1.0`) | Calificaciones (`EP6`), Viajes (`EP7`), Admin CRUD (`EP10`), Azure Content Safety F0, Blob CDN y Push FCM. |
| **Etapa 3** | `0.8.0` | `v0.8.0` | `0.8.0-{sha}` | `0.6.0+5` | `/api/v1` (`X-API-Version: 1.0`) | Integración ML (`.joblib`, `EP8`) + GenAI Grounded (`EP9`) end-to-end. |
| **Etapa 4** | `0.9.0` | `v0.9.0` | `0.9.0-{sha}` | `0.8.0+6` | `/api/v1` (`X-API-Version: 1.0`) | Terraform IaC crear/destruir en `rg-bustrack-ncus`, ≥2 réplicas ACA HTTPS + Envoy LB, App Insights, SWA Landing + GA4. |
| **Etapa 5** | `1.0.0` | `v1.0.0` | `1.0.0-{sha}` | `1.0.0+7` | `/api/v1` (`X-API-Version: 1.0`) | App móvil Flutter completa (`MO-01..34`), SQLite offline/sync, `.aab` Play Store, DAST/k6, entrega `.rar` + script BD. |

## 3. Reglas de Aplicación en Despliegues

1. Cada `push` a `main` etiqueta la imagen Docker con `:<semver>-<short_sha>` y `:latest` manteniendo trazabilidad inmutable.
2. Ningún despliegue puede decrementar `MAJOR.MINOR.PATCH` ni `versionCode`.
3. Toda respuesta exitosa de la API incluye cabecera `api-supported-versions: 1.0` vía `Asp.Versioning`.
