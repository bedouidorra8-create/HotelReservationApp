using HotelReservation.Domain.Common;

namespace HotelReservation.Domain.Entities;

/// <summary>Représente l'association persistée entre un utilisateur et un hôtel favori.</summary>
public class Favori : Entity<Guid>
{
    public Guid UtilisateurId { get; private set; }
    public Guid HotelId { get; private set; }
    public DateTime DateAjout { get; private set; }

    private Favori() { }

    public Favori(Guid id, Guid utilisateurId, Guid hotelId) : base(id)
    {
        UtilisateurId = utilisateurId;
        HotelId = hotelId;
        DateAjout = DateTime.UtcNow;
    }
}