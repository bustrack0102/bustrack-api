# Plan técnico — 22-endpoint-tarifas — Endpoint de tarifas

> Spec: `spec.md` · Endpoint: `GET /api/v1/rutas/{id}/tarifas` · Estado: Propuesto.

## 1. Estrategia

Definir contrato, esquema de entrada/salida, validaciones y pruebas antes de implementar.

## 2. Diseño

- Stack objetivo: ASP.NET Core .NET 10 + Azure Cosmos DB for NoSQL, conservando JWT HS256, Argon2id y FluentValidation.
- Componentes: controlador/router, servicio, repositorio o cliente HTTP.
- Tecnología: ASP.NET Core .NET 10 + Azure Cosmos DB for NoSQL, conservando JWT HS256, Argon2id y FluentValidation.

## 3. Archivos

```text
API_SDD/Controllers/V1/TarifasController.cs (nuevo)
```

## 4. Seguridad

- ASVS L2, Argon2id, validación JWT estricta, rate limiting, CSP con nonce, Serilog scrubbing y cero secretos en Git.
- Validar entradas y no registrar secretos/PII.

## 5. Errores

- Respuesta uniforme sin stack traces ni detalles internos.
- Registrar correlación sin datos sensibles.

## 6. Pruebas

Pruebas de contrato, validación, autenticación y errores cuando el endpoint exista; casos definidos mientras sea propuesto.

## 7. Riesgos

- Contrato desalineado con el consumidor.
- Errores no cubiertos.
- Exposición de datos sensibles.
