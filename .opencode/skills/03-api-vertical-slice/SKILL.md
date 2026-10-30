---
name: 03-api-vertical-slice
description: Molde entidad→DTO→validator→controller→service→tests
metadata:
  scope: full
  mvp: hosting-ready-sin-saga
  opencode/slash: "true"
---
# 03-api-vertical-slice MVP

## What I do
- `Dtos RowVersion base64 + Validators espejo auto-registrados por reflexión (ValidatorRegistration, sin paquete DI extra) + Service público sin caché + Controller [Authorize AdminPolicy]+[EnableRateLimiting] + Common/PagedResult<T>`.
- Códigos `200/201/204/400/401/403/404/409/422/429`. `PagedResult{Items,TotalCount,Page,PageSize}`.
- Sin `CacheService` entidades. Sin `try/catch` ad-hoc en controllers.
- Slices auth sin entidad (login/logout): DTOs + servicio + controller, sin migración; `Unauthorized→401`.

## Verify
- `NotFound→404, Forbidden→403, Concurrencia→409, Validation→422` probados.
