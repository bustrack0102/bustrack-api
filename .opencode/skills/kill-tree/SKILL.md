---
name: kill-tree
description: Matar proceso tests
metadata:
  scope: lite
  variant: lite
  opencode/slash: "true"
---
# kill-tree MVP

## Incluye
- Helper matar árbol proceso real con `finally`, sin huérfanos para Pact futuro/test ports.
- Apphost local se mata por nombre (`APISDD`, nunca `dotnet.exe`); suites con contenedor nunca en paralelo (puerto fijo).

## Verify
- Sin procesos huérfanos tras tests (puertos libres).
