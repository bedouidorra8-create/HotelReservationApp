using HotelReservation.Application.DTOs;
using HotelReservation.Domain.Common;
using HotelReservation.Domain.Repositories;
using HotelReservation.Domain.ValueObjects;

namespace HotelReservation.Application.UseCases;

public class ModifierAvisUseCase
{
    private readonly IAvisRepository _avisRepository;
    private readonly IReservationRepository _reservationRepository;

    public ModifierAvisUseCase(IAvisRepository avisRepository, IReservationRepository reservationRepository)
    {
        _avisRepository = avisRepository;
        _reservationRepository = reservationRepository;
    }

    public async Task<AvisDto> ExecuteAsync(
        Guid avisId,
        Guid clientId,
        ModifierAvisRequest request,
        CancellationToken cancellationToken = default)
    {
        var avis = await _avisRepository.GetByIdAsync(avisId, cancellationToken)
            ?? throw new DomainException($"Avis avec l'ID '{avisId}' introuvable.");

        if (avis.ClientId != clientId)
            throw new DomainException("Un client ne peut modifier que ses propres avis.");
        var reservation = await _reservationRepository.GetByIdAsync(avis.ReservationId, cancellationToken)
            ?? throw new DomainException("La réservation associée est introuvable.");
        if (reservation.Statut != Domain.Enums.StatutReservation.Terminee)
            throw new DomainException("Un avis ne peut être modifié qu'après une réservation terminée.");

        avis.Modifier(new NoteAvis(request.Note), request.Commentaire);
        await _avisRepository.UpdateAsync(avis, cancellationToken);

        return new AvisDto(
            avis.Id,
            avis.ClientId,
            avis.HotelId,
            avis.ReservationId,
            avis.Note.Valeur,
            avis.Commentaire,
            avis.Date,
            avis.Statut);
    }
}
