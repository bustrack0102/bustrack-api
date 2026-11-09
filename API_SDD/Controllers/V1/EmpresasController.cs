// ---------------------------------------------------------------------------
// Controlador público para los endpoints de Empresas (listado, autocompletar, detalle).
// ---------------------------------------------------------------------------
using ApiSDD.Dtos.Empresa;
using ApiSDD.Services.Empresa;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiSDD.Controllers.V1;

[ApiController]
[Route("api/v1/empresas")]
public sealed class EmpresasController : ControllerBase
{
    private readonly IEmpresaService _service;

    public EmpresasController(IEmpresaService service) => _service = service;

    // GET /api/v1/empresas
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> List([FromQuery] EmpresaQueryDto query)
    {
        var result = await _service.SearchAsync(query).ConfigureAwait(false);
        return Ok(result);
    }

    // GET /api/v1/empresas/autocompletar
    [HttpGet("autocompletar")]
    [AllowAnonymous]
    public async Task<IActionResult> Autocomplete([FromQuery] string q, [FromQuery] int limite = 10)
    {
        var result = await _service.AutocompleteAsync(q, limite).ConfigureAwait(false);
        return Ok(result);
    }

    // GET /api/v1/empresas/{id}
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> Detail(string id)
    {
        var result = await _service.GetDetailAsync(id).ConfigureAwait(false);
        if (result == null) return NotFound();
        return Ok(result);
    }
}

