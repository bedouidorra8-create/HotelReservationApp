using HotelReservation.Application.DTOs;
using HotelReservation.Domain.Common;
using HotelReservation.Domain.Repositories;

namespace HotelReservation.Application.UseCases;

public class DeconnexionUseCase
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;

    public DeconnexionUseCase(IRefreshTokenRepository refreshTokenRepository)
    {
        _refreshTokenRepository = refreshTokenRepository;
    }

    public async Task<MessageResponse> ExecuteAsync(Guid utilisateurId, DeconnexionRequest request, CancellationToken cancellationToken = default)
    {
        var refreshToken = await _refreshTokenRepository.GetByTokenAsync(request.RefreshToken, cancellationToken)
            ?? throw new DomainException("Refresh token invalide.");

        if (refreshToken.UtilisateurId != utilisateurId)
            throw new DomainException("Ce refresh token n'appartient pas à l'utilisateur connecté.");

        refreshToken.Revoquer();
        await _refreshTokenRepository.UpdateAsync(refreshToken, cancellationToken);
        return new MessageResponse("Déconnexion réussie.");
    }
}