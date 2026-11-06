# Endpoints — vivo (nv-01)

> Fuente contratos. Los specs solo enlazan, no duplican. Constitución §6.
> Filas CRUD en `nv-04`, login en `nv-05`, logout en `nv-06` (refresh/2FA en `nv-07`).

| Método | Ruta | Auth | Rate | Códigos | Spec |
|---|---|---|---|---|---|
| GET | `/health` | no | global | 200 | nv-01 |
| GET | `/health/ready` (check SQL externo) | no | global | 200/503 | nv-01 |
| GET | `/api/v1/ping` | no | global | 200 | nv-01 |
| GET | `/api/v1/probe/timeout` (no-prod) | no | global | 408/404 | nv-01 |
| GET | `/api/v1/probe/error` (no-prod) | no | global | 500/404 | nv-01 |
| GET | `/api/v1/probe/forbidden` (no-prod) | no | global | 403/404 | nv-01 |
| GET | `/api/v1/probe/conflict` (no-prod) | no | global | 409/404 | nv-01 |
| GET | `/api/v1/probe/invalid` (no-prod) | no | global | 422/404 | nv-01 |
| POST | `/api/v1/sddusuarios` | JWT AdminPolicy | Admin + ConcurrentWrites | 201/400/401/403/409/422/429 | nv-04 |
| PUT | `/api/v1/sddusuarios/{id}` | JWT AdminPolicy | Admin + ConcurrentWrites | 200/400/401/403/404/409/422/429 | nv-04 |
| GET | `/api/v1/sddusuarios?page=&pageSize=` | JWT AdminPolicy | Admin | 200/401/403/422/429 | nv-04 |
| GET | `/api/v1/sddusuarios/{id}` | JWT AdminPolicy | Admin | 200/401/403/404/429 | nv-04 |
| DELETE | `/api/v1/sddusuarios/{id}?rowVersion={base64}` | JWT AdminPolicy | Admin + ConcurrentWrites | 204/400/401/403/404/409/429 | nv-04 |
| POST | `/api/v1/auth/login` | no | Login + Global | 200/400/401/422/429 | nv-05 |
| POST | `/api/v1/auth/logout` | JWT | Global | 204/401/429 | nv-06 |
| GET | `/openapi/v1.json` (solo Dev) | no | — | 200/404 | nv-01 |
| GET | `/scalar/v1` (solo Dev) | no | — | 200/404 | nv-01 |

## Endpoints propuestos (dominio de transporte, seguridad avanzada e integraciones)

