using FluentValidation;
using HotelReservation.Application.DTOs;

namespace HotelReservation.Application.Validators;

public sealed class CreerReservationRequestValidator : AbstractValidator<CreerReservationRequest>
{
    public CreerReservationRequestValidator()
    {
        RuleFor(x => x.ChambreId).NotEmpty();
        RuleFor(x => x.DateDepart).GreaterThan(x => x.DateArrivee);
        RuleFor(x => x.NbPersonnes).GreaterThan(0);
    }
}