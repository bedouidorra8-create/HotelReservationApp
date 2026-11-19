using HotelReservation.Domain.Entities;

namespace HotelReservation.Domain.Repositories;

public interface IHotelRepository
{
    Task<Hotel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Hotel>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Hotel>> GetByVilleAsync(string ville, CancellationToken cancellationToken = default);
    Task<IEnumerable<Hotel>> RechercherAsync(
        string? ville,
        decimal? prixMin,
        decimal? prixMax,
        int? etoilesMin,
        CancellationToken cancellationToken = default);
    Task AddAsync(Hotel hotel, CancellationToken cancellationToken = default);
    Task UpdateAsync(Hotel hotel, CancellationToken cancellationToken = default);
    Task DeleteAsync(Hotel hotel, CancellationToken cancellationToken = default);
}

