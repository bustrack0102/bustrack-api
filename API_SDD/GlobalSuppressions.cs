using System.Diagnostics.CodeAnalysis;

// Mínimo privilegio (constitución §2.2): la superficie es internal + InternalsVisibleTo.
// Solo lo exigido por el framework (controllers/OpenAPI, Program/WAF, mapped
// exceptions per S3871) queda público, cada caso documentado.
// Scope "namespace" no aplica en este toolchain: supresiones por tipo.
[assembly: SuppressMessage(
    "Maintainability",
    "CA1515:Consider making public types internal",
    Scope = "type",
    Target = "~T:ApiSDD.Common.HttpErrorException",
    Justification = "Public exceptions are catchable cross-assembly by design (S3871); mapped, never remoted.")]
[assembly: SuppressMessage(
    "Maintainability",
    "CA1515:Consider making public types internal",
    Scope = "type",
    Target = "~T:ApiSDD.Common.NotFoundException",
    Justification = "Public mapped exception, constructed by tests and future slices.")]
[assembly: SuppressMessage(
    "Maintainability",
    "CA1515:Consider making public types internal",
    Scope = "type",
    Target = "~T:ApiSDD.Common.ForbiddenException",
    Justification = "Public mapped exception, constructed by tests and future slices.")]
[assembly: SuppressMessage(
    "Maintainability",
    "CA1515:Consider making public types internal",
    Scope = "type",
    Target = "~T:ApiSDD.Common.ConflictException",
    Justification = "Public mapped exception, constructed by tests and future slices.")]
[assembly: SuppressMessage(
    "Maintainability",
    "CA1515:Consider making public types internal",
    Scope = "type",
    Target = "~T:ApiSDD.Common.RequestValidationException",
    Justification = "Public mapped exception, constructed by tests and future slices.")]
[assembly: SuppressMessage(
    "Design",
    "CA1032:Implement standard exception constructors",
    Scope = "type",
    Target = "~T:ApiSDD.Common.HttpErrorException",
    Justification = "Abstract base never constructed directly; concrete types carry the full standard set.")]
[assembly: SuppressMessage(
    "Maintainability",
    "CA1515:Consider making public types internal",
    Scope = "type",
    Target = "~T:ApiSDD.Controllers.V1.PingController",
    Justification = "MVC discovers only public controllers.")]
[assembly: SuppressMessage(
    "Maintainability",
    "CA1515:Consider making public types internal",
    Scope = "type",
    Target = "~T:ApiSDD.Controllers.V1.PingResponse",
    Justification = "DTO in a public ActionResult + OpenAPI schema; wire is legacy via JsonPropertyName.")]
[assembly: SuppressMessage(
    "Maintainability",
    "CA1515:Consider making public types internal",
    Scope = "type",
    Target = "~T:ApiSDD.Controllers.V1.ProbeController",
    Justification = "MVC discovers only public controllers.")]
[assembly: SuppressMessage(
    "Maintainability",
    "CA1515:Consider making public types internal",
    Scope = "type",
    Target = "~T:ApiSDD.Data.AppDbContext",
    Justification = "Resolved cross-assembly by DatabaseTest + dotnet-ef design time.")]
[assembly: SuppressMessage(
    "Maintainability",
    "CA1515:Consider making public types internal",
    Scope = "type",
    Target = "~T:ApiSDD.Data.DesignTimeDbContextFactory",
    Justification = "dotnet-ef discovers the design-time factory cross-assembly.")]
[assembly: SuppressMessage(
    "Maintainability",
    "CA1515:Consider making public types internal",
    Scope = "type",
    Target = "~T:ApiSDD.Data.Entities.SDDUsuario",
    Justification = "Exposed via public DbSet for EF + DatabaseTest model assertions.")]
[assembly: SuppressMessage(
    "Maintainability",
    "CA1515:Consider making public types internal",
    Scope = "type",
    Target = "~T:ApiSDD.Data.Entities.SDDSegBloqueo",
    Justification = "Exposed via public DbSet for EF + lockout persistence; 1-instance stateless via SQL.")]
[assembly: SuppressMessage(
    "Maintainability",
    "CA1515:Consider making public types internal",
    Scope = "type",
    Target = "~T:ApiSDD.Data.Migrations.AddInitialSDDUsuario",
    Justification = "EF discovers public migrations at Migrate() time.")]
