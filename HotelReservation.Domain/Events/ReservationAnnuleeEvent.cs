using HotelReservation.Domain.Common;

namespace HotelReservation.Domain.Events;

public class ReservationAnnuleeEvent : IDomainEvent
{
    public Guid ReservationId { get; }
    public Guid ChambreId { get; }
    public DateTime OccurredOn { get; }

    public ReservationAnnuleeEvent(Guid reservationId, Guid chambreId)
    {
        ReservationId = reservationId;
        ChambreId = chambreId;
        OccurredOn = DateTime.UtcNow;
    }
}
