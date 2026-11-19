using HotelReservation.Application.DTOs;
using HotelReservation.Application.UseCases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelReservation.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClientsController : ControllerBase
{
    private readonly CreerClientUseCase _creerClientUseCase;
    private readonly ConsulterClientUseCase _consulterClientUseCase;
    private readonly ConsulterListeClientsUseCase _listeUseCase;
    private readonly RechercherClientUseCase _rechercherUseCase;
    private readonly ModifierClientUseCase _modifierUseCase;
    private readonly SupprimerClientUseCase _supprimerUseCase;

    public ClientsController(
        CreerClientUseCase creerClientUseCase,
        ConsulterClientUseCase consulterClientUseCase,
        ConsulterListeClientsUseCase listeUseCase,
        RechercherClientUseCase rechercherUseCase,
        ModifierClientUseCase modifierUseCase,
        SupprimerClientUseCase supprimerUseCase)
    {
        _creerClientUseCase = creerClientUseCase;
        _consulterClientUseCase = consulterClientUseCase;
        _listeUseCase = listeUseCase;
        _rechercherUseCase = rechercherUseCase;
        _modifierUseCase = modifierUseCase;
        _supprimerUseCase = supprimerUseCase;
    }

    // POST /api/clients — Administrateur ou Agent
    [HttpPost]
    [Authorize(Roles = "Administrateur,Agent")]
    public async Task<ActionResult<ClientDto>> Create([FromBody] CreerClientRequest request)
    {
        var result = await _creerClientUseCase.ExecuteAsync(request);
        return Ok(result);
    }

    // GET /api/clients/{id} — Administrateur ou Agent
    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Administrateur,Agent")]
    public async Task<ActionResult<ClientDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var dto = await _consulterClientUseCase.ExecuteAsync(id, cancellationToken);
        return Ok(dto);
    }

    [HttpGet]
    [Authorize(Roles = "Administrateur,Agent")]
    public async Task<ActionResult<IEnumerable<ClientDto>>> GetAll()
    {
        return Ok(await _listeUseCase.ExecuteAsync());
    }

    [HttpGet("recherche")]
    [Authorize(Roles = "Administrateur,Agent")]
    public async Task<ActionResult<IEnumerable<ClientDto>>> Search([FromQuery] string? terme)
    {
        return Ok(await _rechercherUseCase.ExecuteAsync(new RechercherClientRequest(terme)));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Administrateur,Agent")]
    public async Task<ActionResult<ClientDto>> Update(Guid id, [FromBody] ModifierClientRequest request)
    {
        return Ok(await _modifierUseCase.ExecuteAsync(id, request));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Administrateur")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _supprimerUseCase.ExecuteAsync(id);
        return NoContent();
    }
}
