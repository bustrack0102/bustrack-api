# Diagramas — 34-endpoint-auth-2fa — Endpoint de autenticación de dos factores (2FA / MFA)

> Spec: `spec.md` · Plan: `plan.md`.

## 1. Flujo del endpoint

```mermaid
flowchart TD
  A[Login exitoso usuario Admin] --> B[Emitir JWT temporal con claim pre-mfa]
  B --> C[POST /api/v1/auth/2fa/verify con código TOTP]
  C --> D{¿Código válido en ventana 30s?}
  D -->|No| E[401 Unauthorized / Sumar intento fallido]
  D -->|Sí| F[Emitir JWT definitivo con claim role: Admin]
  F --> G[200 OK]
```

## 2. Componentes

```mermaid
flowchart LR
  C[Angular Admin / Flutter] --> AC[Auth2FaController]
  AC --> TS[TotpService]
  TS --> DB[(Persistencia Secretos Cifrados)]
```

## 3. Secuencia

```mermaid
sequenceDiagram
  participant C as Cliente Admin
  participant A as Auth2FaController
  participant T as TotpService
  C->>A: POST /api/v1/auth/2fa/verify { "code": "482910" }
  A->>T: ValidarTotp(userId, code)
  T-->>A: Válido
  A-->>C: 200 OK (AccessToken definitivo)
```

## 4. Notas

Diagramas de proceso; no representan recursos desplegados.

