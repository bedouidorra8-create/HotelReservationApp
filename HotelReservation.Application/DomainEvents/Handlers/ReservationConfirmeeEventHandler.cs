using HotelReservation.Application.DomainEvents;
using HotelReservation.Domain.Enums;
using HotelReservation.Domain.Events;
using HotelReservation.Domain.Repositories;
using MediatR;

namespace HotelReservation.Application.DomainEvents.Handlers;

public sealed class ReservationConfirmeeEventHandler
    : INotificationHandler<DomainEventNotification<ReservationConfirmeeEvent>>
{
    private readonly IHotelRepository _hotelRepository;

    public ReservationConfirmeeEventHandler(IHotelRepository hotelRepository)
    {
        _hotelRepository = hotelRepository;
    }

    public async Task Handle(
        DomainEventNotification<ReservationConfirmeeEvent> notification,
        CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;
        var hotel = (await _hotelRepository.GetAllAsync(cancellationToken))
            .FirstOrDefault(candidate => candidate.Chambres.Any(chambre => chambre.Id == domainEvent.ChambreId))
            ?? throw new InvalidOperationException($"Hôtel de la chambre '{domainEvent.ChambreId}' introuvable.");

        hotel.ChangerStatutChambre(domainEvent.ChambreId, StatutChambre.Occupee);
        await _hotelRepository.UpdateAsync(hotel, cancellationToken);
    }
}