using HotelReservation.Application.DTOs;
using HotelReservation.Domain.Common;
using HotelReservation.Domain.Repositories;

namespace HotelReservation.Application.UseCases;

public class VerifierEmailUseCase
{
    private readonly IUtilisateurRepository _utilisateurRepository;

    public VerifierEmailUseCase(IUtilisateurRepository utilisateurRepository)
    {
        _utilisateurRepository = utilisateurRepository;
    }

    public async Task<MessageResponse> ExecuteAsync(string token, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new DomainException("Token de verification manquant.");

        var tous = await _utilisateurRepository.GetAllAsync(ct);
        var utilisateur = tous.FirstOrDefault(u => u.TokenVerificationEmail == token);

        if (utilisateur == null)
            throw new DomainException("Token de verification invalide.");

        // La methode domain verifie l'expiration, passe EmailVerifie=true et invalide le token
        utilisateur.VerifierEmail(token);
        await _utilisateurRepository.UpdateAsync(utilisateur, ct);

        return new MessageResponse("Email verifie avec succes. Vous pouvez maintenant vous connecter.");
    }
}
