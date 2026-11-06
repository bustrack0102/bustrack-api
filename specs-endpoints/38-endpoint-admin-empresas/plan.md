# Plan técnico — 38-endpoint-admin-empresas — Endpoint de administración de empresas de transporte

> Spec: `spec.md` · Endpoint: `POST/PUT/DELETE /api/v1/empresas` · Estado: Propuesto.

## 1. Estrategia

Extender `EmpresasController` con acciones mutables protegidas por `[Authorize(Policy = "AdminPolicy")]`, inyectando el validador de FluentValidation y el servicio de gestión de empresas.

## 2. Diseño

- Stack objetivo: ASP.NET Core .NET 10 + Azure Cosmos DB for NoSQL.
- Componentes: `EmpresasController`, `IEmpresaAdminService`, `CrearEmpresaDto`, `ActualizarEmpresaDto`.
- Concurrencia: Validación de `ETag` de Cosmos DB o `RowVersion`.

## 3. Archivos

```text
API_SDD/Controllers/V1/EmpresasController.cs
API_SDD/Dtos/Transporte/CrearEmpresaDto.cs
API_SDD/Dtos/Transporte/ActualizarEmpresaDto.cs
API_SDD/Validators/Transporte/CrearEmpresaValidator.cs
API_SDD/Services/Transporte/IEmpresaAdminService.cs
```

## 4. Seguridad

- ASVS L2, AdminPolicy obligatoria.
- Rate limiting para escrituras concurrentes (`ConcurrentWritesPolicy`).

## 5. Errores

- Retorno de RFC 7807 uniforme para 409 Conflict y 422 Unprocessable Entity.

## 6. Pruebas

- Pruebas unitarias de validación FluentValidation.
- Pruebas de integración para autenticación, autorización y concurrencia.

## 7. Riesgos

- Modificación concurrente mitigada mediante verificación estricta de versión.

