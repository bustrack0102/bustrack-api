# Plan técnico — 44-endpoint-usuario-perfil-me — Endpoint de consulta de perfil de usuario autenticado (EP-AU-07)

> Spec: `spec.md` · Endpoint: `GET /api/v1/usuarios/me` · Estado: Propuesto.

## 1. Estrategia

Implementar la acción `GetProfileAsync` en `UsuariosController` protegida con `[Authorize]`, leyendo el identificador del usuario de `User.FindFirstValue(ClaimTypes.NameIdentifier)`.

## 2. Diseño

- Stack: ASP.NET Core .NET 10 + Azure Cosmos DB for NoSQL (o EF Core).
- Componentes: `UsuariosController`, `IUsuarioService`, `UsuarioPerfilDto`.

## 3. Archivos

```text
API_SDD/Controllers/V1/UsuariosController.cs
API_SDD/Dtos/Usuario/UsuarioPerfilDto.cs
API_SDD/Services/IUsuarioService.cs
```

## 4. Seguridad

- ASVS L2, protección anti-IDOR garantizada: el usuario solo puede consultar su propio registro extraído de los claims criptográficamente firmados del JWT.

## 5. Errores

- Retorno RFC 7807 uniforme.

## 6. Pruebas

- Pruebas unitarias de mapeo de DTO y agregados de actividad.
- Pruebas de integración validando 200 con token válido y 401 sin token.

## 7. Riesgos

- N+1 en conteos resuelto mediante proyecciones optimizadas o consultas agregadas en base de datos.
