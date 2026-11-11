# Plan — 47-endpoint-moderacion

1. Implementar `ModeracionController` con regex compilados (`GeneratedRegex`) para prompt injection, PII (correo/teléfono), filtro léxico y validación URI segura.
2. Cubrir casos positivos, negativos y error `400` en `UnitTest/Controllers/V1EndpointsTests.cs`.
