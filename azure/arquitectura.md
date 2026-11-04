# Detalle de arquitectura Azure — BusTrack API

## Red y exposición

- API: HTTPS público para clientes; CORS restringido a orígenes aprobados.
- ML/GenAI: solo accesibles desde la API (red interna o token de servicio).
- Cosmos DB: sin exposición pública; acceso por identidad administrada.
- Key Vault: acceso por identidad administrada.

## Persistencia

- API: base de datos NoSQL con contenedores por agregado, clave de partición documentada y ETag para concurrencia.
- Migración desde SQL Server pendiente; ver `../specs/12-migracion-de-persistencia-a-cosmos-db/`.

## Observabilidad

- Correlation ID extremo a extremo.
- Métricas de latencia, error y saturación.
- Alertas sin PII.

## Seguridad

- TLS extremo a extremo.
- Secretos rotables.
- Sin credenciales en imágenes ni en el repositorio.
- Guardrails y límites en GenAI; validación Pydantic en ML.

## Pendientes

- Provisionar recursos (requiere autorización).
- Definir SKU y presupuesto.
- Configurar despliegue CI/CD.
- Aprobar estrategia de sesión Angular.
