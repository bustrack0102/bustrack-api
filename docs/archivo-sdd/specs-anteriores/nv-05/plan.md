# plan.md — NV-05 Login + JWT

> Deriva de `specs/nv-05/spec.md`. Respeta constitución §4 (pipeline fijo) y §3 (evaluar librerías; aquí ninguna nueva: hasher nv-04 + `System.IdentityModel.Tokens.Jwt` ya transitivo).

## 1. Contexto

Spec origen: `specs/nv-05/spec.md`. Depende de `nv-04` Borrador (entidad + hasher). Define `AdminPolicy` que `nv-04` consume. Usa `RateLimiting Login` y `Jwt` ya configurados en `Hosting/ServiceCollectionExtensions.cs:53-184` y `appsettings.Example.json:13-32`.

## 2. Diseño técnico

- **DTOs** `Dtos/LoginDtos.cs`: `LoginRequestDto{strNombreUsuario, strPWD}` + `LoginResponseDto{strToken, dteExpira}`. Cero `strPWD` en respuesta.
- **Validator** `Validators/LoginRequestValidator.cs` (auto-registro nv-02): constantes de `Security/PasswordPolicy.cs` compartidas con nv-04 (regex, 8–10) para evitar drift.
- **Servicio** `Services/IAuthService.cs + AuthService.cs` (scoped, `TimeProvider` inyectado, sin `new` clave): `LoginAsync(req)` implementa §5 del spec (fake-hash, lockout, re-hash). `ISegBloqueoStore` sobre `SDDSegBloqueo` con 1 reintento `DbUpdateConcurrency` fail-closed.
- **Emisor** `Security/JwtTokenService.cs` (`IJwtTokenService`, singleton, `IOptionsMonitor<JwtOptions>` lazy) + `Security/JwtOptions.cs` (`Key/Issuer/Audience/AccessTokenMinutes`): `CreateAccessToken(id, strRol)` → `sub/jti/role`, `expires = TimeProvider + AccessTokenMinutes`.
- **Controller** `Controllers/AuthController.cs`: `[ApiVersion("1.0")][Route("api/v{version:apiVersion}/[controller]")]`, `POST login [AllowAnonymous][EnableRateLimiting("Login")]` → `200`, sin `try/catch` (`401/422` vía excepciones de dominio + middleware).
- **AuthZ**: `AddAuthorization(o => o.AddPolicy("AdminPolicy", p => p.RequireRole("Admin")))` + `AddSingleton(TimeProvider.System)` (requerido por `AuthService`; si ya existe, reutilizar) sin tocar `AddJwt/Cors/RateLimiter/Hsts`; `Program.cs` intacto.
- **Migraciones**: `AddRolSDDUsuario` + `AddSegBloqueo` (sin `_`); `DbSet<SDDSegBloqueo>` + Fluent API; seed idempotente no crea admins con secretos (solo si `Jwt:Key` y admin inicial vienen por env documentado).

## 3. Pipeline orden

Sin cambios: `SecurityHeaders outermost → … → CORS 1 origen → RateLimiter antes Auth → Auth → AuthZ → Controllers` (`Program.cs`). `critic B3` por índice. `B4` exige +1 fila `/api/v1/auth/login` en `docs/endpoints.md`.

## 4. Riesgos / Deuda

- `nv-04` sin M1/M2 no pasa este slice → bloquear T1 hasta migrar (trazabilidad nv-04 ↔ nv-05).
- `IMemoryCache` blacklist (TTL = vida remanente, `nv-06`) se pierde al reciclar (1-instancia, sin Redis; documentar en `Memoria.md`).
- Fake-hash y `NeedsRehash` añaden tiempo CPU Argon2id bajo rate `Login`; medido empíricamente antes de ajustar expiración (principio §2.5).
- `refresh/2FA` fuera: no introducir `ReplacedBy`/TOTP aquí para no abrir superficie sin spec (`nv-07`).

## 5. Verificación

`build -c Release --no-restore 0/0 → 5 suites --no-build 100% → critic PASS → endpoints OK → sdd-lint → coverage ≥45% → delta advisory 80% → stryker advisory + build posterior → semgrep 0 → publish + curl /health/ready + login humo + 401 idéntico + headers`.
