# Plan Template — SDD

> Deriva de `specs/NV-0X/spec.md`. Respeta constitución §4 pipeline fijo.

## 1. Contexto
<!-- Spec origen: specs/NV-0X/spec.md -->

## 2. Diseño técnico
<!-- Migración EF → DTO FluentValidation → servicio DI → controller sin try/catch ad-hoc → Serilog CorrelationId -->

## 3. Pipeline orden
1. SecurityHeaders outermost → ExceptionHandling → ForwardedHeaders → HSTS no-Dev → HttpsRedirection → CORS 1 origen → RateLimiter antes Auth → Auth → AuthZ → Controllers

## 4. Riesgos / Deuda
<!-- IMemoryCache blacklist:{jti} TTL 120s se pierde al reciclar. Documentar en Memoria.md -->

## 5. Verificación
<!-- build → unit → integration SQL real → security → contract → semgrep -->
