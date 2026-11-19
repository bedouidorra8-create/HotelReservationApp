using HotelReservation.Domain.Common;
using HotelReservation.Domain.Repositories;

namespace HotelReservation.Application.UseCases;

public class SupprimerHotelUseCase
{
    private readonly IHotelRepository _repository;

    public SupprimerHotelUseCase(IHotelRepository repository)
    {
        _repository = repository;
    }

    public async Task ExecuteAsync(Guid hotelId, CancellationToken cancellationToken = default)
    {
        var hotel = await _repository.GetByIdAsync(hotelId, cancellationToken)
            ?? throw new DomainException($"Hôtel avec l'ID '{hotelId}' introuvable.");
        await _repository.DeleteAsync(hotel, cancellationToken);
    }
}
