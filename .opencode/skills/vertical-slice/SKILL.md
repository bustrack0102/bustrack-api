---
name: vertical-slice
description: 1 slice = 1 PR liberable
metadata:
  scope: full
  mvp: hosting-ready-sin-saga
  opencode/slash: "true"
---
# vertical-slice MVP

## What I do
- Por CRUD: `spec 1 pág → modelo/migración → DTO/validator → servicio → controller+auth+rate → docs append → tests → gates → tag vX.Y.Z-crudN`.
- Nunca commitea Fase A sin Fase B. Cada slice liberable `dotnet publish` listo.

## Verify
- `CHECKLIST_PR.md` + 1 revisor + `Memoria.md` al día por PR.
