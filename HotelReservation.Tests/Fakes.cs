using HotelReservation.Domain.Entities;
using HotelReservation.Domain.Repositories;
using HotelReservation.Domain.ValueObjects;

namespace HotelReservation.Tests;

public class TestUtilisateurRepository : IUtilisateurRepository
{
    private readonly List<Utilisateur> _items = new();
    public Task<Utilisateur?> GetByIdAsync(Guid id, CancellationToken ct = default) => Task.FromResult(_items.FirstOrDefault(x => x.Id == id));
    public Task<Utilisateur?> GetByEmailAsync(Email email, CancellationToken ct = default) => Task.FromResult(_items.FirstOrDefault(x => x.Email.Value.Equals(email.Value, StringComparison.OrdinalIgnoreCase)));
    public Task<IEnumerable<Utilisateur>> GetAllAsync(CancellationToken ct = default) => Task.FromResult<IEnumerable<Utilisateur>>(_items);
    public Task AddAsync(Utilisateur item, CancellationToken ct = default) { _items.Add(item); return Task.CompletedTask; }
    public Task UpdateAsync(Utilisateur item, CancellationToken ct = default) => Task.CompletedTask;
    public Task DeleteAsync(Utilisateur item, CancellationToken ct = default) => Task.CompletedTask;
}

public class TestHotelRepository : IHotelRepository
{
    private readonly List<Hotel> _items = new();
    public Task<Hotel?> GetByIdAsync(Guid id, CancellationToken ct = default) => Task.FromResult(_items.FirstOrDefault(x => x.Id == id));
    public Task<IEnumerable<Hotel>> GetAllAsync(CancellationToken ct = default) => Task.FromResult<IEnumerable<Hotel>>(_items);
    public Task<IEnumerable<Hotel>> GetByVilleAsync(string ville, CancellationToken ct = default) => Task.FromResult<IEnumerable<Hotel>>(_items.Where(h => h.Ville.Equals(ville, StringComparison.OrdinalIgnoreCase)));
    public Task<IEnumerable<Hotel>> RechercherAsync(string? ville, decimal? prixMin, decimal? prixMax, int? etoilesMin, CancellationToken ct = default) => Task.FromResult<IEnumerable<Hotel>>(_items);
    public Task AddAsync(Hotel item, CancellationToken ct = default) { _items.Add(item); return Task.CompletedTask; }
    public Task UpdateAsync(Hotel item, CancellationToken ct = default) => Task.CompletedTask;
    public Task DeleteAsync(Hotel item, CancellationToken ct = default) => Task.CompletedTask;
}

public class TestReservationRepository : IReservationRepository
{
    private readonly List<Reservation> _items = new();
    public Task<Reservation?> GetByIdAsync(Guid id, CancellationToken ct = default) => Task.FromResult(_items.FirstOrDefault(x => x.Id == id));
    public Task<IEnumerable<Reservation>> GetReservationsActivesPourChambre(Guid id, CancellationToken ct = default) => Task.FromResult<IEnumerable<Reservation>>(_items.Where(x => x.ChambreId == id));
    public Task<IEnumerable<Reservation>> GetByClientIdAsync(Guid id, CancellationToken ct = default) => Task.FromResult<IEnumerable<Reservation>>(_items.Where(x => x.ClientId == id));
    public Task<IEnumerable<Reservation>> GetByHotelIdAsync(Guid id, CancellationToken ct = default) => Task.FromResult<IEnumerable<Reservation>>(_items.Where(x => x.HotelId == id));
    public Task<IEnumerable<Reservation>> GetAllAsync(CancellationToken ct = default) => Task.FromResult<IEnumerable<Reservation>>(_items);
    public Task AddAsync(Reservation item, CancellationToken ct = default) { _items.Add(item); return Task.CompletedTask; }
    public Task UpdateAsync(Reservation item, CancellationToken ct = default) => Task.CompletedTask;
}

public class TestPaiementRepository : IPaiementRepository
{
    private readonly List<Paiement> _items = new();
    public Task<Paiement?> GetByIdAsync(Guid id, CancellationToken ct = default) => Task.FromResult(_items.FirstOrDefault(x => x.Id == id));
    public Task<IEnumerable<Paiement>> GetByReservationIdAsync(Guid id, CancellationToken ct = default) => Task.FromResult<IEnumerable<Paiement>>(_items.Where(x => x.ReservationId == id));
    public Task<IEnumerable<Paiement>> GetAllAsync(CancellationToken ct = default) => Task.FromResult<IEnumerable<Paiement>>(_items);
    public Task AddAsync(Paiement item, CancellationToken ct = default) { _items.Add(item); return Task.CompletedTask; }
    public Task UpdateAsync(Paiement item, CancellationToken ct = default) => Task.CompletedTask;
}

public class TestAvisRepository : IAvisRepository
{
    private readonly List<Avis> _items = new();
    public Task<Avis?> GetByIdAsync(Guid id, CancellationToken ct = default) => Task.FromResult(_items.FirstOrDefault(x => x.Id == id));
    public Task<IEnumerable<Avis>> GetByHotelIdAsync(Guid id, CancellationToken ct = default) => Task.FromResult<IEnumerable<Avis>>(_items.Where(x => x.HotelId == id));
    public Task<IEnumerable<Avis>> GetByClientIdAsync(Guid id, CancellationToken ct = default) => Task.FromResult<IEnumerable<Avis>>(_items.Where(x => x.ClientId == id));
    public Task<IEnumerable<Avis>> GetAllAsync(CancellationToken ct = default) => Task.FromResult<IEnumerable<Avis>>(_items);
    public Task AddAsync(Avis avis, CancellationToken ct = default) { _items.Add(avis); return Task.CompletedTask; }
    public Task UpdateAsync(Avis avis, CancellationToken ct = default) => Task.CompletedTask;
    public Task DeleteAsync(Avis avis, CancellationToken ct = default) => Task.CompletedTask;
}
