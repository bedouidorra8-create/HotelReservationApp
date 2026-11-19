using HotelReservation.Application.DTOs;
using HotelReservation.Domain.Common;
using HotelReservation.Domain.Enums;
using HotelReservation.Domain.Repositories;

namespace HotelReservation.Application.UseCases;

/// <summary>
/// Cas d'utilisation permettant de modifier le statut opérationnel d'une chambre (Libre, Occupee, Maintenance).
/// Conforme au pattern DDD : charge l'agrégat Hotel, applique la transition d'état via la méthode métier de l'hôtel, et persiste l'agrégat.
/// </summary>
public class ChangerStatutChambreUseCase
{
    private readonly IHotelRepository _hotelRepository;

    /// <summary>
    /// Initialise une nouvelle instance de <see cref="ChangerStatutChambreUseCase"/>.
    /// </summary>
    /// <param name="hotelRepository">Repository des hôtels.</param>
    public ChangerStatutChambreUseCase(IHotelRepository hotelRepository)
    {
        _hotelRepository = hotelRepository;
    }

    /// <summary>
    /// Exécute le changement de statut opérationnel de la chambre.
    /// </summary>
    /// <param name="chambreId">Identifiant unique de la chambre.</param>
    /// <param name="nouveauStatut">Nouveau statut cible (Libre, Occupee, Maintenance).</param>
    /// <param name="cancellationToken">Jeton d'annulation asynchrone.</param>
    /// <returns>Le DTO de la chambre avec son statut mis à jour.</returns>
    /// <exception cref="DomainException">Levée si la chambre est introuvable ou si la transition est illégale.</exception>
    public async Task<ChambreDto> ExecuteAsync(Guid chambreId, StatutChambre nouveauStatut, CancellationToken cancellationToken = default)
    {
        var hotels = await _hotelRepository.GetAllAsync(cancellationToken);
        var hotel = hotels.FirstOrDefault(h => h.Chambres.Any(c => c.Id == chambreId))
            ?? throw new DomainException($"Chambre avec l'ID '{chambreId}' introuvable.");

        // RÈGLE MÉTIER : Transition vérifiée par le domaine sur l'entité Chambre au sein de l'agrégat Hotel
        hotel.ChangerStatutChambre(chambreId, nouveauStatut);
        await _hotelRepository.UpdateAsync(hotel, cancellationToken);

        var chambre = hotel.Chambres.First(c => c.Id == chambreId);
        return new ChambreDto(
            chambre.Id,
            chambre.HotelId,
            chambre.Numero,
            chambre.Type,
            chambre.Etage,
            chambre.Prix.Amount,
            chambre.Prix.Currency,
            chambre.Actif,
            chambre.Statut
        );
    }
}
