namespace HotelReservation.Application.Services;

/// <summary>
/// Abstraction du temps système permettant de manipuler des dates déterministes dans les tests.
/// </summary>
public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
    DateOnly Today { get; }
}
