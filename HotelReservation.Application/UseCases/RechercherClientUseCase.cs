using HotelReservation.Application.DTOs;
using HotelReservation.Domain.Repositories;

namespace HotelReservation.Application.UseCases;

public class RechercherClientUseCase
{
    private readonly IClientRepository _repository;

    public RechercherClientUseCase(IClientRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<ClientDto>> ExecuteAsync(RechercherClientRequest request, CancellationToken cancellationToken = default)
    {
        var clients = await _repository.GetAllAsync(cancellationToken);
        var terme = request.Terme?.Trim();
        if (string.IsNullOrWhiteSpace(terme))
            return clients.Select(ConsulterListeClientsUseCase.ToDto);

        return clients.Where(client =>
                client.Nom.Contains(terme, StringComparison.OrdinalIgnoreCase)
                || client.Prenom.Contains(terme, StringComparison.OrdinalIgnoreCase)
                || client.Email.Value.Contains(terme, StringComparison.OrdinalIgnoreCase)
                || client.Telephone.Contains(terme, StringComparison.OrdinalIgnoreCase))
            .Select(ConsulterListeClientsUseCase.ToDto);
    }
}
