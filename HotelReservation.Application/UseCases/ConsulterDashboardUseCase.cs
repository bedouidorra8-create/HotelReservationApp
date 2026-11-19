using HotelReservation.Application.DTOs;
using HotelReservation.Application.Services;
using HotelReservation.Domain.Common;

namespace HotelReservation.Application.UseCases;

/// <summary>
/// Cas d'utilisation fournissant les données du tableau de bord selon le rôle de l'utilisateur.
/// </summary>
public class ConsulterDashboardUseCase
{
    private readonly ITableauDeBordService _dashboardService;

    public ConsulterDashboardUseCase(ITableauDeBordService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    /// <summary>
    /// Exécute la collecte des métriques selon le rôle.
    /// </summary>
    public async Task<DashboardDto> ExecuteAsync(string role, CancellationToken cancellationToken = default)
    {
        var reservationsDuJour = await _dashboardService.ObtenirReservationsDuJourAsync(cancellationToken);
        var paiementsEnAttente = await _dashboardService.ObtenirPaiementsEnAttenteAsync(cancellationToken);
        var dernieresReservations = await _dashboardService.ObtenirDernieresReservationsAsync(10, cancellationToken);

        if (string.Equals(role, "Agent", StringComparison.OrdinalIgnoreCase))
        {
            // L'Agent ne voit QUE réservations du jour, paiements en attente, et dernières réservations.
            return new DashboardDto(
                NombreTotalUtilisateurs: null,
                NombreHotels: null,
                NombreReservations: null,
                ReservationsDuJour: reservationsDuJour,
                ChiffreAffaires: null,
                PaiementsEnAttente: paiementsEnAttente,
                DernieresReservations: dernieresReservations,
                GraphiqueReservationsParMois: null,
                GraphiqueRevenusParMois: null
            );
        }
        else if (string.Equals(role, "Administrateur", StringComparison.OrdinalIgnoreCase))
        {
            var totalUtilisateurs = await _dashboardService.ObtenirNombreUtilisateursAsync(cancellationToken);
            var totalHotels = await _dashboardService.ObtenirNombreHotelsAsync(cancellationToken);
            var totalReservations = await _dashboardService.ObtenirNombreReservationsAsync(cancellationToken);
            var chiffreAffaires = await _dashboardService.ObtenirChiffreAffairesAsync(cancellationToken);
            var graphiqueReservations = await _dashboardService.ObtenirGraphiqueReservationsParMoisAsync(cancellationToken);
            var graphiqueRevenus = await _dashboardService.ObtenirGraphiqueRevenusParMoisAsync(cancellationToken);

            return new DashboardDto(
                totalUtilisateurs,
                totalHotels,
                totalReservations,
                reservationsDuJour,
                chiffreAffaires,
                paiementsEnAttente,
                dernieresReservations,
                graphiqueReservations,
                graphiqueRevenus
            );
        }

        throw new DomainException("Accès refusé au tableau de bord pour ce rôle.");
    }
}
