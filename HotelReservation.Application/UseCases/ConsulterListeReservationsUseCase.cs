using HotelReservation.Application.DTOs;
using HotelReservation.Domain.Entities;
using HotelReservation.Domain.Repositories;

namespace HotelReservation.Application.UseCases;

/// <summary>
/// Cas d'utilisation permettant de consulter l'ensemble des réservations du système.
/// </summary>
public class ConsulterListeReservationsUseCase
{
    private readonly IReservationRepository _repository;

    /// <summary>
    /// Initialise une nouvelle instance de <see cref="ConsulterListeReservationsUseCase"/>.
    /// </summary>
    /// <param name="repository">Repository des réservations.</param>
    public ConsulterListeReservationsUseCase(IReservationRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Récupère la liste de toutes les réservations.
    /// </summary>
    /// <param name="cancellationToken">Jeton d'annulation asynchrone.</param>
    /// <returns>La liste des DTOs de réservations.</returns>
    public async Task<IEnumerable<ReservationDto>> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var reservations = await _repository.GetAllAsync(cancellationToken);
        return reservations.Select(ToDto);
    }

    /// <summary>
    /// Mappe une entité Reservation vers son DTO complet incluant les montants financiers.
    /// </summary>
    public static ReservationDto ToDto(Reservation reservation) => new(
        reservation.Id,
        reservation.ChambreId,
        reservation.ClientId,
        reservation.Periode.DateArrivee,
        reservation.Periode.DateDepart,
        reservation.NbNuits,
        reservation.NbPersonnes,
        reservation.Statut,
        reservation.DateCreation,
        reservation.PrixUnitaire?.Amount ?? 0m,
        reservation.PrixUnitaire?.Currency ?? "EUR",
        reservation.Total?.Amount ?? 0m,
        reservation.Total?.Currency ?? "EUR"
    );
}
