using HotelReservation.Application.DTOs;
using HotelReservation.Domain.Common;
using HotelReservation.Domain.Repositories;

namespace HotelReservation.Application.UseCases;

/// <summary>
/// Cas d'utilisation permettant de consulter la liste des chambres associées à un établissement hôtelier.
/// Conforme au pattern DDD : charge la racine d'agrégat Hotel via IHotelRepository.
/// </summary>
public class ConsulterChambresUseCase
{
    private readonly IHotelRepository _hotelRepository;

    /// <summary>
    /// Initialise une nouvelle instance de <see cref="ConsulterChambresUseCase"/>.
    /// </summary>
    /// <param name="hotelRepository">Repository d'accès aux hôtels.</param>
    public ConsulterChambresUseCase(IHotelRepository hotelRepository)
    {
        _hotelRepository = hotelRepository;
    }

    /// <summary>
    /// Récupère la liste des chambres d'un hôtel donné.
    /// </summary>
    /// <param name="hotelId">Identifiant unique de l'hôtel.</param>
    /// <param name="cancellationToken">Jeton d'annulation asynchrone.</param>
    /// <returns>La liste des DTOs des chambres de l'hôtel.</returns>
    /// <exception cref="DomainException">Levée si l'hôtel est introuvable.</exception>
    public async Task<List<ChambreDto>> ExecuteAsync(Guid hotelId, CancellationToken cancellationToken = default)
    {
        var hotel = await _hotelRepository.GetByIdAsync(hotelId, cancellationToken)
            ?? throw new DomainException($"Hôtel avec l'ID '{hotelId}' introuvable.");

        return hotel.Chambres.Select(chambre => new ChambreDto(
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
