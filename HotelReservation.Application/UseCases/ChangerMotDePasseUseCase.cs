using HotelReservation.Application.DTOs;
using HotelReservation.Domain.Common;
using HotelReservation.Domain.Repositories;

namespace HotelReservation.Application.UseCases;

public class ChangerMotDePasseUseCase
{
    private readonly IUtilisateurRepository _utilisateurRepository;

    public ChangerMotDePasseUseCase(IUtilisateurRepository utilisateurRepository)
    {
        _utilisateurRepository = utilisateurRepository;
    }

    public async Task<MessageResponse> ExecuteAsync(Guid utilisateurId, ChangerMotDePasseRequest request, CancellationToken ct = default)
    {
        var utilisateur = await _utilisateurRepository.GetByIdAsync(utilisateurId, ct);
        if (utilisateur == null)
            throw new DomainException("Utilisateur introuvable.");

        if (string.IsNullOrWhiteSpace(request.NouveauMotDePasse))
            throw new DomainException("Le nouveau mot de passe est obligatoire.");

        var nouveauHash = BCrypt.Net.BCrypt.HashPassword(request.NouveauMotDePasse);

        // La methode domain verifie l'ancien hash via BCrypt.Verify
        utilisateur.ChangerMotDePasse(
            request.AncienMotDePasse,
            nouveauHash,
            (ancienClair, hashStocke) => BCrypt.Net.BCrypt.Verify(ancienClair, hashStocke));

        await _utilisateurRepository.UpdateAsync(utilisateur, ct);

        return new MessageResponse("Mot de passe modifie avec succes.");
    }
}
