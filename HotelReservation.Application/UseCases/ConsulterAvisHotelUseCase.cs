using HotelReservation.Application.DTOs;
using HotelReservation.Domain.Enums;
using HotelReservation.Domain.Repositories;

namespace HotelReservation.Application.UseCases;

public class ConsulterAvisHotelUseCase
{
    private readonly IAvisRepository _avisRepository;

    public ConsulterAvisHotelUseCase(IAvisRepository avisRepository)
    {
        _avisRepository = avisRepository;
    }

    public async Task<IEnumerable<AvisDto>> ExecuteAsync(Guid hotelId, CancellationToken cancellationToken = default)
    {
        var avisList = (await _avisRepository.GetByHotelIdAsync(hotelId))
            .Where(a => a.Statut == StatutAvis.Valide);

        return avisList.Select(a => new AvisDto(
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
