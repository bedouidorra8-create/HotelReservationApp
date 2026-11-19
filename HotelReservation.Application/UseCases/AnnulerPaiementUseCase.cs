using HotelReservation.Application.DTOs;
using HotelReservation.Domain.Common;
using HotelReservation.Domain.Repositories;

namespace HotelReservation.Application.UseCases;

public class AnnulerPaiementUseCase
{
    private readonly IPaiementRepository _repository;
    private readonly IReservationRepository _reservationRepository;

    public AnnulerPaiementUseCase(IPaiementRepository repository, IReservationRepository reservationRepository)
    {
        _repository = repository;
        _reservationRepository = reservationRepository;
    }

    public async Task<PaiementDto> ExecuteAsync(Guid paiementId, CancellationToken cancellationToken = default)
    {
        var paiement = await _repository.GetByIdAsync(paiementId, cancellationToken)
            ?? throw new DomainException($"Paiement '{paiementId}' introuvable.");

        paiement.Annuler();
        await _repository.UpdateAsync(paiement, cancellationToken);

        var reservation = await _reservationRepository.GetByIdAsync(paiement.ReservationId, cancellationToken)
            ?? throw new DomainException($"Réservation '{paiement.ReservationId}' introuvable.");
        reservation.Annuler();
        await _reservationRepository.UpdateAsync(reservation, cancellationToken);

        return new PaiementDto(
            paiement.Id, paiement.ReservationId, paiement.Montant.Amount, paiement.Montant.Currency,
            paiement.Date, paiement.ModePaiement, paiement.Reference, paiement.Statut, paiement.EnregistrePar);
    }
}
