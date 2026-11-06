# Plan técnico — 28-endpoint-asociaciones — Endpoint de cooperativas y asociaciones de transporte

> Spec: `spec.md` · Endpoint: `GET /api/v1/asociaciones` · Estado: Propuesto.

## 1. Estrategia

Crear `AsociacionesController` con soporte de paginación uniforme (`PagedResult<T>`) y consultas indexadas por nombre o sigla.

## 2. Diseño

- Stack objetivo: ASP.NET Core .NET 10 + Azure Cosmos DB for NoSQL.
- Componentes: `AsociacionesController`, `IAsociacionService`, `AsociacionDto`.

## 3. Archivos

```text
API_SDD/Controllers/V1/AsociacionesController.cs
API_SDD/Dtos/Transporte/AsociacionDto.cs
API_SDD/Services/Transporte/IAsociacionService.cs
specs/08-consulta-de-empresas-y-asociaciones/
```

## 4. Seguridad

- Paginación estricta (máximo 50 elementos por página).
- Entrada sanitizada sin riesgo de inyecciones NoSQL.

## 5. Errores

- Retorno uniforme RFC 7807 en validación de parámetros.

## 6. Pruebas

- Pruebas unitarias de filtro y paginación.
- Pruebas de contrato con respuestas vacías y pobladas.

## 7. Riesgos

- Sobrecarga de base de datos por consultas sin índice.

