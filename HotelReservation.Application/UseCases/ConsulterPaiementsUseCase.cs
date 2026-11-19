using HotelReservation.Application.DTOs;
using HotelReservation.Domain.Repositories;

namespace HotelReservation.Application.UseCases;

public class ConsulterPaiementsUseCase
{
    private readonly IPaiementRepository _repository;
    private readonly IReservationRepository _reservationRepository;

    public ConsulterPaiementsUseCase(IPaiementRepository repository, IReservationRepository reservationRepository)
    {
        _repository = repository;
        _reservationRepository = reservationRepository;
    }

    public async Task<IEnumerable<PaiementDto>> ExecuteAsync(Guid? reservationId = null, CancellationToken cancellationToken = default)
    {
        var paiements = reservationId.HasValue
            ? await _repository.GetByReservationIdAsync(reservationId.Value, cancellationToken)
            : await _repository.GetAllAsync(cancellationToken);
        return paiements.Select(p => new PaiementDto(
            p.Id, p.ReservationId, p.Montant.Amount, p.Montant.Currency,
            p.Date, p.ModePaiement, p.Reference, p.Statut, p.EnregistrePar));
    }

    public async Task<IEnumerable<PaiementDto>> ExecuteForClientAsync(Guid clientId, CancellationToken cancellationToken = default)
    {
        var reservations = await _reservationRepository.GetByClientIdAsync(clientId, cancellationToken);
        var reservationIds = reservations.Select(reservation => reservation.Id).ToHashSet();
        var paiements = await _repository.GetAllAsync(cancellationToken);

        return paiements
            .Where(paiement => reservationIds.Contains(paiement.ReservationId))
            .Select(p => new PaiementDto(
                p.Id, p.ReservationId, p.Montant.Amount, p.Montant.Currency,
                p.Date, p.ModePaiement, p.Reference, p.Statut, p.EnregistrePar));
    }
}
