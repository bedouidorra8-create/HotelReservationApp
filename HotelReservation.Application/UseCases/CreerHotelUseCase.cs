using HotelReservation.Application.DTOs;
using HotelReservation.Domain.Entities;
using HotelReservation.Domain.Enums;
using HotelReservation.Domain.Repositories;
using HotelReservation.Domain.ValueObjects;
using FluentValidation;

namespace HotelReservation.Application.UseCases;

/// <summary>
/// Cas d'utilisation permettant la création d'un nouvel établissement hôtelier.
/// </summary>
public class CreerHotelUseCase
{
    private readonly IHotelRepository _hotelRepository;
    private readonly IValidator<CreerHotelRequest> _validator;

    /// <summary>
    /// Initialise une nouvelle instance de <see cref="CreerHotelUseCase"/>.
    /// </summary>
    /// <param name="hotelRepository">Repository des hôtels.</param>
    public CreerHotelUseCase(IHotelRepository hotelRepository, IValidator<CreerHotelRequest> validator)
    {
        _hotelRepository = hotelRepository;
        _validator = validator;
    }

    /// <summary>
    /// Exécute la création d'un hôtel.
    /// </summary>
    /// <param name="request">Informations du nouvel hôtel.</param>
    /// <param name="cancellationToken">Jeton d'annulation asynchrone.</param>
    /// <returns>Le DTO de l'hôtel créé.</returns>
    public async Task<HotelDto> ExecuteAsync(CreerHotelRequest request, CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);
        var email = new Email(request.Email);
        var hotel = new Hotel(
            Guid.NewGuid(),
            request.Nom,
            request.Ville,
            request.Adresse,
            request.NbEtoiles,
            request.Description,
            request.Telephone,
            email,
            request.Images
        );

        await _hotelRepository.AddAsync(hotel, cancellationToken);

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
            new List<ChambreDto>(),
            null
        );
    }
}
