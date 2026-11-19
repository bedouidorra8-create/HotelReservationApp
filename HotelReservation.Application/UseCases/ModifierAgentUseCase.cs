using HotelReservation.Application.DTOs;
using HotelReservation.Domain.Common;
using HotelReservation.Domain.Entities;
using HotelReservation.Domain.Repositories;

namespace HotelReservation.Application.UseCases;

public class ModifierAgentUseCase
{
    private readonly IUtilisateurRepository _repository;

    public ModifierAgentUseCase(IUtilisateurRepository repository)
    {
        _repository = repository;
    }

    public async Task<UtilisateurDto> ExecuteAsync(Guid agentId, ModifierAgentRequest request, CancellationToken cancellationToken = default)
    {
        var utilisateur = await _repository.GetByIdAsync(agentId, cancellationToken)
            ?? throw new DomainException($"Agent avec l'ID '{agentId}' introuvable.");
        if (utilisateur is not Agent)
            throw new DomainException("L'utilisateur ciblé n'est pas un agent.");

        utilisateur.ModifierProfil(request.Nom, request.Prenom, request.Telephone, request.Adresse, request.Photo);
        await _repository.UpdateAsync(utilisateur, cancellationToken);
        return ConsulterListeUtilisateursUseCase.ToDto(utilisateur);
    }
}
