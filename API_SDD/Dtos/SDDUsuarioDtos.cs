// SDD mirrors the mandated entity name on the wire; Sonar S101 PascalCase cannot apply.
#pragma warning disable S101

namespace ApiSDD.Dtos;

/// <summary>
/// nv-04: user creation payload. Plain secret lives only at the boundary.
/// </summary>
public sealed record SDDUsuarioCreateDto(
    string strNombreUsuario,
    string strPWD,
    string strCorreoElectronico,
    string? strRol);

/// <summary>
/// nv-04: user patch payload. RowVersion travels as base64; the stored
/// date is immutable and any value sent for it is ignored.
/// </summary>
public sealed record SDDUsuarioUpdateDto(
    string? strNombreUsuario,
    string? strPWD,
    string? strCorreoElectronico,
    string? strRol,
    string RowVersion);

/// <summary>
/// nv-04: user read model. Never carries hashes or plain secrets.
/// </summary>
public sealed record SDDUsuarioReadDto(
    int id,
    string strNombreUsuario,
    string strCorreoElectronico,
    string strRol,
    DateTime? dteFechaRegistro,
    string RowVersion);
