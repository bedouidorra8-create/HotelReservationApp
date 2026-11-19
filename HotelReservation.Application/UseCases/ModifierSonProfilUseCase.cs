using FluentValidation;
using HotelReservation.Application.DTOs;
using HotelReservation.Domain.Common;
using HotelReservation.Domain.Entities;
using HotelReservation.Domain.Repositories;

namespace HotelReservation.Application.UseCases;

public class ModifierSonProfilUseCase
{
    private readonly IUtilisateurRepository _utilisateurRepository;
    private readonly IValidator<ModifierProfilRequest> _validator;

    public ModifierSonProfilUseCase(
        IUtilisateurRepository utilisateurRepository,
        IValidator<ModifierProfilRequest> validator)
    {
        _utilisateurRepository = utilisateurRepository;
        _validator = validator;
    }

    public async Task<ProfilUtilisateurDto> ExecuteAsync(
        Guid userId,
        ModifierProfilRequest request,
        CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        var user = await _utilisateurRepository.GetByIdAsync(userId, cancellationToken)
            ?? throw new DomainException($"Utilisateur avec l'ID '{userId}' introuvable.");

        user.ModifierProfil(
            request.Nom,
            request.Prenom,
            request.Telephone,
            request.Adresse,
            request.Photo,
            request.DateNaissance,
            request.Sexe);

        await _utilisateurRepository.UpdateAsync(user, cancellationToken);

        var role = user switch
        {
            Administrateur => "Administrateur",
            Agent => "Agent",
            Client => "Client",
            _ => "Utilisateur"
        };

        return new ProfilUtilisateurDto(
            user.Id,
            user.Nom,
            user.Prenom,
            user.Email.Value,
            user.Telephone,
            role,
            user.DateNaissance,
            user.Sexe,
            user.Adresse,
            user.Photo);
    }
}
