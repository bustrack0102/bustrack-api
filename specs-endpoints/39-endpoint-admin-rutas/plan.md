# Plan técnico — 39-endpoint-admin-rutas — Endpoint de administración de rutas y paradas

> Spec: `spec.md` · Endpoint: `POST/PUT/DELETE /api/v1/rutas` · Estado: Propuesto.

## 1. Estrategia

Extender `RutasController` para permitir mutaciones administrativas con autorización `AdminPolicy`, validación geométrica de GeoJSON y transaccionalidad en Cosmos DB for NoSQL.

## 2. Diseño

- Stack objetivo: ASP.NET Core .NET 10 + Azure Cosmos DB for NoSQL.
- Componentes: `RutasController`, `IRutaAdminService`, `CrearRutaDto`, `ActualizarRutaDto`.

## 3. Archivos

```text
API_SDD/Controllers/V1/RutasController.cs
API_SDD/Dtos/Transporte/CrearRutaDto.cs
API_SDD/Dtos/Transporte/ActualizarRutaDto.cs
API_SDD/Validators/Transporte/CrearRutaValidator.cs
API_SDD/Services/Transporte/IRutaAdminService.cs
```

## 4. Seguridad

- ASVS L2, validación de claim `role: Admin`.
- Protección contra inyección de geometrías gigantescas limitando el payload a 1 MB.

## 5. Errores

- Retorno de RFC 7807 uniforme.

## 6. Pruebas

- Pruebas unitarias de validador GeoJSON y secuencias de paradas.
- Pruebas de integración para escenarios de éxito y código duplicado (409).

## 7. Riesgos

- Inconsistencia de paradas prevenida mediante validación referencial antes de persistir.

