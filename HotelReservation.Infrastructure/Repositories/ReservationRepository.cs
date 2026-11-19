using HotelReservation.Domain.Entities;
using HotelReservation.Domain.Enums;
using HotelReservation.Domain.Repositories;
using HotelReservation.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HotelReservation.Infrastructure.Repositories;

public class ReservationRepository : IReservationRepository
{
    private readonly AppDbContext _context;

    public ReservationRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Reservation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Reservations.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Reservation>> GetReservationsActivesPourChambre(Guid chambreId, CancellationToken cancellationToken = default)
    {
        return await _context.Reservations
            .Where(r => r.ChambreId == chambreId && r.Statut != StatutReservation.Annulee)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Reservation>> GetByClientIdAsync(Guid clientId, CancellationToken cancellationToken = default)
    {
        return await _context.Reservations
            .Where(r => r.ClientId == clientId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Reservation>> GetByHotelIdAsync(Guid hotelId, CancellationToken cancellationToken = default)
    {
        var chambreIds = await _context.Chambres
            .Where(c => c.HotelId == hotelId)
            .Select(c => c.Id)
            .ToListAsync(cancellationToken);

        return await _context.Reservations
            .Where(r => chambreIds.Contains(r.ChambreId))
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Reservation>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Reservations.ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Reservation reservation, CancellationToken cancellationToken = default)
    {
        await _context.Reservations.AddAsync(reservation, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Reservation reservation, CancellationToken cancellationToken = default)
    {
        _context.Reservations.Update(reservation);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
