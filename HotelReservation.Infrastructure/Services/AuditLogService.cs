using HotelReservation.Application.Services;
using HotelReservation.Domain.Entities;
using HotelReservation.Domain.Repositories;

namespace HotelReservation.Infrastructure.Services;

public class AuditLogService : IAuditLogService
{
    private readonly IAuditLogRepository _repository;

    public AuditLogService(IAuditLogRepository repository)
    {
        _repository = repository;
    }

    public Task EnregistrerAsync(
        Guid? utilisateurId,
        string action,
        string entite,
        Guid? entiteId,
        string? ancienneValeur = null,
        string? nouvelleValeur = null,
        CancellationToken cancellationToken = default)
    {
        var log = new AuditLog(
            Guid.NewGuid(),
            utilisateurId,
            action,
            entite,
            entiteId,
            ancienneValeur,
            nouvelleValeur);
        return _repository.AddAsync(log, cancellationToken);
    }
}
