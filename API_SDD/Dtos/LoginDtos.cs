// Login wire names keep the legacy convention; Sonar S101 PascalCase cannot apply.
#pragma warning disable S101

namespace ApiSDD.Dtos;

/// <summary>
/// nv-05: login request. Plain secret lives only at the boundary, never persisted or logged.
/// </summary>
public sealed record LoginRequestDto(
    string strNombreUsuario,
    string strPWD);

/// <summary>
/// nv-05: login response. Carries the short-lived HS256 token and its expiry.
/// </summary>
public sealed record LoginResponseDto(
    string strToken,
    DateTime dteExpira);
