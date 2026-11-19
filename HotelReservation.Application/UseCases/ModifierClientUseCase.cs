using HotelReservation.Application.DTOs;
using HotelReservation.Domain.Common;
using HotelReservation.Domain.Repositories;

namespace HotelReservation.Application.UseCases;

public class ModifierClientUseCase
{
    private readonly IClientRepository _repository;

    public ModifierClientUseCase(IClientRepository repository)
    {
        _repository = repository;
    }

    public async Task<ClientDto> ExecuteAsync(Guid clientId, ModifierClientRequest request, CancellationToken cancellationToken = default)
    {
        var client = await _repository.GetByIdAsync(clientId, cancellationToken)
            ?? throw new DomainException($"Client avec l'ID '{clientId}' introuvable.");
        client.ModifierProfil(request.Nom, request.Prenom, request.Telephone, request.Adresse, request.Photo);
        await _repository.UpdateAsync(client, cancellationToken);
        return ConsulterListeClientsUseCase.ToDto(client);
    }
}
