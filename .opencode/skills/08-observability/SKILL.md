---
name: 08-observability
description: Serilog+health sin OTel (lite)
metadata:
  scope: lite
  variant: lite
  opencode/slash: "true"
---
# 08-observability MVP (lite)

## Incluye
- `Serilog JSON rolling + filtro secretos + CorrelationId + /health + /health/ready SQL`.

## Excluye
- OTel, Prometheus /metrics full, Grafana.

## Verify
- Logs JSON sin secretos + `/health/ready` con check SQL.
