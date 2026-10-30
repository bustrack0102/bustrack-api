---
name: 02-domain-data
description: Modelo Prefijo* + RowVersion + DbContext
metadata:
  scope: full
  mvp: hosting-ready-sin-saga
  opencode/slash: "true"
---
# 02-domain-data MVP

## What I do
- Tablas `Prefijo* + str/int/dec/dte/bln + IConcurrenteAuditable RowVersion IsRowVersion() + decimal(18,2)`.
- Nombres `SDD*` eximidos de Sonar S101 con `#pragma` por archivo (el prefijo es convención, no casing).
- `DbContext` lee `IConfiguration` lazy en factory, compatible WAF. Sin `byte[]{1}` salvo dummy.
- Sin connection string hardcodeada. Inglés en código.

## Verify
- Migración compila + `RowVersion` concurrencia → 409 probado.
