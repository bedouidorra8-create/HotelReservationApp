using HotelReservation.Domain.Entities;
using HotelReservation.Domain.Repositories;
using HotelReservation.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HotelReservation.Infrastructure.Repositories;

/// <summary>
/// Implémentation Entity Framework Core du repository des avis clients.
/// </summary>
public class AvisRepository : IAvisRepository
{
    private readonly AppDbContext _context;

    /// <summary>
    /// Initialise une nouvelle instance de <see cref="AvisRepository"/>.
    /// </summary>
    public AvisRepository(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>Récupère un avis par son identifiant unique.</summary>
    public async Task<Avis?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Avis.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    /// <summary>Récupère tous les avis d'un hôtel donné.</summary>
    public async Task<IEnumerable<Avis>> GetByHotelIdAsync(Guid hotelId, CancellationToken cancellationToken = default)
    {
        return await _context.Avis
            .Where(a => a.HotelId == hotelId)
            .ToListAsync(cancellationToken);
    }

    /// <summary>Récupère tous les avis rédigés par un client donné.</summary>
    public async Task<IEnumerable<Avis>> GetByClientIdAsync(Guid clientId, CancellationToken cancellationToken = default)
    {
        return await _context.Avis
            .Where(a => a.ClientId == clientId)
            .ToListAsync(cancellationToken);
    }

    /// <summary>Récupère l'ensemble des avis enregistrés.</summary>
    public async Task<IEnumerable<Avis>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Avis.ToListAsync(cancellationToken);
    }

    /// <summary>Ajoute et persiste un nouvel avis.</summary>
    public async Task AddAsync(Avis avis, CancellationToken cancellationToken = default)
    {
        await _context.Avis.AddAsync(avis, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>Met à jour et persiste un avis modifié.</summary>
    public async Task UpdateAsync(Avis avis, CancellationToken cancellationToken = default)
    {
        _context.Avis.Update(avis);
        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>Supprime un avis de la base de données.</summary>
    public async Task DeleteAsync(Avis avis, CancellationToken cancellationToken = default)
    {
        _context.Avis.Remove(avis);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
