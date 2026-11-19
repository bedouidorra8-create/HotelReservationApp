using HotelReservation.Application.DTOs;
using HotelReservation.Domain.Common;
using HotelReservation.Domain.Entities;
using HotelReservation.Domain.Repositories;
using HotelReservation.Domain.ValueObjects;
using HotelReservation.Domain.Enums;
using FluentValidation;

namespace HotelReservation.Application.UseCases;

public class EnregistrerPaiementUseCase
{
    private readonly IPaiementRepository _paiementRepository;
    private readonly IReservationRepository _reservationRepository;
    private readonly IValidator<EnregistrerPaiementRequest> _validator;

    public EnregistrerPaiementUseCase(
        IPaiementRepository paiementRepository,
        IReservationRepository reservationRepository,
        IValidator<EnregistrerPaiementRequest> validator)
    {
        _paiementRepository = paiementRepository;
        _reservationRepository = reservationRepository;
        _validator = validator;
    }

    public async Task<PaiementDto> ExecuteAsync(Guid clientId, EnregistrerPaiementRequest request, CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);
        var reservation = await _reservationRepository.GetByIdAsync(request.ReservationId, cancellationToken);
        if (reservation == null)
            throw new DomainException($"Réservation '{request.ReservationId}' introuvable.");

        if (reservation.ClientId != clientId)
            throw new DomainException("La réservation n'appartient pas à ce client.");

        if (reservation.Statut == StatutReservation.Annulee)
            throw new DomainException("Impossible d'enregistrer un paiement pour une réservation annulée.");

        var paiements = await _paiementRepository.GetByReservationIdAsync(request.ReservationId, cancellationToken);
        if (paiements.Any(p => p.Statut == StatutPaiement.Paye))
            throw new DomainException("Cette réservation possède déjà un paiement.");

        if (request.Montant != reservation.Total.Amount)
            throw new DomainException("Le montant du paiement doit correspondre exactement au total de la réservation.");

        if (!string.Equals(request.Devise, reservation.Total.Currency, StringComparison.OrdinalIgnoreCase))
            throw new DomainException("La devise du paiement doit correspondre à celle de la réservation.");

        var montant = new Money(request.Montant, request.Devise);
        var paiement = new Paiement(
            Guid.NewGuid(),
            request.ReservationId,
            montant,
            request.ModePaiement,
            request.Reference
        );

        await _paiementRepository.AddAsync(paiement, cancellationToken);

        return new PaiementDto(
            paiement.Id,
            paiement.ReservationId,
            paiement.Montant.Amount,
            paiement.Montant.Currency,
            paiement.Date,
            paiement.ModePaiement,
            paiement.Reference,
            paiement.Statut,
            paiement.EnregistrePar
        );
    }
}
