using HotelReservation.Domain.Common;

namespace HotelReservation.Domain.Events;

public class DemandeAvisCreeeEvent : IDomainEvent
{
    public Guid AvisId { get; }
    public Guid ClientId { get; }
    public Guid HotelId { get; }
    public DateTime OccurredOn { get; }

    public DemandeAvisCreeeEvent(Guid avisId, Guid clientId, Guid hotelId)
    {
        AvisId = avisId;
        ClientId = clientId;
        HotelId = hotelId;
        OccurredOn = DateTime.UtcNow;
    }
}
