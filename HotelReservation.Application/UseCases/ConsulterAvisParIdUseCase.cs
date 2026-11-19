using HotelReservation.Application.DTOs;
using HotelReservation.Domain.Common;
using HotelReservation.Domain.Repositories;

namespace HotelReservation.Application.UseCases;

public class ConsulterAvisParIdUseCase
{
    private readonly IAvisRepository _avisRepository;

    public ConsulterAvisParIdUseCase(IAvisRepository avisRepository)
    {
        _avisRepository = avisRepository;
    }

    public async Task<AvisDto> ExecuteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var avis = await _avisRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new DomainException($"Avis avec l'ID '{id}' introuvable.");

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
