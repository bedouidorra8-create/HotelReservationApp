using HotelReservation.Application.DTOs;
using HotelReservation.Application.Services;
using HotelReservation.Domain.Common;
using HotelReservation.Domain.Entities;
using HotelReservation.Domain.Repositories;
using HotelReservation.Domain.ValueObjects;

namespace HotelReservation.Application.UseCases;

public class InscrireUtilisateurUseCase
{
    private readonly IUtilisateurRepository _utilisateurRepository;
    private readonly ITokenService _tokenService;
    private readonly IEmailService _emailService;

    public InscrireUtilisateurUseCase(
        IUtilisateurRepository utilisateurRepository,
        ITokenService tokenService,
        IEmailService emailService)
    {
        _utilisateurRepository = utilisateurRepository;
        _tokenService = tokenService;
        _emailService = emailService;
    }

    public async Task<MessageResponse> ExecuteAsync(InscrireUtilisateurRequest request, CancellationToken ct = default)
    {
        if (!string.Equals(request.Role, "Client", StringComparison.OrdinalIgnoreCase))
            throw new DomainException("La création des comptes Agent et Administrateur est réservée à l'administration.");

        var email = new Email(request.Email);

        var existant = await _utilisateurRepository.GetByEmailAsync(email, ct);
        if (existant != null)
            throw new DomainException("Un compte avec cet email existe deja.");

        // Hash BCrypt du mot de passe
        var hash = BCrypt.Net.BCrypt.HashPassword(request.MotDePasse);

        Utilisateur utilisateur = new Client(
            Guid.NewGuid(),
            request.Nom,
            request.Prenom,
            email,
            hash,
            request.Telephone,
            request.DateNaissance,
            request.Sexe,
            request.Adresse);

        // EmailVerifie=false par defaut, generer token de verification
        var tokenVerif = _tokenService.GenererTokenSecurise();
        utilisateur.DefinirTokenVerificationEmail(tokenVerif);

        await _utilisateurRepository.AddAsync(utilisateur, ct);

        // Envoyer email de verification (non bloquant sur l'inscription)
        await _emailService.EnvoyerVerificationEmailAsync(email.Value, request.Nom, tokenVerif, ct);

        return new MessageResponse("Compte cree avec succes. Veuillez verifier votre email.");
    }
}
