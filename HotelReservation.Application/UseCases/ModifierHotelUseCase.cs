using HotelReservation.Application.DTOs;
using HotelReservation.Domain.Common;
using HotelReservation.Domain.Enums;
using HotelReservation.Domain.Repositories;

namespace HotelReservation.Application.UseCases;

/// <summary>
/// Cas d'utilisation permettant la modification des informations d'un hôtel existant.
/// </summary>
public class ModifierHotelUseCase
{
    private readonly IHotelRepository _repository;

    /// <summary>
    /// Initialise une nouvelle instance de <see cref="ModifierHotelUseCase"/>.
    /// </summary>
    /// <param name="repository">Repository des hôtels.</param>
    public ModifierHotelUseCase(IHotelRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Exécute la modification de l'hôtel.
    /// </summary>
    /// <param name="hotelId">Identifiant unique de l'hôtel.</param>
    /// <param name="request">Données modifiées.</param>
    /// <param name="cancellationToken">Jeton d'annulation asynchrone.</param>
    /// <returns>Le DTO de l'hôtel mis à jour.</returns>
    /// <exception cref="DomainException">Levée si l'hôtel est introuvable.</exception>
    public async Task<HotelDto> ExecuteAsync(Guid hotelId, ModifierHotelRequest request, CancellationToken cancellationToken = default)
    {
        var hotel = await _repository.GetByIdAsync(hotelId, cancellationToken)
            ?? throw new DomainException($"Hôtel avec l'ID '{hotelId}' introuvable.");

        // RÈGLE MÉTIER : Modification des informations générales sans PrixParNuit (géré par chambre)
        hotel.Modifier(request.Nom, request.Ville, request.Adresse, request.NbEtoiles, request.Description, request.Telephone, request.Images);
        await _repository.UpdateAsync(hotel, cancellationToken);

        decimal? prixMin = hotel.Chambres.Any() ? hotel.Chambres.Min(c => c.Prix.Amount) : null;

        return new HotelDto(
            hotel.Id,
            hotel.Nom,
            hotel.Ville,
            hotel.Adresse,
            hotel.NbEtoiles,
            hotel.Description,
            hotel.Telephone,
            hotel.Email.Value,
            hotel.Images,
            hotel.Statut == StatutHotel.Actif,
            hotel.Chambres.Select(chambre => new ChambreDto(
                chambre.Id,
                chambre.HotelId,
                chambre.Numero,
                chambre.Type,
                chambre.Etage,
                chambre.Prix.Amount,
                chambre.Prix.Currency,
                chambre.Actif,
                chambre.Statut
            )).ToList(),
            prixMin
        );
    }
}
