# plan.md — NV-06 Logout

> Deriva de `specs/nv-06/spec.md`. Respeta §4 (pipeline fijo) y §3 (ninguna librería nueva).

## 1. Contexto

Spec origen: `specs/nv-06/spec.md`. Depende de `nv-05` (A1: `jti`+`exp` garantizados). No toca `nv-04`. Reutiliza `IMemoryCache` + `TimeProvider` ya registrados y `OnTokenValidated` existente.

## 2. Diseño técnico

- **Abstracción** `Security/ITokenRevocationService.cs + TokenRevocationService.cs` (singleton, `IMemoryCache` + `TimeProvider` inyectados, sin `new` clave, sin estáticos): `RevokeAsync(jti, exp)` (key `blacklist:{jti}`, `AbsoluteExpiration = exp-now`; no-op si `exp <= now`) + `IsRevokedAsync(jti)`. Constante de prefijo compartida con el formato leído en `ServiceCollectionExtensions.cs:174`.
- **Controller**: añade a `Controllers/AuthController.cs` (creado en nv-05) la acción `POST logout [Authorize][EnableRateLimiting("Global")]` → lee `jti` (`FindFirst("jti")`) + `exp` (`exp` unix → `DateTimeOffset.FromUnixTimeSeconds`), llama al servicio → `204`. Sin DTOs/validators, sin `try/catch` (`401` lo emite Auth).
- **DI**: `AddSingleton<ITokenRevocationService, TokenRevocationService>`; sin reordenar `AddJwt/RateLimiter/Cors/Hsts`; `Program.cs` intacto. Test estructural `toda action con policy` (`04-auth-matrix`): `logout` lleva `[Authorize]` explícito.

## 3. Pipeline orden

Sin cambios: `… → CORS 1 origen → RateLimiter antes Auth → Auth → AuthZ → Controllers`. `critic B3` por índice. `B4` exige +1 fila `/api/v1/auth/logout` en `docs/endpoints.md`.

## 4. Riesgos / Deuda

- TTL remanente vs "120s" literal de §2 → mitigación: decisión + test empírico en `Memoria.md`; revisor puede exigir enmienda si lo considera normativo.
- Blacklist en memoria se pierde al reciclar (deuda §2, sin Redis por alcance).
- Doble logout da `401`, no `204` idempotente a nivel HTTP → documentado como comportamiento del pipeline, no del servicio (a nivel servicio `RevokeAsync` sí es idempotente).
- No revoca refresh (no existe aún) → reservado `nv-07`, sin código anticipado.

## 5. Verificación

`build -c Release --no-restore 0/0 → 5 suites --no-build 100% → critic PASS (B2 sin token en logs) → endpoints OK → sdd-lint → coverage ≥45% → delta advisory → stryker advisory + build posterior → semgrep 0 → publish + humo login/logout/reúso-401 + headers`.
