using System.Security.Claims;
using HotelReservation.Application.DTOs;
using HotelReservation.Application.UseCases;
using HotelReservation.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelReservation.Api.Controllers;

/// <summary>
/// Contrôleur API réservé aux administrateurs pour la gestion des utilisateurs, agents, administrateurs, rôles et modération des avis.
/// </summary>
[ApiController]
[Route("api/administration")]
[Authorize(Roles = "Administrateur")]
public class AdministrationController : ControllerBase
{
    private readonly ConsulterListeUtilisateursUseCase _listeUtilisateursUseCase;
    private readonly ConsulterUtilisateurUseCase _consulterUtilisateurUseCase;
    private readonly RechercherUtilisateurUseCase _rechercherUtilisateurUseCase;
    private readonly ModifierAgentUseCase _modifierAgentUseCase;
    private readonly SupprimerAgentUseCase _supprimerAgentUseCase;
    private readonly ModifierAdministrateurUseCase _modifierAdministrateurUseCase;
    private readonly SupprimerAdministrateurUseCase _supprimerAdministrateurUseCase;
    private readonly AjouterUtilisateurUseCase _ajouterUtilisateurUseCase;
    private readonly ActiverDesactiverCompteUseCase _activerCompteUseCase;
    private readonly GererRolesUseCase _gererRolesUseCase;
    private readonly ValiderAvisUseCase _validerAvisUseCase;
    private readonly RejeterAvisUseCase _rejeterAvisUseCase;
    private readonly ConsulterTousAvisUseCase _consulterTousAvisUseCase;

    public AdministrationController(
        ConsulterListeUtilisateursUseCase listeUtilisateursUseCase,
        ConsulterUtilisateurUseCase consulterUtilisateurUseCase,
        RechercherUtilisateurUseCase rechercherUtilisateurUseCase,
        ModifierAgentUseCase modifierAgentUseCase,
        SupprimerAgentUseCase supprimerAgentUseCase,
        ModifierAdministrateurUseCase modifierAdministrateurUseCase,
        SupprimerAdministrateurUseCase supprimerAdministrateurUseCase,
        AjouterUtilisateurUseCase ajouterUtilisateurUseCase,
        ActiverDesactiverCompteUseCase activerCompteUseCase,
        GererRolesUseCase gererRolesUseCase,
        ValiderAvisUseCase validerAvisUseCase,
        RejeterAvisUseCase rejeterAvisUseCase,
        ConsulterTousAvisUseCase consulterTousAvisUseCase)
    {
        _listeUtilisateursUseCase = listeUtilisateursUseCase;
        _consulterUtilisateurUseCase = consulterUtilisateurUseCase;
        _rechercherUtilisateurUseCase = rechercherUtilisateurUseCase;
        _modifierAgentUseCase = modifierAgentUseCase;
        _supprimerAgentUseCase = supprimerAgentUseCase;
        _modifierAdministrateurUseCase = modifierAdministrateurUseCase;
        _supprimerAdministrateurUseCase = supprimerAdministrateurUseCase;
        _ajouterUtilisateurUseCase = ajouterUtilisateurUseCase;
        _activerCompteUseCase = activerCompteUseCase;
        _gererRolesUseCase = gererRolesUseCase;
        _validerAvisUseCase = validerAvisUseCase;
        _rejeterAvisUseCase = rejeterAvisUseCase;
        _consulterTousAvisUseCase = consulterTousAvisUseCase;
    }

    /// <summary>Récupère la liste de tous les utilisateurs enregistrés.</summary>
    [HttpGet("utilisateurs")]
    public async Task<ActionResult<IEnumerable<UtilisateurDto>>> GetUsers(CancellationToken cancellationToken)
    {
        return Ok(await _listeUtilisateursUseCase.ExecuteAsync(cancellationToken));
    }

