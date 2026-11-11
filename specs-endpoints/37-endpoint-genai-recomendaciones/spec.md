# 37-endpoint-genai-recomendaciones — Endpoint de recomendaciones y planificación de viajes (GenAI)

> Repositorio: BusTrack API · Endpoint: `POST /api/v1/recomendaciones` · Estado: Propuesto · Fecha: 2026-11-05.

## 1. Objetivo

Brindar recomendaciones de rutas, combinaciones de transporte y respuestas a dudas de movilidad al usuario mediante inteligencia artificial generativa, inyectando estricta y exclusivamente contexto verificado extraído de Cosmos DB para garantizar que el modelo no invente rutas, tarifas, destinos ni horarios.

## 2. Contrato

| Campo | Valor |
|---|---|
| Método | `POST` |
| Ruta | `/api/v1/recomendaciones` |
| Autenticación | JWT de usuario autenticado o anónimo con rate limit estricto |
| Consumidor | Flutter y Angular |
| Estado | Propuesto |

**Parámetros / entrada:**

- Body JSON con origen, destino, preferencias del usuario o consulta textual:

```json
{
  "origen": "Terminal Central Sur",
  "destino": "Plaza España Zona 10",
  "preferencia": "MENOR_TIEMPO",
  "horaSalidaDeseada": "07:30:00",
  "preguntaLibre": "¿Qué ruta me deja más cerca de la embajada?"
}
```

**Respuesta:**

- 200: Recomendación estructurada con rutas reales asociadas y explicación en lenguaje natural.
- 400: Parámetros inválidos.
- 422: Consulta no resoluble o fuera del perímetro de transporte soportado.
- 502/504: Falla de comunicación con el servicio GenAI interno.

```json
{
  "recomendacionId": "1a2b3c4d-5e6f-7a8b-9c0d-1e2f3a4b5c6d",
  "rutasSugeridas": [
    {
      "rutaId": "7ee293a8-9b5b-89a6-f73a-603a73a9e0e1",
      "codigo": "R-101",
      "nombre": "Terminal Sur - Zona 10",
      "tarifaEstimada": 5.00,
      "tiempoEstimadoMinutos": 45,
      "paradaAbordaje": "Parada Central Trébol",
      "paradaDescenso": "Plaza España Sur"
    }
  ],
  "explicacion": "Se recomienda abordar la ruta R-101 en Parada Central Trébol. Cuenta con servicio regular cada 15 minutos en ese horario.",
  "fuenteDatos": "Catálogo Oficial BusTrack API"
}
```

## 3. Códigos HTTP

- 200: Recomendación generada exitosamente.
- 400: Parámetros requeridos ausentes.
- 401: No autorizado (si la política exige sesión activa).
- 429: Rate limit de consultas GenAI excedido.
- 502: Error al conectar con el servicio interno de GenAI.
- 504: Timeout en la generación de respuesta.

## 4. Validaciones

1. Restricción absoluta (Constitución §8): Prohibida la invención de rutas, horarios, tarifas, empresas o destinos por parte de IA.
2. Todo identificador de ruta en la respuesta debe existir en el catálogo de transporte de Cosmos DB.
3. Sanitización de prompts para evitar prompt injection de clientes.

## 5. Seguridad

- ASVS L2, scrubbing de PII en consultas antes de enviar a GenAI.
- Ningún cliente accede directamente a OpenAI API ni al servicio interno de GenAI (Docs Contratos §1).
- Rate limit estricto para mitigar costos de cómputo y consumo de tokens.

## 6. Criterios de aceptación

- [ ] La API recupera primero las rutas candidatas reales desde Cosmos DB y las entrega como contexto al modelo GenAI.
- [ ] No inventa rutas ni destinos inexistentes.
- [ ] Devuelve 502/504 controlado si el servicio de IA no responde a tiempo.

## 7. Dependencias

- `constitution.md` §1, §3 y §8
- `docs/contratos-integracion.md` §3
- `specs/07-modelo-de-dominio-de-transporte/`

## Aprobación

- [x] Revisión de arquitectura y contrato completada — fecha: 2026-11-11
- [x] CHECKLIST_PR.md validado
- [x] Sin conflicto con constitution.md
