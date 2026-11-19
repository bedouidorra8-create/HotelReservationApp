using HotelReservation.Domain.Entities;

namespace HotelReservation.Domain.Repositories;

public interface IReservationRepository
{
    Task<Reservation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Reservation>> GetReservationsActivesPourChambre(Guid chambreId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Reservation>> GetByClientIdAsync(Guid clientId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Reservation>> GetByHotelIdAsync(Guid hotelId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Reservation>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Reservation reservation, CancellationToken cancellationToken = default);
    Task UpdateAsync(Reservation reservation, CancellationToken cancellationToken = default);
}
