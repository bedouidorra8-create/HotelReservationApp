using HotelReservation.Domain.Common;
using HotelReservation.Domain.Repositories;

namespace HotelReservation.Application.UseCases;

public class SupprimerClientUseCase
{
    private readonly IClientRepository _repository;

    public SupprimerClientUseCase(IClientRepository repository)
    {
        _repository = repository;
    }

    public async Task ExecuteAsync(Guid clientId, CancellationToken cancellationToken = default)
    {
        var client = await _repository.GetByIdAsync(clientId, cancellationToken)
            ?? throw new DomainException($"Client avec l'ID '{clientId}' introuvable.");
        await _repository.DeleteAsync(client, cancellationToken);
    }
}
