using HotelReservation.Application.DTOs;
using HotelReservation.Domain.Repositories;

namespace HotelReservation.Application.UseCases;

public class RechercherUtilisateurUseCase
{
    private readonly IUtilisateurRepository _repository;

    public RechercherUtilisateurUseCase(IUtilisateurRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<UtilisateurDto>> ExecuteAsync(RechercherUtilisateurRequest request, CancellationToken cancellationToken = default)
    {
        var terme = request.Terme?.Trim();
        var utilisateurs = await _repository.GetAllAsync(cancellationToken);
        return utilisateurs.Where(utilisateur =>
                (string.IsNullOrWhiteSpace(terme)
                    || utilisateur.Nom.Contains(terme, StringComparison.OrdinalIgnoreCase)
                    || utilisateur.Prenom.Contains(terme, StringComparison.OrdinalIgnoreCase)
                    || utilisateur.Email.Value.Contains(terme, StringComparison.OrdinalIgnoreCase))
                && (string.IsNullOrWhiteSpace(request.Role)
                    || string.Equals(ConsulterListeUtilisateursUseCase.RoleOf(utilisateur), request.Role, StringComparison.OrdinalIgnoreCase)))
            .Select(ConsulterListeUtilisateursUseCase.ToDto);
    }
}
