using FluentValidation;
using HotelReservation.Application.DTOs;

namespace HotelReservation.Application.Validators;

public sealed class CreerChambreRequestValidator : AbstractValidator<CreerChambreRequest>
{
    public CreerChambreRequestValidator()
    {
        RuleFor(x => x.Numero).NotEmpty();
        RuleFor(x => x.PrixMontant).GreaterThan(0);
    }
}