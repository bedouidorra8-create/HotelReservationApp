using HotelReservation.Domain.Common;

namespace HotelReservation.Domain.ValueObjects;

public class DateRange : ValueObject
{
    public DateTime DateArrivee { get; }
    public DateTime DateDepart { get; }

    private DateRange() { }

    public DateRange(DateTime dateArrivee, DateTime dateDepart)
    {
        if (dateDepart <= dateArrivee)
            throw new DomainException("La date de départ doit être strictement supérieure à la date d'arrivée.");

        DateArrivee = dateArrivee.Date;
        DateDepart = dateDepart.Date;
    }

    public int NbNuits()
    {
        return (DateDepart - DateArrivee).Days;
    }

    public bool ChevaucheAvec(DateRange autre)
    {
        if (autre == null) return false;
        // Intersections between [DateArrivee, DateDepart) and [autre.DateArrivee, autre.DateDepart)
        return DateArrivee < autre.DateDepart && autre.DateArrivee < DateDepart;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return DateArrivee;
        yield return DateDepart;
    }

    public override string ToString() => $"{DateArrivee:yyyy-MM-dd} -> {DateDepart:yyyy-MM-dd} ({NbNuits()} nuits)";
}
