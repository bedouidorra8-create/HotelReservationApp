using HotelReservation.Application.DomainEvents;
using HotelReservation.Domain.Events;
using HotelReservation.Domain.Repositories;
using MediatR;

namespace HotelReservation.Application.DomainEvents.Handlers;

public sealed class ReservationAnnuleeEventHandler
    : INotificationHandler<DomainEventNotification<ReservationAnnuleeEvent>>
{
    private readonly IHotelRepository _hotelRepository;

    public ReservationAnnuleeEventHandler(IHotelRepository hotelRepository)
    {
        _hotelRepository = hotelRepository;
    }

    public async Task Handle(
        DomainEventNotification<ReservationAnnuleeEvent> notification,
        CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;
        var hotel = (await _hotelRepository.GetAllAsync(cancellationToken))
            .FirstOrDefault(candidate => candidate.Chambres.Any(chambre => chambre.Id == domainEvent.ChambreId))
            ?? throw new InvalidOperationException($"Hôtel de la chambre '{domainEvent.ChambreId}' introuvable.");

        hotel.LibererUneChambre(domainEvent.ChambreId);
        await _hotelRepository.UpdateAsync(hotel, cancellationToken);
    }
}