using HotelReservation.Application.DTOs;
using HotelReservation.Domain.Common;
using HotelReservation.Domain.Entities;
using HotelReservation.Domain.Repositories;

namespace HotelReservation.Application.UseCases;

/// <summary>Ajoute un hôtel aux favoris d'un utilisateur après contrôle d'unicité.</summary>
public class AjouterFavoriUseCase
{
    private readonly IHotelRepository _hotelRepository;
    private readonly IFavoriRepository _favoriRepository;

    public AjouterFavoriUseCase(IHotelRepository hotelRepository, IFavoriRepository favoriRepository)
    {
        _hotelRepository = hotelRepository;
        _favoriRepository = favoriRepository;
    }

    public async Task<FavoriDto> ExecuteAsync(Guid utilisateurId, AjouterFavoriRequest request, CancellationToken cancellationToken = default)
    {
        var hotel = await _hotelRepository.GetByIdAsync(request.HotelId, cancellationToken)
            ?? throw new DomainException("Cet hôtel est introuvable.");
        if (await _favoriRepository.ExisteAsync(utilisateurId, request.HotelId, cancellationToken))
            throw new DomainException("Cet hôtel est déjà dans vos favoris.");

        var favori = new Favori(Guid.NewGuid(), utilisateurId, hotel.Id);
        await _favoriRepository.AddAsync(favori, cancellationToken);
        return new FavoriDto(favori.Id, hotel.Id, hotel.Nom, hotel.Ville, favori.DateAjout);
    }
}