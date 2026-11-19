using HotelReservation.Application.DTOs;
using HotelReservation.Domain.Common;
using HotelReservation.Domain.Enums;
using HotelReservation.Domain.Repositories;
using HotelReservation.Domain.ValueObjects;

namespace HotelReservation.Application.UseCases;

public class ConsulterDisponibilitesUseCase
{
    private readonly IHotelRepository _hotelRepository;
    private readonly IReservationRepository _reservationRepository;

    public ConsulterDisponibilitesUseCase(
        IHotelRepository hotelRepository,
        IReservationRepository reservationRepository)
    {
        _hotelRepository = hotelRepository;
        _reservationRepository = reservationRepository;
    }

    public async Task<DisponibilitesHotelResponse> ExecuteAsync(
        Guid hotelId,
        DateTime dateDebut,
        DateTime dateFin,
        CancellationToken cancellationToken = default)
    {
        var hotel = await _hotelRepository.GetByIdAsync(hotelId, cancellationToken);
        if (hotel == null)
            throw new DomainException($"Hôtel avec l'ID '{hotelId}' introuvable.");

        var queryRange = new DateRange(dateDebut, dateFin);

        var allHotelReservations = await _reservationRepository.GetByHotelIdAsync(hotelId, cancellationToken);
        var activeReservations = allHotelReservations.Where(r => r.Statut != StatutReservation.Annulee).ToList();

        var chambreDisponibilites = new List<DisponibiliteChambreDto>();

        foreach (var chambre in hotel.Chambres.Where(c => c.Actif))
        {
            var statutParJour = new Dictionary<string, string>();
            var currentDate = dateDebut.Date;

            while (currentDate < dateFin.Date)
            {
                var dayKey = currentDate.ToString("yyyy-MM-dd");

                if (chambre.Statut == StatutChambre.Maintenance)
                {
                    statutParJour[dayKey] = "Maintenance";
                }
                else
                {
                    // Check if there is an active reservation overlapping currentDate
                    var isReserved = activeReservations.Any(r =>
                        r.ChambreId == chambre.Id &&
                        currentDate >= r.Periode.DateArrivee &&
                        currentDate < r.Periode.DateDepart);

                    statutParJour[dayKey] = isReserved ? "Occupee" : "Libre";
                }

                currentDate = currentDate.AddDays(1);
            }

            chambreDisponibilites.Add(new DisponibiliteChambreDto(
                chambre.Id,
                chambre.Numero,
                chambre.Type,
                chambre.Etage,
                chambre.Prix.Amount,
                chambre.Prix.Currency,
                statutParJour
            ));
        }

        return new DisponibilitesHotelResponse(
            hotel.Id,
            hotel.Nom,
            dateDebut,
            dateFin,
            chambreDisponibilites
        );
    }
}
