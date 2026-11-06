# Plan técnico — 26-endpoint-documentacion-openapi — Endpoints de documentación OpenAPI y Scalar

> Spec: `spec.md` · Endpoint: `GET /openapi/v1.json` y `GET /scalar/v1` · Estado: Implementado.

## 1. Estrategia

Configurar la generación de OpenAPI con Microsoft.AspNetCore.OpenApi y Scalar.AspNetCore en el pipeline de hosting, condicionada estrictamente a `IsDevelopment()`.

## 2. Diseño

- Stack: ASP.NET Core .NET 10, Scalar OpenAPI UI.
- Extensiones de hosting en `API_SDD/Hosting/`.

## 3. Archivos

```text
API_SDD/Hosting/OpenApiExtensions.cs
API_SDD/Program.cs
```

## 4. Seguridad

- Restricción condicional a desarrollo.
- Headers CSP adaptados para la interfaz web de Scalar únicamente en dev.

## 5. Errores

- Retorno de 404 fuera de desarrollo.

## 6. Pruebas

- Prueba de integración comprobando que `/openapi/v1.json` responde 200 en dev y 404 en producción.

## 7. Riesgos

- Exposición inadvertida de metadatos de rutas en ambientes productivos evitada con condicional de entorno en Program.cs.

