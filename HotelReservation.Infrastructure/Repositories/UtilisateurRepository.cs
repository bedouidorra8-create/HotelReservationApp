using HotelReservation.Domain.Entities;
using HotelReservation.Domain.Repositories;
using HotelReservation.Domain.ValueObjects;
using HotelReservation.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HotelReservation.Infrastructure.Repositories;

public class UtilisateurRepository : IUtilisateurRepository
{
    private readonly AppDbContext _context;

    public UtilisateurRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Utilisateur?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Utilisateurs.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<Utilisateur?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default)
    {
        return await _context.Utilisateurs.FirstOrDefaultAsync(u => u.Email.Value == email.Value, cancellationToken);
    }

    public async Task<IEnumerable<Utilisateur>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Utilisateurs.ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Utilisateur utilisateur, CancellationToken cancellationToken = default)
    {
        await _context.Utilisateurs.AddAsync(utilisateur, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Utilisateur utilisateur, CancellationToken cancellationToken = default)
    {
        _context.Utilisateurs.Update(utilisateur);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Utilisateur utilisateur, CancellationToken cancellationToken = default)
    {
        _context.Utilisateurs.Remove(utilisateur);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
