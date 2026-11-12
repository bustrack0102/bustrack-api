# Auditoría Integral de Cumplimiento — Rúbrica Docente BusTrack (Etapas 0–4)

> Última actualización: 2026-11-12 · Evaluado hasta Etapa 4 (`v0.9.0`).  
> Nota de alcance: La app móvil Flutter (`bustrack-mobile`, Etapa 5) fue dejada en pausa explícita por instrucción del equipo/usuario para completarse en la siguiente fase.

## 1. Trazabilidad de Requisitos del Docente (1–20)

| Req. | Exigencia | Estado Actual (Etapas 0–4) | Evidencia Verificada en Código / Nube / Tests |
|---|---|---|---|
| **1** | Descarga desde Google Play Store | Parcial (Landing lista; App en E5) | CTA configurable `VITE_PLAY_STORE_URL` en `bustrack-landing` (`LandingPage.tsx`); `.aab` móvil en E5. |
| **2** | Cumplimiento completo y verificable | Completo (alcance E0–E4) | Este documento + `docs/endpoints.md` + `docs/versionamiento.md` + suites en los 4 repositorios. |
| **3** | Nube, BD, Backend, Móvil y Landing | 4/5 activos + móvil en E5 | `bustrack-api`, `bustrack-ml`, `bustrack-genai`, `bustrack-landing` + MongoDB Atlas `BusTrackNoSQL` + `rg-bustrack-ncus`. |
| **4** | Móvil fiel a Figma sin eliminar funciones | Pendiente (Etapa 5 solicitada aparte) | Reservado para la siguiente fase sobre `bustrack-mobile`. |
| **5** | SDD y DevSecOps | Completo (4 repos activos) | `scripts/check_sdd.ps1` (`SDD-OK`), `critic-guardrails.ps1` (`PASS`), `Semgrep`, `0` warnings-as-errors, `black`/`flake8`, `tsc`/`vitest`. |
| **6** | Desarrollado con Agentes IA / SDD | Completo | `AGENTS.md`, `constitution.md`, `specs/`, `specs-endpoints/`, `Memoria.md` respetados en cada commit. |
| **7** | Pruebas Unitarias, Seguridad e Integración | Completo en API, ML, GenAI y Landing | API: `UnitTest` 133, `IntegrationTest` 66, `SecurityTest` 12; ML: 17; GenAI: 12; Landing: 23. |
| **8** | CI/CD automatizado | Completo | `ci-pr.yml` + `cd-azure.yml` + `infra-lifecycle.yml` en `bustrack0102`. |
| **9** | Caché BD, Backend y Frontend | Completo (BD/API/Web); Móvil en E5 | MongoDB índices, Backend `IDistributedCache`/Redis + `ResponseCache`, Landing SWR cache (`publicStatusCache.ts`) + Nginx CDN `Cache-Control`. |
| **10** | Logout Backend y Móvil | Completo en Backend (Móvil en E5) | `POST /api/v1/auth/logout` con blacklist distribuida `blacklist:{jti}` en `TokenRevocationService` + revocación refresh tokens. |
| **11** | Matriz de versionamiento previa al desarrollo | Completo | `docs/versionamiento.md` definida previo a E1 + tags Git & Docker `0.2.0` -> `0.4.0` -> `0.6.0` -> `0.8.0` -> `0.9.0`. |
| **12** | Uso de MCP / tooling | Completo sin saltar controles | Uso de GitHub CLI (`gh`), Azure CLI (`az`), Docker/ACR y flujos SDD. |
| **13** | Nube, LB, Terraform IaC crear/eliminar, HTTPS | Completo | `infra/terraform/` + `.github/workflows/infra-lifecycle.yml` (`plan`/`apply`/`destroy`), Container Apps Envoy HTTPS LB. |
| **14** | Base(s) de datos NoSQL | Completo | MongoDB Atlas `BusTrackNoSQL` (`2dsphere`, `unique`, compuestos) + Redis `IDistributedCache` + `db/mongodb/init-bustrack-nosql.js`. |
| **15** | Backend ≥2 instancias, costo cero, CDN media, observabilidad, JWT+Redis, perf, patrones | Completo | Container App `bustrack-api` `min_replicas = 2`, Blob Storage HTTPS CDN, Log Analytics + App Insights, Brotli/Gzip, Source-Gen JSON, Polly, Repository/Strategy. |
| **16** | Moderación Cloud texto/imagen/video, Push, Privacidad, Baja cuenta, ML + GenAI | Completo en Backend (Móvil UI en E5) | `ContentModerationService` (Azure AI Content Safety `F0` + JPEG/PNG/WebP/MP4/WebM), `BlobMediaService`, `PushNotificationService` FCM v1, `DELETE /usuarios/me`, `EP-ML`, `EP-IA`. |
| **17** | Landing: hosting, tienda, privacidad, manejo datos, Google Analytics | Completo | `bustrack-landing` en Azure HTTPS, `/privacidad`, `/manejo-datos`, GA4 `gtag.ts` por consentimiento (`VITE_GA_MEASUREMENT_ID`), Play Store CTA configurable. |
| **18** | Cuenta única `bustrack0102` y correo único | Completo | Todos los repos, ACR y suscripción Azure bajo `bustrack0102` (`bustrack0102@gmail.com`). |
| **19** | ML y GenAI con mismas prácticas SDD/DevSecOps | Completo | `bustrack-ml` (RandomForest `.joblib`, `F1=1.0`, `ROC-AUC=1.0`) y `bustrack-genai` (grounding estricto) con ingreso interno HTTPS y `X-Internal-Key`. |
| **20** | Fecha límite 2026-12-02 + script BD | En curso / Entregable BD listo | `db/mongodb/init-bustrack-nosql.js` validado (`node --check`) e idempotente; entrega final `.rar` tras E5. |

