using HotelReservation.Domain.Entities;

namespace HotelReservation.Domain.Repositories;

/// <summary>
/// Contrat de persistance et de consultation pour les avis clients.
/// </summary>
public interface IAvisRepository
{
    /// <summary>Récupère un avis par son identifiant unique.</summary>
    Task<Avis?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Récupère tous les avis déposés pour un hôtel donné.</summary>
    Task<IEnumerable<Avis>> GetByHotelIdAsync(Guid hotelId, CancellationToken cancellationToken = default);

    /// <summary>Récupère tous les avis déposés par un client donné.</summary>
    Task<IEnumerable<Avis>> GetByClientIdAsync(Guid clientId, CancellationToken cancellationToken = default);

    /// <summary>Récupère la totalité des avis présents dans le système.</summary>
    Task<IEnumerable<Avis>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>Ajoute un nouvel avis.</summary>
    Task AddAsync(Avis avis, CancellationToken cancellationToken = default);

    /// <summary>Met à jour un avis existant.</summary>
    Task UpdateAsync(Avis avis, CancellationToken cancellationToken = default);

    /// <summary>Supprime un avis existant.</summary>
    Task DeleteAsync(Avis avis, CancellationToken cancellationToken = default);
}
