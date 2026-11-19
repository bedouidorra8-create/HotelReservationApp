using HotelReservation.Domain.Entities;
using HotelReservation.Domain.Repositories;
using HotelReservation.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HotelReservation.Infrastructure.Repositories;

public class PaiementRepository : IPaiementRepository
{
    private readonly AppDbContext _context;

    public PaiementRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Paiement?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Paiements.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Paiement>> GetByReservationIdAsync(Guid reservationId, CancellationToken cancellationToken = default)
    {
        return await _context.Paiements
            .Where(p => p.ReservationId == reservationId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Paiement>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Paiements.ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Paiement paiement, CancellationToken cancellationToken = default)
    {
        await _context.Paiements.AddAsync(paiement, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Paiement paiement, CancellationToken cancellationToken = default)
    {
        _context.Paiements.Update(paiement);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
