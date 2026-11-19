using HotelReservation.Application.DTOs;
using HotelReservation.Domain.Entities;
using HotelReservation.Domain.Repositories;

namespace HotelReservation.Application.UseCases;

public class ConsulterListeUtilisateursUseCase
{
    private readonly IUtilisateurRepository _repository;

    public ConsulterListeUtilisateursUseCase(IUtilisateurRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<UtilisateurDto>> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var utilisateurs = await _repository.GetAllAsync(cancellationToken);
        return utilisateurs.Select(ToDto);
    }

    internal static UtilisateurDto ToDto(Utilisateur utilisateur) => new(
        utilisateur.Id, utilisateur.Nom, utilisateur.Prenom, utilisateur.Email.Value,
        utilisateur.Telephone, RoleOf(utilisateur), utilisateur.Statut, utilisateur.DateCreation);

    internal static string RoleOf(Utilisateur utilisateur) => utilisateur switch
    {
        Administrateur => "Administrateur",
        Agent => "Agent",
        _ => "Client"
    };
}
