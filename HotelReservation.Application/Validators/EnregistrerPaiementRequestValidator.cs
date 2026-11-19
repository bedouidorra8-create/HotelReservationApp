using FluentValidation;
using HotelReservation.Application.DTOs;

namespace HotelReservation.Application.Validators;

public sealed class EnregistrerPaiementRequestValidator : AbstractValidator<EnregistrerPaiementRequest>
{
    public EnregistrerPaiementRequestValidator()
    {
        RuleFor(x => x.ReservationId).NotEmpty();
        RuleFor(x => x.Montant).GreaterThan(0);
        RuleFor(x => x.Devise).NotEmpty();
        RuleFor(x => x.Reference).NotEmpty();
    }
}