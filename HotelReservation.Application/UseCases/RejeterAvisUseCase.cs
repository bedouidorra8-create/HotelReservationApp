using HotelReservation.Application.DTOs;
using HotelReservation.Domain.Common;
using HotelReservation.Domain.Repositories;

namespace HotelReservation.Application.UseCases;

public class RejeterAvisUseCase
{
    private readonly IAvisRepository _repository;

    public RejeterAvisUseCase(IAvisRepository repository)
    {
        _repository = repository;
    }

    public async Task<AvisDto> ExecuteAsync(Guid avisId, CancellationToken cancellationToken = default)
    {
        var avis = await _repository.GetByIdAsync(avisId, cancellationToken)
            ?? throw new DomainException($"Avis avec l'ID '{avisId}' introuvable.");
        avis.Rejeter();
        await _repository.UpdateAsync(avis, cancellationToken);
        return ValiderAvisUseCase.ToDto(avis);
    }
}
