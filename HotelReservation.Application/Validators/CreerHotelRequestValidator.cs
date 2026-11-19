using FluentValidation;
using HotelReservation.Application.DTOs;

namespace HotelReservation.Application.Validators;

public sealed class CreerHotelRequestValidator : AbstractValidator<CreerHotelRequest>
{
    public CreerHotelRequestValidator()
    {
        RuleFor(x => x.Nom).NotEmpty();
        RuleFor(x => x.NbEtoiles).InclusiveBetween(1, 5);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
    }
}