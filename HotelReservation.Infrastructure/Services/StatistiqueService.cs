using HotelReservation.Application.DTOs;
using HotelReservation.Application.Services;
using HotelReservation.Domain.Enums;
using HotelReservation.Domain.Repositories;

namespace HotelReservation.Infrastructure.Services;

public class StatistiqueService : IStatistiqueService
{
    private readonly IUtilisateurRepository _utilisateurRepository;
    private readonly IHotelRepository _hotelRepository;
    private readonly IReservationRepository _reservationRepository;
    private readonly IPaiementRepository _paiementRepository;
    private readonly IAvisRepository _avisRepository;

    public StatistiqueService(
        IUtilisateurRepository utilisateurRepository,
        IHotelRepository hotelRepository,
        IReservationRepository reservationRepository,
        IPaiementRepository paiementRepository,
        IAvisRepository avisRepository)
    {
        _utilisateurRepository = utilisateurRepository;
        _hotelRepository = hotelRepository;
        _reservationRepository = reservationRepository;
        _paiementRepository = paiementRepository;
        _avisRepository = avisRepository;
    }

    public async Task<List<ReservationsParMoisDto>> ObtenirReservationsParMoisAsync(DateTime? dateDebut = null, DateTime? dateFin = null, CancellationToken cancellationToken = default)
    {
        var reservations = (await _reservationRepository.GetAllAsync(cancellationToken)).ToList();
        if (dateDebut.HasValue) reservations = reservations.Where(r => r.DateCreation >= dateDebut.Value).ToList();
        if (dateFin.HasValue) reservations = reservations.Where(r => r.DateCreation <= dateFin.Value).ToList();

        return reservations
            .GroupBy(r => new { r.DateCreation.Year, r.DateCreation.Month })
            .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
            .Select(g => new ReservationsParMoisDto(g.Key.Year, g.Key.Month, g.Count()))
            .ToList();
    }

    public async Task<List<ChiffreAffairesMensuelDto>> ObtenirChiffreAffairesMensuelAsync(DateTime? dateDebut = null, DateTime? dateFin = null, CancellationToken cancellationToken = default)
    {
        var paiements = (await _paiementRepository.GetAllAsync(cancellationToken)).ToList();
        if (dateDebut.HasValue) paiements = paiements.Where(p => p.Date >= dateDebut.Value).ToList();
        if (dateFin.HasValue) paiements = paiements.Where(p => p.Date <= dateFin.Value).ToList();

        return paiements
            .Where(p => p.Statut == StatutPaiement.Paye)
            .GroupBy(p => new { p.Date.Year, p.Date.Month })
            .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
            .Select(g => new ChiffreAffairesMensuelDto(g.Key.Year, g.Key.Month, g.Sum(p => p.Montant.Amount)))
            .ToList();
    }

    public async Task<List<HotelPlusReserveDto>> ObtenirHotelsLesPlusReservesAsync(DateTime? dateDebut = null, DateTime? dateFin = null, CancellationToken cancellationToken = default)
    {
        var hotels = (await _hotelRepository.GetAllAsync(cancellationToken)).ToList();
        var reservations = (await _reservationRepository.GetAllAsync(cancellationToken)).ToList();
        if (dateDebut.HasValue) reservations = reservations.Where(r => r.DateCreation >= dateDebut.Value).ToList();
        if (dateFin.HasValue) reservations = reservations.Where(r => r.DateCreation <= dateFin.Value).ToList();

        var chambreToHotelMap = hotels
            .SelectMany(h => h.Chambres.Select(c => new { ChambreId = c.Id, Hotel = h }))
            .ToDictionary(x => x.ChambreId, x => x.Hotel);

        return reservations
            .Where(r => chambreToHotelMap.ContainsKey(r.ChambreId))
            .GroupBy(r => chambreToHotelMap[r.ChambreId])
            .Select(g => new HotelPlusReserveDto(g.Key.Id, g.Key.Nom, g.Count()))
            .OrderByDescending(h => h.NombreReservations)
            .ToList();
    }

    public async Task<List<PaiementsParModeDto>> ObtenirRepartitionPaiementsParModeAsync(DateTime? dateDebut = null, DateTime? dateFin = null, CancellationToken cancellationToken = default)
    {
        var paiements = (await _paiementRepository.GetAllAsync(cancellationToken)).ToList();
        if (dateDebut.HasValue) paiements = paiements.Where(p => p.Date >= dateDebut.Value).ToList();
        if (dateFin.HasValue) paiements = paiements.Where(p => p.Date <= dateFin.Value).ToList();

        return paiements
            .GroupBy(p => p.ModePaiement)
            .Select(g => new PaiementsParModeDto(g.Key, g.Key.ToString(), g.Count(), g.Sum(p => p.Montant.Amount)))
            .OrderByDescending(p => p.NombrePaiements)
            .ToList();
    }

    public async Task<List<NouveauxUtilisateursParMoisDto>> ObtenirNombreNouveauxUtilisateursAsync(DateTime? dateDebut = null, DateTime? dateFin = null, CancellationToken cancellationToken = default)
    {
        var utilisateurs = (await _utilisateurRepository.GetAllAsync(cancellationToken)).ToList();
        if (dateDebut.HasValue) utilisateurs = utilisateurs.Where(u => u.DateCreation >= dateDebut.Value).ToList();
        if (dateFin.HasValue) utilisateurs = utilisateurs.Where(u => u.DateCreation <= dateFin.Value).ToList();

        return utilisateurs
            .GroupBy(u => new { u.DateCreation.Year, u.DateCreation.Month })
            .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
            .Select(g => new NouveauxUtilisateursParMoisDto(g.Key.Year, g.Key.Month, g.Count()))
            .ToList();
    }

    public async Task<List<TauxOccupationHotelDto>> ObtenirTauxOccupationHotelsAsync(DateTime? dateDebut = null, DateTime? dateFin = null, CancellationToken cancellationToken = default)
    {
        var hotels = (await _hotelRepository.GetAllAsync(cancellationToken)).ToList();
        // Formule du taux d'occupation : (chambres occupées / chambres actives totales) * 100 avec garde contre la division par zéro
        return hotels
            .Select(h =>
            {
                var totalChambres = h.Chambres.Count(c => c.Actif);
                var chambresOccupees = h.Chambres.Count(c => c.Actif && c.Statut == StatutChambre.Occupee);
                var taux = totalChambres > 0 ? Math.Round((double)chambresOccupees / totalChambres * 100.0, 2) : 0.0;
                return new TauxOccupationHotelDto(h.Id, h.Nom, totalChambres, chambresOccupees, taux);
            })
            .ToList();
    }

    public async Task<List<EvolutionAvisDto>> ObtenirEvolutionAvisAsync(DateTime? dateDebut = null, DateTime? dateFin = null, CancellationToken cancellationToken = default)
    {
        var avisList = (await _avisRepository.GetAllAsync(cancellationToken)).ToList();
        if (dateDebut.HasValue) avisList = avisList.Where(a => a.Date >= dateDebut.Value).ToList();
        if (dateFin.HasValue) avisList = avisList.Where(a => a.Date <= dateFin.Value).ToList();

        return avisList
            .GroupBy(a => new { a.Date.Year, a.Date.Month })
            .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
            .Select(g => new EvolutionAvisDto(
                g.Key.Year,
                g.Key.Month,
                g.Count(),
                g.Any() ? Math.Round(g.Average(a => (double)a.Note.Valeur), 2) : 0.0))
            .ToList();
    }
}