    /// <summary>Récupère la fiche détaillée d'un utilisateur par son ID (BG-04).</summary>
    [HttpGet("utilisateurs/{id:guid}")]
    public async Task<ActionResult<UtilisateurDto>> GetUserById(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _consulterUtilisateurUseCase.ExecuteAsync(id, cancellationToken));
    }

    /// <summary>Recherche des utilisateurs par terme (nom/email) et rôle.</summary>
    [HttpGet("utilisateurs/recherche")]
    public async Task<ActionResult<IEnumerable<UtilisateurDto>>> SearchUsers(
        [FromQuery] string? terme,
        [FromQuery] string? role,
        CancellationToken cancellationToken)
    {
        return Ok(await _rechercherUtilisateurUseCase.ExecuteAsync(
            new RechercherUtilisateurRequest(terme, role), cancellationToken));
    }

    /// <summary>Crée un nouvel utilisateur (personnel ou administrateur).</summary>
    [HttpPost("utilisateurs")]
    public async Task<ActionResult<UtilisateurDto>> AddUser(
        [FromBody] AjouterUtilisateurRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(await _ajouterUtilisateurUseCase.ExecuteAsync(request, cancellationToken));
    }

    /// <summary>Met à jour les coordonnées d'un agent.</summary>
    [HttpPut("agents/{id:guid}")]
    public async Task<ActionResult<UtilisateurDto>> UpdateAgent(
        Guid id,
        [FromBody] ModifierAgentRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(await _modifierAgentUseCase.ExecuteAsync(id, request, cancellationToken));
    }

    /// <summary>Supprime un compte agent.</summary>
    [HttpDelete("agents/{id:guid}")]
    public async Task<IActionResult> DeleteAgent(Guid id, CancellationToken cancellationToken)
    {
        await _supprimerAgentUseCase.ExecuteAsync(id, cancellationToken);
        return NoContent();
    }

    /// <summary>Met à jour les coordonnées d'un administrateur.</summary>
    [HttpPut("administrateurs/{id:guid}")]
    public async Task<ActionResult<UtilisateurDto>> UpdateAdministrateur(
        Guid id,
        [FromBody] ModifierAdministrateurRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(await _modifierAdministrateurUseCase.ExecuteAsync(id, request, cancellationToken));
    }

    /// <summary>Supprime un compte administrateur.</summary>
    [HttpDelete("administrateurs/{id:guid}")]
    public async Task<IActionResult> DeleteAdministrateur(Guid id, CancellationToken cancellationToken)
    {
        if (Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var adminId) && adminId == id)
            return BadRequest(new { message = "Un administrateur ne peut pas supprimer son propre compte." });

        await _supprimerAdministrateurUseCase.ExecuteAsync(id, cancellationToken);
        return NoContent();
    }

    /// <summary>Active ou désactive un compte utilisateur.</summary>
    [HttpPatch("utilisateurs/{id:guid}/actif")]
    public async Task<ActionResult<UtilisateurDto>> SetAccountStatus(
        Guid id,
        [FromBody] ActiverDesactiverCompteRequest request,
        CancellationToken cancellationToken)
    {
        if (!request.Actif && Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var adminId) && adminId == id)
            return BadRequest(new { message = "Un administrateur ne peut pas désactiver son propre compte." });

        return Ok(await _activerCompteUseCase.ExecuteAsync(id, request.Actif, cancellationToken));
    }

    /// <summary>Récupère la liste de tous les rôles configurés.</summary>
    [HttpGet("roles")]
    public async Task<ActionResult<IEnumerable<RoleDto>>> GetRoles(CancellationToken cancellationToken)
    {
        return Ok(await _gererRolesUseCase.GetAllAsync(cancellationToken));
    }

    /// <summary>Récupère la configuration d'un rôle par son identifiant unique.</summary>
    [HttpGet("roles/{id:guid}")]
    public async Task<ActionResult<RoleDto>> GetRoleById(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _gererRolesUseCase.GetByIdAsync(id, cancellationToken));
    }

    /// <summary>Crée ou met à jour un rôle et ses permissions.</summary>
    [HttpPost("roles")]
    public async Task<ActionResult<RoleDto>> ManageRole(
        [FromBody] GererRolesRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(await _gererRolesUseCase.ExecuteAsync(request, cancellationToken));
    }

    /// <summary>Consulte la liste complète des avis soumis pour modération (BG-02).</summary>
    [HttpGet("avis")]
    public async Task<ActionResult<IEnumerable<AvisDto>>> GetAvis(
        [FromQuery] StatutAvis? statut,
        CancellationToken cancellationToken)
    {
        return Ok(await _consulterTousAvisUseCase.ExecuteAsync(statut, cancellationToken));
    }

    /// <summary>Valide et publie un avis client soumis.</summary>
    [HttpPost("avis/{id:guid}/valider")]
    public async Task<ActionResult<AvisDto>> ValidateReview(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _validerAvisUseCase.ExecuteAsync(id, cancellationToken));
    }

    /// <summary>Rejette un avis client inapproprié ou non conforme.</summary>
    [HttpPost("avis/{id:guid}/rejeter")]
    public async Task<ActionResult<AvisDto>> RejectReview(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _rejeterAvisUseCase.ExecuteAsync(id, cancellationToken));
    }
}
