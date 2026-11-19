using HotelReservation.Application.DTOs;
using HotelReservation.Application.UseCases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelReservation.Api.Controllers;

[ApiController]
[Route("api/audit-logs")]
[Authorize(Roles = "Administrateur")]
public class AuditLogsController : ControllerBase
{
    private readonly ConsulterAuditLogsUseCase _useCase;

    public AuditLogsController(ConsulterAuditLogsUseCase useCase)
    {
        _useCase = useCase;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AuditLogDto>>> Get(
        [FromQuery] DateTime? dateDebut,
        [FromQuery] DateTime? dateFin,
        CancellationToken cancellationToken)
    {
        if (dateDebut.HasValue && dateFin.HasValue && dateFin <= dateDebut)
            return BadRequest("La dateFin doit être supérieure à la dateDebut.");

        return Ok(await _useCase.ExecuteAsync(dateDebut, dateFin, cancellationToken));
    }
}
