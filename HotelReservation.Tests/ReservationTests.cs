using HotelReservation.Domain.Common;
using HotelReservation.Domain.Entities;
using HotelReservation.Domain.Enums;
using HotelReservation.Domain.Events;
using HotelReservation.Domain.Services;
using HotelReservation.Domain.ValueObjects;
using Xunit;

namespace HotelReservation.Tests;

public class ReservationTests
{
    private static Reservation CreateReservation(IEnumerable<Reservation>? existing = null)
    {
        var service = new ReservationDomainService();
        return service.CreerReservation(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            new DateRange(DateTime.UtcNow.Date.AddDays(1), DateTime.UtcNow.Date.AddDays(3)),
            2, existing ?? [], new Money(100, "EUR"));
    }

    [Fact]
    public void Creation_calculates_total_and_nights()
    {
        var reservation = CreateReservation();

        Assert.Equal(2, reservation.NbNuits);
        Assert.Equal(200, reservation.Total.Amount);
        Assert.Equal(StatutReservation.EnAttente, reservation.Statut);
    }

    [Fact]
    public void Overlapping_reservation_is_rejected()
    {
        var existing = CreateReservation();
        var service = new ReservationDomainService();

        Assert.Throws<DomainException>(() => service.CreerReservation(
            existing.ChambreId, Guid.NewGuid(), existing.HotelId,
            new DateRange(existing.DateArrivee.AddDays(1), existing.DateDepart.AddDays(1)),
            1, [existing], new Money(100)));
    }

    [Fact]
    public void Confirming_raises_event()
    {
        var reservation = CreateReservation();

        reservation.Confirmer();

        Assert.IsType<ReservationConfirmeeEvent>(Assert.Single(reservation.DomainEvents));
    }

    [Fact]
    public void Cancelling_raises_event()
    {
        var reservation = CreateReservation();

        reservation.Annuler();

        Assert.IsType<ReservationAnnuleeEvent>(Assert.Single(reservation.DomainEvents));
    }

    [Fact]
    public void Terminated_Reservation_Cannot_Be_Cancelled()
    {
        var reservation = CreateReservation();
        reservation.Confirmer();
        reservation.ChangerStatut(StatutReservation.Terminee);

        var ex = Assert.Throws<DomainException>(() => reservation.Annuler());
        Assert.Contains("terminée", ex.Message);
    }

    [Fact]
    public void Cancelled_Reservation_Does_Not_Block_Room_Availability()
    {
        var existing = CreateReservation();
        existing.Annuler();

        var service = new ReservationDomainService();
        var canBook = service.PeutReserver(
            existing.ChambreId,
            new DateRange(existing.DateArrivee, existing.DateDepart),
            new[] { existing });

        Assert.True(canBook);
    }

    [Fact]
    public void Modifying_Non_Pending_Reservation_Throws_DomainException()
    {
        var reservation = CreateReservation();
        reservation.Confirmer();

        var ex = Assert.Throws<DomainException>(() =>
            reservation.Modifier(DateTime.UtcNow.AddDays(10), DateTime.UtcNow.AddDays(14), 2));
        Assert.Contains("confirmée", ex.Message);
    }
}
