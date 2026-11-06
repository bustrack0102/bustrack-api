# Plan técnico — 31-endpoint-ruta-individual — Endpoint de detalle de ruta

> Spec: `spec.md` · Endpoint: `GET /api/v1/rutas/{id}` · Estado: Propuesto.

## 1. Estrategia

Implementar `GetByIdAsync` en `RutasController` para devolver la información estructurada de la ruta y su trazado geográfico serializado.

## 2. Diseño

- Stack objetivo: ASP.NET Core .NET 10 + Azure Cosmos DB for NoSQL.
- Contenedor Cosmos: `Rutas` con partition key `/empresaId` o `/id`.
- DTO: `RutaDetalleDto`.

## 3. Archivos

```text
API_SDD/Controllers/V1/RutasController.cs
API_SDD/Dtos/Transporte/RutaDetalleDto.cs
API_SDD/Services/Transporte/IRutaService.cs
specs/09-consulta-de-rutas-y-paradas/
```

## 4. Seguridad

- Rate limit global.
- Verificación estricta de GUID para evitar ataques de inyección.

## 5. Errores

- Retorno de RFC 7807 uniforme para 404 y 400.

## 6. Pruebas

- Pruebas unitarias de mapeo DTO y trazado GeoJSON.
- Pruebas de integración para rutas existentes e inexistentes.

## 7. Riesgos

- Geometría de trazado pesada mitigada mediante compresión Gzip/Brotli del pipeline.

