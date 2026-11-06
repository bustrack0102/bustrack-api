# Plan técnico — 25-endpoint-probes — Endpoints de diagnóstico y sondeo de errores

> Spec: `spec.md` · Endpoint: `GET /api/v1/probe/*` · Estado: Implementado.

## 1. Estrategia

Implementar un controlador dedicado `ProbeController` que valide el entorno y genere intencionalmente los errores de control para certificar que el middleware de excepciones traduce todo a RFC 7807.

## 2. Diseño

- Stack objetivo: ASP.NET Core .NET 10.
- Componentes: `ProbeController` con inyección de `IHostEnvironment`.
- Middleware: `GlobalExceptionHandlerMiddleware` capturando fallos y estandarizando salidas.

## 3. Archivos

```text
API_SDD/Controllers/V1/ProbeController.cs (existente)
API_SDD/Middleware/ExceptionHandlingMiddleware.cs
```

## 4. Seguridad

- Retorno de 404 inmediato si el entorno es Productivo.
- Cero información interna expuesta.

## 5. Errores

- ProblemDetails con Type, Title, Status, Detail e Instance (CorrelationId).

## 6. Pruebas

- Pruebas en IntegrationTest que recorren cada subruta y comprueban el código de estado y el Content-Type `application/problem+json`.

## 7. Riesgos

- Exposición inadvertida en producción mitigada con validación `_environment.IsProduction()`.

