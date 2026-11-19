using FluentValidation;
using HotelReservation.Application.DTOs;

namespace HotelReservation.Application.Validators;

public class ModifierProfilValidator : AbstractValidator<ModifierProfilRequest>
{
    public ModifierProfilValidator()
    {
        RuleFor(x => x.Nom).NotEmpty().WithMessage("Le nom est obligatoire.").MaximumLength(100);
        RuleFor(x => x.Prenom).NotEmpty().WithMessage("Le prénom est obligatoire.").MaximumLength(100);
        RuleFor(x => x.Telephone).MaximumLength(30);
        RuleFor(x => x.Adresse).MaximumLength(250);
        RuleFor(x => x.Sexe).IsInEnum().When(x => x.Sexe.HasValue).WithMessage("Le sexe fourni est invalide.");
    }
}
