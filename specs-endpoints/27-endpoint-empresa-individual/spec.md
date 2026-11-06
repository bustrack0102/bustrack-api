# 27-endpoint-empresa-individual — Endpoint de detalle de empresa de transporte

> Repositorio: BusTrack API · Endpoint: `GET /api/v1/empresas/{id}` · Estado: Propuesto · Fecha: 2026-11-05.

## 1. Objetivo

Obtener la información detallada de una empresa de transporte concesionaria o permisionaria específica, incluyendo asociación a la que pertenece, flota registrada y rutas autorizadas.

## 2. Contrato

| Campo | Valor |
|---|---|
| Método | `GET` |
| Ruta | `/api/v1/empresas/{id}` |
| Autenticación | Opcional / Pública o JWT de usuario móvil/web |
| Consumidor | Flutter y Angular |
| Estado | Propuesto |

**Parámetros / entrada:**

- Path: `id` (identificador único GUID de la empresa).

**Respuesta:**

- 200: Objeto JSON con el detalle de la empresa.
- 400: Identificador con formato inválido.
- 404: Empresa inexistente o inactiva.

```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "razonSocial": "Transportes Metropolitanos S.A.",
  "nombreComercial": "TransMetro",
  "asociacionId": "4bb96075-6828-5673-c40d-3d074077bfb7",
  "asociacionNombre": "Asociación de Concesionarios del Norte",
  "telefonoContacto": "+502 2222-3333",
  "correoContacto": "contacto@transmetro.gt",
  "activo": true,
  "cantidadRutas": 12
}
```

## 3. Códigos HTTP

- 200: Consulta exitosa.
- 400: Parámetro `id` con formato inválido.
- 404: Empresa no encontrada.
- 429: Rate limit excedido.
- 500: Error interno no controlado.

## 4. Validaciones

1. `id` debe ser un GUID válido.
2. La empresa debe encontrarse en estado activo para consulta pública.

## 5. Seguridad

- ASVS L2, rate limiting, validación de formato GUID contra inyecciones, cero secretos en Git.
- No se expone información de contacto privada de personas naturales si no está autorizada.

## 6. Criterios de aceptación

- [ ] Devuelve 200 con el DTO detallado de la empresa si existe.
- [ ] Devuelve 404 cuando la empresa no existe.
- [ ] Devuelve 400 cuando el id es inválido.
- [ ] `Memoria.md` refleja el estado resultante.

## 7. Dependencias

- `docs/contratos-integracion.md`
- `specs/08-consulta-de-empresas-y-asociaciones/`
- `specs-endpoints/18-endpoint-empresas/`

