using HotelReservation.Application.DTOs;
using HotelReservation.Application.UseCases;
using HotelReservation.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelReservation.Api.Controllers;

/// <summary>
/// Contrôleur API dédié à la gestion des chambres hôtelières (création, consultation, modification, statut, suppression et recherche).
/// Respecte le pattern DDD où toute mutation de chambre transite par la racine d'agrégat Hotel.
/// </summary>
[ApiController]
public class ChambresController : ControllerBase
{
    private readonly CreerChambreUseCase _creerChambreUseCase;
    private readonly ConsulterChambresUseCase _consulterChambresUseCase;
    private readonly ConsulterDisponibilitesUseCase _consulterDisponibilitesUseCase;
    private readonly ModifierChambreUseCase _modifierChambreUseCase;
    private readonly SupprimerChambreUseCase _supprimerChambreUseCase;
    private readonly ChangerStatutChambreUseCase _changerStatutChambreUseCase;
    private readonly ActiverDesactiverChambreUseCase _activerChambreUseCase;
    private readonly RechercherChambreUseCase _rechercherChambreUseCase;

    /// <summary>
    /// Initialise une nouvelle instance de <see cref="ChambresController"/>.
    /// </summary>
    public ChambresController(
        CreerChambreUseCase creerChambreUseCase,
        ConsulterChambresUseCase consulterChambresUseCase,
        ConsulterDisponibilitesUseCase consulterDisponibilitesUseCase,
        ModifierChambreUseCase modifierChambreUseCase,
        SupprimerChambreUseCase supprimerChambreUseCase,
        ChangerStatutChambreUseCase changerStatutChambreUseCase,
        ActiverDesactiverChambreUseCase activerChambreUseCase,
        RechercherChambreUseCase rechercherChambreUseCase)
    {
        _creerChambreUseCase = creerChambreUseCase;
        _consulterChambresUseCase = consulterChambresUseCase;
        _consulterDisponibilitesUseCase = consulterDisponibilitesUseCase;
        _modifierChambreUseCase = modifierChambreUseCase;
        _supprimerChambreUseCase = supprimerChambreUseCase;
        _changerStatutChambreUseCase = changerStatutChambreUseCase;
        _activerChambreUseCase = activerChambreUseCase;
        _rechercherChambreUseCase = rechercherChambreUseCase;
    }

