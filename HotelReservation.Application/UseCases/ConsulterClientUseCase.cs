using HotelReservation.Application.DTOs;
using HotelReservation.Domain.Common;
using HotelReservation.Domain.Repositories;

namespace HotelReservation.Application.UseCases;

public class ConsulterClientUseCase
{
    private readonly IClientRepository _clientRepository;

    public ConsulterClientUseCase(IClientRepository clientRepository)
    {
        _clientRepository = clientRepository;
    }

    public async Task<ClientDto> ExecuteAsync(Guid clientId, CancellationToken cancellationToken = default)
    {
        var client = await _clientRepository.GetByIdAsync(clientId, cancellationToken)
            ?? throw new DomainException($"Client avec l'ID '{clientId}' introuvable.");

        return new ClientDto(
            client.Id,
            client.Nom,
            client.Prenom,
            client.Email.Value,
            client.Telephone,
            client.DateNaissance,
            client.Sexe,
            client.Adresse,
            client.Photo,
            client.Statut,
            client.DateCreation
        );
    }
}
