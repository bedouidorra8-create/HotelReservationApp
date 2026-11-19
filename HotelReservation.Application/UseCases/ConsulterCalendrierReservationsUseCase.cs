using HotelReservation.Application.DTOs;
using HotelReservation.Domain.Common;
using HotelReservation.Domain.Enums;
using HotelReservation.Domain.Repositories;

namespace HotelReservation.Application.UseCases;

public class ConsulterCalendrierReservationsUseCase
{
    private readonly IHotelRepository _hotelRepository;
    private readonly IReservationRepository _reservationRepository;

    public ConsulterCalendrierReservationsUseCase(
        IHotelRepository hotelRepository,
        IReservationRepository reservationRepository)
    {
        _hotelRepository = hotelRepository;
        _reservationRepository = reservationRepository;
    }

    public async Task<CalendrierReservationsResponse> ExecuteAsync(
        Guid hotelId,
        DateTime dateDebut,
        DateTime dateFin,
        CancellationToken cancellationToken = default)
    {
        if (dateFin <= dateDebut)
            throw new DomainException("La date de fin doit être postérieure à la date de début.");

        var hotel = await _hotelRepository.GetByIdAsync(hotelId, cancellationToken)
            ?? throw new DomainException($"Hôtel avec l'ID '{hotelId}' introuvable.");
        var chambres = hotel.Chambres.ToDictionary(c => c.Id);
        var reservations = await _reservationRepository.GetByHotelIdAsync(hotelId, cancellationToken);

        var resultats = reservations
            .Where(r => r.Statut != StatutReservation.Annulee
                && r.Periode.DateArrivee < dateFin
                && r.Periode.DateDepart > dateDebut)
            .Where(r => chambres.ContainsKey(r.ChambreId))
            .OrderBy(r => r.Periode.DateArrivee)
            .Select(r =>
            {
                var chambre = chambres[r.ChambreId];
                return new ReservationCalendrierDto(
                    r.Id,
                    hotel.Id,
                    hotel.Nom,
                    chambre.Id,
                    chambre.Numero,
                    r.ClientId,
                    r.Periode.DateArrivee,
                    r.Periode.DateDepart,
                    r.Statut,
                    r.NbPersonnes);
            })
            .ToList();

        return new CalendrierReservationsResponse(hotel.Id, hotel.Nom, dateDebut, dateFin, resultats);
    }
}
