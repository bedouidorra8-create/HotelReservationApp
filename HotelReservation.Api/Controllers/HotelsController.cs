using HotelReservation.Application.DTOs;
using HotelReservation.Application.UseCases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelReservation.Api.Controllers;

/// <summary>
/// Contrôleur API exposant la gestion des établissements hôteliers (création, consultation, modification, statut, suppression et disponibilités).
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class HotelsController : ControllerBase
{
    private readonly ConsulterHotelsUseCase _consulterHotelsUseCase;
    private readonly CreerHotelUseCase _creerHotelUseCase;
    private readonly CreerChambreUseCase _creerChambreUseCase;
    private readonly ConsulterDisponibilitesUseCase _consulterDisponibilitesUseCase;
    private readonly RechercherHotelsUseCase _rechercherHotelsUseCase;
    private readonly ModifierHotelUseCase _modifierHotelUseCase;
    private readonly ActiverDesactiverHotelUseCase _activerDesactiverHotelUseCase;
    private readonly SupprimerHotelUseCase _supprimerHotelUseCase;

    /// <summary>
    /// Initialise une nouvelle instance de <see cref="HotelsController"/>.
    /// </summary>
    public HotelsController(
        ConsulterHotelsUseCase consulterHotelsUseCase,
        CreerHotelUseCase creerHotelUseCase,
        CreerChambreUseCase creerChambreUseCase,
        ConsulterDisponibilitesUseCase consulterDisponibilitesUseCase,
        RechercherHotelsUseCase rechercherHotelsUseCase,
        ModifierHotelUseCase modifierHotelUseCase,
        ActiverDesactiverHotelUseCase activerDesactiverHotelUseCase,
        SupprimerHotelUseCase supprimerHotelUseCase)
    {
        _consulterHotelsUseCase = consulterHotelsUseCase;
        _creerHotelUseCase = creerHotelUseCase;
        _creerChambreUseCase = creerChambreUseCase;
        _consulterDisponibilitesUseCase = consulterDisponibilitesUseCase;
        _rechercherHotelsUseCase = rechercherHotelsUseCase;
        _modifierHotelUseCase = modifierHotelUseCase;
        _activerDesactiverHotelUseCase = activerDesactiverHotelUseCase;
        _supprimerHotelUseCase = supprimerHotelUseCase;
    }

    /// <summary>
    /// Récupère la liste de tous les hôtels disponibles.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<HotelDto>>> GetAll()
    {
        var result = await _consulterHotelsUseCase.GetAllAsync();
        return Ok(result);
    }

    /// <summary>
    /// Récupère la fiche détaillée d'un hôtel par son identifiant.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<HotelDto>> GetById(Guid id)
    {
        var result = await _consulterHotelsUseCase.GetByIdAsync(id);
        return Ok(result);
    }

    /// <summary>
    /// Recherche multicritère d'hôtels avec calcul du prix indicatif le plus bas.
    /// </summary>
    [HttpGet("recherche")]
    public async Task<ActionResult<IEnumerable<HotelResultatRechercheDto>>> Rechercher(
        [FromQuery] string? ville,
        [FromQuery] decimal? prixMin,
        [FromQuery] decimal? prixMax,
        [FromQuery] int? etoilesMin,
        [FromQuery] DateTime? dateDebut,
        [FromQuery] DateTime? dateFin,
        [FromQuery] bool disponibleUniquement = false,
        [FromQuery] string? tri = "prixAsc")
    {
        var request = new RechercherHotelsRequest(ville, prixMin, prixMax, etoilesMin, dateDebut, dateFin, disponibleUniquement, tri);
        var result = await _rechercherHotelsUseCase.ExecuteAsync(request);
        return Ok(result);
    }

    /// <summary>
    /// Crée un nouvel hôtel. Réservé aux administrateurs et agents.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Administrateur,Agent")]
    public async Task<ActionResult<HotelDto>> Create([FromBody] CreerHotelRequest request)
    {
        var result = await _creerHotelUseCase.ExecuteAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Ajoute une nouvelle chambre à l'hôtel spécifié.
    /// </summary>
    [HttpPost("{hotelId:guid}/chambres")]
    [Authorize(Roles = "Administrateur,Agent")]
    public async Task<ActionResult<ChambreDto>> AddChambre(Guid hotelId, [FromBody] CreerChambreRequest request)
    {
        var result = await _creerChambreUseCase.ExecuteAsync(hotelId, request);
        return Ok(result);
    }

    /// <summary>
    /// Met à jour les informations d'un hôtel.
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Administrateur,Agent")]
    public async Task<ActionResult<HotelDto>> Update(Guid id, [FromBody] ModifierHotelRequest request)
    {
        return Ok(await _modifierHotelUseCase.ExecuteAsync(id, request));
    }

    /// <summary>
    /// Active ou désactive un hôtel.
    /// </summary>
    [HttpPatch("{id:guid}/actif")]
    [Authorize(Roles = "Administrateur,Agent")]
    public async Task<IActionResult> SetActif(Guid id, [FromQuery] bool actif)
    {
        await _activerDesactiverHotelUseCase.ExecuteAsync(id, actif);
        return NoContent();
    }

    /// <summary>
    /// Supprime définitivement un hôtel et toutes ses chambres associées.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Administrateur")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _supprimerHotelUseCase.ExecuteAsync(id);
        return NoContent();
    }

    /// <summary>
    /// Récupère la disponibilité journalière des chambres pour un hôtel.
    /// </summary>
    [HttpGet("{hotelId:guid}/disponibilites")]
    public async Task<ActionResult<DisponibilitesHotelResponse>> GetDisponibilites(
        Guid hotelId,
        [FromQuery] DateTime dateDebut,
        [FromQuery] DateTime dateFin)
    {
        if (dateFin <= dateDebut)
        {
            return BadRequest("La dateFin doit etre superieure a la dateDebut.");
        }

        var result = await _consulterDisponibilitesUseCase.ExecuteAsync(hotelId, dateDebut, dateFin);
        return Ok(result);
    }
}

