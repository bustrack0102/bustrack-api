# Plan técnico — 36-endpoint-ml-inferencia — Endpoint de inferencia de telemetría y calidad vial (ML)

> Spec: `spec.md` · Endpoint: `POST /api/v1/telemetria/inferencia` · Estado: Propuesto.

## 1. Estrategia

Diseñar un cliente HTTP tipado (`IMlServiceClient`) con política de reintentos y circuit breaker mediante Polly, encapsulado detrás de `TelemetriaController`.

## 2. Diseño

- Stack: ASP.NET Core .NET 10, HttpClientFactory, Polly.
- Componentes: `TelemetriaController`, `IMlServiceClient`, `LoteTelemetriaRequestDto`, `InferenciaResponseDto`.
- Servicio destino: Microservicio interno FastAPI `POST /inference` en red privada.

## 3. Archivos

```text
API_SDD/Controllers/V1/TelemetriaController.cs
API_SDD/Dtos/Ml/LoteTelemetriaRequestDto.cs
API_SDD/Dtos/Ml/InferenciaResponseDto.cs
API_SDD/Services/Ml/IMlServiceClient.cs
API_SDD/Services/Ml/MlServiceClient.cs
```

## 4. Seguridad

- ASVS L2, autenticación interna de servicio a servicio mediante token Bearer interno o Managed Identity.
- No almacena datos en texto claro ni correlaciona con la identidad de la persona usuaria.

## 5. Errores

- Retorno de RFC 7807 uniforme para 502 Bad Gateway y 504 Gateway Timeout.

## 6. Pruebas

- Pruebas unitarias de validación del lote de acelerometría.
- Pruebas con simulación de fallback y timeout de HttpClient.

## 7. Riesgos

- Sobrecarga o latencia del modelo ML gestionada con timeout de 2000 ms y circuit breaker.

