# Endpoints — vivo (nv-01 + EP-AU + EP-EM + Rutas/Paradas + IA/Moderación)

> Fuente contratos. Los specs solo enlazan, no duplican. Constitución §6.

| Método | Ruta | Auth | Rate | Códigos | Spec |
|---|---|---|---|---|---|
| GET | `/health` | no | global | 200 | 15-endpoint-health (nv-01) |
| GET | `/health/ready` (check DB resiliente) | no | global | 200/503 | 15-endpoint-health (nv-01) |
| GET | `/api/v1/ping` | no | global | 200 | 24-endpoint-ping (nv-01) |
| GET | `/api/v1/probe/timeout` (no-prod) | no | global | 408/404 | 25-endpoint-probes (nv-01) |
| GET | `/api/v1/probe/error` (no-prod) | no | global | 500/404 | 25-endpoint-probes (nv-01) |
| GET | `/api/v1/probe/forbidden` (no-prod) | no | global | 403/404 | 25-endpoint-probes (nv-01) |
| GET | `/api/v1/probe/conflict` (no-prod) | no | global | 409/404 | 25-endpoint-probes (nv-01) |
| GET | `/api/v1/probe/invalid` (no-prod) | no | global | 422/404 | 25-endpoint-probes (nv-01) |
| POST | `/api/v1/sddusuarios` | JWT AdminPolicy | Admin + ConcurrentWrites | 201/400/401/403/409/422/429 | 16-endpoint-usuarios-coleccion (nv-04) |
| PUT | `/api/v1/sddusuarios/{id}` | JWT AdminPolicy | Admin + ConcurrentWrites | 200/400/401/403/404/409/422/429 | 17-endpoint-usuario-individual (nv-04) |
| GET | `/api/v1/sddusuarios?page=&pageSize=` | JWT AdminPolicy | Admin | 200/401/403/422/429 | 16-endpoint-usuarios-coleccion (nv-04) |
| GET | `/api/v1/sddusuarios/{id}` | JWT AdminPolicy | Admin | 200/401/403/404/429 | 17-endpoint-usuario-individual (nv-04) |
| DELETE | `/api/v1/sddusuarios/{id}?rowVersion={base64}` | JWT AdminPolicy | Admin + ConcurrentWrites | 204/400/401/403/404/409/429 | 17-endpoint-usuario-individual (nv-04) |
| POST | `/api/v1/auth/registro` | no | Login | 201/400/409/422/429 | 41-endpoint-auth-registro (EP-AU-01) |
| POST | `/api/v1/auth/login` | no | Login + Global | 200/400/401/422/429 | 13-endpoint-login (EP-AU-02) |
| POST | `/api/v1/auth/refresh` | no | Login | 200/400/401/429 | 33-endpoint-auth-refresh (EP-AU-03) |
| POST | `/api/v1/auth/logout` | JWT | Global | 200/204/401/429 | 14-endpoint-logout (EP-AU-04) |
| POST | `/api/v1/auth/recuperar-password` | no | Login | 200/400/429 | 42-endpoint-auth-recuperar-password (EP-AU-05) |
| POST | `/api/v1/auth/restablecer-password` | no | Login | 200/400/422/429 | 43-endpoint-auth-restablecer-password (EP-AU-06) |
| GET | `/api/v1/usuarios/me` | JWT | Global | 200/401/404/429 | 44-endpoint-usuario-perfil-me (EP-AU-07) |
| PUT | `/api/v1/usuarios/me` | JWT | ConcurrentWrites | 200/400/401/409/422/429 | 45-endpoint-usuario-perfil-editar (EP-AU-08) |
| DELETE | `/api/v1/usuarios/me` | JWT | ConcurrentWrites | 200/400/401/429 | 46-endpoint-usuario-perfil-baja (EP-AU-09) |
| GET | `/api/v1/usuarios` | JWT AdminPolicy | Admin | 200/401/403/422/429 | 16-endpoint-usuarios-coleccion (EP-AU-10) |
| GET | `/api/v1/empresas` | no | Global | 200/400/429 | 18-endpoint-empresas (EP-EM-01) |
| GET | `/api/v1/empresas/autocompletar` | no | Global | 200/400/429 | 18-endpoint-empresas (EP-EM-02) |
| GET | `/api/v1/empresas/{id}` | no | Global | 200/400/404/429 | 27-endpoint-empresa-individual (EP-EM-03) |
| GET | `/api/v1/empresas/{id}/rutas` | no | Global | 200/400/404/429 | 27-endpoint-empresa-individual (EP-EM-04) |
| GET | `/api/v1/empresas/comparar` | no | Global | 200/400/429 | 18-endpoint-empresas (EP-EM-05) |
| POST | `/api/v1/empresas` | JWT AdminPolicy | ConcurrentWrites | 201/400/401/403/409/422/429 | 38-endpoint-admin-empresas (EP-EM-06) |
| PUT | `/api/v1/empresas/{id}` | JWT AdminPolicy | ConcurrentWrites | 200/400/401/403/404/409/422/429 | 38-endpoint-admin-empresas (EP-EM-07) |
| DELETE | `/api/v1/empresas/{id}` | JWT AdminPolicy | ConcurrentWrites | 200/401/403/404/429 | 38-endpoint-admin-empresas (EP-EM-08) |
| GET | `/api/v1/medios-transporte` | no | Global | 200/429 | 30-endpoint-medios-transporte (EP-EM-09) |
| POST | `/api/v1/medios-transporte` | JWT AdminPolicy | ConcurrentWrites | 201/400/401/403/409/422/429 | 30-endpoint-medios-transporte (EP-EM-10) |
| PUT | `/api/v1/medios-transporte/{id}` | JWT AdminPolicy | ConcurrentWrites | 200/400/401/403/404/409/422/429 | 30-endpoint-medios-transporte (EP-EM-11) |
| DELETE | `/api/v1/medios-transporte/{id}` | JWT AdminPolicy | ConcurrentWrites | 200/401/403/404/409/429 | 30-endpoint-medios-transporte (EP-EM-12) |
| GET | `/api/v1/rutas` | no / opcional | Global | 200/400/429 | 19-endpoint-rutas |
| GET | `/api/v1/rutas/buscar` | no / opcional | Global | 200/400/429 | 19-endpoint-rutas |
| GET | `/api/v1/rutas/autocompletar` | no / opcional | Global | 200/400/429 | 19-endpoint-rutas |
| GET | `/api/v1/rutas/{id}` | no / opcional | Global | 200/400/404/429 | 31-endpoint-ruta-individual |
| GET | `/api/v1/rutas/{id}/paradas` | no / opcional | Global | 200/400/404/429 | 20-endpoint-paradas |
| GET | `/api/v1/rutas/{id}/geometria` | no / opcional | Global | 200/400/404/429 | 31-endpoint-ruta-individual |
| GET | `/api/v1/rutas/{id}/horarios` | no / opcional | Global | 200/400/404/429 | 21-endpoint-horarios |
| GET | `/api/v1/rutas/{id}/tarifas` | no / opcional | Global | 200/400/404/429 | 22-endpoint-tarifas |
| GET | `/api/v1/paradas` | no / opcional | Global | 200/400/429 | 20-endpoint-paradas |
| GET | `/api/v1/paradas/autocompletar` | no / opcional | Global | 200/400/429 | 20-endpoint-paradas |
| GET | `/api/v1/paradas/{id}` | no / opcional | Global | 200/404/429 | 32-endpoint-parada-individual |
| GET | `/api/v1/paradas/cercanas` | no / opcional | Global | 200/400/429 | 32-endpoint-parada-individual |
| GET | `/api/v1/mapa/elementos` | no / opcional | Global | 200/400/429 | 48-endpoint-mapa-elementos |
| GET | `/api/v1/horarios` | no / opcional | Global | 200/400/429 | 21-endpoint-horarios |
| GET | `/api/v1/horarios/siguiente` | no / opcional | Global | 200/400/404/429 | 21-endpoint-horarios |
| GET | `/api/v1/tarifas` | no / opcional | Global | 200/400/429 | 22-endpoint-tarifas |
| GET | `/api/v1/tarifas/calcular` | no / opcional | Global | 200/400/404/429 | 22-endpoint-tarifas |
| GET | `/api/v1/destinos` | no / opcional | Global | 200/400/429 | 29-endpoint-destinos |
| GET | `/api/v1/destinos/autocompletar` | no / opcional | Global | 200/400/429 | 29-endpoint-destinos |
| GET | `/api/v1/destinos/categorias` | no / opcional | Global | 200/429 | 29-endpoint-destinos |
| GET | `/api/v1/destinos/{id}` | no / opcional | Global | 200/404/429 | 29-endpoint-destinos |
| GET | `/api/v1/destinos/{id}/rutas` | no / opcional | Global | 200/404/429 | 29-endpoint-destinos |
| GET | `/api/v1/catalogos/estado-actualizacion` | no | Global | 200/429 | 49-endpoint-catalogos-estado |
| GET | `/api/v1/health` | no | Global | 200/503 | 49-endpoint-catalogos-estado |
| POST | `/api/v1/moderacion` | no | Global | 200/400/429 | 47-endpoint-moderacion |
| POST | `/api/v1/moderacion/archivo` | JWT | ConcurrentWrites | 200/400/401/422/429 | 47-endpoint-moderacion |
| POST | `/api/v1/media/upload` | JWT | ConcurrentWrites | 201/400/401/422/429 | 51-endpoint-media-cdn |
| GET | `/api/v1/dispositivos/mios` | JWT | Global | 200/401/429 | 52-endpoint-dispositivos-fcm |
| POST | `/api/v1/dispositivos` | JWT | ConcurrentWrites | 201/400/401/422/429 | 52-endpoint-dispositivos-fcm |
| DELETE | `/api/v1/dispositivos/{id}` | JWT | ConcurrentWrites | 204/401/404/429 | 52-endpoint-dispositivos-fcm |
| GET | `/api/v1/calificaciones` | no | Global | 200/429 | 23-endpoint-calificaciones |
| GET | `/api/v1/calificaciones/resumen` | no | Global | 200/429 | 23-endpoint-calificaciones |
| GET | `/api/v1/calificaciones/mias` | JWT | Global | 200/401/429 | 23-endpoint-calificaciones |
| POST | `/api/v1/calificaciones` | JWT | ConcurrentWrites | 201/400/401/404/409/422/429 | 23-endpoint-calificaciones |
| PUT | `/api/v1/calificaciones/{id}` | JWT | ConcurrentWrites | 200/401/403/404/422/429 | 23-endpoint-calificaciones |
| DELETE | `/api/v1/calificaciones/{id}` | JWT | ConcurrentWrites | 204/401/403/404/429 | 23-endpoint-calificaciones |
| GET | `/api/v1/viajes` | JWT | Global | 200/401/429 | 50-endpoint-viajes |
| GET | `/api/v1/viajes/{id}` | JWT | Global | 200/401/403/404/429 | 50-endpoint-viajes |
| POST | `/api/v1/viajes` | JWT | ConcurrentWrites | 201/400/401/404/422/429 | 50-endpoint-viajes |
| PUT | `/api/v1/viajes/{id}` | JWT | ConcurrentWrites | 200/401/403/404/422/429 | 50-endpoint-viajes |
| DELETE | `/api/v1/viajes/{id}` | JWT | ConcurrentWrites | 204/401/403/404/429 | 50-endpoint-viajes |
| POST | `/api/v1/paradas` | JWT AdminPolicy | ConcurrentWrites | 201/401/403/409/422/429 | 39-endpoint-admin-rutas |
| PUT | `/api/v1/paradas/{id}` | JWT AdminPolicy | ConcurrentWrites | 200/401/403/404/422/429 | 39-endpoint-admin-rutas |
| DELETE | `/api/v1/paradas/{id}` | JWT AdminPolicy | ConcurrentWrites | 204/401/403/404/429 | 39-endpoint-admin-rutas |
| POST | `/api/v1/rutas` | JWT AdminPolicy | ConcurrentWrites | 201/401/403/404/409/422/429 | 39-endpoint-admin-rutas |
| PUT | `/api/v1/rutas/{id}` | JWT AdminPolicy | ConcurrentWrites | 200/401/403/404/409/422/429 | 39-endpoint-admin-rutas |
| DELETE | `/api/v1/rutas/{id}` | JWT AdminPolicy | ConcurrentWrites | 204/401/403/404/429 | 39-endpoint-admin-rutas |
| POST | `/api/v1/horarios` | JWT AdminPolicy | ConcurrentWrites | 201/401/403/404/422/429 | 40-endpoint-admin-horarios-tarifas |
| PUT | `/api/v1/horarios/{id}` | JWT AdminPolicy | ConcurrentWrites | 200/401/403/404/422/429 | 40-endpoint-admin-horarios-tarifas |
| DELETE | `/api/v1/horarios/{id}` | JWT AdminPolicy | ConcurrentWrites | 204/401/403/404/429 | 40-endpoint-admin-horarios-tarifas |
| POST | `/api/v1/tarifas` | JWT AdminPolicy | ConcurrentWrites | 201/401/403/404/422/429 | 40-endpoint-admin-horarios-tarifas |
| PUT | `/api/v1/tarifas/{id}` | JWT AdminPolicy | ConcurrentWrites | 200/401/403/404/422/429 | 40-endpoint-admin-horarios-tarifas |
| DELETE | `/api/v1/tarifas/{id}` | JWT AdminPolicy | ConcurrentWrites | 204/401/403/404/429 | 40-endpoint-admin-horarios-tarifas |
| POST | `/api/v1/destinos` | JWT AdminPolicy | ConcurrentWrites | 201/401/403/409/422/429 | 29-endpoint-destinos |
| PUT | `/api/v1/destinos/{id}` | JWT AdminPolicy | ConcurrentWrites | 200/401/403/404/422/429 | 29-endpoint-destinos |
| DELETE | `/api/v1/destinos/{id}` | JWT AdminPolicy | ConcurrentWrites | 204/401/403/404/429 | 29-endpoint-destinos |
| GET | `/api/v1/admin/fuentes-datos` | JWT AdminPolicy | Admin | 200/401/403/429 | 53-endpoint-admin-fuentes-importaciones |
| POST | `/api/v1/admin/fuentes-datos` | JWT AdminPolicy | ConcurrentWrites | 201/401/403/422/429 | 53-endpoint-admin-fuentes-importaciones |
| PUT | `/api/v1/admin/fuentes-datos/{id}` | JWT AdminPolicy | ConcurrentWrites | 200/401/403/404/422/429 | 53-endpoint-admin-fuentes-importaciones |
| DELETE | `/api/v1/admin/fuentes-datos/{id}` | JWT AdminPolicy | ConcurrentWrites | 204/401/403/404/429 | 53-endpoint-admin-fuentes-importaciones |
| POST | `/api/v1/admin/importaciones` | JWT AdminPolicy | ConcurrentWrites | 201/401/403/404/422/429 | 53-endpoint-admin-fuentes-importaciones |
| GET | `/api/v1/admin/importaciones` | JWT AdminPolicy | Admin | 200/401/403/429 | 53-endpoint-admin-fuentes-importaciones |
| POST | `/api/v1/telemetria/inferencia` | no / consentimiento | Global | 200/400/422/429 | 36-endpoint-ml-inferencia |
| GET | `/api/v1/consentimientos/me` | JWT | Global | 200/401/429 | 54-endpoint-consentimientos-sensores |
| POST | `/api/v1/consentimientos` | JWT | ConcurrentWrites | 200/401/422/429 | 54-endpoint-consentimientos-sensores |
| DELETE | `/api/v1/consentimientos/me` | JWT | ConcurrentWrites | 200/401/429 | 54-endpoint-consentimientos-sensores |
| POST | `/api/v1/sensores/lotes` | JWT + Consentimiento | ConcurrentWrites | 201/401/403/404/422/429 | 54-endpoint-consentimientos-sensores |
| GET | `/api/v1/sensores/lotes` | JWT AdminPolicy | Admin | 200/401/403/429 | 54-endpoint-consentimientos-sensores |
| GET | `/api/v1/candidatos-parada` | JWT AdminPolicy | Admin | 200/401/403/429 | 54-endpoint-consentimientos-sensores |
| PATCH | `/api/v1/admin/candidatos-parada/{id}/validar` | JWT AdminPolicy | ConcurrentWrites | 200/401/403/404/409/429 | 54-endpoint-consentimientos-sensores |
| POST | `/api/v1/recomendaciones` | no / opcional | Global | 200/400/422/429 | 37-endpoint-genai-recomendaciones |
| GET | `/api/v1/recomendaciones/historial` | JWT | Global | 200/401/429 | 55-endpoint-recomendaciones-historial |
| POST | `/api/v1/recomendaciones/{id}/retroalimentacion` | JWT | ConcurrentWrites | 200/401/403/404/422/429 | 55-endpoint-recomendaciones-historial |
| GET | `/openapi/v1.json` (solo Dev) | no | — | 200/404 | 26-endpoint-documentacion-openapi (nv-01) |
| GET | `/scalar/v1` (solo Dev) | no | — | 200/404 | 26-endpoint-documentacion-openapi (nv-01) |

