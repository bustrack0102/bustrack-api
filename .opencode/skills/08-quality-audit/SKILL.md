---
name: 08-quality-audit
description: Audit tabla sin hash-chain full (lite)
metadata:
  scope: lite
  variant: lite
  opencode/slash: "true"
---
# 08-quality-audit MVP (lite)

## Incluye
- `TraceId + tabla audit sin PII`.

## Excluye
- `hash-chain SHA-256 + collect-quality-metrics.sh` obligatorio.

## Verify
- Tabla audit con `TraceId`, sin PII en logs.
