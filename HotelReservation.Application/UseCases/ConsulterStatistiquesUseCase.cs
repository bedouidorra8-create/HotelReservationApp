using HotelReservation.Application.DTOs;
using HotelReservation.Application.Services;

namespace HotelReservation.Application.UseCases;

/// <summary>
/// Cas d'utilisation consolidant l'ensemble des 7 indicateurs statistiques via IStatistiqueService.
/// </summary>
public class ConsulterStatistiquesUseCase
{
    private readonly IStatistiqueService _statistiqueService;

    public ConsulterStatistiquesUseCase(IStatistiqueService statistiqueService)
    {
        _statistiqueService = statistiqueService;
    }

    public async Task<StatistiquesDto> ExecuteAsync(
        DateTime? dateDebut = null,
        DateTime? dateFin = null,
        CancellationToken cancellationToken = default)
    {
        var reservationsParMois = await _statistiqueService.ObtenirReservationsParMoisAsync(dateDebut, dateFin, cancellationToken);
        var chiffreAffairesMensuel = await _statistiqueService.ObtenirChiffreAffairesMensuelAsync(dateDebut, dateFin, cancellationToken);
        var hotelsLesPlusReserves = await _statistiqueService.ObtenirHotelsLesPlusReservesAsync(dateDebut, dateFin, cancellationToken);
        var repartitionPaiements = await _statistiqueService.ObtenirRepartitionPaiementsParModeAsync(dateDebut, dateFin, cancellationToken);
        var nouveauxUtilisateurs = await _statistiqueService.ObtenirNombreNouveauxUtilisateursAsync(dateDebut, dateFin, cancellationToken);
        var tauxOccupationHotels = await _statistiqueService.ObtenirTauxOccupationHotelsAsync(dateDebut, dateFin, cancellationToken);
        var evolutionAvis = await _statistiqueService.ObtenirEvolutionAvisAsync(dateDebut, dateFin, cancellationToken);

        return new StatistiquesDto(
            reservationsParMois,
            chiffreAffairesMensuel,
            hotelsLesPlusReserves,
            repartitionPaiements,
            nouveauxUtilisateurs,
            tauxOccupationHotels,
            evolutionAvis,
            dateDebut,
            dateFin
        );
    }
}
