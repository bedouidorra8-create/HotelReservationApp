using HotelReservation.Domain.Common;
using HotelReservation.Domain.Enums;
using HotelReservation.Domain.Events;
using HotelReservation.Domain.ValueObjects;

namespace HotelReservation.Domain.Entities;

public class Avis : AggregateRoot<Guid>
{
    public Guid ClientId { get; private set; }
    public Guid HotelId { get; private set; }
    public Guid ReservationId { get; private set; }
    public NoteAvis Note { get; private set; } = null!;
    public string Commentaire { get; private set; } = string.Empty;
    public DateTime Date { get; private set; } = DateTime.UtcNow;
    public StatutAvis Statut { get; private set; }

    private Avis() { }

    public Avis(
        Guid id,
        Guid clientId,
        Guid hotelId,
        Guid reservationId,
        NoteAvis note,
        string commentaire)
        : base(id)
    {
        ClientId = clientId;
        HotelId = hotelId;
        ReservationId = reservationId;
        Note = note ?? throw new ArgumentNullException(nameof(note));
        Commentaire = commentaire;
        Statut = StatutAvis.EnAttente;
        Date = DateTime.UtcNow;

        AddDomainEvent(new DemandeAvisCreeeEvent(id, clientId, hotelId));
    }

    public void Valider()
    {
        Statut = StatutAvis.Valide;
    }

    public void Rejeter()
    {
        Statut = StatutAvis.Rejete;
    }

    // AJOUTÉ — nécessaire pour ModifierAvisUseCase
    // ⚠️ à ajuster : j'ai supposé "pas de modif si déjà validé" + un event AvisModifieEvent.
    // Supprime ces 2 lignes si tu ne veux pas cette contrainte / cet event pour l'instant.
    public void Modifier(NoteAvis note, string commentaire)
    {
        if (Statut == StatutAvis.Valide)
            throw new DomainException("Impossible de modifier un avis déjà validé");

        Note = note ?? throw new ArgumentNullException(nameof(note));
        Commentaire = commentaire;
        Statut = StatutAvis.EnAttente;

        AddDomainEvent(new AvisModifieEvent(Id, ClientId, HotelId));
    }

    /// <summary>Permet d'ajuster la date de l'avis lors du seeding ou de l'import d'historique.</summary>
    public void DefinirDate(DateTime date)
    {
        Date = date;
    }
}