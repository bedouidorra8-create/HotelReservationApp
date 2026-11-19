using HotelReservation.Application.DTOs;
using HotelReservation.Application.Services;
using HotelReservation.Domain.Common;
using HotelReservation.Domain.Repositories;
using System.Text.Json;

namespace HotelReservation.Application.UseCases;

/// <summary>
/// Cas d'utilisation permettant la confirmation d'une réservation en attente.
/// </summary>
public class ConfirmerReservationUseCase
{
    private readonly IReservationRepository _reservationRepository;
    private readonly IAuditLogService _auditLogService;

    /// <summary>
    /// Initialise une nouvelle instance de <see cref="ConfirmerReservationUseCase"/>.
    /// </summary>
    public ConfirmerReservationUseCase(IReservationRepository reservationRepository, IAuditLogService auditLogService)
    {
        _reservationRepository = reservationRepository;
        _auditLogService = auditLogService;
    }

    /// <summary>
    /// Confirme une réservation existante.
    /// </summary>
    /// <param name="reservationId">Identifiant unique de la réservation.</param>
    /// <param name="cancellationToken">Jeton d'annulation asynchrone.</param>
    /// <returns>Le DTO de la réservation confirmée.</returns>
    /// <exception cref="DomainException">Levée si la réservation est introuvable ou n'est pas en attente.</exception>
    public async Task<ReservationDto> ExecuteAsync(Guid reservationId, CancellationToken cancellationToken = default, Guid? utilisateurId = null)
    {
        var reservation = await _reservationRepository.GetByIdAsync(reservationId, cancellationToken);
        if (reservation == null)
            throw new DomainException($"Réservation avec l'ID '{reservationId}' introuvable.");

        reservation.Confirmer();
        await _reservationRepository.UpdateAsync(reservation, cancellationToken);
        await _auditLogService.EnregistrerAsync(utilisateurId, "Confirmation", "Reservation", reservation.Id, null,
            JsonSerializer.Serialize(new { reservation.Statut }), cancellationToken);

        return ConsulterListeReservationsUseCase.ToDto(reservation);
    }
}
