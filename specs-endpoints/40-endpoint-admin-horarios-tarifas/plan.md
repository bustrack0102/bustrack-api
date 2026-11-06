# Plan técnico — 40-endpoint-admin-horarios-tarifas — Endpoint de administración de horarios y tarifas de ruta

> Spec: `spec.md` · Endpoint: `POST/PUT/DELETE /api/v1/rutas/{id}/horarios|tarifas` · Estado: Propuesto.

## 1. Estrategia

Diseñar endpoints anidados bajo el recurso ruta en `RutasController` o en controladores especializados `RutaHorariosController` y `RutaTarifasController`, protegiendo las mutaciones con `AdminPolicy` y verificando solapamientos temporales.

## 2. Diseño

- Stack objetivo: ASP.NET Core .NET 10 + Azure Cosmos DB for NoSQL.
- Componentes: `RutaHorariosController`, `RutaTarifasController`, `CrearHorarioDto`, `CrearTarifaDto`.

## 3. Archivos

```text
API_SDD/Controllers/V1/RutaHorariosController.cs
API_SDD/Controllers/V1/RutaTarifasController.cs
API_SDD/Dtos/Transporte/CrearHorarioDto.cs
API_SDD/Dtos/Transporte/CrearTarifaDto.cs
API_SDD/Validators/Transporte/CrearHorarioValidator.cs
API_SDD/Validators/Transporte/CrearTarifaValidator.cs
API_SDD/Services/Transporte/IHorarioTarifaAdminService.cs
```

## 4. Seguridad

- ASVS L2, AdminPolicy.
- Registro en logs de auditoría para cada cambio en tarifas oficiales.

## 5. Errores

- Retorno de 409 ante solapamientos de horario o conflicto de fechas de vigencia.

## 6. Pruebas

- Pruebas unitarias de detección de solapamiento horario.
- Pruebas de integración de CRUD administrativo de horarios y tarifas.

## 7. Riesgos

- Tarifas desactualizadas o inconsistentes resueltas con validación de fechas de vigencia.

