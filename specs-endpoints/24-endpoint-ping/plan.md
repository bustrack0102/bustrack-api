# Plan técnico — 24-endpoint-ping — Endpoint de ping

> Spec: `spec.md` · Endpoint: `GET /api/v1/ping` · Estado: Implementado.

## 1. Estrategia

Exponer un endpoint ultraligero que devuelva 200 OK con timestamp y correlation ID, para liveness y pruebas de conectividad de clientes.

## 2. Diseño

- Stack objetivo: ASP.NET Core .NET 10 + Azure Cosmos DB for NoSQL, conservando JWT HS256, Argon2id y FluentValidation.
- Componentes: `PingController` heredando de `BaseApiController`.
- Tecnología: ASP.NET Core .NET 10, Serilog.

## 3. Archivos

```text
API_SDD/Controllers/V1/PingController.cs (existente)
UnitTest/Controllers/PingControllerTests.cs (o suite de integración)
```

## 4. Seguridad

- Rate limiting global.
- Sin acceso a base de datos ni recursos protegidos.
- Inyección de headers de seguridad estándar (HSTS, CSP, X-Content-Type-Options).

## 5. Errores

- En caso de fallo de infraestructura, devolver RFC 7807 sin stack traces.

## 6. Pruebas

- Prueba unitaria de PingController verificando StatusCode 200 y payload.
- Prueba de integración validando headers HTTP y correlación.

## 7. Riesgos

- Saturación por escaneo externo mitigada mediante rate limit global.

