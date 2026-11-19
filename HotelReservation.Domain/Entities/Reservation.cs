using HotelReservation.Domain.Common;
using HotelReservation.Domain.Enums;
using HotelReservation.Domain.Events;
using HotelReservation.Domain.ValueObjects;

namespace HotelReservation.Domain.Entities;

/// <summary>
/// Racine d'agrégat représentant une réservation effectuée par un client pour une chambre d'hôtel sur une période donnée.
/// Gère le cycle de vie (EnAttente, Confirmée, Annulée, Terminée) et fige les conditions financières (PrixUnitaire, Total).
/// </summary>
public class Reservation : AggregateRoot<Guid>
{
    /// <summary>Identifiant unique de la chambre réservée.</summary>
    public Guid ChambreId { get; private set; }

    /// <summary>Identifiant unique du client ayant réservé.</summary>
    public Guid ClientId { get; private set; }

    /// <summary>Identifiant unique de l'hôtel associé.</summary>
    public Guid HotelId { get; private set; }

    /// <summary>Date d'arrivée prévue du client.</summary>
    public DateTime DateArrivee { get; private set; }

    /// <summary>Date de départ prévue du client.</summary>
    public DateTime DateDepart { get; private set; }

    /// <summary>Nombre de nuits calculé pour le séjour.</summary>
    public int NbNuits => (DateDepart - DateArrivee).Days;

    /// <summary>Nombre de personnes déclarées pour le séjour.</summary>
    public int NbPersonnes { get; private set; }

    /// <summary>
    /// Tarif par nuit de la chambre au moment exact de la réservation.
    /// RÈGLE MÉTIER : Ce tarif est figé à la création et ne varie pas si le prix courant de la chambre est modifié ultérieurement.
    /// </summary>
    public Money PrixUnitaire { get; private set; } = null!;

    /// <summary>
    /// Montant total calculé de la réservation (PrixUnitaire * NbNuits).
    /// RÈGLE MÉTIER : Fiché dès la réservation pour garantir la stabilité financière et contractuelle.
    /// </summary>
    public Money Total { get; private set; } = null!;

    /// <summary>Statut courant de la réservation (EnAttente, Confirmee, Annulee, Terminee).</summary>
    public StatutReservation Statut { get; private set; }

    /// <summary>Date et heure d'enregistrement de la réservation (UTC).</summary>
    public DateTime DateCreation { get; private set; }

    /// <summary>Plage de dates calculée représentant l'intervalle du séjour.</summary>
    public DateRange Periode => new(DateArrivee, DateDepart);

    private Reservation() { }

    /// <summary>
    /// Initialise une nouvelle réservation avec tarif unitaire et total figés.
    /// </summary>
    /// <param name="id">Identifiant unique de la réservation.</param>
    /// <param name="chambreId">Identifiant de la chambre.</param>
    /// <param name="clientId">Identifiant du client.</param>
    /// <param name="hotelId">Identifiant de l'hôtel.</param>
    /// <param name="dateArrivee">Date d'arrivée.</param>
    /// <param name="dateDepart">Date de départ.</param>
    /// <param name="nbPersonnes">Nombre de personnes.</param>
    /// <param name="prixUnitaire">Prix unitaire par nuit de la chambre.</param>
    /// <exception cref="DomainException">Levée si les dates ou le nombre de personnes sont invalides.</exception>
    /// <exception cref="ArgumentNullException">Levée si le prix unitaire est nul.</exception>
    internal Reservation(
        Guid id,
        Guid chambreId,
        Guid clientId,
        Guid hotelId,
        DateTime dateArrivee,
        DateTime dateDepart,
        int nbPersonnes,
        Money prixUnitaire)
        : base(id)
    {
        ValiderPeriode(dateArrivee, dateDepart);
        if (nbPersonnes <= 0)
            throw new DomainException("Le nombre de personnes doit être positif.");

        ChambreId = chambreId;
        ClientId = clientId;
        HotelId = hotelId;
        DateArrivee = dateArrivee;
        DateDepart = dateDepart;
        NbPersonnes = nbPersonnes;

        // RÈGLE MÉTIER CRITIQUE : Figeage du prix unitaire et du total contractuel lors de la réservation
        PrixUnitaire = prixUnitaire ?? throw new ArgumentNullException(nameof(prixUnitaire));
        Total = new Money(prixUnitaire.Amount * NbNuits, prixUnitaire.Currency);

        Statut = StatutReservation.EnAttente;
        DateCreation = DateTime.UtcNow;
    }

    /// <summary>
    /// Modifie les dates ou le nombre de personnes d'une réservation en attente.
    /// </summary>
    /// <param name="dateArrivee">Nouvelle date d'arrivée.</param>
    /// <param name="dateDepart">Nouvelle date de départ.</param>
    /// <param name="nbPersonnes">Nouveau nombre de personnes.</param>
    /// <exception cref="DomainException">Levée si la réservation n'est plus en attente ou si les données sont invalides.</exception>
    public void Modifier(DateTime dateArrivee, DateTime dateDepart, int nbPersonnes)
    {
        if (Statut != StatutReservation.EnAttente)
            throw new DomainException("Impossible de modifier une réservation déjà confirmée ou annulée.");

        ValiderPeriode(dateArrivee, dateDepart);
        if (nbPersonnes <= 0)
            throw new DomainException("Le nombre de personnes doit être positif.");

        DateArrivee = dateArrivee;
        DateDepart = dateDepart;
        NbPersonnes = nbPersonnes;

        // Recalcul du montant total basé sur le prix unitaire d'origine figé
        Total = new Money(PrixUnitaire.Amount * NbNuits, PrixUnitaire.Currency);
    }

    /// <summary>
    /// Confirme la réservation.
    /// </summary>
    /// <exception cref="DomainException">Levée si la réservation n'est pas en attente.</exception>
    public void Confirmer()
    {
        if (Statut != StatutReservation.EnAttente)
            throw new DomainException("Impossible de confirmer cette réservation.");

        Statut = StatutReservation.Confirmee;
        AddDomainEvent(new ReservationConfirmeeEvent(Id, ChambreId, ClientId));
    }

    /// <summary>
    /// Annule la réservation.
    /// </summary>
    /// <exception cref="DomainException">Levée si la réservation est déjà terminée.</exception>
    public void Annuler()
    {
        if (Statut == StatutReservation.Terminee)
            throw new DomainException("Impossible d'annuler une réservation terminée.");

        Statut = StatutReservation.Annulee;
        AddDomainEvent(new ReservationAnnuleeEvent(Id, ChambreId));
    }

    /// <summary>
    /// Change le statut de la réservation.
    /// </summary>
    /// <param name="nouveauStatut">Nouveau statut.</param>
    /// <exception cref="DomainException">Levée si la transition est interdite.</exception>
    public void ChangerStatut(StatutReservation nouveauStatut)
    {
        if (Statut == StatutReservation.Annulee)
            throw new DomainException("Impossible de changer le statut d'une réservation annulée.");
        if (Statut == StatutReservation.Terminee)
            throw new DomainException("Impossible de changer le statut d'une réservation terminée.");

        Statut = nouveauStatut;
    }

    /// <summary>
    /// Permet de définir la date de création lors du seeding ou de l'import d'historique.
    /// </summary>
    public void DefinirDateCreation(DateTime date)
    {
        DateCreation = date;
    }

    private static void ValiderPeriode(DateTime arrivee, DateTime depart)
    {
        // RÈGLE MÉTIER : La date de départ doit être strictement postérieure à la date d'arrivée
        if (depart <= arrivee)
            throw new DomainException("La date de départ doit être après la date d'arrivée.");
    }
}