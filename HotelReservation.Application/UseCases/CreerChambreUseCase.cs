using HotelReservation.Application.DTOs;
using HotelReservation.Domain.Common;
using HotelReservation.Domain.Repositories;
using HotelReservation.Domain.ValueObjects;
using FluentValidation;

namespace HotelReservation.Application.UseCases;

public class CreerChambreUseCase
{
    private readonly IHotelRepository _hotelRepository;
    private readonly IValidator<CreerChambreRequest> _validator;

    public CreerChambreUseCase(IHotelRepository hotelRepository, IValidator<CreerChambreRequest> validator)
    {
        _hotelRepository = hotelRepository;
        _validator = validator;
    }

    public async Task<ChambreDto> ExecuteAsync(Guid hotelId, CreerChambreRequest request, CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);
        var hotel = await _hotelRepository.GetByIdAsync(hotelId, cancellationToken);
        if (hotel == null)
            throw new DomainException($"Hôtel avec l'ID '{hotelId}' introuvable.");

        var prix = new Money(request.PrixMontant, request.PrixDevise ?? "EUR");

        // Execute domain method with invariant enforcement
        var chambre = hotel.AjouterChambre(request.Numero, request.Type, request.Etage, prix, request.Actif);

        await _hotelRepository.UpdateAsync(hotel, cancellationToken);

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
