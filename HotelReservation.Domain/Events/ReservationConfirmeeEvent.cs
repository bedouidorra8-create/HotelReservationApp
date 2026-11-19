using HotelReservation.Domain.Common;

namespace HotelReservation.Domain.Events;

public class ReservationConfirmeeEvent : IDomainEvent
{
    public Guid ReservationId { get; }
    public Guid ChambreId { get; }
    public Guid ClientId { get; }
    public DateTime OccurredOn { get; }

    public ReservationConfirmeeEvent(Guid reservationId, Guid chambreId, Guid clientId)
    {
        ReservationId = reservationId;
        ChambreId = chambreId;
        ClientId = clientId;
        OccurredOn = DateTime.UtcNow;
    }
}
