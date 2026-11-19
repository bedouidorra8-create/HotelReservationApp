using HotelReservation.Domain.Common;
using HotelReservation.Domain.Entities;
using HotelReservation.Domain.Enums;
using HotelReservation.Domain.Repositories;
using HotelReservation.Domain.ValueObjects;

namespace HotelReservation.Domain.Services;

/// <summary>
/// Service de domaine gérant les règles métiers transverses liées aux réservations de chambres,
/// notamment la vérification stricte de disponibilité et de non-chevauchement de dates.
/// </summary>
public class ReservationDomainService
{
    /// <summary>
    /// Vérifie si une chambre est disponible pour une période donnée en s'assurant
    /// qu'aucune réservation active (non annulée) ne chevauche l'intervalle demandé.
    /// </summary>
    /// <param name="repository">Repository pour charger les réservations actives de la chambre.</param>
    /// <param name="chambreId">Identifiant unique de la chambre ciblée.</param>
    /// <param name="dateArrivee">Date d'arrivée souhaitée.</param>
    /// <param name="dateDepart">Date de départ souhaitée.</param>
    /// <param name="reservationIdAExclure">Identifiant optionnel d'une réservation à exclure de la vérification (ex: en cas de modification de dates d'une réservation existante).</param>
    /// <param name="cancellationToken">Jeton d'annulation asynchrone.</param>
    /// <exception cref="DomainException">Levée si la chambre fait déjà l'objet d'une réservation active sur cette période.</exception>
    public async Task VerifierDisponibilite(
        IReservationRepository repository,
        Guid chambreId,
        DateTime dateArrivee,
        DateTime dateDepart,
        Guid? reservationIdAExclure = null,
        CancellationToken cancellationToken = default)
    {
        if (dateDepart <= dateArrivee)
            throw new DomainException("La date de départ doit être postérieure à la date d'arrivée.");

        var dateDebut = dateArrivee.Date;
        var dateFin = dateDepart.Date;
        var reservationsActives = await repository.GetReservationsActivesPourChambre(chambreId, cancellationToken);

        // RÈGLE MÉTIER CRITIQUE : Deux réservations non annulées pour une même chambre ne peuvent pas se chevaucher dans le temps.
        // Formule de chevauchement d'intervalles [DateArrivee, DateDepart[ : r.DateArrivee < dateFin && r.DateDepart > dateDebut
        var chevauchement = reservationsActives.Any(r =>
            (reservationIdAExclure == null || r.Id != reservationIdAExclure.Value)
            && r.Statut != StatutReservation.Annulee
            && r.Periode.DateArrivee < dateFin
            && r.Periode.DateDepart > dateDebut);

        if (chevauchement)
        {
            throw new DomainException("La chambre n'est pas disponible pour cette période.");
        }
    }

    /// <summary>
    /// Vérifie si une chambre peut être réservée sur une période donnée en mémoire.
    /// </summary>
    /// <param name="chambreId">Identifiant de la chambre.</param>
    /// <param name="periode">Plage de dates souhaitée.</param>
    /// <param name="reservationsActivesDeLaChambre">Liste des réservations existantes.</param>
    /// <returns>Vrai si la chambre est disponible, faux sinon.</returns>
    public bool PeutReserver(Guid chambreId, DateRange periode, IEnumerable<Reservation> reservationsActivesDeLaChambre)
    {
        if (periode == null) throw new ArgumentNullException(nameof(periode));

        // Règle métier : détection de chevauchement sur la liste des réservations actives
        foreach (var res in reservationsActivesDeLaChambre)
        {
            if (res.ChambreId == chambreId && res.Statut != StatutReservation.Annulee)
            {
                if (res.Periode.ChevaucheAvec(periode))
                {
                    return false;
                }
            }
        }

        return true;
    }

    /// <summary>
    /// Crée et initialise une nouvelle instance de réservation après vérification de disponibilité.
    /// </summary>
    /// <param name="chambreId">Identifiant de la chambre.</param>
    /// <param name="clientId">Identifiant du client ayant réservé.</param>
    /// <param name="hotelId">Identifiant de l'hôtel.</param>
    /// <param name="periode">Plage de dates de la réservation.</param>
    /// <param name="nbPersonnes">Nombre d'occupants.</param>
    /// <param name="reservationsActivesDeLaChambre">Réservations actives actuelles pour la chambre.</param>
    /// <param name="prixUnitaire">Tarif par nuit au moment de la réservation (Value Object Money).</param>
    /// <returns>La nouvelle entité Réservation créée.</returns>
    /// <exception cref="DomainException">Levée si la chambre n'est pas disponible.</exception>
    public Reservation CreerReservation(
        Guid chambreId,
        Guid clientId,
        Guid hotelId,
        DateRange periode,
        int nbPersonnes,
        IEnumerable<Reservation> reservationsActivesDeLaChambre,
        Money prixUnitaire)
    {
        if (!PeutReserver(chambreId, periode, reservationsActivesDeLaChambre))
        {
            throw new DomainException($"La chambre n'est pas disponible pour la période du {periode.DateArrivee:dd/MM/yyyy} au {periode.DateDepart:dd/MM/yyyy}.");
        }

        return new Reservation(Guid.NewGuid(), chambreId, clientId, hotelId,
            periode.DateArrivee, periode.DateDepart, nbPersonnes, prixUnitaire);
    }
}
