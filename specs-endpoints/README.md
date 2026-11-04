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
| `18-endpoint-empresas` | `GET /api/v1/empresas` | Endpoint de empresas y asociaciones | Propuesto |
| `19-endpoint-rutas` | `GET /api/v1/rutas` | Endpoint de rutas | Propuesto |
| `20-endpoint-paradas` | `GET /api/v1/rutas/{id}/paradas` | Endpoint de paradas | Propuesto |
| `21-endpoint-horarios` | `GET /api/v1/rutas/{id}/horarios` | Endpoint de horarios | Propuesto |
| `22-endpoint-tarifas` | `GET /api/v1/rutas/{id}/tarifas` | Endpoint de tarifas | Propuesto |
| `23-endpoint-calificaciones` | `GET/POST /api/v1/calificaciones` | Endpoint de calificaciones | Propuesto |

## Reglas

- Diferenciar endpoints implementados de propuestos.
- No inventar endpoints ni métodos.
- Cada endpoint declara autenticación, validaciones, códigos HTTP y pruebas.
