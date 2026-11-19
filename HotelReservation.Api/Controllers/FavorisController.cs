using HotelReservation.Application.DTOs;
using HotelReservation.Application.UseCases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HotelReservation.Api.Controllers;

[ApiController]
[Route("api/favoris")]
[Authorize(Roles = "Client")]
public class FavorisController : ControllerBase
{
    private readonly AjouterFavoriUseCase _ajouter;
    private readonly SupprimerFavoriUseCase _supprimer;
    private readonly ConsulterFavorisUseCase _consulter;

    public FavorisController(AjouterFavoriUseCase ajouter, SupprimerFavoriUseCase supprimer, ConsulterFavorisUseCase consulter)
    {
        _ajouter = ajouter;
        _supprimer = supprimer;
        _consulter = consulter;
    }

    [HttpPost]
    public async Task<ActionResult<FavoriDto>> Ajouter(AjouterFavoriRequest request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var utilisateurId)) return Unauthorized();
        return Ok(await _ajouter.ExecuteAsync(utilisateurId, request, cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Supprimer(Guid id, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var utilisateurId)) return Unauthorized();
        await _supprimer.ExecuteAsync(utilisateurId, id, cancellationToken);
        return NoContent();
    }

    [HttpGet("client/{clientId:guid}")]
    public async Task<ActionResult<IEnumerable<FavoriDto>>> GetByClient(Guid clientId, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var utilisateurId) || utilisateurId != clientId)
            return Forbid();
        return Ok(await _consulter.ExecuteAsync(clientId, cancellationToken));
    }
}