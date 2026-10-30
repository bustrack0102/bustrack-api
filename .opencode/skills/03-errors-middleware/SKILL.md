---
name: 03-errors-middleware
description: ErrorResponse uniforme + sondas probe
metadata:
  scope: full
  mvp: hosting-ready-sin-saga
  opencode/slash: "true"
---
# 03-errors-middleware MVP

## What I do
- Único `ExceptionHandlingMiddleware`: `404/403/409/422/400/408/500`. `ErrorResponse{Error,Status,TraceId,Detail solo no-prod}`.
- Cero `try/catch` en controllers. Sondas `probe/{timeout,error,forbidden}` gateadas no-prod. Tests en `Staging`.
- Fail-fast: sin tragar excepciones.

## Verify
- Matriz de errores probada WAF Staging.