## 2. Cobertura de los 86 Endpoints del Excel (`EP-AU` a `EP-AD`)

| Módulo | Endpoints del Excel | Rutas Implementadas en `API_SDD/Controllers/V1/` | Estado |
|---|---|---|---|
| `EP1` Auth & Usuarios | `EP-AU-01..10` (10) | `AuthController` (`registro`, `login`, `refresh`, `logout`, `recuperar-password`, `restablecer-password`), `UsuariosController` (`me` GET/PUT/DELETE, `usuarios`) | **10/10** |
| `EP2` Empresas & Medios | `EP-EM-01..12` (12) | `EmpresasController` (`GET/POST/PUT/DELETE`, `autocompletar`, `{id}`, `{id}/rutas`, `comparar`), `MediosTransporteController` (`GET/POST/PUT/DELETE`) | **12/12** |
| `EP3` Rutas, Paradas & Mapa | `EP-RP-01..16` (16) | `ParadasController` (`autocompletar`, `cercanas`, `{id}`, CRUD), `RutasController` (`buscar`, `autocompletar`, `{id}`, `{id}/paradas`, `{id}/geometria`, CRUD), `MapaController` (`/mapa/elementos`) | **16/16** |
| `EP4` Horarios & Tarifas | `EP-HT-01..10` (10) | `HorariosController` (`GET`, `siguiente`, CRUD), `TarifasController` (`GET`, `calcular`, CRUD) | **10/10** |
| `EP5` Destinos Turísticos | `EP-DE-01..08` (8) | `DestinosController` (`autocompletar`, `GET`, `categorias`, `{id}`, `{id}/rutas`, CRUD) | **8/8** |
| `EP6` Calificaciones | `EP-CA-01..06` (6) | `CalificacionesController` (`GET`, `resumen`, `mias`, `POST`, `PUT`, `DELETE` con moderación previa y Push FCM) | **6/6** |
| `EP7` Viajes Guardados | `EP-VG-01..05` (5) | `ViajesController` (`GET`, `{id}`, `POST`, `PUT`, `DELETE` con protección IDOR y moderación previa) | **5/5** |
| `EP8` Sensores & ML | `EP-ML-01..07` (7) | `ConsentimientosController` (`me`, `POST`, `DELETE`), `SensoresController` (`lotes` POST/GET), `CandidatosParadaController` (`GET`), `bustrack-ml` (`/inference`) | **7/7** |
| `EP9` Recomendaciones IA | `EP-IA-01..03` (3) | `InferenciaController` (`POST /recomendaciones`, `GET /recomendaciones/historial`, `POST /recomendaciones/{id}/retroalimentacion`) + `bustrack-genai` | **3/3** |
| `EP10` Admin & Estado | `EP-AD-01..09` (9) | `CatalogosController` (`/catalogos/estado-actualizacion`, `/api/v1/health`), `AdminController` (`/admin/fuentes-datos` CRUD, `/admin/importaciones` POST/GET), `CandidatosParadaController` (`PATCH /admin/candidatos-parada/{id}/validar`) | **9/9** |
| **Extras Plataforma** | Media, Moderación & Push | `MediaController` (`/media/upload`), `ModeracionController` (`/moderacion`, `/moderacion/archivo`), `DispositivosController` (`/dispositivos/mios`, `POST`, `DELETE`) | **+5 activos** |
| **TOTAL EXCEL** | **86 endpoints** | **Todos implementados, documentados en `docs/endpoints.md` y respaldados por pruebas** | **86/86 (100%)** |