    /// <summary>
    /// Ajoute une nouvelle chambre à un hôtel donné.
    /// </summary>
    /// <param name="hotelId">Identifiant unique de l'hôtel.</param>
    /// <param name="request">Caractéristiques de la chambre à créer.</param>
    /// <param name="cancellationToken">Jeton d'annulation asynchrone.</param>
    /// <returns>Le DTO de la chambre créée.</returns>
    [HttpPost("api/hotels/{hotelId:guid}/chambres")]
    [Authorize(Roles = "Administrateur,Agent")]
    public async Task<ActionResult<ChambreDto>> AddChambre(
        Guid hotelId,
        [FromBody] CreerChambreRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _creerChambreUseCase.ExecuteAsync(hotelId, request, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Récupère la liste de toutes les chambres appartenant à un hôtel spécifique.
    /// </summary>
    /// <param name="hotelId">Identifiant unique de l'hôtel.</param>
    /// <param name="cancellationToken">Jeton d'annulation asynchrone.</param>
    /// <returns>La liste des chambres de l'hôtel.</returns>
    [HttpGet("api/hotels/{hotelId:guid}/chambres")]
    public async Task<ActionResult<IEnumerable<ChambreDto>>> GetByHotel(
        Guid hotelId,
        CancellationToken cancellationToken)
    {
        var result = await _consulterChambresUseCase.ExecuteAsync(hotelId, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Consulte les disponibilités journalières de toutes les chambres d'un hôtel pour une période donnée.
    /// </summary>
    /// <param name="hotelId">Identifiant unique de l'hôtel.</param>
    /// <param name="dateDebut">Date de début de l'intervalle.</param>
    /// <param name="dateFin">Date de fin de l'intervalle.</param>
    /// <param name="cancellationToken">Jeton d'annulation asynchrone.</param>
    /// <returns>La grille de disponibilité journalière des chambres.</returns>
    [HttpGet("api/hotels/{hotelId:guid}/chambres/disponibilites")]
    public async Task<ActionResult<DisponibilitesHotelResponse>> GetDisponibilites(
        Guid hotelId,
        [FromQuery] DateTime dateDebut,
        [FromQuery] DateTime dateFin,
        CancellationToken cancellationToken)
    {
        if (dateFin <= dateDebut)
        {
            return BadRequest("La dateFin doit être supérieure à la dateDebut.");
        }

        var result = await _consulterDisponibilitesUseCase.ExecuteAsync(hotelId, dateDebut, dateFin, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Modifie les caractéristiques d'une chambre existante (type, étage, tarif).
    /// </summary>
    /// <param name="id">Identifiant unique de la chambre.</param>
    /// <param name="request">Nouvelles données de la chambre.</param>
    /// <param name="cancellationToken">Jeton d'annulation asynchrone.</param>
    /// <returns>Le DTO de la chambre modifiée.</returns>
    [HttpPut("api/chambres/{id:guid}")]
    [Authorize(Roles = "Administrateur,Agent")]
    public async Task<ActionResult<ChambreDto>> Update(
        Guid id,
        [FromBody] ModifierChambreRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _modifierChambreUseCase.ExecuteAsync(id, request, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Supprime définitivement une chambre d'hôtel (si elle n'a aucune réservation active).
    /// </summary>
    /// <param name="id">Identifiant unique de la chambre.</param>
    /// <param name="cancellationToken">Jeton d'annulation asynchrone.</param>
    /// <returns>NoContent en cas de succès.</returns>
    [HttpDelete("api/chambres/{id:guid}")]
    [Authorize(Roles = "Administrateur,Agent")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _supprimerChambreUseCase.ExecuteAsync(id, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Met à jour le statut opérationnel d'une chambre (Libre, Occupee, Maintenance).
    /// </summary>
    /// <param name="id">Identifiant unique de la chambre.</param>
    /// <param name="statut">Nouveau statut cible.</param>
    /// <param name="cancellationToken">Jeton d'annulation asynchrone.</param>
    /// <returns>Le DTO de la chambre avec son nouveau statut.</returns>
    [HttpPatch("api/chambres/{id:guid}/statut")]
    [Authorize(Roles = "Administrateur,Agent")]
    public async Task<ActionResult<ChambreDto>> ChangeStatus(
        Guid id,
        [FromQuery] StatutChambre statut,
        CancellationToken cancellationToken)
    {
        var result = await _changerStatutChambreUseCase.ExecuteAsync(id, statut, cancellationToken);
        return Ok(result);
    }

    [HttpPatch("api/chambres/{id:guid}/actif")]
    [Authorize(Roles = "Administrateur,Agent")]
    public async Task<ActionResult<ChambreDto>> ToggleActive(
        Guid id,
        [FromQuery] bool actif,
        CancellationToken cancellationToken)
    {
        var result = await _activerChambreUseCase.ExecuteAsync(id, actif, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Effectue une recherche multicritère parmi toutes les chambres du parc hôtelier.
    /// </summary>
    /// <param name="type">Type de chambre souhaité (Simple, Double, Suite, Familiale).</param>
    /// <param name="prixMin">Prix minimum par nuit.</param>
    /// <param name="prixMax">Prix maximum par nuit.</param>
    /// <param name="statut">Statut de chambre recherché.</param>
    /// <param name="cancellationToken">Jeton d'annulation asynchrone.</param>
    /// <returns>La liste des chambres correspondant aux critères.</returns>
    [HttpGet("api/chambres/recherche")]
    public async Task<ActionResult<IEnumerable<ChambreDto>>> Search(
        [FromQuery] TypeChambre? type,
        [FromQuery] decimal? prixMin,
        [FromQuery] decimal? prixMax,
        [FromQuery] StatutChambre? statut,
        CancellationToken cancellationToken)
    {
        var request = new RechercherChambreRequest(type, prixMin, prixMax, statut);
        var result = await _rechercherChambreUseCase.ExecuteAsync(request, cancellationToken);
        return Ok(result);
    }
}
