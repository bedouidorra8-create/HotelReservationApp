using HotelReservation.Application.DTOs;
using HotelReservation.Domain.Repositories;

namespace HotelReservation.Application.UseCases;

public class ConsulterListeClientsUseCase
{
    private readonly IClientRepository _repository;

    public ConsulterListeClientsUseCase(IClientRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<ClientDto>> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var clients = await _repository.GetAllAsync(cancellationToken);
        return clients.Select(ToDto);
    }

    internal static ClientDto ToDto(Domain.Entities.Client client) => new(
        client.Id, client.Nom, client.Prenom, client.Email.Value, client.Telephone,
        client.DateNaissance, client.Sexe, client.Adresse, client.Photo, client.Statut, client.DateCreation);
}
