using System.Security.Claims;
using HotelReservation.Application.DTOs;
using HotelReservation.Application.UseCases;
using HotelReservation.Domain.Common;
using HotelReservation.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelReservation.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReservationsController : ControllerBase
{
    private readonly CreerReservationUseCase _creerReservationUseCase;
    private readonly ConfirmerReservationUseCase _confirmerReservationUseCase;
    private readonly AnnulerReservationUseCase _annulerReservationUseCase;
    private readonly ConsulterCalendrierReservationsUseCase _consulterCalendrierUseCase;
    private readonly ModifierReservationUseCase _modifierReservationUseCase;
    private readonly ChangerStatutReservationUseCase _changerStatutUseCase;
    private readonly ConsulterListeReservationsUseCase _consulterListeUseCase;
    private readonly RechercherReservationUseCase _rechercherUseCase;
    private readonly ConsulterMesReservationsUseCase _consulterMesUseCase;
    private readonly ConsulterReservationUseCase _consulterReservationUseCase;

    public ReservationsController(
        CreerReservationUseCase creerReservationUseCase,
        ConfirmerReservationUseCase confirmerReservationUseCase,
        AnnulerReservationUseCase annulerReservationUseCase,
        ConsulterCalendrierReservationsUseCase consulterCalendrierUseCase,
        ModifierReservationUseCase modifierReservationUseCase,
        ChangerStatutReservationUseCase changerStatutUseCase,
        ConsulterListeReservationsUseCase consulterListeUseCase,
        RechercherReservationUseCase rechercherUseCase,
        ConsulterMesReservationsUseCase consulterMesUseCase,
        ConsulterReservationUseCase consulterReservationUseCase)
    {
        _creerReservationUseCase = creerReservationUseCase;
        _confirmerReservationUseCase = confirmerReservationUseCase;
        _annulerReservationUseCase = annulerReservationUseCase;
        _consulterCalendrierUseCase = consulterCalendrierUseCase;
        _modifierReservationUseCase = modifierReservationUseCase;
        _changerStatutUseCase = changerStatutUseCase;
        _consulterListeUseCase = consulterListeUseCase;
        _rechercherUseCase = rechercherUseCase;
        _consulterMesUseCase = consulterMesUseCase;
        _consulterReservationUseCase = consulterReservationUseCase;
    }

    /// <summary>Récupère toutes les réservations (Personnel uniquement).</summary>
    [HttpGet]
    [Authorize(Roles = "Administrateur,Agent")]
    public async Task<ActionResult<IEnumerable<ReservationDto>>> GetAll()
    {
        return Ok(await _consulterListeUseCase.ExecuteAsync());
    }

    /// <summary>Consulter une réservation par son identifiant unique.</summary>
    [HttpGet("{id:guid}")]
    [Authorize]
    public async Task<ActionResult<ReservationDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (!Guid.TryParse(claim, out var userId))
            return Unauthorized();

        var isPersonnel = User.IsInRole("Administrateur") || User.IsInRole("Agent");
        var result = await _consulterReservationUseCase.ExecuteAsync(id, userId, isPersonnel, cancellationToken);
        return Ok(result);
    }

    /// <summary>Récupère la liste des réservations d'un client donné.</summary>
    [HttpGet("client/{clientId:guid}")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<ReservationDto>>> GetByClient(Guid clientId, [FromQuery] StatutReservation? statut = null)
    {
        if (User.IsInRole("Client"))
        {
            var claim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            if (!Guid.TryParse(claim, out var userId) || userId != clientId)
                return Forbid();
        }

        return Ok(await _consulterMesUseCase.ExecuteAsync(clientId, statut));
    }

    /// <summary>Recherche multicritère de réservations.</summary>
    [HttpGet("recherche")]
    [Authorize(Roles = "Administrateur,Agent")]
    public async Task<ActionResult<IEnumerable<ReservationDto>>> Search(
        [FromQuery] Guid? clientId,
        [FromQuery] Guid? hotelId,
        [FromQuery] StatutReservation? statut,
        [FromQuery] DateTime? dateDebut,
        [FromQuery] DateTime? dateFin)
    {
        return Ok(await _rechercherUseCase.ExecuteAsync(
            new RechercherReservationRequest(clientId, hotelId, statut, dateDebut, dateFin)));
    }

    [HttpGet("calendrier")]
    [Authorize(Roles = "Administrateur,Agent")]
    public async Task<ActionResult<CalendrierReservationsResponse>> GetCalendrier(
        [FromQuery] Guid hotelId,
        [FromQuery] DateTime dateDebut,
        [FromQuery] DateTime dateFin,
        CancellationToken cancellationToken)
    {
        return Ok(await _consulterCalendrierUseCase.ExecuteAsync(hotelId, dateDebut, dateFin, cancellationToken));
    }

    /// <summary>Créer une réservation (Client ou Personnel).</summary>
    [HttpPost]
    [Authorize(Roles = "Client,Administrateur,Agent")]
    public async Task<ActionResult<ReservationDto>> Create([FromBody] CreerReservationRequest request)
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (!Guid.TryParse(claim, out var tokenUserId))
            return Unauthorized();

        Guid targetClientId;
        if (User.IsInRole("Client"))
        {
            targetClientId = tokenUserId; // L'identifiant du jeton prévaut toujours pour un Client
        }
        else
        {
            targetClientId = request.ClientId ?? tokenUserId; // Le Personnel peut fournir un clientId
        }

        var result = await _creerReservationUseCase.ExecuteAsync(targetClientId, request);
        return Ok(result);
    }

    /// <summary>Confirmer une réservation (Personnel uniquement).</summary>
    [HttpPost("{id:guid}/confirmer")]
    [Authorize(Roles = "Administrateur,Agent")]
    public async Task<ActionResult<ReservationDto>> Confirmer(Guid id)
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (!Guid.TryParse(claim, out var userId)) return Unauthorized();

        var result = await _confirmerReservationUseCase.ExecuteAsync(id, utilisateurId: userId);
        return Ok(result);
    }

    /// <summary>Modifier une réservation (Propriétaire ou Personnel).</summary>
    [HttpPut("{id:guid}")]
    [Authorize]
    public async Task<ActionResult<ReservationDto>> Update(Guid id, [FromBody] ModifierReservationRequest request)
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (!Guid.TryParse(claim, out var userId)) return Unauthorized();

        var isPersonnel = User.IsInRole("Administrateur") || User.IsInRole("Agent");
        var result = await _modifierReservationUseCase.ExecuteAsync(id, request, utilisateurId: userId, isPersonnel: isPersonnel);
        return Ok(result);
    }

    /// <summary>Changer le statut d'une réservation (Personnel uniquement).</summary>
    [HttpPatch("{id:guid}/statut")]
    [Authorize(Roles = "Administrateur,Agent")]
    public async Task<ActionResult<ReservationDto>> ChangeStatus(Guid id, [FromQuery] StatutReservation statut)
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (!Guid.TryParse(claim, out var userId)) return Unauthorized();

        return Ok(await _changerStatutUseCase.ExecuteAsync(id, statut, utilisateurId: userId));
    }

    /// <summary>Annuler une réservation (Client sa réservation, ou Personnel).</summary>
    [HttpPost("{id:guid}/annuler")]
    [Authorize]
    public async Task<ActionResult<ReservationDto>> Annuler(Guid id)
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (!Guid.TryParse(claim, out var userId)) return Unauthorized();

        var isPersonnel = User.IsInRole("Administrateur") || User.IsInRole("Agent");
        var result = await _annulerReservationUseCase.ExecuteAsync(id, utilisateurId: userId, isPersonnel: isPersonnel);
        return Ok(result);
    }
}
