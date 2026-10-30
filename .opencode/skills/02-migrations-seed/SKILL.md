---
name: 02-migrations-seed
description: Migración InitialCreate + seed idempotente DB externa
metadata:
  scope: full
  mvp: hosting-ready-sin-saga
  opencode/slash: "true"
---
# 02-migrations-seed MVP

## What I do
- `dotnet ef migrations add InitialCreate --project API_SDD`. UNIQUEs filtrados. Migraciones sin `_` (CA1707).
- `IDesignTimeDbContextFactory` (env primero, sin secretos) + `Microsoft.EntityFrameworkCore.Design` PrivateAssets; migración manuscrita válida si el nombre CLI trae `_`.
- Seeder diferido: sin seed en repo; bootstrap del primer Admin por token auto-firmado (ver `AGENTS.md` §9).
- `UseInMemoryDatabase=false` default. `ConnectionStrings__Default` por env externa.

## Verify
- `dotnet ef database update` contra SQL externa. Sin seed: el primer Admin se crea por bootstrap operativo.
