using HotelReservation.Application.DTOs;

namespace HotelReservation.Application.Services;

public interface ITableauDeBordService
{
    Task<int> ObtenirNombreUtilisateursAsync(CancellationToken cancellationToken = default);
    Task<int> ObtenirNombreHotelsAsync(CancellationToken cancellationToken = default);
    Task<int> ObtenirNombreReservationsAsync(CancellationToken cancellationToken = default);
    Task<int> ObtenirReservationsDuJourAsync(CancellationToken cancellationToken = default);
    Task<List<ReservationDto>> ObtenirDernieresReservationsAsync(int count = 10, CancellationToken cancellationToken = default);
    Task<int> ObtenirPaiementsEnAttenteAsync(CancellationToken cancellationToken = default);
    Task<decimal> ObtenirChiffreAffairesAsync(CancellationToken cancellationToken = default);
    Task<Dictionary<string, int>> ObtenirGraphiqueReservationsParMoisAsync(CancellationToken cancellationToken = default);
    Task<Dictionary<string, decimal>> ObtenirGraphiqueRevenusParMoisAsync(CancellationToken cancellationToken = default);
}
