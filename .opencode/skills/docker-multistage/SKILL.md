---
name: docker-multistage
description: Dockerfile sin hardening bloqueante (lite)
metadata:
  scope: lite
  variant: lite
  opencode/slash: "true"
---
# docker-multistage MVP (lite)

## Incluye
- `multistage + non-root + .dockerignore`.

## Excluye
- Trivy HIGH/CRITICAL bloqueante, dockle obligatorio.

## Verify
- Build multistage + non-root OK. Sin hosting aún.
