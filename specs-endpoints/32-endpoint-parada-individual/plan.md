# Plan técnico — 32-endpoint-parada-individual — Endpoint de detalle y cercanía de paradas

> Spec: `spec.md` · Endpoint: `GET /api/v1/paradas/{id}` y `/cercanas` · Estado: Propuesto.

## 1. Estrategia

Implementar `ParadasController` con métodos para consultar una parada puntual por su identificador y un método de proximidad geoespacial utilizando funciones de índice espacial en Cosmos DB (`ST_DISTANCE`).

## 2. Diseño

- Stack objetivo: ASP.NET Core .NET 10 + Azure Cosmos DB for NoSQL.
- Componentes: `ParadasController`, `IParadaService`, `ParadaDetalleDto`, `ParadaCercanaDto`.

## 3. Archivos

```text
API_SDD/Controllers/V1/ParadasController.cs
API_SDD/Dtos/Transporte/ParadaDetalleDto.cs
API_SDD/Dtos/Transporte/ParadaCercanaDto.cs
API_SDD/Services/Transporte/IParadaService.cs
specs/09-consulta-de-rutas-y-paradas/
```

## 4. Seguridad

- Validación de parámetros de entrada con FluentValidation.
- Rate limit para evitar scraping masivo de coordenadas.

## 5. Errores

- Retorno de RFC 7807 ante coordenadas mal formadas.

## 6. Pruebas

- Pruebas unitarias del cálculo de distancia Haversine.
- Pruebas de integración para búsqueda de paradas en radio determinado.

## 7. Riesgos

- Latencia en cálculos espaciales mitigada mediante índices geoespaciales Point en Cosmos DB.