[assembly: SuppressMessage(
    "Maintainability",
    "CA1515:Consider making public types internal",
    Scope = "type",
    Target = "~T:ApiSDD.Data.Migrations.AddSegBloqueo",
    Justification = "EF discovers public migrations at Migrate() time.")]
[assembly: SuppressMessage(
    "Maintainability",
    "CA1515:Consider making public types internal",
    Scope = "type",
    Target = "~T:ApiSDD.Data.Migrations.AppDbContextModelSnapshot",
    Justification = "EF design-time snapshot must stay public.")]
[assembly: SuppressMessage(
    "Maintainability",
    "CA1515:Consider making public types internal",
    Scope = "type",
    Target = "~T:ApiSDD.Dtos.SDDUsuarioCreateDto",
    Justification = "Public controller signature + OpenAPI schema; wire stays legacy.")]
[assembly: SuppressMessage(
    "Maintainability",
    "CA1515:Consider making public types internal",
    Scope = "type",
    Target = "~T:ApiSDD.Dtos.SDDUsuarioUpdateDto",
    Justification = "Public controller signature + OpenAPI schema; wire stays legacy.")]
[assembly: SuppressMessage(
    "Maintainability",
    "CA1515:Consider making public types internal",
    Scope = "type",
    Target = "~T:ApiSDD.Dtos.SDDUsuarioReadDto",
    Justification = "Public controller signature + OpenAPI schema; never carries secrets.")]
[assembly: SuppressMessage(
    "Maintainability",
    "CA1515:Consider making public types internal",
    Scope = "type",
    Target = "~T:ApiSDD.Dtos.LoginRequestDto",
    Justification = "Public controller signature + OpenAPI schema; plain secret at boundary only.")]
[assembly: SuppressMessage(
    "Maintainability",
    "CA1515:Consider making public types internal",
    Scope = "type",
    Target = "~T:ApiSDD.Dtos.LoginResponseDto",
    Justification = "Public controller signature + OpenAPI schema; carries only the issued token.")]
[assembly: SuppressMessage(
    "Maintainability",
    "CA1515:Consider making public types internal",
    Scope = "type",
    Target = "~T:ApiSDD.Services.IAuthService",
    Justification = "Constructor parameter of the public controller; resolved via DI.")]
[assembly: SuppressMessage(
    "Maintainability",
    "CA1515:Consider making public types internal",
    Scope = "type",
    Target = "~T:ApiSDD.Security.ITokenRevocationService",
    Justification = "Constructor parameter of the public controller; resolved via DI.")]
[assembly: SuppressMessage(
    "Maintainability",
    "CA1515:Consider making public types internal",
    Scope = "type",
    Target = "~T:ApiSDD.Controllers.V1.AuthController",
    Justification = "MVC discovers only public controllers.")]
[assembly: SuppressMessage(
    "Maintainability",
    "CA1515:Consider making public types internal",
    Scope = "type",
    Target = "~T:ApiSDD.Common.UnauthorizedException",
    Justification = "Public mapped exception, constructed by the login slice.")]
[assembly: SuppressMessage(
    "Maintainability",
    "CA1515:Consider making public types internal",
    Scope = "type",
    Target = "~T:ApiSDD.Services.ISDDUsuarioService",
    Justification = "Constructor parameter of the public controller; resolved via DI.")]
[assembly: SuppressMessage(
    "Maintainability",
    "CA1515:Consider making public types internal",
    Scope = "type",
    Target = "~T:ApiSDD.Controllers.V1.SDDUsuariosController",
    Justification = "MVC discovers only public controllers.")]
[assembly: SuppressMessage(
    "Maintainability",
    "CA1515:Consider making public types internal",
    Scope = "type",
    Target = "~T:ApiSDD.Common.PagedResult`1",
    Justification = "Uniform paging contract across public controller signatures.")]
[assembly: SuppressMessage(
    "Maintainability",
    "CA1515:Consider making public types internal",
    Scope = "type",
    Target = "~T:ApiSDD.Dtos.PaginationQuery",
    Justification = "Public service and controller paging parameter.")]
