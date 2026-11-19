using HotelReservation.Application.DTOs;
using HotelReservation.Domain.Common;
using HotelReservation.Domain.Entities;
using HotelReservation.Domain.Repositories;

namespace HotelReservation.Application.UseCases;

public class ConsulterMonProfilUseCase
{
    private readonly IUtilisateurRepository _utilisateurRepository;

    public ConsulterMonProfilUseCase(IUtilisateurRepository utilisateurRepository)
    {
        _utilisateurRepository = utilisateurRepository;
    }

    public async Task<ProfilUtilisateurDto> ExecuteAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _utilisateurRepository.GetByIdAsync(userId, cancellationToken)
            ?? throw new DomainException($"Utilisateur avec l'ID '{userId}' introuvable.");

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
