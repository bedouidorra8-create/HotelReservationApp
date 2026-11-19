using HotelReservation.Domain.Common;

namespace HotelReservation.Domain.ValueObjects;

public class NoteAvis : ValueObject
{
    public int Valeur { get; }

    private NoteAvis() { }

    public NoteAvis(int valeur)
    {
        if (valeur < 1 || valeur > 5)
            throw new DomainException($"La note de l'avis doit être comprise entre 1 et 5. Valeur reçue: {valeur}");

        Valeur = valeur;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Valeur;
    }

    public override string ToString() => $"{Valeur}/5";
}
