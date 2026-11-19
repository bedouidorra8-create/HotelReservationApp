using HotelReservation.Domain.Entities;
using HotelReservation.Domain.ValueObjects;

namespace HotelReservation.Domain.Repositories;

public interface IUtilisateurRepository
{
    Task<Utilisateur?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Utilisateur?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default);
    Task<IEnumerable<Utilisateur>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Utilisateur utilisateur, CancellationToken cancellationToken = default);
    Task UpdateAsync(Utilisateur utilisateur, CancellationToken cancellationToken = default);
    Task DeleteAsync(Utilisateur utilisateur, CancellationToken cancellationToken = default);
}
