using HotelReservation.Application.DTOs;

namespace HotelReservation.Application.Services;

public interface IStatistiqueService
{
    Task<List<ReservationsParMoisDto>> ObtenirReservationsParMoisAsync(DateTime? dateDebut = null, DateTime? dateFin = null, CancellationToken cancellationToken = default);
    Task<List<ChiffreAffairesMensuelDto>> ObtenirChiffreAffairesMensuelAsync(DateTime? dateDebut = null, DateTime? dateFin = null, CancellationToken cancellationToken = default);
    Task<List<HotelPlusReserveDto>> ObtenirHotelsLesPlusReservesAsync(DateTime? dateDebut = null, DateTime? dateFin = null, CancellationToken cancellationToken = default);
    Task<List<PaiementsParModeDto>> ObtenirRepartitionPaiementsParModeAsync(DateTime? dateDebut = null, DateTime? dateFin = null, CancellationToken cancellationToken = default);
    Task<List<NouveauxUtilisateursParMoisDto>> ObtenirNombreNouveauxUtilisateursAsync(DateTime? dateDebut = null, DateTime? dateFin = null, CancellationToken cancellationToken = default);
    Task<List<TauxOccupationHotelDto>> ObtenirTauxOccupationHotelsAsync(DateTime? dateDebut = null, DateTime? dateFin = null, CancellationToken cancellationToken = default);
    Task<List<EvolutionAvisDto>> ObtenirEvolutionAvisAsync(DateTime? dateDebut = null, DateTime? dateFin = null, CancellationToken cancellationToken = default);
}
