using HotelReservation.Application.DTOs;
using HotelReservation.Application.UseCases;
using HotelReservation.Domain.Enums;
using HotelReservation.Domain.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HotelReservation.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaiementsController : ControllerBase
{
    private readonly EnregistrerPaiementUseCase _enregistrerPaiementUseCase;
    private readonly AnnulerPaiementUseCase _annulerPaiementUseCase;
    private readonly ConsulterPaiementsUseCase _consulterPaiementsUseCase;
    private readonly ConsulterTousPaiementsUseCase _consulterTousPaiementsUseCase;
    private readonly GenererFactureUseCase _genererFactureUseCase;
    private readonly GenererFacturePdfUseCase _genererFacturePdfUseCase;
    private readonly IReservationRepository _reservationRepository;

    public PaiementsController(
        EnregistrerPaiementUseCase enregistrerPaiementUseCase,
        AnnulerPaiementUseCase annulerPaiementUseCase,
        ConsulterPaiementsUseCase consulterPaiementsUseCase,
        ConsulterTousPaiementsUseCase consulterTousPaiementsUseCase,
        GenererFactureUseCase genererFactureUseCase,
        GenererFacturePdfUseCase genererFacturePdfUseCase,
        IReservationRepository reservationRepository)
    {
        _enregistrerPaiementUseCase = enregistrerPaiementUseCase;
        _annulerPaiementUseCase = annulerPaiementUseCase;
        _consulterPaiementsUseCase = consulterPaiementsUseCase;
        _consulterTousPaiementsUseCase = consulterTousPaiementsUseCase;
        _genererFactureUseCase = genererFactureUseCase;
        _genererFacturePdfUseCase = genererFacturePdfUseCase;
        _reservationRepository = reservationRepository;
    }

    public PaiementsController(
        EnregistrerPaiementUseCase enregistrerPaiementUseCase,
        AnnulerPaiementUseCase annulerPaiementUseCase,
        ConsulterPaiementsUseCase consulterPaiementsUseCase,
        GenererFactureUseCase genererFactureUseCase,
        IReservationRepository reservationRepository)
        : this(enregistrerPaiementUseCase, annulerPaiementUseCase, consulterPaiementsUseCase, null!, genererFactureUseCase, null!, reservationRepository)
    {
    }

    /// <summary>Récupère la liste globale des transactions avec filtres (Administrateur et Agent).</summary>
    [HttpGet]
    [Authorize(Roles = "Administrateur,Agent")]
    public async Task<ActionResult<IEnumerable<PaiementDto>>> GetAll(
        [FromQuery] StatutPaiement? statut,
        [FromQuery] ModePaiement? mode,
        [FromQuery] DateTime? dateDebut,
        [FromQuery] DateTime? dateFin,
        CancellationToken cancellationToken)
    {
        var result = await _consulterTousPaiementsUseCase.ExecuteAsync(statut, mode, dateDebut, dateFin, cancellationToken);
        return Ok(result);
    }

    /// <summary>Enregistre un paiement pour une réservation (Client uniquement).</summary>
    [HttpPost]
    [Authorize(Roles = "Client")]
    public async Task<ActionResult<PaiementDto>> Create([FromBody] EnregistrerPaiementRequest request)
    {
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var clientId))
            return Unauthorized();

        var reservation = await _reservationRepository.GetByIdAsync(request.ReservationId);
        if (reservation == null)
            return NotFound();
        if (reservation.ClientId != clientId)
            return Forbid();

        var result = await _enregistrerPaiementUseCase.ExecuteAsync(clientId, request);
        return Ok(result);
    }

    /// <summary>Consulte les paiements du client connecté.</summary>
    [HttpGet("me")]
    [Authorize(Roles = "Client")]
    public async Task<ActionResult<IEnumerable<PaiementDto>>> GetMine()
    {
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var clientId))
            return Unauthorized();

        return Ok(await _consulterPaiementsUseCase.ExecuteForClientAsync(clientId));
    }

    /// <summary>Consulte les paiements rattachés à une réservation.</summary>
    [HttpGet("reservation/{reservationId:guid}")]
    [Authorize(Roles = "Administrateur,Agent")]
    public async Task<ActionResult<IEnumerable<PaiementDto>>> GetByReservation(Guid reservationId)
    {
        return Ok(await _consulterPaiementsUseCase.ExecuteAsync(reservationId));
    }

    /// <summary>Annule un paiement existant (Personnel uniquement).</summary>
    [HttpPost("{id:guid}/annuler")]
    [Authorize(Roles = "Administrateur,Agent")]
    public async Task<ActionResult<PaiementDto>> Cancel(Guid id)
    {
        return Ok(await _annulerPaiementUseCase.ExecuteAsync(id));
    }

    /// <summary>Génère la facture détaillée au format JSON.</summary>
    [HttpGet("{reservationId:guid}/facture")]
    [Authorize]
    public async Task<ActionResult<FactureDto>> GetFacture(Guid reservationId)
    {
        if (User.IsInRole("Client"))
        {
            if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var clientId))
                return Unauthorized();

            var reservation = await _reservationRepository.GetByIdAsync(reservationId);
            if (reservation == null) return NotFound();
            if (reservation.ClientId != clientId) return Forbid();
        }

        return Ok(await _genererFactureUseCase.ExecuteAsync(reservationId));
    }

    /// <summary>Télécharge la facture officielle au format binaire PDF (BG-01).</summary>
    [HttpGet("{reservationId:guid}/facture/pdf")]
    [Authorize]
    public async Task<IActionResult> DownloadFacturePdf(Guid reservationId, CancellationToken cancellationToken)
    {
        if (User.IsInRole("Client"))
        {
            if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var clientId))
                return Unauthorized();

            var reservation = await _reservationRepository.GetByIdAsync(reservationId, cancellationToken);
            if (reservation == null) return NotFound();
            if (reservation.ClientId != clientId) return Forbid();
        }

        var pdfBytes = await _genererFacturePdfUseCase.ExecuteAsync(reservationId, cancellationToken);
        var filename = $"Facture-{reservationId.ToString().Substring(0, 8).ToUpper()}.pdf";
        return File(pdfBytes, "application/pdf", filename);
    }
}
