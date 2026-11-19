namespace HotelReservation.Domain.Common;

public interface IDomainEvent
{
    DateTime OccurredOn { get; }
}
