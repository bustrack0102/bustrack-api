---
name: 03-api-base
description: Versionado + catálogo + JSON legacy
metadata:
  scope: full
  mvp: hosting-ready-sin-saga
  opencode/slash: "true"
---
# 03-api-base MVP

## What I do
- `api/v{version}/[controller]` v1.0 estricta, sin versión 404. `PropertyNamingPolicy=null`.
- OpenAPI doc único v1: transformer `v{version}` → `v1` + retira parámetro `version` (sin Swagger/ApiExplorer).
- JSON legacy medido `prefijos minúsculos + resto PascalCase + id`, NO PascalCase puro.
- `docs/endpoints.md` canónico + `check_endpoints.ps1 OK`. Scalar solo Dev.

## Verify
- `scripts/check_endpoints.ps1` → OK + contrato `IsConventional` legacy.
