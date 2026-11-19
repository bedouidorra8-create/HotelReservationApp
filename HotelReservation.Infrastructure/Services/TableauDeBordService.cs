using HotelReservation.Application.DTOs;
using HotelReservation.Application.Services;
using HotelReservation.Application.UseCases;
using HotelReservation.Domain.Enums;
using HotelReservation.Domain.Repositories;

namespace HotelReservation.Infrastructure.Services;

public class TableauDeBordService : ITableauDeBordService
{
    private readonly IUtilisateurRepository _utilisateurRepository;
    private readonly IHotelRepository _hotelRepository;
    private readonly IReservationRepository _reservationRepository;
    private readonly IPaiementRepository _paiementRepository;

    public TableauDeBordService(
        IUtilisateurRepository utilisateurRepository,
        IHotelRepository hotelRepository,
        IReservationRepository reservationRepository,
        IPaiementRepository paiementRepository)
    {
        _utilisateurRepository = utilisateurRepository;
        _hotelRepository = hotelRepository;
        _reservationRepository = reservationRepository;
        _paiementRepository = paiementRepository;
    }

    public async Task<int> ObtenirNombreUtilisateursAsync(CancellationToken cancellationToken = default)
    {
        var utilisateurs = await _utilisateurRepository.GetAllAsync(cancellationToken);
        return utilisateurs.Count();
    }

    public async Task<int> ObtenirNombreHotelsAsync(CancellationToken cancellationToken = default)
    {
        var hotels = await _hotelRepository.GetAllAsync(cancellationToken);
        return hotels.Count();
    }

    public async Task<int> ObtenirNombreReservationsAsync(CancellationToken cancellationToken = default)
    {
        var reservations = await _reservationRepository.GetAllAsync(cancellationToken);
        return reservations.Count();
    }

    public async Task<int> ObtenirReservationsDuJourAsync(CancellationToken cancellationToken = default)
    {
        var reservations = await _reservationRepository.GetAllAsync(cancellationToken);
        var today = DateTime.UtcNow.Date;
        return reservations.Count(r => r.DateCreation.Date == today);
    }

    public async Task<List<ReservationDto>> ObtenirDernieresReservationsAsync(int count = 10, CancellationToken cancellationToken = default)
    {
        var reservations = await _reservationRepository.GetAllAsync(cancellationToken);
        return reservations
            .OrderByDescending(r => r.DateCreation)
            .Take(count)
            .Select(ConsulterListeReservationsUseCase.ToDto)
            .ToList();
    }

    public async Task<int> ObtenirPaiementsEnAttenteAsync(CancellationToken cancellationToken = default)
    {
        var paiements = await _paiementRepository.GetAllAsync(cancellationToken);
        return paiements.Count(p => p.Statut == StatutPaiement.EnAttente);
    }

    public async Task<decimal> ObtenirChiffreAffairesAsync(CancellationToken cancellationToken = default)
    {
        var paiements = await _paiementRepository.GetAllAsync(cancellationToken);
        return paiements
            .Where(p => p.Statut == StatutPaiement.Paye)
            .Sum(p => p.Montant.Amount);
    }

    public async Task<Dictionary<string, int>> ObtenirGraphiqueReservationsParMoisAsync(CancellationToken cancellationToken = default)
    {
        var reservations = await _reservationRepository.GetAllAsync(cancellationToken);
        var today = DateTime.UtcNow.Date;
        var mois12 = Enumerable.Range(0, 12)
            .Select(i => new DateTime(today.Year, today.Month, 1).AddMonths(-11 + i))
            .ToList();

        return mois12.ToDictionary(
            m => m.ToString("yyyy-MM"),
            m => reservations.Count(r => r.DateCreation.Year == m.Year && r.DateCreation.Month == m.Month)
        );
    }

    public async Task<Dictionary<string, decimal>> ObtenirGraphiqueRevenusParMoisAsync(CancellationToken cancellationToken = default)
    {
        var paiements = await _paiementRepository.GetAllAsync(cancellationToken);
        var today = DateTime.UtcNow.Date;
        var mois12 = Enumerable.Range(0, 12)
            .Select(i => new DateTime(today.Year, today.Month, 1).AddMonths(-11 + i))
            .ToList();

        return mois12.ToDictionary(
            m => m.ToString("yyyy-MM"),
            m => paiements
                .Where(p => p.Statut == StatutPaiement.Paye && p.Date.Year == m.Year && p.Date.Month == m.Month)
                .Sum(p => p.Montant.Amount)
        );
    }
}
