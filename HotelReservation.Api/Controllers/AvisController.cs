using HotelReservation.Application.DTOs;
using HotelReservation.Application.UseCases;
using HotelReservation.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HotelReservation.Api.Controllers;

/// <summary>
/// Contrôleur API exposant la gestion des avis clients sur les hôtels (création, modification, consultation et suppression).
/// Totalement découplé de la persistance : transite exclusivement par les cas d'utilisation applicatifs.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AvisController : ControllerBase
{
    private readonly AjouterAvisUseCase _ajouterAvisUseCase;
    private readonly ModifierAvisUseCase _modifierAvisUseCase;
    private readonly SupprimerAvisUseCase _supprimerAvisUseCase;
    private readonly ConsulterAvisHotelUseCase _consulterAvisHotelUseCase;
    private readonly ConsulterAvisClientUseCase _consulterAvisClientUseCase;
    private readonly ConsulterTousAvisUseCase _consulterTousAvisUseCase;
    private readonly ConsulterAvisParIdUseCase _consulterAvisParIdUseCase;

    public AvisController(
        AjouterAvisUseCase ajouterAvisUseCase,
        ModifierAvisUseCase modifierAvisUseCase,
        SupprimerAvisUseCase supprimerAvisUseCase,
        ConsulterAvisHotelUseCase consulterAvisHotelUseCase,
        ConsulterAvisClientUseCase consulterAvisClientUseCase,
        ConsulterTousAvisUseCase consulterTousAvisUseCase,
        ConsulterAvisParIdUseCase consulterAvisParIdUseCase)
    {
        _ajouterAvisUseCase = ajouterAvisUseCase;
        _modifierAvisUseCase = modifierAvisUseCase;
        _supprimerAvisUseCase = supprimerAvisUseCase;
        _consulterAvisHotelUseCase = consulterAvisHotelUseCase;
        _consulterAvisClientUseCase = consulterAvisClientUseCase;
        _consulterTousAvisUseCase = consulterTousAvisUseCase;
        _consulterAvisParIdUseCase = consulterAvisParIdUseCase;
    }

    /// <summary>Récupère un avis par son identifiant unique.</summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AvisDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _consulterAvisParIdUseCase.ExecuteAsync(id, cancellationToken));
    }

    /// <summary>Crée un nouvel avis client pour un hôtel après un séjour.</summary>
    [HttpPost]
    [Authorize(Roles = "Client")]
    public async Task<ActionResult<AvisDto>> Create([FromBody] AjouterAvisRequest request)
    {
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var clientId))
            return Unauthorized();

        var result = await _ajouterAvisUseCase.ExecuteAsync(clientId, request);
        return Ok(result);
    }

    /// <summary>Modifie un avis existant déposé par le client connecté.</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Client")]
    public async Task<ActionResult<AvisDto>> Update(
        Guid id,
        [FromBody] ModifierAvisRequest request,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var clientId))
            return Unauthorized();

        return Ok(await _modifierAvisUseCase.ExecuteAsync(id, clientId, request, cancellationToken));
    }

    /// <summary>Supprime un avis existant (auteur client ou administrateur).</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Client,Administrateur")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var isAdministrateur = User.IsInRole("Administrateur");
        Guid? userId = null;
        if (Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var parsedId))
            userId = parsedId;

        await _supprimerAvisUseCase.ExecuteAsync(id, userId, isAdministrateur, cancellationToken);
        return NoContent();
    }

    /// <summary>Récupère la liste de tous les avis validés pour un hôtel spécifique.</summary>
    [HttpGet("hotel/{hotelId:guid}")]
    public async Task<ActionResult<IEnumerable<AvisDto>>> GetByHotel(Guid hotelId, CancellationToken cancellationToken)
    {
        return Ok(await _consulterAvisHotelUseCase.ExecuteAsync(hotelId, cancellationToken));
    }

    /// <summary>Récupère tous les avis déposés par un client.</summary>
    [HttpGet("client/{clientId:guid}")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<AvisDto>>> GetByClient(Guid clientId, CancellationToken cancellationToken)
    {
        if (User.IsInRole("Client"))
        {
            if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId) || userId != clientId)
                return Forbid();
        }

        return Ok(await _consulterAvisClientUseCase.ExecuteAsync(clientId, cancellationToken));
    }

    /// <summary>Récupère la liste des avis en attente de modération (Administrateur uniquement).</summary>
    [HttpGet("en-attente")]
    [Authorize(Roles = "Administrateur")]
    public async Task<ActionResult<IEnumerable<AvisDto>>> GetPending(CancellationToken cancellationToken)
    {
        return Ok(await _consulterTousAvisUseCase.ExecuteAsync(StatutAvis.EnAttente, cancellationToken));
    }

    /// <summary>Récupère l'ensemble des avis avec filtre optionnel par statut (Administrateur uniquement).</summary>
    [HttpGet]
    [Authorize(Roles = "Administrateur")]
    public async Task<ActionResult<IEnumerable<AvisDto>>> GetAll([FromQuery] StatutAvis? statut, CancellationToken cancellationToken)
    {
        return Ok(await _consulterTousAvisUseCase.ExecuteAsync(statut, cancellationToken));
    }
}
