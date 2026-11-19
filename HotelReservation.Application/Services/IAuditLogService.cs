namespace HotelReservation.Application.Services;

public interface IAuditLogService
{
    Task EnregistrerAsync(
        Guid? utilisateurId,
        string action,
        string entite,
        Guid? entiteId,
        string? ancienneValeur = null,
        string? nouvelleValeur = null,
        CancellationToken cancellationToken = default);
}
