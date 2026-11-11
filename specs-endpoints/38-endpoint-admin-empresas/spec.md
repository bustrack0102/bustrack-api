# 38-endpoint-admin-empresas — Endpoint de administración de empresas de transporte

> Repositorio: BusTrack API · Endpoint: `POST/PUT/DELETE /api/v1/empresas` · Estado: Implementado · Fecha: 2026-11-05.

## 1. Objetivo

Permitir a los usuarios administradores del sistema crear, actualizar y dar de baja lógica empresas de transporte autorizadas, con control de concurrencia y validación de políticas administrativas.

## 2. Contrato

| Campo | Valor |
|---|---|
| Método | `POST /api/v1/empresas`, `PUT /api/v1/empresas/{id}`, `DELETE /api/v1/empresas/{id}` |
| Ruta | `/api/v1/empresas` y `/api/v1/empresas/{id}` |
| Autenticación | JWT con claim `role: Admin` (`AdminPolicy`) |
| Consumidor | Angular (Panel de Administración) |
| Estado | Implementado |

**Operaciones:**

- `POST /api/v1/empresas`: Crea una nueva empresa de transporte. Retorna 201 Created con cabecera `Location`.
- `PUT /api/v1/empresas/{id}`: Actualiza los datos de la empresa. Requiere `ETag` / `RowVersion` para control de concurrencia optimista. Retorna 200 OK.
- `DELETE /api/v1/empresas/{id}`: Da de baja lógica a la empresa (`activo = false`). Retorna 204 No Content.

**Body POST:**

```json
{
  "razonSocial": "Transportes Unidos del Norte S.A.",
  "nombreComercial": "TransNorte",
  "nit": "1234567-8",
  "asociacionId": "4bb96075-6828-5673-c40d-3d074077bfb7",
  "telefonoContacto": "+502 2233-4455",
  "correoContacto": "operaciones@transnorte.gt"
}
```

## 3. Códigos HTTP

- 201: Creado con éxito.
- 200: Modificado con éxito.
- 204: Baja lógica ejecutada.
- 400: Datos de entrada inválidos.
- 401: No autenticado.
- 403: No tiene rol de Administrador.
- 404: Empresa no encontrada.
- 409: Conflicto de concurrencia (ETag no coincide) o NIT duplicado.
- 422: Reglas de validación de negocio no superadas.
- 429: Rate limit de escrituras concurrentes.
- 500: Error interno no controlado.

## 4. Validaciones

1. Razón social y NIT obligatorios y únicos en el sistema.
2. `asociacionId` debe existir previamente si se proporciona.
3. Formato de correo y teléfono válidos.
4. Concurrencia optimista en actualizaciones (`ETag` / `_etag` de Cosmos DB o `RowVersion`).

## 5. Seguridad

- ASVS L2, autorización estricta `AdminPolicy`.
- Auditoría en logs de Serilog registrando `userId` del administrador que realiza la mutación (sin PII sensible).

## 6. Criterios de aceptación

- [ ] Administrador autenticado puede crear, modificar y dar de baja una empresa.
- [ ] Peticiones sin token o sin rol Admin son rechazadas con 401 o 403.
- [ ] Concurrencia optimista previene sobrescrituras accidentales con código 409.

## 7. Dependencias

- `constitution.md`
- `specs/04-gestion-de-usuarios/`
- `specs/08-consulta-de-empresas-y-asociaciones/`
- `specs-endpoints/18-endpoint-empresas/`

## Aprobación

- [x] Revisión de arquitectura y contrato completada — fecha: 2026-11-11
- [x] CHECKLIST_PR.md validado
- [x] Sin conflicto con constitution.md
