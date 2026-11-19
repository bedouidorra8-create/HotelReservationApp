using HotelReservation.Domain.Entities;

namespace HotelReservation.Domain.Repositories;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<IEnumerable<RefreshToken>> GetByUtilisateurIdAsync(Guid utilisateurId, CancellationToken cancellationToken = default);
    Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);
    Task UpdateAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);
    Task RevoquerTousLesTokensAsync(Guid utilisateurId, CancellationToken cancellationToken = default);
}
