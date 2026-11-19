using HotelReservation.Domain.Common;
using HotelReservation.Domain.Enums;
using HotelReservation.Domain.Repositories;

namespace HotelReservation.Application.UseCases;

/// <summary>
/// Cas d'utilisation permettant d'activer ou de désactiver un hôtel.
/// </summary>
public class ActiverDesactiverHotelUseCase
{
    private readonly IHotelRepository _repository;

    /// <summary>
    /// Initialise une nouvelle instance de <see cref="ActiverDesactiverHotelUseCase"/>.
    /// </summary>
    /// <param name="repository">Repository des hôtels.</param>
    public ActiverDesactiverHotelUseCase(IHotelRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Modifie le statut opérationnel d'un hôtel (Actif ou Inactif).
    /// </summary>
    /// <param name="hotelId">Identifiant unique de l'hôtel.</param>
    /// <param name="statut">Nouveau statut de l'hôtel.</param>
    /// <param name="cancellationToken">Jeton d'annulation asynchrone.</param>
    /// <exception cref="DomainException">Levée si l'hôtel est introuvable.</exception>
    public async Task ExecuteAsync(Guid hotelId, StatutHotel statut, CancellationToken cancellationToken = default)
    {
        var hotel = await _repository.GetByIdAsync(hotelId, cancellationToken)
            ?? throw new DomainException($"Hôtel avec l'ID '{hotelId}' introuvable.");

        hotel.ActiverDesactiver(statut);
        await _repository.UpdateAsync(hotel, cancellationToken);
    }

    /// <summary>
    /// Surcharge de commodité acceptant un booléen (vrai = Actif, faux = Inactif).
    /// </summary>
    public async Task ExecuteAsync(Guid hotelId, bool actif, CancellationToken cancellationToken = default)
    {
        var statut = actif ? StatutHotel.Actif : StatutHotel.Inactif;
        await ExecuteAsync(hotelId, statut, cancellationToken);
    }
}
