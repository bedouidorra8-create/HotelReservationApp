using System.Security.Claims;
using HotelReservation.Application.DTOs;
using HotelReservation.Application.UseCases;
using HotelReservation.Domain.Common;
using HotelReservation.Domain.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelReservation.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly InscrireUtilisateurUseCase _inscrireUseCase;
    private readonly SeConnecterUseCase _seConnecterUseCase;
    private readonly RafraichirTokenUseCase _rafraichirTokenUseCase;
    private readonly DemanderResetPasswordUseCase _demanderResetUseCase;
    private readonly ReinitialiserMotDePasseUseCase _reinitialiserUseCase;
    private readonly ChangerMotDePasseUseCase _changerMdpUseCase;
    private readonly VerifierEmailUseCase _verifierEmailUseCase;
    private readonly DeconnexionUseCase _deconnexionUseCase;
    private readonly ConsulterMonProfilUseCase _consulterProfilUseCase;
    private readonly ModifierSonProfilUseCase _modifierProfilUseCase;

    public AuthController(
        InscrireUtilisateurUseCase inscrireUseCase,
        SeConnecterUseCase seConnecterUseCase,
        RafraichirTokenUseCase rafraichirTokenUseCase,
        DemanderResetPasswordUseCase demanderResetUseCase,
        ReinitialiserMotDePasseUseCase reinitialiserUseCase,
        ChangerMotDePasseUseCase changerMdpUseCase,
        VerifierEmailUseCase verifierEmailUseCase,
        DeconnexionUseCase deconnexionUseCase,
        ConsulterMonProfilUseCase consulterProfilUseCase,
        ModifierSonProfilUseCase modifierProfilUseCase)
    {
        _inscrireUseCase = inscrireUseCase;
        _seConnecterUseCase = seConnecterUseCase;
        _rafraichirTokenUseCase = rafraichirTokenUseCase;
        _demanderResetUseCase = demanderResetUseCase;
        _reinitialiserUseCase = reinitialiserUseCase;
        _changerMdpUseCase = changerMdpUseCase;
        _verifierEmailUseCase = verifierEmailUseCase;
        _deconnexionUseCase = deconnexionUseCase;
        _consulterProfilUseCase = consulterProfilUseCase;
        _modifierProfilUseCase = modifierProfilUseCase;
    }

    /// <summary>POST /api/auth/inscription</summary>
    [HttpPost("inscription")]
    public async Task<ActionResult<MessageResponse>> Inscription([FromBody] InscrireUtilisateurRequest request)
    {
        try
        {
            var result = await _inscrireUseCase.ExecuteAsync(request);
            return Ok(result);
        }
        catch (DomainException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    /// <summary>POST /api/auth/connexion</summary>
    [HttpPost("connexion")]
    public async Task<ActionResult<ConnexionResponse>> Connexion([FromBody] ConnexionRequest request)
    {
        try
        {
            var result = await _seConnecterUseCase.ExecuteAsync(request);
            return Ok(result);
        }
        catch (DomainException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    /// <summary>POST /api/auth/rafraichir-token</summary>
    [HttpPost("rafraichir-token")]
    public async Task<ActionResult<TokenResponse>> RafraichirToken([FromBody] RafraichirTokenRequest request)
    {
        try
        {
            var result = await _rafraichirTokenUseCase.ExecuteAsync(request);
            return Ok(result);
        }
        catch (DomainException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    /// <summary>POST /api/auth/mot-de-passe-oublie</summary>
    [HttpPost("mot-de-passe-oublie")]
    public async Task<ActionResult<MessageResponse>> MotDePasseOublie([FromBody] DemanderResetPasswordRequest request)
    {
        var result = await _demanderResetUseCase.ExecuteAsync(request);
        return Ok(result);
    }

    /// <summary>POST /api/auth/reinitialiser-mot-de-passe</summary>
    [HttpPost("reinitialiser-mot-de-passe")]
    public async Task<ActionResult<MessageResponse>> ReinitialiserMotDePasse([FromBody] ReinitialiserMotDePasseRequest request)
    {
        try
        {
            var result = await _reinitialiserUseCase.ExecuteAsync(request);
            return Ok(result);
        }
        catch (DomainException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>PUT /api/auth/changer-mot-de-passe — [Authorize]</summary>
    [HttpPut("changer-mot-de-passe")]
    [Authorize]
    public async Task<ActionResult<MessageResponse>> ChangerMotDePasse([FromBody] ChangerMotDePasseRequest request)
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)
                          ?? User.FindFirst("sub");
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            return Unauthorized(new { message = "Impossible d'identifier l'utilisateur connecte." });

        try
        {
            var result = await _changerMdpUseCase.ExecuteAsync(userId, request);
            return Ok(result);
        }
        catch (DomainException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>GET /api/auth/verifier-email?token=...</summary>
    [HttpGet("verifier-email")]
    public async Task<ActionResult<MessageResponse>> VerifierEmail([FromQuery] string token)
    {
        try
        {
            var result = await _verifierEmailUseCase.ExecuteAsync(token);
            return Ok(result);
        }
        catch (DomainException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<ActionResult<MessageResponse>> Logout([FromBody] DeconnexionRequest request)
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (!Guid.TryParse(claim, out var userId))
            return Unauthorized();

        return Ok(await _deconnexionUseCase.ExecuteAsync(userId, request));
    }

    [HttpGet("profil")]
    [Authorize]
    public async Task<ActionResult<ProfilUtilisateurDto>> GetProfil(CancellationToken cancellationToken)
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (!Guid.TryParse(claim, out var userId))
            return Unauthorized(new { message = "Impossible d'identifier l'utilisateur connecté." });

        var result = await _consulterProfilUseCase.ExecuteAsync(userId, cancellationToken);
        return Ok(result);
    }

    [HttpPut("profil")]
    [Authorize]
    public async Task<ActionResult<ProfilUtilisateurDto>> UpdateProfil([FromBody] ModifierProfilRequest request, CancellationToken cancellationToken)
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (!Guid.TryParse(claim, out var userId))
            return Unauthorized(new { message = "Impossible d'identifier l'utilisateur connecté." });

        var result = await _modifierProfilUseCase.ExecuteAsync(userId, request, cancellationToken);
        return Ok(result);
    }
}
