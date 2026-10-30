---
name: empirical-verification
description: Medir antes de fijar
metadata:
  scope: full
  mvp: hosting-ready-sin-saga
  opencode/slash: "true"
---
# empirical-verification MVP

## What I do
- No fijar timeouts/umbrales/nombres sin `curl /health + build + test` medido. `LC_NUMERIC=C`, scripts atómicos `mktemp+mv`.
- Empírico antes que teórico (constitución §2.5).

## Verify
- Toda cifra (timeout, rate, umbral) con evidencia de medición citada.
