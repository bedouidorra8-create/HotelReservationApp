using FluentValidation;
using HotelReservation.Application.DTOs;

namespace HotelReservation.Application.Validators;

public sealed class AjouterAvisRequestValidator : AbstractValidator<AjouterAvisRequest>
{
    public AjouterAvisRequestValidator()
    {
        RuleFor(x => x.Note).InclusiveBetween(1, 5);
        RuleFor(x => x.Commentaire).NotEmpty();
    }
}