using HotelReservation.Application.DTOs;
using HotelReservation.Domain.Common;
using HotelReservation.Domain.Repositories;

namespace HotelReservation.Application.UseCases;

public class ActiverDesactiverCompteUseCase
{
    private readonly IUtilisateurRepository _repository;

    public ActiverDesactiverCompteUseCase(IUtilisateurRepository repository)
    {
        _repository = repository;
    }

    public async Task<UtilisateurDto> ExecuteAsync(Guid utilisateurId, bool actif, CancellationToken cancellationToken = default)
    {
        var utilisateur = await _repository.GetByIdAsync(utilisateurId, cancellationToken)
            ?? throw new DomainException($"Utilisateur avec l'ID '{utilisateurId}' introuvable.");
        if (actif) utilisateur.Activer(); else utilisateur.Desactiver();
        await _repository.UpdateAsync(utilisateur, cancellationToken);
        return ConsulterListeUtilisateursUseCase.ToDto(utilisateur);
    }
}
