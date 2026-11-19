using HotelReservation.Domain.Common;

namespace HotelReservation.Domain.ValueObjects;

public class Money : ValueObject
{
    public decimal Amount { get; }
    public string Currency { get; }

    private Money()
    {
        Currency = "EUR";
    }

    public Money(decimal amount, string currency = "EUR")
    {
        if (amount < 0)
            throw new DomainException("Le montant ne peut pas être négatif.");

        if (string.IsNullOrWhiteSpace(currency))
            throw new DomainException("La devise ne peut pas être vide.");

        Amount = amount;
        Currency = currency.ToUpperInvariant();
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }

    public override string ToString() => $"{Amount:F2} {Currency}";
}
