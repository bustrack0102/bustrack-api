---
name: traceability
description: Enlazar spec↔código↔tests
metadata:
  scope: full
  mvp: hosting-ready-sin-saga
  opencode/slash: "true"
---
# traceability MVP

## What I do
- Cada PR cita `spec + archivos + tests + criterios`. `traceability-clerk` reporta drift, no edita.
- Specs enlazan `docs/endpoints.md`, no duplican tablas vivas.

## Verify
- `OK` o drift `spec.md:línea ↔ Controller.cs:línea ↔ Test.cs:línea`.
