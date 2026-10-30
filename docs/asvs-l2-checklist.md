# ASVS L2 Checklist — vivo

> Constitución §6. Trazable a NV-03.

- [ ] V2 Auth: JWT HS256 `key≥32B env`, `ClockSkew=Zero`, `ValidAlgorithms=[HS256]`, claims `sub/jti/role/2fa_temp`
- [ ] V2 Auth: refresh rotación (nv-07) + blacklist `{jti}` (nv-06) + `OnTokenValidated`
- [ ] V2 2FA: TOTP Otp.NET real, attempts IMemoryCache TTL 120s, lockout DB 5→15min fail-closed
- [ ] V3 Sesión: stateless 1-instancia, sin sesión en proceso
- [ ] V4 Acceso: denegar por defecto, mínimo privilegio, 403/404 correctos
- [ ] V5 Validación: FluentValidation en frontera, sin secretos en código/logs/errores
- [ ] V7 Logs: Serilog JSON + CorrelationId + TraceId, sin PII, filtro secretos
- [ ] V9 Comms: HSTS 365d no-Dev, HTTPS redirect, CORS 1 origen, CSP nonce 16B/req, nosniff/DENY
- [ ] V10 Código: CA3000+ error, complejidad ≤10/15, `Detail` solo no-prod
- [ ] V14 Config: `DataProtection:KeysPath` fuera wwwroot, Kestrel 10MB/2min, timeout 60s
