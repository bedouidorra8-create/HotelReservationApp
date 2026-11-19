using HotelReservation.Domain.Common;
using HotelReservation.Domain.Entities;
using HotelReservation.Domain.Repositories;

namespace HotelReservation.Application.UseCases;

public class SupprimerAgentUseCase
{
    private readonly IUtilisateurRepository _repository;

    public SupprimerAgentUseCase(IUtilisateurRepository repository)
    {
        _repository = repository;
    }

    public async Task ExecuteAsync(Guid agentId, CancellationToken cancellationToken = default)
    {
        var utilisateur = await _repository.GetByIdAsync(agentId, cancellationToken)
            ?? throw new DomainException($"Agent avec l'ID '{agentId}' introuvable.");
        if (utilisateur is not Agent)
            throw new DomainException("L'utilisateur ciblé n'est pas un agent.");
        await _repository.DeleteAsync(utilisateur, cancellationToken);
    }
}
