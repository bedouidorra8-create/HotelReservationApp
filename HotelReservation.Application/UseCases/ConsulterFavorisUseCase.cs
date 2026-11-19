using HotelReservation.Application.DTOs;
using HotelReservation.Domain.Repositories;

namespace HotelReservation.Application.UseCases;

/// <summary>Retourne les favoris avec les informations de leurs hôtels.</summary>
public class ConsulterFavorisUseCase
{
    private readonly IFavoriRepository _favoriRepository;
    private readonly IHotelRepository _hotelRepository;

    public ConsulterFavorisUseCase(IFavoriRepository favoriRepository, IHotelRepository hotelRepository)
    {
        _favoriRepository = favoriRepository;
        _hotelRepository = hotelRepository;
    }

    public async Task<IEnumerable<FavoriDto>> ExecuteAsync(Guid utilisateurId, CancellationToken cancellationToken = default)
    {
        var favoris = await _favoriRepository.GetByUtilisateurIdAsync(utilisateurId, cancellationToken);
        var resultats = new List<FavoriDto>();
        foreach (var favori in favoris)
        {
            var hotel = await _hotelRepository.GetByIdAsync(favori.HotelId, cancellationToken);
            if (hotel != null)
                resultats.Add(new FavoriDto(favori.Id, hotel.Id, hotel.Nom, hotel.Ville, favori.DateAjout));
        }
        return resultats;
    }
}