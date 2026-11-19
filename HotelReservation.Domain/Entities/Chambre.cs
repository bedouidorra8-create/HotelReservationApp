using HotelReservation.Domain.Common;
using HotelReservation.Domain.Enums;
using HotelReservation.Domain.ValueObjects;

namespace HotelReservation.Domain.Entities;

/// <summary>
/// Entité représentant une chambre au sein d'un hôtel (appartenant à l'agrégat Hotel).
/// Ne dispose pas de repository propre conformément aux principes du DDD.
/// </summary>
public class Chambre : Entity<Guid>
{
    /// <summary>Identifiant de l'hôtel auquel appartient la chambre.</summary>
    public Guid HotelId { get; private set; }

    /// <summary>Numéro d'identification de la chambre dans l'hôtel.</summary>
    public string Numero { get; private set; } = string.Empty;

    /// <summary>Type de chambre (Simple, Double, Suite, Familiale).</summary>
    public TypeChambre Type { get; private set; }

    /// <summary>Étage de situation de la chambre.</summary>
    public int Etage { get; private set; }

    /// <summary>Tarif par nuit de la chambre (Value Object Money).</summary>
    public Money Prix { get; private set; } = null!;

    /// <summary>Indique si la chambre est activée et utilisable pour la réservation.</summary>
    public bool Actif { get; private set; } = true;

    /// <summary>Statut courant de la chambre (Libre, Occupee, Maintenance).</summary>
    public StatutChambre Statut { get; private set; }

    private Chambre() { }

    /// <summary>
    /// Initialise une nouvelle instance d'une chambre.
    /// </summary>
    /// <param name="id">Identifiant unique de la chambre.</param>
    /// <param name="hotelId">Identifiant de l'hôtel parent.</param>
    /// <param name="numero">Numéro de la chambre.</param>
    /// <param name="type">Type de chambre.</param>
    /// <param name="etage">Étage.</param>
    /// <param name="prix">Prix par nuit.</param>
    /// <exception cref="DomainException">Levée si le numéro est invalide.</exception>
    /// <exception cref="ArgumentNullException">Levée si le prix est nul.</exception>
    public Chambre(Guid id, Guid hotelId, string numero, TypeChambre type, int etage, Money prix, bool actif = true)
        : base(id)
    {
        if (string.IsNullOrWhiteSpace(numero))
            throw new DomainException("Le numéro de chambre est obligatoire.");

        HotelId = hotelId;
        Numero = numero.Trim();
        Type = type;
        Etage = etage;
        Prix = prix ?? throw new ArgumentNullException(nameof(prix));
        Actif = actif;
        Statut = StatutChambre.Libre;
    }

    /// <summary>
    /// Modifie les caractéristiques de la chambre (type, étage et tarif).
    /// </summary>
    /// <param name="type">Nouveau type de chambre.</param>
    /// <param name="etage">Nouvel étage.</param>
    /// <param name="prix">Nouveau tarif par nuit.</param>
    /// <exception cref="ArgumentNullException">Levée si le prix est nul.</exception>
    public void Modifier(TypeChambre type, int etage, Money prix, bool? actif = null)
    {
        Type = type;
        Etage = etage;
        Prix = prix ?? throw new ArgumentNullException(nameof(prix));
        if (actif.HasValue)
            Actif = actif.Value;
    }

    /// <summary>
    /// Change le statut opérationnel de la chambre en vérifiant les transitions autorisées.
    /// </summary>
    /// <param name="nouveauStatut">Nouveau statut cible.</param>
    /// <exception cref="DomainException">Levée si la transition est interdite par les règles métiers.</exception>
    public void ChangerStatut(StatutChambre nouveauStatut)
    {
        if (!Actif)
            throw new DomainException("La chambre est désactivée et ne peut pas recevoir un nouveau statut.");

        if (Statut == nouveauStatut)
            return;

        // RÈGLE MÉTIER : Une chambre en maintenance doit d'abord être validée et repasser à 'Libre'
        // avant d'être attribuée et occupée par un client, pour des raisons de conformité et propreté.
        if (Statut == StatutChambre.Maintenance && nouveauStatut == StatutChambre.Occupee)
        {
            throw new DomainException("Une chambre en maintenance doit d'abord repasser au statut Libre avant de pouvoir être occupée.");
        }

        Statut = nouveauStatut;
    }

    public void SetActif(bool actif)
    {
        if (Actif == actif)
            return;

        if (!actif && Statut == StatutChambre.Occupee)
            throw new DomainException("Une chambre occupée ne peut pas être désactivée.");

        Actif = actif;
    }

    public void Activer() => SetActif(true);
    public void Desactiver() => SetActif(false);

    /// <summary>
    /// Modifie le prix de la chambre.
    /// </summary>
    /// <param name="nouveauPrix">Nouveau prix par nuit.</param>
    /// <exception cref="ArgumentNullException">Levée si le prix est nul.</exception>
    public void ModifierPrix(Money nouveauPrix)
    {
        Prix = nouveauPrix ?? throw new ArgumentNullException(nameof(nouveauPrix));
    }
}
