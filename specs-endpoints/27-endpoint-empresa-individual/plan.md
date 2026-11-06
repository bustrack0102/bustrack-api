# Plan técnico — 27-endpoint-empresa-individual — Endpoint de detalle de empresa de transporte

> Spec: `spec.md` · Endpoint: `GET /api/v1/empresas/{id}` · Estado: Propuesto.

## 1. Estrategia

Definir la acción de detalle en `EmpresasController` con validación de identificador, inyección del servicio de dominio de empresas y lectura optimizada de Cosmos DB for NoSQL (o EF Core durante la transición).

## 2. Diseño

- Stack objetivo: ASP.NET Core .NET 10 + Azure Cosmos DB for NoSQL.
- Componentes: `EmpresasController`, `IEmpresaService`, `EmpresaDetalleDto`.
- Persistencia: Particionado por `/id` en Cosmos DB contenedor `Empresas`.

## 3. Archivos

```text
API_SDD/Controllers/V1/EmpresasController.cs
API_SDD/Dtos/Transporte/EmpresaDetalleDto.cs
API_SDD/Services/Transporte/IEmpresaService.cs
specs/08-consulta-de-empresas-y-asociaciones/
```

## 4. Seguridad

- Rate limit global.
- Verificación de formato y mitigación de IDOR.

## 5. Errores

- RFC 7807 uniforme para 404 y 400.

## 6. Pruebas

- Pruebas unitarias de mapeo DTO y servicio.
- Pruebas de integración para casos de éxito (200), inexistente (404) e id mal formado (400).

## 7. Riesgos

- Inconsistencia de datos entre empresa y asociación.

