using HotelReservation.Application.DTOs;
using HotelReservation.Domain.Common;
using HotelReservation.Domain.Repositories;

namespace HotelReservation.Application.UseCases;

public class ReinitialiserMotDePasseUseCase
{
    private readonly IUtilisateurRepository _utilisateurRepository;

    public ReinitialiserMotDePasseUseCase(IUtilisateurRepository utilisateurRepository)
    {
        _utilisateurRepository = utilisateurRepository;
    }

    public async Task<MessageResponse> ExecuteAsync(ReinitialiserMotDePasseRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.NouveauMotDePasse))
            throw new DomainException("Le nouveau mot de passe est obligatoire.");

        // Retrouver l'utilisateur via le token de reset
        var tous = await _utilisateurRepository.GetAllAsync(ct);
        var utilisateur = tous.FirstOrDefault(u => u.TokenResetMotDePasse == request.Token);

        if (utilisateur == null)
            throw new DomainException("Token de reinitialisation invalide.");

        var nouveauHash = BCrypt.Net.BCrypt.HashPassword(request.NouveauMotDePasse);

        // La methode domain verifie l'expiration et invalide le token (usage unique)
        utilisateur.ReinitialiserMotDePasse(request.Token, nouveauHash);
        await _utilisateurRepository.UpdateAsync(utilisateur, ct);

        return new MessageResponse("Mot de passe reinitialise avec succes.");
    }
}
