using HotelReservation.Domain.Common;
using HotelReservation.Domain.Repositories;

namespace HotelReservation.Application.UseCases;

/// <summary>Supprime un favori uniquement si l'utilisateur courant en est propriétaire.</summary>
public class SupprimerFavoriUseCase
{
    private readonly IFavoriRepository _favoriRepository;

    public SupprimerFavoriUseCase(IFavoriRepository favoriRepository) => _favoriRepository = favoriRepository;

    public async Task ExecuteAsync(Guid utilisateurId, Guid favoriId, CancellationToken cancellationToken = default)
    {
        var favori = await _favoriRepository.GetByIdAsync(favoriId, cancellationToken)
            ?? throw new DomainException("Ce favori est introuvable.");
        if (favori.UtilisateurId != utilisateurId)
            throw new DomainException("Vous ne pouvez supprimer que vos propres favoris.");
        await _favoriRepository.DeleteAsync(favori, cancellationToken);
    }
}