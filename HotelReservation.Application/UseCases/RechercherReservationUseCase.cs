using HotelReservation.Application.DTOs;
using HotelReservation.Domain.Repositories;

namespace HotelReservation.Application.UseCases;

public class RechercherReservationUseCase
{
    private readonly IReservationRepository _repository;

    public RechercherReservationUseCase(IReservationRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<ReservationDto>> ExecuteAsync(RechercherReservationRequest request, CancellationToken cancellationToken = default)
    {
        var reservations = await _repository.GetAllAsync(cancellationToken);
        return reservations
            .Where(r => !request.ClientId.HasValue || r.ClientId == request.ClientId)
            .Where(r => !request.HotelId.HasValue || r.HotelId == request.HotelId)
            .Where(r => !request.Statut.HasValue || r.Statut == request.Statut)
            .Where(r => !request.DateDebut.HasValue || r.Periode.DateDepart > request.DateDebut)
            .Where(r => !request.DateFin.HasValue || r.Periode.DateArrivee < request.DateFin)
            .Select(ConsulterListeReservationsUseCase.ToDto);
    }
}
