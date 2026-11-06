# Plan técnico — 29-endpoint-destinos — Endpoint de destinos y terminales

> Spec: `spec.md` · Endpoint: `GET /api/v1/destinos` · Estado: Propuesto.

## 1. Estrategia

Diseñar `DestinosController` para servir los puntos de referencia principales de la red de transporte y alimentar los autocompletados en las apps móviles.

## 2. Diseño

- Stack objetivo: ASP.NET Core .NET 10 + Azure Cosmos DB for NoSQL.
- Componentes: `DestinosController`, `IDestinoService`, `DestinoDto`.

## 3. Archivos

```text
API_SDD/Controllers/V1/DestinosController.cs
API_SDD/Dtos/Transporte/DestinoDto.cs
API_SDD/Services/Transporte/IDestinoService.cs
specs/07-modelo-de-dominio-de-transporte/
```

## 4. Seguridad

- ASVS L2, sanitización de búsquedas de texto.
- Cero generación dinámica o datos inventados por IA.

## 5. Errores

- Retorno de ProblemDetails ante parámetros inválidos.

## 6. Pruebas

- Pruebas unitarias de filtros por zona y municipio.
- Pruebas de contrato comprobando esquema `DestinoDto`.

## 7. Riesgos

- Destinos duplicados o con coordenadas inválidas prevenidos mediante validadores FluentValidation.

