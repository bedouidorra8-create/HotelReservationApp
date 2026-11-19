using HotelReservation.Application.DTOs;
using HotelReservation.Domain.Common;
using HotelReservation.Domain.Repositories;

namespace HotelReservation.Application.UseCases;

/// <summary>
/// Cas d'utilisation de consultation d'une réservation par son identifiant.
/// Gère le contrôle d'accès : un Client ne peut consulter que SA réservation (sinon DomainException / 403),
/// tandis que le Personnel (Administrateur, Agent) peut consulter toute réservation.
/// </summary>
public class ConsulterReservationUseCase
{
    private readonly IReservationRepository _reservationRepository;

    public ConsulterReservationUseCase(IReservationRepository reservationRepository)
    {
        _reservationRepository = reservationRepository;
    }

    public async Task<ReservationDto> ExecuteAsync(
        Guid reservationId,
        Guid requesterId,
        bool isPersonnel,
        CancellationToken cancellationToken = default)
    {
        var reservation = await _reservationRepository.GetByIdAsync(reservationId, cancellationToken)
            ?? throw new DomainException($"Réservation avec l'ID '{reservationId}' introuvable.");

        if (!isPersonnel && reservation.ClientId != requesterId)
        {
            throw new DomainException("Accès refusé : vous n'êtes pas propriétaire de cette réservation.");
        }

        return ConsulterListeReservationsUseCase.ToDto(reservation);
    }
}