## Endpoints propuestos restantes (dominio de transporte e integraciones)

| Método | Ruta | Auth | Rate | Códigos | Spec |
|---|---|---|---|---|---|
| GET | `/api/v1/asociaciones` | no / opcional | Global | 200/400/429 | 28-endpoint-asociaciones |
| GET | `/api/v1/destinos` | no / opcional | Global | 200/400/429 | 29-endpoint-destinos |
| POST | `/api/v1/rutas` | JWT AdminPolicy | Admin + ConcurrentWrites | 201/400/401/403/404/409/422/429 | 39-endpoint-admin-rutas |
| PUT | `/api/v1/rutas/{id}` | JWT AdminPolicy | Admin + ConcurrentWrites | 200/400/401/403/404/409/422/429 | 39-endpoint-admin-rutas |
| DELETE | `/api/v1/rutas/{id}` | JWT AdminPolicy | Admin + ConcurrentWrites | 204/401/403/404/429 | 39-endpoint-admin-rutas |
| GET | `/api/v1/paradas/{id}` | no / opcional | Global | 200/400/404/429 | 32-endpoint-parada-individual |
| GET | `/api/v1/rutas/{id}/horarios` | no / opcional | Global | 200/400/404/429 | 21-endpoint-horarios |
| POST/PUT/DELETE | `/api/v1/rutas/{id}/horarios` | JWT AdminPolicy | Admin + ConcurrentWrites | 201/200/204/400/401/403/404/409/429 | 40-endpoint-admin-horarios-tarifas |
| GET | `/api/v1/rutas/{id}/tarifas` | no / opcional | Global | 200/400/404/429 | 22-endpoint-tarifas |
| POST/PUT/DELETE | `/api/v1/rutas/{id}/tarifas` | JWT AdminPolicy | Admin + ConcurrentWrites | 201/200/204/400/401/403/404/409/429 | 40-endpoint-admin-horarios-tarifas |
| GET/POST | `/api/v1/calificaciones` | JWT Usuario | Global + Writes | 200/201/400/401/409/422/429 | 23-endpoint-calificaciones |
| POST | `/api/v1/auth/2fa/verify` | Pre-Auth JWT | Auth | 200/400/401/429 | 34-endpoint-auth-2fa |
| POST | `/api/v1/admin/bootstrap` | X-Bootstrap-Key | Bootstrap | 201/400/401/403/409/429 | 35-endpoint-admin-bootstrap |
