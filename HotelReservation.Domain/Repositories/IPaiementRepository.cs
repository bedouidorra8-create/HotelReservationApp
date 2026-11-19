using HotelReservation.Domain.Entities;

namespace HotelReservation.Domain.Repositories;

public interface IPaiementRepository
{
    Task<Paiement?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Paiement>> GetByReservationIdAsync(Guid reservationId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Paiement>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Paiement paiement, CancellationToken cancellationToken = default);
    Task UpdateAsync(Paiement paiement, CancellationToken cancellationToken = default);
}
