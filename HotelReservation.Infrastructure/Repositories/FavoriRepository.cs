using HotelReservation.Domain.Entities;
using HotelReservation.Domain.Repositories;
using HotelReservation.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HotelReservation.Infrastructure.Repositories;

/// <summary>Implémentation EF Core du repository des favoris.</summary>
public class FavoriRepository : IFavoriRepository
{
    private readonly AppDbContext _context;

    public FavoriRepository(AppDbContext context) => _context = context;

    public async Task AddAsync(Favori favori, CancellationToken cancellationToken = default)
    {
        await _context.Favoris.AddAsync(favori, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public Task<bool> ExisteAsync(Guid utilisateurId, Guid hotelId, CancellationToken cancellationToken = default) =>
        _context.Favoris.AnyAsync(f => f.UtilisateurId == utilisateurId && f.HotelId == hotelId, cancellationToken);

    public Task<Favori?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _context.Favoris.FirstOrDefaultAsync(f => f.Id == id, cancellationToken);

    public async Task<IEnumerable<Favori>> GetByUtilisateurIdAsync(Guid utilisateurId, CancellationToken cancellationToken = default) =>
        await _context.Favoris.Where(f => f.UtilisateurId == utilisateurId).OrderByDescending(f => f.DateAjout).ToListAsync(cancellationToken);

    public async Task DeleteAsync(Favori favori, CancellationToken cancellationToken = default)
    {
        _context.Favoris.Remove(favori);
        await _context.SaveChangesAsync(cancellationToken);
    }
}