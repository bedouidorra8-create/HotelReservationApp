using HotelReservation.Domain.Common;
using HotelReservation.Domain.Repositories;

namespace HotelReservation.Application.UseCases;

/// <summary>
/// Cas d'utilisation permettant la suppression d'une chambre d'hôtel.
/// Vérifie préalablement qu'aucune réservation active n'est liée à cette chambre.
/// Conforme au pattern DDD : charge l'agrégat Hotel, supprime la chambre via la méthode de domaine, et persiste l'hôtel.
/// </summary>
public class SupprimerChambreUseCase
{
    private readonly IHotelRepository _hotelRepository;
    private readonly IReservationRepository _reservationRepository;

    /// <summary>
    /// Initialise une nouvelle instance de <see cref="SupprimerChambreUseCase"/>.
    /// </summary>
    /// <param name="hotelRepository">Repository des hôtels.</param>
    /// <param name="reservationRepository">Repository des réservations.</param>
    public SupprimerChambreUseCase(
        IHotelRepository hotelRepository,
        IReservationRepository reservationRepository)
    {
        _hotelRepository = hotelRepository;
        _reservationRepository = reservationRepository;
    }

    /// <summary>
    /// Exécute la suppression de la chambre après contrôle des réservations actives.
    /// </summary>
    /// <param name="chambreId">Identifiant unique de la chambre à supprimer.</param>
    /// <param name="cancellationToken">Jeton d'annulation asynchrone.</param>
    /// <exception cref="DomainException">Levée si la chambre est introuvable ou si elle possède des réservations actives.</exception>
    public async Task ExecuteAsync(Guid chambreId, CancellationToken cancellationToken = default)
    {
        // RÈGLE MÉTIER CRITIQUE : Interdiction absolue de supprimer une chambre rattachée à des réservations actives non terminées/annulées
        var reservationsActives = await _reservationRepository.GetReservationsActivesPourChambre(chambreId, cancellationToken);
        if (reservationsActives.Any())
        {
            throw new DomainException("Impossible de supprimer une chambre ayant des réservations actives.");
        }

        var hotels = await _hotelRepository.GetAllAsync(cancellationToken);
        var hotel = hotels.FirstOrDefault(h => h.Chambres.Any(c => c.Id == chambreId))
            ?? throw new DomainException($"Chambre avec l'ID '{chambreId}' introuvable.");

        hotel.SupprimerChambre(chambreId);
        await _hotelRepository.UpdateAsync(hotel, cancellationToken);
    }
}
