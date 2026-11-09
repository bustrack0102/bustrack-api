// ---------------------------------------------------------------------------
// DTO para autocompletar empresas.
// ---------------------------------------------------------------------------
namespace ApiSDD.Dtos.Empresa;

public sealed class EmpresaAutocompleteDto
{
    public string Id { get; set; } = default!;
    public string Nombre { get; set; } = default!;
    public string? Siglas { get; set; }
    public string MunicipioBase { get; set; } = default!;
    // Se expone como string (no Uri) para no alterar el contrato JSON del cliente.
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Design", "CA1056:URI-returning properties should not be strings",
        Justification = "Contrato JSON estable; el cliente espera string.")]
    public string? LogoUrl { get; set; }
    public double CalificacionPromedio { get; set; }
}

