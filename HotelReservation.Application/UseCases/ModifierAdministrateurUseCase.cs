using HotelReservation.Application.DTOs;
using HotelReservation.Domain.Common;
using HotelReservation.Domain.Entities;
using HotelReservation.Domain.Repositories;

namespace HotelReservation.Application.UseCases;

/// <summary>
/// Cas d'utilisation permettant la modification du profil d'un compte administrateur.
/// </summary>
public class ModifierAdministrateurUseCase
{
    private readonly IUtilisateurRepository _repository;

    /// <summary>
    /// Initialise une nouvelle instance de <see cref="ModifierAdministrateurUseCase"/>.
    /// </summary>
    /// <param name="repository">Repository des utilisateurs.</param>
    public ModifierAdministrateurUseCase(IUtilisateurRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Exécute la modification des informations de profil de l'administrateur.
    /// </summary>
    /// <param name="administrateurId">Identifiant unique de l'administrateur.</param>
    /// <param name="request">Données à modifier (nom, prénom, téléphone, adresse, photo).</param>
    /// <param name="cancellationToken">Jeton d'annulation asynchrone.</param>
    /// <returns>Le DTO de l'administrateur mis à jour.</returns>
    /// <exception cref="DomainException">Levée si l'utilisateur est introuvable ou n'est pas un administrateur.</exception>
    public async Task<UtilisateurDto> ExecuteAsync(Guid administrateurId, ModifierAdministrateurRequest request, CancellationToken cancellationToken = default)
    {
        var utilisateur = await _repository.GetByIdAsync(administrateurId, cancellationToken)
            ?? throw new DomainException($"Administrateur avec l'ID '{administrateurId}' introuvable.");

        if (utilisateur is not Administrateur)
            throw new DomainException("L'utilisateur ciblé n'est pas un administrateur.");

        utilisateur.ModifierProfil(request.Nom, request.Prenom, request.Telephone, request.Adresse, request.Photo);
        await _repository.UpdateAsync(utilisateur, cancellationToken);

        return ConsulterListeUtilisateursUseCase.ToDto(utilisateur);
    }
}