| Método | Ruta | Auth | Rate | Códigos | Spec |
|---|---|---|---|---|---|
| GET | `/api/v1/empresas` | no / opcional | Global | 200/400/429 | 18-endpoint-empresas |
| GET | `/api/v1/empresas/{id}` | no / opcional | Global | 200/400/404/429 | 27-endpoint-empresa-individual |
| POST | `/api/v1/empresas` | JWT AdminPolicy | Admin + ConcurrentWrites | 201/400/401/403/409/422/429 | 38-endpoint-admin-empresas |
| PUT | `/api/v1/empresas/{id}` | JWT AdminPolicy | Admin + ConcurrentWrites | 200/400/401/403/404/409/422/429 | 38-endpoint-admin-empresas |
| DELETE | `/api/v1/empresas/{id}` | JWT AdminPolicy | Admin + ConcurrentWrites | 204/401/403/404/429 | 38-endpoint-admin-empresas |
| GET | `/api/v1/asociaciones` | no / opcional | Global | 200/400/429 | 28-endpoint-asociaciones |
| GET | `/api/v1/destinos` | no / opcional | Global | 200/400/429 | 29-endpoint-destinos |
| GET | `/api/v1/medios-transporte` | no / opcional | Global | 200/429 | 30-endpoint-medios-transporte |
| GET | `/api/v1/rutas` | no / opcional | Global | 200/400/404/429 | 19-endpoint-rutas |
| GET | `/api/v1/rutas/{id}` | no / opcional | Global | 200/400/404/429 | 31-endpoint-ruta-individual |
| POST | `/api/v1/rutas` | JWT AdminPolicy | Admin + ConcurrentWrites | 201/400/401/403/404/409/422/429 | 39-endpoint-admin-rutas |
| PUT | `/api/v1/rutas/{id}` | JWT AdminPolicy | Admin + ConcurrentWrites | 200/400/401/403/404/409/422/429 | 39-endpoint-admin-rutas |
| DELETE | `/api/v1/rutas/{id}` | JWT AdminPolicy | Admin + ConcurrentWrites | 204/401/403/404/429 | 39-endpoint-admin-rutas |
| GET | `/api/v1/rutas/{id}/paradas` | no / opcional | Global | 200/400/404/429 | 20-endpoint-paradas |
| GET | `/api/v1/paradas/{id}` | no / opcional | Global | 200/400/404/429 | 32-endpoint-parada-individual |
| GET | `/api/v1/paradas/cercanas` | no / opcional | Global | 200/400/429 | 32-endpoint-parada-individual |
| GET | `/api/v1/rutas/{id}/horarios` | no / opcional | Global | 200/400/404/429 | 21-endpoint-horarios |
| POST/PUT/DELETE | `/api/v1/rutas/{id}/horarios` | JWT AdminPolicy | Admin + ConcurrentWrites | 201/200/204/400/401/403/404/409/429 | 40-endpoint-admin-horarios-tarifas |
| GET | `/api/v1/rutas/{id}/tarifas` | no / opcional | Global | 200/400/404/429 | 22-endpoint-tarifas |
| POST/PUT/DELETE | `/api/v1/rutas/{id}/tarifas` | JWT AdminPolicy | Admin + ConcurrentWrites | 201/200/204/400/401/403/404/409/429 | 40-endpoint-admin-horarios-tarifas |
| GET/POST | `/api/v1/calificaciones` | JWT Usuario | Global + Writes | 200/201/400/401/409/422/429 | 23-endpoint-calificaciones |
| POST | `/api/v1/auth/refresh` | Refresh Token | Auth | 200/400/401/429 | 33-endpoint-auth-refresh |
| POST | `/api/v1/auth/2fa/verify` | Pre-Auth JWT | Auth | 200/400/401/429 | 34-endpoint-auth-2fa |
| POST | `/api/v1/admin/bootstrap` | X-Bootstrap-Key | Bootstrap | 201/400/401/403/409/429 | 35-endpoint-admin-bootstrap |
| POST | `/api/v1/telemetria/inferencia` | JWT / Consentimiento | Telemetría | 200/400/401/422/429/502/504 | 36-endpoint-ml-inferencia |
| POST | `/api/v1/recomendaciones` | JWT / Anónimo | GenAI | 200/400/401/422/429/502/504 | 37-endpoint-genai-recomendaciones |
| POST | `/api/v1/auth/registro` | no | Auth + Registro | 201/400/409/422/429 | 41-endpoint-auth-registro |
| POST | `/api/v1/auth/recuperar-password` | no | Auth + Recup | 200/400/429 | 42-endpoint-auth-recuperar-password |
| POST | `/api/v1/auth/restablecer-password` | no (con token) | Auth + Recup | 200/400/422/429 | 43-endpoint-auth-restablecer-password |
| GET | `/api/v1/usuarios/me` | JWT | Global | 200/401/404/429 | 44-endpoint-usuario-perfil-me |
| PUT | `/api/v1/usuarios/me` | JWT | Writes | 200/400/401/409/422/429 | 45-endpoint-usuario-perfil-editar |
| DELETE | `/api/v1/usuarios/me` | JWT | Writes | 200/400/401/429 | 46-endpoint-usuario-perfil-baja |

Convención JSON legacy: `prefijos minúsculos + resto PascalCase + id/sufijo`, `PropertyNamingPolicy=null`.
`PagedResult{Items,TotalCount,Page,PageSize}` (desde nv-02). OpenAPI + Scalar solo Dev.
