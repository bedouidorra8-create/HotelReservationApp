using HotelReservation.Application.DTOs;
using HotelReservation.Application.Services;
using HotelReservation.Domain.Common;
using HotelReservation.Domain.Entities;
using HotelReservation.Domain.Repositories;

namespace HotelReservation.Application.UseCases;

public class RafraichirTokenUseCase
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUtilisateurRepository _utilisateurRepository;
    private readonly ITokenService _tokenService;

    public RafraichirTokenUseCase(
        IRefreshTokenRepository refreshTokenRepository,
        IUtilisateurRepository utilisateurRepository,
        ITokenService tokenService)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _utilisateurRepository = utilisateurRepository;
        _tokenService = tokenService;
    }

    public async Task<TokenResponse> ExecuteAsync(RafraichirTokenRequest request, CancellationToken ct = default)
    {
        var refreshToken = await _refreshTokenRepository.GetByTokenAsync(request.RefreshToken, ct);

        if (refreshToken == null || !refreshToken.EstValide())
            throw new DomainException("Refresh token invalide ou expire.");

        var utilisateur = await _utilisateurRepository.GetByIdAsync(refreshToken.UtilisateurId, ct);
        if (utilisateur == null)
            throw new DomainException("Utilisateur introuvable.");

        var role = utilisateur switch
        {
            Administrateur => "Administrateur",
            Agent          => "Agent",
            _              => "Client"
        };

        // Revoquer l'ancien refresh token
        refreshToken.Revoquer();
        await _refreshTokenRepository.UpdateAsync(refreshToken, ct);

        // Generer nouveau JWT + nouveau RefreshToken (rotation)
        var nouveauJwt = _tokenService.GenererJwt(utilisateur.Id, utilisateur.Email.Value, role);
        var nouveauRefreshTokenValeur = _tokenService.GenererRefreshToken();
        var jwtExpiration = DateTime.UtcNow.AddMinutes(15);

        var nouveauRefreshToken = new RefreshToken(
            Guid.NewGuid(),
            utilisateur.Id,
            nouveauRefreshTokenValeur,
            DateTime.UtcNow.AddDays(7));

        await _refreshTokenRepository.AddAsync(nouveauRefreshToken, ct);

        return new TokenResponse(nouveauJwt, nouveauRefreshTokenValeur, jwtExpiration);
    }
}
