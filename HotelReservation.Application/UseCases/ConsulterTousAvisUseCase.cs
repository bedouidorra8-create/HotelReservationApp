using HotelReservation.Application.DTOs;
using HotelReservation.Domain.Enums;
using HotelReservation.Domain.Repositories;

namespace HotelReservation.Application.UseCases;

public class ConsulterTousAvisUseCase
{
    private readonly IAvisRepository _avisRepository;

    public ConsulterTousAvisUseCase(IAvisRepository avisRepository)
    {
        _avisRepository = avisRepository;
    }

    public async Task<IEnumerable<AvisDto>> ExecuteAsync(StatutAvis? statut = null, CancellationToken cancellationToken = default)
    {
        var allAvis = await _avisRepository.GetAllAsync(cancellationToken);
        if (statut.HasValue)
        {
            allAvis = allAvis.Where(a => a.Statut == statut.Value);
        }

        return allAvis
            .OrderByDescending(a => a.Date)
            .Select(a => new AvisDto(
                a.Id,
                a.ClientId,
                a.HotelId,
                a.ReservationId,
                a.Note.Valeur,
                a.Commentaire,
                a.Date,
                a.Statut
            ));
    }
}
