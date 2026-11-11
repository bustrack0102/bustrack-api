# 40-endpoint-admin-horarios-tarifas — Endpoint de administración de horarios y tarifas de ruta

> Repositorio: BusTrack API · Endpoint: `POST/PUT/DELETE /api/v1/rutas/{id}/horarios` y `POST/PUT/DELETE /api/v1/rutas/{id}/tarifas` · Estado: Propuesto · Fecha: 2026-11-05.

## 1. Objetivo

Permitir a los administradores registrar, actualizar vigencias y eliminar tablas de horarios oficiales (por tipo de día: laborable, sábado, domingo/festivo) y esquemas tarifarios aprobados por la autoridad reguladora para cada ruta de transporte.

## 2. Contrato

| Campo | Valor |
|---|---|
| Método | `POST/PUT/DELETE` |
| Ruta | `/api/v1/rutas/{id}/horarios` y `/api/v1/rutas/{id}/tarifas` |
| Autenticación | JWT con claim `role: Admin` (`AdminPolicy`) |
| Consumidor | Angular (Panel de Administración) |
| Estado | Propuesto |

**Endpoints y operaciones:**

1. **Horarios:**
   - `POST /api/v1/rutas/{id}/horarios`: Agrega un bloque de horario (tipo de día, hora inicio, hora fin, frecuencia en minutos).
   - `DELETE /api/v1/rutas/{id}/horarios/{horarioId}`: Elimina un horario.
2. **Tarifas:**
   - `POST /api/v1/rutas/{id}/tarifas`: Registra una nueva tarifa oficial (tipo usuario: general, estudiante, adulto mayor, monto, fecha inicio vigencia).
   - `PUT /api/v1/rutas/{id}/tarifas/{tarifaId}`: Actualiza la vigencia o estado de una tarifa.

**Body POST Tarifa:**

```json
{
  "tipoUsuario": "GENERAL",
  "monto": 5.00,
  "moneda": "GTQ",
  "fechaInicioVigencia": "2026-11-01T00:00:00Z",
  "fechaFinVigencia": null,
  "activo": true
}
```

**Body POST Horario:**

```json
{
  "tipoDia": "LABORAL",
  "horaInicio": "05:00:00",
  "horaFin": "21:00:00",
  "frecuenciaMinutos": 12
}
```

## 3. Códigos HTTP

- 201: Horario o tarifa registrada.
- 200: Modificación exitosa.
- 204: Eliminación completada.
- 400: Parámetros inválidos o solapamiento de rangos de hora.
- 401: No autenticado.
- 403: Privilegios insuficientes (requiere Admin).
- 404: Ruta, tarifa u horario no encontrado.
- 409: Conflicto de vigencia o solapamiento de horarios.
- 422: Validación de monto o formato de hora fallida.
- 429: Rate limit de escrituras.
- 500: Error interno no controlado.

## 4. Validaciones

1. `monto` de tarifa mayor que cero y moneda autorizada ("GTQ").
2. `horaInicio` menor que `horaFin` y `frecuenciaMinutos > 0`.
3. No permitir solapamiento de horarios para el mismo `tipoDia` en la misma ruta.
4. Concurrencia y vigencias auditadas.

## 5. Seguridad

- ASVS L2, AdminPolicy estricta.
- Cero manipulación de tarifas por usuarios no autorizados (Constitución §8).

## 6. Criterios de aceptación

- [ ] Administrador autenticado puede registrar tarifas y frecuencias de servicio.
- [ ] Rechaza solapamientos de horarios para el mismo día de servicio.
- [ ] Rechaza con 401/403 si la petición no incluye rol de Administrador.

## 7. Dependencias

- `constitution.md`
- `specs/10-consulta-de-horarios-y-tarifas/`
- `specs-endpoints/21-endpoint-horarios/`
- `specs-endpoints/22-endpoint-tarifas/`

## Aprobación

- [x] Revisión de arquitectura y contrato completada — fecha: 2026-11-11
- [x] CHECKLIST_PR.md validado
- [x] Sin conflicto con constitution.md
