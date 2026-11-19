using HotelReservation.Domain.Common;
using HotelReservation.Domain.Repositories;

namespace HotelReservation.Application.UseCases;

/// <summary>
/// Cas d'utilisation permettant la suppression définitive d'un avis client.
/// Accessible au client auteur de l'avis ainsi qu'aux administrateurs de la plateforme.
/// </summary>
public class SupprimerAvisUseCase
{
    private readonly IAvisRepository _avisRepository;

    /// <summary>
    /// Initialise une nouvelle instance de <see cref="SupprimerAvisUseCase"/>.
    /// </summary>
    /// <param name="avisRepository">Repository d'accès et persistance des avis.</param>
    public SupprimerAvisUseCase(IAvisRepository avisRepository)
    {
        _avisRepository = avisRepository;
    }

    /// <summary>
    /// Exécute la suppression d'un avis existant en validant les droits de l'appelant.
    /// </summary>
    public async Task ExecuteAsync(
        Guid avisId,
        Guid? utilisateurId = null,
        bool isAdministrateur = false,
        CancellationToken cancellationToken = default)
    {
        var avis = await _avisRepository.GetByIdAsync(avisId, cancellationToken)
            ?? throw new DomainException($"Avis avec l'ID '{avisId}' introuvable.");

        if (!isAdministrateur)
        {
            if (!utilisateurId.HasValue || avis.ClientId != utilisateurId.Value)
            {
                throw new DomainException("Un client ne peut supprimer que son propre avis.");
            }
        }

        await _avisRepository.DeleteAsync(avis, cancellationToken);
    }
}
