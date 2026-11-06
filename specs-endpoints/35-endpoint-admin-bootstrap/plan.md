# Plan técnico — 35-endpoint-admin-bootstrap — Endpoint de bootstrap inicial de administrador

> Spec: `spec.md` · Endpoint: `POST /api/v1/admin/bootstrap` · Estado: Propuesto.

## 1. Estrategia

Diseñar un controlador de bootstrap que opere exclusivamente si la base de datos no contiene usuarios con rol `Admin` y valide una clave de inicialización secreta configurada fuera del repositorio.

## 2. Diseño

- Stack objetivo: ASP.NET Core .NET 10 + Azure Cosmos DB for NoSQL, Argon2id.
- Componentes: `BootstrapController`, `IBootstrapService`, `BootstrapRequestDto`.

## 3. Archivos

```text
API_SDD/Controllers/V1/BootstrapController.cs
API_SDD/Dtos/Admin/BootstrapRequestDto.cs
API_SDD/Services/Admin/IBootstrapService.cs
specs/04-gestion-de-usuarios/
```

## 4. Seguridad

- ASVS L2, clave `X-Bootstrap-Key` comparada en tiempo constante (`CryptographicOperations.FixedTimeEquals`).
- El primer administrador recibe forzosamente el requerimiento de activar 2FA en su primer inicio de sesión.

## 5. Errores

- Retorno de 409 Conflict si la inicialización ya se completó.

## 6. Pruebas

- Prueba de creación exitosa en base de datos vacía.
- Prueba de rechazo en base de datos ya inicializada.

## 7. Riesgos

- Ejecución no autorizada prevenida con clave secreta y condición estricta de conteo cero de administradores.

