namespace HotelReservation.Application.Services;

/// <summary>
/// Fournit les informations de l'utilisateur courant authentifié.
/// Permet à la couche Application d'effectuer les contrôles d'autorisation métier et d'isolation IDOR.
/// </summary>
public interface ICurrentUserService
{
    Guid? UserId { get; }
    string? Role { get; }
    bool IsAuthenticated { get; }
    bool IsInRole(string role);
}