[assembly: SuppressMessage(
    "Performance",
    "CA1819:Properties should not return arrays",
    Scope = "member",
    Target = "~P:ApiSDD.Data.Entities.SDDUsuario.RowVersion",
    Justification = "EF IsRowVersion concurrency tokens require byte[].")]
[assembly: SuppressMessage(
    "Performance",
    "CA1819:Properties should not return arrays",
    Scope = "member",
    Target = "~P:ApiSDD.Data.Entities.SDDSegBloqueo.RowVersion",
    Justification = "EF IsRowVersion concurrency tokens require byte[].")]
[assembly: SuppressMessage(
    "Globalization",
    "CA1308:Normalize strings to uppercase",
    Scope = "member",
    Target = "~M:ApiSDD.Services.SDDUsuarioService.CreateAsync(ApiSDD.Dtos.SDDUsuarioCreateDto,System.Boolean,System.Threading.CancellationToken)",
    Justification = "Spec mandates lowercase email normalization at the boundary.")]
[assembly: SuppressMessage(
    "Globalization",
    "CA1308:Normalize strings to uppercase",
    Scope = "member",
    Target = "~M:ApiSDD.Services.SDDUsuarioService.UpdateAsync(System.Int32,ApiSDD.Dtos.SDDUsuarioUpdateDto,System.Boolean,System.Threading.CancellationToken)",
    Justification = "Spec mandates lowercase email normalization at the boundary.")]
[assembly: SuppressMessage(
    "Globalization",
    "CA1308:Normalize strings to uppercase",
    Scope = "member",
    Target = "~M:ApiSDD.Services.SDDUsuarioService.ExistsUserNameAsync(System.String,System.Nullable{System.Int32},System.Threading.CancellationToken)",
    Justification = "Case-insensitive duplicate check pairs with the unique index; storage keeps typed case.")]
[assembly: SuppressMessage(
    "Globalization",
    "CA1304:Specify CultureInfo",
    Scope = "member",
    Target = "~M:ApiSDD.Services.SDDUsuarioService.ExistsUserNameAsync(System.String,System.Nullable{System.Int32},System.Threading.CancellationToken)",
    Justification = "Parameterless ToLower is the EF-translatable form of the database LOWER check.")]
[assembly: SuppressMessage(
    "Globalization",
    "CA1311:Specify a culture or use an invariant version",
    Scope = "member",
    Target = "~M:ApiSDD.Services.SDDUsuarioService.ExistsUserNameAsync(System.String,System.Nullable{System.Int32},System.Threading.CancellationToken)",
    Justification = "Parameterless ToLower is the EF-translatable form of the database LOWER check.")]
[assembly: SuppressMessage(
    "Performance",
    "CA1862:Prefer string.Equals with StringComparison",
    Scope = "member",
    Target = "~M:ApiSDD.Services.SDDUsuarioService.ExistsUserNameAsync(System.String,System.Nullable{System.Int32},System.Threading.CancellationToken)",
    Justification = "OrdinalIgnoreCase Equals is not SQL-translatable; LOWER comparison is.")]
[assembly: SuppressMessage(
    "Maintainability",
    "CA1515:Consider making public types internal",
    Scope = "type",
    Target = "~T:Program",
    Justification = "WebApplicationFactory<T> requires an accessible entry-point type.")]
[assembly: SuppressMessage(
    "Design",
    "CA1031:Do not catch general exception types",
    Scope = "type",
    Target = "~T:ApiSDD.Health.SqlServerHealthCheck",
    Justification = "A health check must translate any failure into Unhealthy (fail-fast reporting); no exception escapes.")]
[assembly: SuppressMessage(
    "Design",
    "CA1031:Do not catch general exception types",
    Scope = "type",
    Target = "~T:ApiSDD.Middleware.ExceptionHandlingMiddleware",
    Justification = "Last-resort boundary: every unhandled exception becomes a uniform ErrorResponse; rethrows when the response already started.")]
[assembly: SuppressMessage(
    "Security",
    "S5332:Using http protocol is insecure",
    Scope = "member",
    Target = "~M:Program.<Main>$(System.String[])",
    Justification = "PORT env listen address for containers/dev; TLS terminates at the proxy, HSTS + prod-only HttpsRedirection enforced in pipeline.")]
