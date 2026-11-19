using HotelReservation.Domain.Entities;
using HotelReservation.Domain.Enums;
using HotelReservation.Domain.Repositories;
using HotelReservation.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HotelReservation.Infrastructure.Repositories;

/// <summary>
/// Implémentation Entity Framework Core du repository de la racine d'agrégat Hotel.
/// Inclut systématiquement la collection des chambres enfants pour préserver les invariants DDD.
/// </summary>
public class HotelRepository : IHotelRepository
{
    private readonly AppDbContext _context;

    /// <summary>
    /// Initialise une nouvelle instance de <see cref="HotelRepository"/>.
    /// </summary>
    public HotelRepository(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>Récupère un hôtel par son identifiant avec ses chambres.</summary>
    public async Task<Hotel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Hotels
            .Include(h => h.Chambres)
            .FirstOrDefaultAsync(h => h.Id == id, cancellationToken);
    }

    /// <summary>Récupère l'ensemble des hôtels avec leurs chambres.</summary>
    public async Task<IEnumerable<Hotel>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Hotels
            .Include(h => h.Chambres)
            .ToListAsync(cancellationToken);
    }

    /// <summary>Récupère les hôtels situés dans une ville spécifique.</summary>
    public async Task<IEnumerable<Hotel>> GetByVilleAsync(string ville, CancellationToken cancellationToken = default)
    {
        return await _context.Hotels
            .Include(h => h.Chambres)
            .Where(h => h.Ville.ToLower() == ville.ToLower())
            .ToListAsync(cancellationToken);
    }

    /// <summary>Recherche les hôtels actifs selon les filtres de recherche.</summary>
    public async Task<IEnumerable<Hotel>> RechercherAsync(
        string? ville,
        decimal? prixMin,
        decimal? prixMax,
        int? etoilesMin,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Hotels
            .Include(h => h.Chambres)
            .Where(h => h.Statut == StatutHotel.Actif)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(ville))
            query = query.Where(h => h.Ville.ToLower().Contains(ville.ToLower()));

        if (etoilesMin.HasValue)
            query = query.Where(h => h.NbEtoiles >= etoilesMin.Value);

        if (prixMin.HasValue || prixMax.HasValue)
        {
            query = query.Where(h => h.Chambres.Any(c =>
                (!prixMin.HasValue || c.Prix.Amount >= prixMin.Value)
                && (!prixMax.HasValue || c.Prix.Amount <= prixMax.Value)));
        }

        return await query.ToListAsync(cancellationToken);
    }

    /// <summary>Ajoute un nouvel hôtel avec ses chambres.</summary>
    public async Task AddAsync(Hotel hotel, CancellationToken cancellationToken = default)
    {
        await _context.Hotels.AddAsync(hotel, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>Met à jour l'agrégat Hotel et ses entités enfants.</summary>
    public async Task UpdateAsync(Hotel hotel, CancellationToken cancellationToken = default)
    {
        _context.Hotels.Update(hotel);
        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>Supprime un hôtel et ses chambres en cascade.</summary>
    public async Task DeleteAsync(Hotel hotel, CancellationToken cancellationToken = default)
    {
        _context.Hotels.Remove(hotel);
        await _context.SaveChangesAsync(cancellationToken);
    }
}

