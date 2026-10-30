---
name: 01-foundation
description: Pipeline final día 1 + Program partial
metadata:
  scope: full
  mvp: hosting-ready-sin-saga
  opencode/slash: "true"
---
# 01-foundation MVP

## What I do
- Orden: SecurityHeaders outermost → ExceptionHandling → ForwardedHeaders → HSTS no-Dev → HttpsRedirection → CORS 1 origen → RateLimiter antes Auth → JWT → Authorization.
- `Program public partial` para WAF. Kestrel 10MB/2min, timeout 60s.
- Sin slots diferidos 04/05/08.

## Cuando usarme
- Al crear `API_SDD/Program.cs` o tocar el pipeline. No tocar pipeline sin orden §4 constitución.

## Verify
- `dotnet build -c Release --no-restore` + WAF arranca en Integration/Security.
