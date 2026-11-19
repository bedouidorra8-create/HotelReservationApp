using HotelReservation.Domain.Entities;

namespace HotelReservation.Domain.Repositories;

/// <summary>Définit l'accès aux associations hôtel-utilisateur favorites.</summary>
public interface IFavoriRepository
{
    Task AddAsync(Favori favori, CancellationToken cancellationToken = default);
    Task<bool> ExisteAsync(Guid utilisateurId, Guid hotelId, CancellationToken cancellationToken = default);
    Task<Favori?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Favori>> GetByUtilisateurIdAsync(Guid utilisateurId, CancellationToken cancellationToken = default);
    Task DeleteAsync(Favori favori, CancellationToken cancellationToken = default);
}