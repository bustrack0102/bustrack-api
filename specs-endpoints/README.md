# Specs de endpoints — BusTrack API

> Contratos HTTP que este repositorio expone o consume. Cada endpoint tiene
> `spec.md`, `plan.md`, `task.md` y `chart.md`. La numeración continúa después
> de la última tarea de `../specs/`.

## Endpoints

| ID | Método y ruta | Nombre | Estado |
|---|---|---|---|
| `13-endpoint-login` | `POST /api/v1/auth/login` | Endpoint de login | Implementado |
| `14-endpoint-logout` | `POST /api/v1/auth/logout` | Endpoint de logout | Implementado |
| `15-endpoint-health` | `GET /health y /health/ready` | Endpoint de salud | Implementado |
| `16-endpoint-usuarios-coleccion` | `GET/POST /api/v1/usuarios` | Endpoint de colección de usuarios | Implementado |
| `17-endpoint-usuario-individual` | `GET/PUT/DELETE /api/v1/usuarios/{id}` | Endpoint de usuario individual | Implementado |
| `18-endpoint-empresas` | `GET /api/v1/empresas` (+ `/autocompletar`, `/comparar`) | Endpoint de empresas y asociaciones (EP-EM-01, EP-EM-02, EP-EM-05) | Implementado |
| `19-endpoint-rutas` | `GET /api/v1/rutas` | Endpoint de rutas | Propuesto |
| `20-endpoint-paradas` | `GET /api/v1/rutas/{id}/paradas` | Endpoint de paradas | Propuesto |
| `21-endpoint-horarios` | `GET /api/v1/rutas/{id}/horarios` | Endpoint de horarios | Propuesto |
| `22-endpoint-tarifas` | `GET /api/v1/rutas/{id}/tarifas` | Endpoint de tarifas | Propuesto |
| `23-endpoint-calificaciones` | `GET/POST /api/v1/calificaciones` | Endpoint de calificaciones | Propuesto |
| `24-endpoint-ping` | `GET /api/v1/ping` | Endpoint de ping | Implementado |
| `25-endpoint-probes` | `GET /api/v1/probe/*` | Endpoints de diagnóstico y sondeo de errores | Implementado |
| `26-endpoint-documentacion-openapi` | `GET /openapi/v1.json y /scalar/v1` | Endpoints de documentación OpenAPI y Scalar | Implementado |
| `27-endpoint-empresa-individual` | `GET /api/v1/empresas/{id}` (+ `/{id}/rutas`) | Endpoint de detalle de empresa de transporte (EP-EM-03, EP-EM-04) | Implementado |
| `28-endpoint-asociaciones` | `GET /api/v1/asociaciones` | Endpoint de cooperativas y asociaciones | Propuesto |
| `29-endpoint-destinos` | `GET /api/v1/destinos` | Endpoint de destinos y terminales | Propuesto |
| `30-endpoint-medios-transporte` | `GET/POST/PUT/DELETE /api/v1/medios-transporte` | Endpoint de medios y modalidades de transporte (EP-EM-09 a EP-EM-12) | Implementado |
| `31-endpoint-ruta-individual` | `GET /api/v1/rutas/{id}` | Endpoint de detalle de ruta | Propuesto |
| `32-endpoint-parada-individual` | `GET /api/v1/paradas/{id} y /cercanas` | Endpoint de detalle y cercanía de paradas | Propuesto |
| `33-endpoint-auth-refresh` | `POST /api/v1/auth/refresh` | Endpoint de refresco y renovación de tokens JWT (EP-AU-03) | Implementado |
| `34-endpoint-auth-2fa` | `POST /api/v1/auth/2fa/*` | Endpoint de autenticación de dos factores (2FA / MFA) | Propuesto |
| `35-endpoint-admin-bootstrap` | `POST /api/v1/admin/bootstrap` | Endpoint de bootstrap inicial de administrador | Propuesto |
| `36-endpoint-ml-inferencia` | `POST /api/v1/telemetria/inferencia` | Endpoint de inferencia de telemetría y calidad vial (ML) | Propuesto |
| `37-endpoint-genai-recomendaciones` | `POST /api/v1/recomendaciones` | Endpoint de recomendaciones y planificación de viajes (GenAI) | Propuesto |
| `38-endpoint-admin-empresas` | `POST/PUT/DELETE /api/v1/empresas` | Endpoint de administración de empresas de transporte (EP-EM-06 a EP-EM-08) | Implementado |
| `39-endpoint-admin-rutas` | `POST/PUT/DELETE /api/v1/rutas` | Endpoint de administración de rutas y paradas | Propuesto |
| `40-endpoint-admin-horarios-tarifas` | `POST/PUT/DELETE /api/v1/rutas/{id}/horarios\|tarifas` | Endpoint de administración de horarios y tarifas | Propuesto |
| `41-endpoint-auth-registro` | `POST /api/v1/auth/registro` | Endpoint de registro de usuario (EP-AU-01) | Implementado |
| `42-endpoint-auth-recuperar-password` | `POST /api/v1/auth/recuperar-password` | Endpoint de solicitud de recuperación de contraseña (EP-AU-05) | Implementado |
| `43-endpoint-auth-restablecer-password` | `POST /api/v1/auth/restablecer-password` | Endpoint de restablecimiento de contraseña (EP-AU-06) | Implementado |
| `44-endpoint-usuario-perfil-me` | `GET /api/v1/usuarios/me` | Endpoint de consulta de perfil de usuario autenticado (EP-AU-07) | Implementado |
| `45-endpoint-usuario-perfil-editar` | `PUT /api/v1/usuarios/me` | Endpoint de edición de perfil de usuario autenticado (EP-AU-08) | Implementado |
| `46-endpoint-usuario-perfil-baja` | `DELETE /api/v1/usuarios/me` | Endpoint de baja de cuenta de usuario autenticado (EP-AU-09) | Implementado |
| `47-endpoint-moderacion` | `POST /api/v1/moderacion` | Endpoint de moderación de contenido | Implementado |
| `48-endpoint-mapa-elementos` | `GET /api/v1/mapa/elementos` | Endpoint de elementos del mapa interactivo (EP-RP-10) | Implementado |
| `49-endpoint-catalogos-estado` | `GET /api/v1/catalogos/estado-actualizacion y /api/v1/health` | Endpoint de versión de catálogos SQLite y health v1 (EP-AD-01/09) | Implementado |

## Reglas

- Diferenciar endpoints implementados de propuestos.
- No inventar endpoints ni métodos.
- Cada endpoint declara autenticación, validaciones, códigos HTTP y pruebas.
