using HotelReservation.Application.DTOs;
using HotelReservation.Domain.Common;
using HotelReservation.Domain.Repositories;

namespace HotelReservation.Application.UseCases;

public class ValiderAvisUseCase
{
    private readonly IAvisRepository _repository;

    public ValiderAvisUseCase(IAvisRepository repository)
    {
        _repository = repository;
    }

    public async Task<AvisDto> ExecuteAsync(Guid avisId, CancellationToken cancellationToken = default)
    {
        var avis = await _repository.GetByIdAsync(avisId, cancellationToken)
            ?? throw new DomainException($"Avis avec l'ID '{avisId}' introuvable.");
        avis.Valider();
        await _repository.UpdateAsync(avis, cancellationToken);
        return ToDto(avis);
    }

    internal static AvisDto ToDto(Domain.Entities.Avis avis) => new(
        avis.Id, avis.ClientId, avis.HotelId, avis.ReservationId,
        avis.Note.Valeur, avis.Commentaire, avis.Date, avis.Statut);
}
