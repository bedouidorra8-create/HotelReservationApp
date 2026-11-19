using HotelReservation.Domain.Entities;
using HotelReservation.Domain.Repositories;
using HotelReservation.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HotelReservation.Infrastructure.Repositories;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly AppDbContext _context;

    public RefreshTokenRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken ct = default)
    {
        return await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == token, ct);
    }

    public async Task<IEnumerable<RefreshToken>> GetByUtilisateurIdAsync(Guid utilisateurId, CancellationToken ct = default)
    {
        return await _context.RefreshTokens
            .Where(rt => rt.UtilisateurId == utilisateurId)
            .ToListAsync(ct);
    }

    public async Task AddAsync(RefreshToken refreshToken, CancellationToken ct = default)
    {
        await _context.RefreshTokens.AddAsync(refreshToken, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(RefreshToken refreshToken, CancellationToken ct = default)
    {
        _context.RefreshTokens.Update(refreshToken);
        await _context.SaveChangesAsync(ct);
    }

    public async Task RevoquerTousLesTokensAsync(Guid utilisateurId, CancellationToken ct = default)
    {
        var tokens = await _context.RefreshTokens
            .Where(rt => rt.UtilisateurId == utilisateurId && !rt.Revoque)
            .ToListAsync(ct);

        foreach (var t in tokens)
            t.Revoquer();

        await _context.SaveChangesAsync(ct);
    }
}
