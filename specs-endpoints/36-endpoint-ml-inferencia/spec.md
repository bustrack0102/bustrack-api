# 36-endpoint-ml-inferencia — Endpoint de inferencia de telemetría y calidad vial (ML)

> Repositorio: BusTrack API · Endpoint: `POST /api/v1/telemetria/inferencia` · Estado: Propuesto · Fecha: 2026-11-05.

## 1. Objetivo

Recibir lotes de datos sensoriales (acelerómetro inercial y giroscopio) enviados de forma periódica y consentida por la aplicación móvil (Flutter), validar la integridad y anonimato del lote, y orquestar la llamada autenticada al microservicio interno de Machine Learning (FastAPI) para detectar baches, vibraciones críticas o anomalías en la vía pública.

## 2. Contrato

| Campo | Valor |
|---|---|
| Método | `POST` |
| Ruta | `/api/v1/telemetria/inferencia` |
| Autenticación | JWT de usuario autenticado o token anónimo de dispositivo con consentimiento explícito |
| Consumidor | Flutter (Cliente móvil) |
| Estado | Propuesto |

**Parámetros / entrada:**

- Header: `Authorization: Bearer <token>`
- Body JSON con identificador de sesión de viaje, lote de muestras inerciales en ventana de tiempo fija:

```json
{
  "viajeId": "8a1b2c3d-4e5f-6a7b-8c9d-0e1f2a3b4c5d",
  "rutaId": "7ee293a8-9b5b-89a6-f73a-603a73a9e0e1",
  "intervaloMuestreoMs": 50,
  "muestras": [
    {
      "timestampRelativoMs": 0,
      "accelX": 0.05,
      "accelY": 9.81,
      "accelZ": 0.12
    },
    {
      "timestampRelativoMs": 50,
      "accelX": 0.15,
      "accelY": 12.45,
      "accelZ": 0.88
    }
  ]
}
```

**Respuesta:**

- 200: Inferencia calculada exitosamente por el modelo ML.
- 400: Tamaño de lote excedido o datos fuera de rango físico admisible.
- 422: Estructura de muestras inconsistente.
- 502/504: Microservicio interno de ML no disponible o timeout superado.

```json
{
  "inferenciaId": "f1e2d3c4-b5a6-9788-7654-3210fedcba98",
  "clasificacion": "BACHE_SEVERO",
  "confianza": 0.94,
  "severidad": 3,
  "procesadoEnMs": 24
}
```

## 3. Códigos HTTP

- 200: Éxito en inferencia.
- 400: Lote vacío o superior a 500 muestras por envío.
- 401: Token ausente o revocado.
- 422: Formato de aceleración corrupto o no numérico.
- 429: Rate limit de telemetría excedido.
- 502: Error al comunicarse con el microservicio ML interno.
- 504: Timeout en la respuesta del modelo de ML.

## 4. Validaciones

1. Consentimiento explícito del usuario obligatorio antes de la captura sensorial (Constitución §8).
2. Sin rastreo continuo: el lote representa solo ventanas discretas de movimiento sin GPS continuo del usuario.
3. Máximo 500 muestras por petición para evitar sobrecarga del motor de inferencia.

## 5. Seguridad

- ASVS L2, comunicación con el microservicio interno ML autenticada mediante mTLS o Bearer token de servicio a servicio.
- Anonimización absoluta de telemetría: se desvinculan datos personales o PII del vector de inferencia.

## 6. Criterios de aceptación

- [ ] La API valida el lote y delega la predicción al microservicio HTTP de Machine Learning.
- [ ] No permite acceso directo de clientes móviles al servicio ML (Docs Contratos §1).
- [ ] Responde 502/504 controlado si el servicio de ML no responde en menos de 2000 ms.

## 7. Dependencias

- `constitution.md` §3 y §8
- `docs/contratos-integracion.md` §3

