using HotelReservation.Application.DTOs;
using HotelReservation.Domain.Repositories;

namespace HotelReservation.Application.UseCases;

public class ConsulterAuditLogsUseCase
{
    private readonly IAuditLogRepository _repository;

    public ConsulterAuditLogsUseCase(IAuditLogRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<AuditLogDto>> ExecuteAsync(
        DateTime? dateDebut = null,
        DateTime? dateFin = null,
        CancellationToken cancellationToken = default)
    {
        var logs = await _repository.GetAsync(dateDebut, dateFin, cancellationToken);
        return logs.Select(log => new AuditLogDto(
            log.Id,
            log.UtilisateurId,
            log.Action,
            log.Entite,
            log.EntiteId,
            log.AncienneValeur,
            log.NouvelleValeur,
            log.DateHeure));
    }
}
