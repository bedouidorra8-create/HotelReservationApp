using HotelReservation.Application.DTOs;
using HotelReservation.Application.Services;
using HotelReservation.Domain.Common;
using HotelReservation.Domain.Enums;
using HotelReservation.Domain.Repositories;
using System.Text.Json;

namespace HotelReservation.Application.UseCases;

/// <summary>
/// Cas d'utilisation permettant la modification manuelle du statut d'une réservation (administration et agents).
/// </summary>
public class ChangerStatutReservationUseCase
{
    private readonly IReservationRepository _repository;
    private readonly IAuditLogService _auditLogService;

    /// <summary>
    /// Initialise une nouvelle instance de <see cref="ChangerStatutReservationUseCase"/>.
    /// </summary>
    /// <param name="repository">Repository des réservations.</param>
    public ChangerStatutReservationUseCase(IReservationRepository repository, IAuditLogService auditLogService)
    {
        _repository = repository;
        _auditLogService = auditLogService;
    }

    /// <summary>
    /// Modifie le statut de la réservation cible.
    /// </summary>
    /// <param name="reservationId">Identifiant de la réservation.</param>
    /// <param name="statut">Nouveau statut.</param>
    /// <param name="cancellationToken">Jeton d'annulation asynchrone.</param>
    /// <returns>Le DTO de la réservation mise à jour.</returns>
    /// <exception cref="DomainException">Levée si la réservation est introuvable ou si la transition est illégale.</exception>
    public async Task<ReservationDto> ExecuteAsync(Guid reservationId, StatutReservation statut, CancellationToken cancellationToken = default, Guid? utilisateurId = null)
    {
        var reservation = await _repository.GetByIdAsync(reservationId, cancellationToken)
            ?? throw new DomainException($"Réservation avec l'ID '{reservationId}' introuvable.");
        var ancienStatut = reservation.Statut;

        if (statut == StatutReservation.Confirmee)
            reservation.Confirmer();
        else if (statut == StatutReservation.Annulee)
            reservation.Annuler();
        else
            reservation.ChangerStatut(statut);
        await _repository.UpdateAsync(reservation, cancellationToken);
        await _auditLogService.EnregistrerAsync(utilisateurId, "ChangementStatut", "Reservation", reservation.Id,
            JsonSerializer.Serialize(new { Statut = ancienStatut }), JsonSerializer.Serialize(new { reservation.Statut }), cancellationToken);

        return ConsulterListeReservationsUseCase.ToDto(reservation);
    }
}
