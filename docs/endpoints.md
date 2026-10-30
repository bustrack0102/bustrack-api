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

Convención JSON legacy: `prefijos minúsculos + resto PascalCase + id/sufijo`, `PropertyNamingPolicy=null`.
`PagedResult{Items,TotalCount,Page,PageSize}` (desde nv-02). OpenAPI + Scalar solo Dev.
