using HotelReservation.Application.DTOs;
using HotelReservation.Domain.Entities;
using HotelReservation.Domain.Repositories;
using HotelReservation.Domain.ValueObjects;

namespace HotelReservation.Application.UseCases;

public class CreerClientUseCase
{
    private readonly IClientRepository _clientRepository;

    public CreerClientUseCase(IClientRepository clientRepository)
    {
        _clientRepository = clientRepository;
    }

    public async Task<ClientDto> ExecuteAsync(CreerClientRequest request, CancellationToken cancellationToken = default)
    {
        var email = new Email(request.Email);
        var client = new Client(
            Guid.NewGuid(),
            request.Nom,
            request.Prenom,
            email,
            request.MotDePasse, // Simple demo hash / string
            request.Telephone,
            request.DateNaissance,
            request.Sexe,
            request.Adresse,
            request.Photo
        );

        await _clientRepository.AddAsync(client, cancellationToken);

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
