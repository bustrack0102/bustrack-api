---
name: ci-artifacts-tolerant
description: Artifacts robustos
metadata:
  scope: lite
  variant: lite
  opencode/slash: "true"
---
# ci-artifacts-tolerant MVP

## Incluye
- `if: always() + continue-on-error + if-no-files-found:warn`.

## Verify
- CI no falla por artifacts ausentes.
