# Rate Limit Matrix — vivo

> Constitución §3, §6. `UseRateLimiter` antes de Auth.

| Política | Límite | Ventana | Ámbito | Spec |
|---|---|---|---|---|
| Global | 1000 | 1 min | todo | nv-01 (reusada nv-04…nv-06) |
| Login | 5 | 5 min | `POST login` | nv-05 |
| Login2faVerify | 10 | 5 min | `POST login/2fa-verify` | nv-07 (reservado) |
| Admin | 200 | 1 min | endpoints admin | nv-04 (completa nv-05) |
| ConcurrentWrites | 10 | concurrente | POST/PUT/DELETE | nv-04 |

429 uniforme con `Retry-After`. Medir antes de fijar timeouts (principio empírico §2.5).
