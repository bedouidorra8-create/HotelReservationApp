using HotelReservation.Application.DTOs;
using HotelReservation.Domain.Repositories;

namespace HotelReservation.Application.UseCases;

/// <summary>
/// Cas d'utilisation permettant la recherche multicritère de chambres (par type, tranche de prix et statut).
/// </summary>
public class RechercherChambreUseCase
{
    private readonly IHotelRepository _hotelRepository;

    /// <summary>
    /// Initialise une nouvelle instance de <see cref="RechercherChambreUseCase"/>.
    /// </summary>
    /// <param name="hotelRepository">Repository d'accès aux hôtels et leurs chambres.</param>
    public RechercherChambreUseCase(IHotelRepository hotelRepository)
    {
        _hotelRepository = hotelRepository;
    }

    /// <summary>
    /// Exécute la recherche de chambres selon les filtres fournis.
    /// </summary>
    /// <param name="request">Filtres de recherche optionnels (type, prix minimum, prix maximum, statut).</param>
    /// <param name="cancellationToken">Jeton d'annulation asynchrone.</param>
    /// <returns>La liste des DTOs des chambres correspondant aux critères.</returns>
    public async Task<List<ChambreDto>> ExecuteAsync(RechercherChambreRequest request, CancellationToken cancellationToken = default)
    {
        var hotels = await _hotelRepository.GetAllAsync(cancellationToken);

        var query = hotels.SelectMany(h => h.Chambres).AsQueryable();

        if (request.Type.HasValue)
            query = query.Where(c => c.Type == request.Type.Value);

        if (request.Statut.HasValue)
            query = query.Where(c => c.Statut == request.Statut.Value);

        if (request.PrixMin.HasValue)
            query = query.Where(c => c.Prix.Amount >= request.PrixMin.Value);

        if (request.PrixMax.HasValue)
            query = query.Where(c => c.Prix.Amount <= request.PrixMax.Value);

        return query.Select(chambre => new ChambreDto(
            chambre.Id,
            chambre.HotelId,
            chambre.Numero,
            chambre.Type,
            chambre.Etage,
            chambre.Prix.Amount,
            chambre.Prix.Currency,
            chambre.Actif,
            chambre.Statut
        )).ToList();
    }
}
