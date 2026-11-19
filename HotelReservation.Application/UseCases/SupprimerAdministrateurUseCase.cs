using HotelReservation.Domain.Common;
using HotelReservation.Domain.Entities;
using HotelReservation.Domain.Repositories;

namespace HotelReservation.Application.UseCases;

/// <summary>
/// Cas d'utilisation permettant la suppression définitive d'un compte administrateur.
/// </summary>
public class SupprimerAdministrateurUseCase
{
    private readonly IUtilisateurRepository _repository;

    /// <summary>
    /// Initialise une nouvelle instance de <see cref="SupprimerAdministrateurUseCase"/>.
    /// </summary>
    /// <param name="repository">Repository des utilisateurs.</param>
    public SupprimerAdministrateurUseCase(IUtilisateurRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Exécute la suppression d'un compte administrateur.
    /// </summary>
    /// <param name="administrateurId">Identifiant unique de l'administrateur à supprimer.</param>
    /// <param name="cancellationToken">Jeton d'annulation asynchrone.</param>
    /// <exception cref="DomainException">Levée si l'utilisateur est introuvable ou n'est pas un administrateur.</exception>
    public async Task ExecuteAsync(Guid administrateurId, CancellationToken cancellationToken = default)
    {
        var utilisateur = await _repository.GetByIdAsync(administrateurId, cancellationToken)
            ?? throw new DomainException($"Administrateur avec l'ID '{administrateurId}' introuvable.");

        if (utilisateur is not Administrateur)
            throw new DomainException("L'utilisateur ciblé n'est pas un administrateur.");

        await _repository.DeleteAsync(utilisateur, cancellationToken);
    }
}
