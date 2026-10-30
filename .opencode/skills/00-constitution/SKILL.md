---
name: 00-constitution
description: Constitución MVP hosting-ready sin saga/Redis/AWS
metadata:
  scope: full
  mvp: hosting-ready-sin-saga
  opencode/slash: "true"
---
# 00-constitution MVP

> Rector: `.specify/memory/constitution.md`. Ante conflicto, manda la constitución.

## What I do
- Stack: .NET10 + EF + SQL externo + JWT HS256 + Argon2id + TOTP + FluentValidation.
- Fuera: SQS/MassTransit, Redis-dist, OTel-full, NBomber/Chaos, AWS.
- Principios: stateless 1-instancia, seguridad primero, fail-fast, empírico, real-first sin fakes.

## Cuando usarme
- Siempre. Cada CRUD sale liberable con todo `00+01+02+03+04` aplicado.

## No hacer
- Sin saga, sin caché entidades, sin `health-ui`/`metrics` full, sin hosting/CD en esta etapa.

## Verify
- `dotnet build -c Release --no-restore` 0/0.
