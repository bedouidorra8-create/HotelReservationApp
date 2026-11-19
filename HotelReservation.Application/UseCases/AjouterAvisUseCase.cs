using HotelReservation.Application.DTOs;
using HotelReservation.Domain.Common;
using HotelReservation.Domain.Entities;
using HotelReservation.Domain.Repositories;
using HotelReservation.Domain.ValueObjects;
using FluentValidation;

namespace HotelReservation.Application.UseCases;

public class AjouterAvisUseCase
{
    private readonly IAvisRepository _avisRepository;
    private readonly IReservationRepository _reservationRepository;
    private readonly IValidator<AjouterAvisRequest> _validator;

    public AjouterAvisUseCase(IAvisRepository avisRepository, IReservationRepository reservationRepository, IValidator<AjouterAvisRequest> validator)
    {
        _avisRepository = avisRepository;
        _reservationRepository = reservationRepository;
        _validator = validator;
    }

    public async Task<AvisDto> ExecuteAsync(Guid clientId, AjouterAvisRequest request, CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);
        var reservation = await _reservationRepository.GetByIdAsync(request.ReservationId, cancellationToken)
            ?? throw new DomainException("La réservation associée est introuvable.");
        if (reservation.ClientId != clientId || reservation.HotelId != request.HotelId)
            throw new DomainException("La réservation n'appartient pas à ce client ou à cet hôtel.");
        if (reservation.Statut != Domain.Enums.StatutReservation.Terminee)
            throw new DomainException("Un avis ne peut être ajouté qu'après une réservation terminée.");
        if ((await _avisRepository.GetByClientIdAsync(clientId, cancellationToken))
            .Any(a => a.ReservationId == request.ReservationId))
            throw new DomainException("Un avis existe déjà pour cette réservation.");

        var note = new NoteAvis(request.Note);
        var avis = new Avis(
            Guid.NewGuid(),
            clientId,
            request.HotelId,
            request.ReservationId,
            note,
            request.Commentaire
        );

        await _avisRepository.AddAsync(avis, cancellationToken);

        return new AvisDto(
            avis.Id,
            avis.ClientId,
            avis.HotelId,
            avis.ReservationId,
            avis.Note.Valeur,
            avis.Commentaire,
            avis.Date,
            avis.Statut
        );
    }
}
