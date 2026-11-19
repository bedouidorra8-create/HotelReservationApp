using HotelReservation.Domain.Common;
using HotelReservation.Domain.Enums;
using HotelReservation.Domain.ValueObjects;

namespace HotelReservation.Domain.Entities;

/// <summary>
/// Agrégat représentant une transaction de paiement associée à une réservation hôtelière.
/// </summary>
public class Paiement : AggregateRoot<Guid>
{
    /// <summary>Identifiant de la réservation associée au paiement.</summary>
    public Guid ReservationId { get; private set; }

    /// <summary>Montant et devise du paiement (Value Object Money).</summary>
    public Money Montant { get; private set; } = null!;

    /// <summary>Date et heure d'enregistrement du paiement (UTC).</summary>
    public DateTime Date { get; private set; } = DateTime.UtcNow;

    /// <summary>Mode de paiement utilisé (Carte, Espèces, Virement, Chèque).</summary>
    public ModePaiement ModePaiement { get; private set; }

    /// <summary>Référence unique de la transaction bancaire ou du reçu.</summary>
    public string Reference { get; private set; } = string.Empty;

    /// <summary>Statut actuel du paiement (EnAttente, Paye, Rembourse, Echoue, Annule).</summary>
    public StatutPaiement Statut { get; private set; }

    /// <summary>Identifiant de l'agent ou de l'administrateur ayant enregistré le paiement (optionnel).</summary>
    public Guid? EnregistrePar { get; private set; }

    private Paiement() { }

    /// <summary>
    /// Initialise une nouvelle instance de paiement avec le statut "Payé".
    /// </summary>
    /// <param name="id">Identifiant unique du paiement.</param>
    /// <param name="reservationId">Identifiant de la réservation liée.</param>
    /// <param name="montant">Montant monétaire de la transaction.</param>
    /// <param name="modePaiement">Mode de règlement.</param>
    /// <param name="reference">Référence transactionnelle.</param>
    /// <param name="enregistrePar">Identifiant optionnel du personnel enregistreur.</param>
    /// <exception cref="ArgumentNullException">Levée si le montant est nul.</exception>
    public Paiement(
        Guid id,
        Guid reservationId,
        Money montant,
        ModePaiement modePaiement,
        string reference,
        Guid? enregistrePar = null)
        : base(id)
    {
        ReservationId = reservationId;
        Montant = montant ?? throw new ArgumentNullException(nameof(montant));
        ModePaiement = modePaiement;
        Reference = reference;
        // Règle métier : à la création effective, le paiement enregistré passe directement au statut Payé
        Statut = StatutPaiement.Paye;
        Date = DateTime.UtcNow;
        EnregistrePar = enregistrePar;
    }

    /// <summary>
    /// Annule un paiement existant.
    /// </summary>
    /// <exception cref="DomainException">Levée si le paiement est déjà annulé.</exception>
    public void Annuler()
    {
        // Règle métier : interdiction d'annuler un paiement déjà dans l'état annulé
        if (Statut == StatutPaiement.Annule)
            throw new DomainException("Ce paiement est déjà annulé.");

        Statut = StatutPaiement.Annule;
    }

    /// <summary>
    /// Permet de définir la date de la transaction lors du seeding ou de l'import d'historique.
    /// </summary>
    public void DefinirDate(DateTime date)
    {
        Date = date;
    }
}
 

