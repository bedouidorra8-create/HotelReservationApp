using HotelReservation.Domain.Common;

namespace HotelReservation.Domain.Events;

public class AvisModifieEvent : IDomainEvent
{
    public Guid AvisId { get; }
    public Guid ClientId { get; }
    public Guid HotelId { get; }
    public DateTime OccurredOn { get; }

    public AvisModifieEvent(Guid avisId, Guid clientId, Guid hotelId)
    {
        AvisId = avisId;
        ClientId = clientId;
        HotelId = hotelId;
        OccurredOn = DateTime.UtcNow;
    }
}