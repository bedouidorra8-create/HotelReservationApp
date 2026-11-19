using HotelReservation.Application.DTOs;
using HotelReservation.Application.Services;
using HotelReservation.Domain.Common;
using HotelReservation.Domain.Repositories;
using HotelReservation.Domain.ValueObjects;

namespace HotelReservation.Application.UseCases;

public class DemanderResetPasswordUseCase
{
    private readonly IUtilisateurRepository _utilisateurRepository;
    private readonly ITokenService _tokenService;
    private readonly IEmailService _emailService;

    public DemanderResetPasswordUseCase(
        IUtilisateurRepository utilisateurRepository,
        ITokenService tokenService,
        IEmailService emailService)
    {
        _utilisateurRepository = utilisateurRepository;
        _tokenService = tokenService;
        _emailService = emailService;
    }

    public async Task<MessageResponse> ExecuteAsync(DemanderResetPasswordRequest request, CancellationToken ct = default)
    {
        var email = new Email(request.Email);
        var utilisateur = await _utilisateurRepository.GetByEmailAsync(email, ct);

        // Ne pas reveler si l'email existe ou non (securite)
        if (utilisateur == null)
            return new MessageResponse("Si cet email est enregistre, un lien de reinitialisation vous sera envoye.");

        var token = _tokenService.GenererTokenSecurise();
        utilisateur.DefinirTokenResetMotDePasse(token);
        await _utilisateurRepository.UpdateAsync(utilisateur, ct);

        await _emailService.EnvoyerResetMotDePasseAsync(email.Value, utilisateur.Nom, token, ct);

        return new MessageResponse("Si cet email est enregistre, un lien de reinitialisation vous sera envoye.");
    }
}
