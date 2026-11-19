using HotelReservation.Application.DTOs;
using HotelReservation.Application.Services;
using HotelReservation.Domain.Common;
using HotelReservation.Domain.Repositories;
using System.Text.Json;

namespace HotelReservation.Application.UseCases;

/// <summary>
/// Cas d'utilisation permettant l'annulation d'une réservation.
/// </summary>
public class AnnulerReservationUseCase
{
    private readonly IReservationRepository _reservationRepository;
    private readonly IAuditLogService _auditLogService;

    /// <summary>
    /// Initialise une nouvelle instance de <see cref="AnnulerReservationUseCase"/>.
    /// </summary>
    public AnnulerReservationUseCase(IReservationRepository reservationRepository, IAuditLogService auditLogService)
    {
        _reservationRepository = reservationRepository;
        _auditLogService = auditLogService;
    }

    /// <summary>
    /// Annule une réservation existante.
    /// </summary>
    /// <param name="reservationId">Identifiant unique de la réservation.</param>
    /// <param name="cancellationToken">Jeton d'annulation asynchrone.</param>
    /// <returns>Le DTO de la réservation annulée.</returns>
    /// <exception cref="DomainException">Levée si la réservation est introuvable ou déjà terminée.</exception>
    public async Task<ReservationDto> ExecuteAsync(Guid reservationId, CancellationToken cancellationToken = default, Guid? utilisateurId = null, bool isPersonnel = true)
    {
        var reservation = await _reservationRepository.GetByIdAsync(reservationId, cancellationToken);
        if (reservation == null)
            throw new DomainException($"Réservation avec l'ID '{reservationId}' introuvable.");

        if (!isPersonnel && utilisateurId.HasValue && reservation.ClientId != utilisateurId.Value)
            throw new DomainException("Accès refusé : vous n'êtes pas propriétaire de cette réservation.");

        reservation.Annuler();
        await _reservationRepository.UpdateAsync(reservation, cancellationToken);
        await _auditLogService.EnregistrerAsync(utilisateurId, "Annulation", "Reservation", reservation.Id, null,
            JsonSerializer.Serialize(new { reservation.Statut }), cancellationToken);

        return ConsulterListeReservationsUseCase.ToDto(reservation);
    }
}
