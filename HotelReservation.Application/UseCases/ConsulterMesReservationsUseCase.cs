using HotelReservation.Application.DTOs;
using HotelReservation.Domain.Enums;
using HotelReservation.Domain.Repositories;

namespace HotelReservation.Application.UseCases;

/// <summary>Retourne l'historique d'un client, avec un filtre de statut optionnel.</summary>
public class ConsulterMesReservationsUseCase
{
    private readonly IReservationRepository _repository;

    public ConsulterMesReservationsUseCase(IReservationRepository repository) => _repository = repository;

    public async Task<List<ReservationDto>> ExecuteAsync(Guid clientId, StatutReservation? statutFiltre = null, CancellationToken cancellationToken = default)
    {
        var reservations = await _repository.GetByClientIdAsync(clientId, cancellationToken);
        return reservations
            .Where(r => !statutFiltre.HasValue || r.Statut == statutFiltre.Value)
            .Select(ConsulterListeReservationsUseCase.ToDto)
            .ToList();
    }
}