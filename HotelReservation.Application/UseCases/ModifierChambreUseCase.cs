using HotelReservation.Application.DTOs;
using HotelReservation.Domain.Common;
using HotelReservation.Domain.Repositories;
using HotelReservation.Domain.ValueObjects;

namespace HotelReservation.Application.UseCases;

/// <summary>
/// Cas d'utilisation permettant la modification des caractéristiques d'une chambre (type, étage, tarif).
/// Conforme au pattern DDD : charge l'agrégat Hotel, invoque la méthode de domaine Hotel.ModifierChambre puis persiste l'agrégat.
/// </summary>
public class ModifierChambreUseCase
{
    private readonly IHotelRepository _hotelRepository;

    /// <summary>
    /// Initialise une nouvelle instance de <see cref="ModifierChambreUseCase"/>.
    /// </summary>
    /// <param name="hotelRepository">Repository des hôtels.</param>
    public ModifierChambreUseCase(IHotelRepository hotelRepository)
    {
        _hotelRepository = hotelRepository;
    }

    /// <summary>
    /// Exécute la modification de la chambre.
    /// </summary>
    /// <param name="chambreId">Identifiant unique de la chambre.</param>
    /// <param name="request">Données de modification (type, étage, montant et devise du prix).</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Le DTO de la chambre mise à jour.</returns>
    /// <exception cref="DomainException">Levée si la chambre ou son hôtel associé est introuvable.</exception>
    public async Task<ChambreDto> ExecuteAsync(Guid chambreId, ModifierChambreRequest request, CancellationToken cancellationToken = default)
    {
        var hotels = await _hotelRepository.GetAllAsync(cancellationToken);
        var hotel = hotels.FirstOrDefault(h => h.Chambres.Any(c => c.Id == chambreId))
            ?? throw new DomainException($"Chambre avec l'ID '{chambreId}' introuvable.");

        var nouveauPrix = new Money(request.PrixMontant, request.PrixDevise ?? "EUR");

        // RÈGLE MÉTIER : Passage par la racine d'agrégat Hotel pour modifier la chambre enfant
        hotel.ModifierChambre(chambreId, request.Type, request.Etage, nouveauPrix, request.Actif);
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
