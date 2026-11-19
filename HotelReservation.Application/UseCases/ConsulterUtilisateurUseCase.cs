using HotelReservation.Application.DTOs;
using HotelReservation.Domain.Common;
using HotelReservation.Domain.Entities;
using HotelReservation.Domain.Repositories;

namespace HotelReservation.Application.UseCases;

public class ConsulterUtilisateurUseCase
{
    private readonly IUtilisateurRepository _utilisateurRepository;

    public ConsulterUtilisateurUseCase(IUtilisateurRepository utilisateurRepository)
    {
        _utilisateurRepository = utilisateurRepository;
    }

    public async Task<UtilisateurDto> ExecuteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var utilisateur = await _utilisateurRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new DomainException($"Utilisateur avec l'ID '{id}' introuvable.");

        var role = utilisateur switch
        {
            Administrateur => "Administrateur",
            Agent => "Agent",
            Client => "Client",
            _ => "Inconnu"
        };

        return new UtilisateurDto(
            utilisateur.Id,
            utilisateur.Nom,
            utilisateur.Prenom,
            utilisateur.Email.Value,
            utilisateur.Telephone,
            role,
            utilisateur.Statut,
            utilisateur.DateCreation
        );
    }
}
