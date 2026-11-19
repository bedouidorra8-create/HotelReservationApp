using System.Security.Claims;
using HotelReservation.Application.DTOs;
using HotelReservation.Application.UseCases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelReservation.Api.Controllers;

/// <summary>
/// Contrôleur API fournissant les indicateurs de synthèse et tableaux de bord pour l'administration et le personnel hôtelier.
/// </summary>
[ApiController]
[Route("api/dashboard")]
public class DashboardController : ControllerBase
{
    private readonly ConsulterDashboardUseCase _dashboardUseCase;
    private readonly ConsulterStatistiquesUseCase _statistiquesUseCase;

    public DashboardController(
        ConsulterDashboardUseCase dashboardUseCase,
        ConsulterStatistiquesUseCase statistiquesUseCase)
    {
        _dashboardUseCase = dashboardUseCase;
        _statistiquesUseCase = statistiquesUseCase;
    }

    /// <summary>
    /// Récupère les métriques consolidées du tableau de bord selon le rôle (Administrateur ou Agent).
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Administrateur,Agent")]
    public async Task<ActionResult<DashboardDto>> GetDashboard(CancellationToken cancellationToken)
    {
        var role = User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
        var result = await _dashboardUseCase.ExecuteAsync(role, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Récupère les 7 indicateurs statistiques détaillés (Administrateur uniquement).
    /// </summary>
    [HttpGet("statistiques")]
    [Authorize(Roles = "Administrateur")]
    public async Task<ActionResult<StatistiquesDto>> GetStatistiques(
        [FromQuery] DateTime? dateDebut,
        [FromQuery] DateTime? dateFin,
        CancellationToken cancellationToken)
    {
        if (dateDebut.HasValue && dateFin.HasValue && dateFin <= dateDebut)
            return BadRequest("La dateFin doit être supérieure à la dateDebut.");

        return Ok(await _statistiquesUseCase.ExecuteAsync(dateDebut, dateFin, cancellationToken));
    }
}
