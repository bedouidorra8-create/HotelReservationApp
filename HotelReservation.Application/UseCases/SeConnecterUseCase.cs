using HotelReservation.Application.DTOs;
using HotelReservation.Application.Services;
using HotelReservation.Domain.Common;
using HotelReservation.Domain.Entities;
using HotelReservation.Domain.Enums;
using HotelReservation.Domain.Repositories;
using HotelReservation.Domain.ValueObjects;

namespace HotelReservation.Application.UseCases;

public class SeConnecterUseCase
{
    private readonly IUtilisateurRepository _utilisateurRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly ITokenService _tokenService;

    public SeConnecterUseCase(
        IUtilisateurRepository utilisateurRepository,
        IRefreshTokenRepository refreshTokenRepository,
        ITokenService tokenService)
    {
        _utilisateurRepository = utilisateurRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _tokenService = tokenService;
    }

    public async Task<ConnexionResponse> ExecuteAsync(ConnexionRequest request, CancellationToken ct = default)
    {
        var email = new Email(request.Email);
        var utilisateur = await _utilisateurRepository.GetByEmailAsync(email, ct);

        if (utilisateur == null || !BCrypt.Net.BCrypt.Verify(request.MotDePasse, utilisateur.MotDePasseHash))
            throw new DomainException("Email ou mot de passe incorrect.");

        if (utilisateur.Statut == StatutUtilisateur.Desactive)
            throw new DomainException("Ce compte est désactivé.");

        var role = utilisateur switch
        {
            Administrateur => "Administrateur",
            Agent          => "Agent",
            _              => "Client"
        };

        var jwt = _tokenService.GenererJwt(utilisateur.Id, utilisateur.Email.Value, role);
        var refreshTokenValeur = _tokenService.GenererRefreshToken();
        var jwtExpiration = DateTime.UtcNow.AddMinutes(15);

        var refreshToken = new RefreshToken(
            Guid.NewGuid(),
            utilisateur.Id,
            refreshTokenValeur,
            DateTime.UtcNow.AddDays(7));

        await _refreshTokenRepository.AddAsync(refreshToken, ct);

        return new ConnexionResponse(
            jwt,
            refreshTokenValeur,
            jwtExpiration,
            utilisateur.Id,
            $"{utilisateur.Prenom} {utilisateur.Nom}",
            utilisateur.Email.Value,
            role);
    }
}
