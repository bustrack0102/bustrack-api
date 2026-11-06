# Plan técnico — 30-endpoint-medios-transporte — Endpoint de medios y modalidades de transporte

> Spec: `spec.md` · Endpoint: `GET /api/v1/medios-transporte` · Estado: Propuesto.

## 1. Estrategia

Servir el catálogo de modalidades de transporte con soporte de caché en memoria y ETag para optimizar las peticiones frecuentes de las aplicaciones cliente.

## 2. Diseño

- Stack objetivo: ASP.NET Core .NET 10.
- Componentes: `MediosTransporteController`, `IMedioTransporteService`, `MedioTransporteDto`.

## 3. Archivos

```text
API_SDD/Controllers/V1/MediosTransporteController.cs
API_SDD/Dtos/Transporte/MedioTransporteDto.cs
API_SDD/Services/Transporte/IMedioTransporteService.cs
specs/07-modelo-de-dominio-de-transporte/
```

## 4. Seguridad

- Endpoint público con rate limiting.
- Cero parámetros de entrada vulnerables.

## 5. Errores

- Retorno de RFC 7807 en fallos de ejecución.

## 6. Pruebas

- Pruebas unitarias de serialización del DTO.
- Pruebas de integración validando respuesta 200 y headers de caché.

## 7. Riesgos

- Modificación no coordinada de códigos que rompa mapeos en Flutter o Angular.

