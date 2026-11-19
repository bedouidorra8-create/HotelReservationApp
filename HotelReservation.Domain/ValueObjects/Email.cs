using System.Text.RegularExpressions;
using HotelReservation.Domain.Common;

namespace HotelReservation.Domain.ValueObjects;

public class Email : ValueObject
{
    private static readonly Regex EmailRegex = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public string Value { get; }

    // Private constructor for EF Core
    private Email()
    {
        Value = string.Empty;
    }

    public Email(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("L'adresse e-mail ne peut pas être vide.");

        var trimmedValue = value.Trim();

        if (!EmailRegex.IsMatch(trimmedValue))
            throw new DomainException($"L'adresse e-mail '{value}' n'est pas valide.");

        Value = trimmedValue;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value.ToLowerInvariant();
    }

    public override string ToString() => Value;
}
