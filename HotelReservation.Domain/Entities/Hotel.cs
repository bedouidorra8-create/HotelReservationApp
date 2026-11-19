using HotelReservation.Domain.Common;
using HotelReservation.Domain.Enums;
using HotelReservation.Domain.ValueObjects;

namespace HotelReservation.Domain.Entities;

/// <summary>
/// Racine d'agrégat (AggregateRoot) représentant un établissement hôtelier et garantissant
/// la cohérence de l'ensemble de ses chambres enfants associées.
/// </summary>
public class Hotel : AggregateRoot<Guid>
{
    /// <summary>Nom commercial de l'hôtel.</summary>
    public string Nom { get; private set; } = string.Empty;

    /// <summary>Ville d'implantation de l'établissement.</summary>
    public string Ville { get; private set; } = string.Empty;

    /// <summary>Adresse postale complète.</summary>
    public string Adresse { get; private set; } = string.Empty;

    /// <summary>Catégorie de l'hôtel (nombre d'étoiles entre 1 et 5).</summary>
    public int NbEtoiles { get; private set; }

    /// <summary>Description détaillée de l'hôtel et de ses prestations.</summary>
    public string Description { get; private set; } = string.Empty;

    /// <summary>Nombre total de chambres enregistrées dans l'établissement.</summary>
    public int NbChambres { get; private set; }

    /// <summary>Numéro de téléphone de contact.</summary>
    public string Telephone { get; private set; } = string.Empty;

    /// <summary>Adresse email professionnelle de contact (Value Object Email).</summary>
    public Email Email { get; private set; } = null!;

    /// <summary>Liste des URLs des photographies illustrant l'hôtel.</summary>
    public List<string> Images { get; private set; } = new();

    /// <summary>Collection des chambres enfants gérées exclusivement par cet agrégat.</summary>
    public List<Chambre> Chambres { get; private set; } = new();

    /// <summary>Statut opérationnel de l'hôtel (Actif ou Inactif).</summary>
    public StatutHotel Statut { get; private set; }

    private Hotel() { }

    /// <summary>
    /// Initialise une nouvelle instance d'un hôtel actif.
    /// </summary>
    /// <param name="id">Identifiant unique de l'établissement.</param>
    /// <param name="nom">Nom de l'hôtel.</param>
    /// <param name="ville">Ville.</param>
    /// <param name="adresse">Adresse postale.</param>
    /// <param name="nbEtoiles">Nombre d'étoiles (1 à 5).</param>
    /// <param name="description">Description commerciale.</param>
    /// <param name="telephone">Téléphone de contact.</param>
    /// <param name="email">Email de contact.</param>
    /// <param name="images">Galerie d'images (optionnelle).</param>
    /// <exception cref="ArgumentNullException">Levée si l'email est nul.</exception>
    /// <exception cref="DomainException">Levée si le nombre d'étoiles est hors limites.</exception>
    public Hotel(
        Guid id,
        string nom,
        string ville,
        string adresse,
        int nbEtoiles,
        string description,
        string telephone,
        Email email,
        List<string>? images = null)
        : base(id)
    {
        ValiderNbEtoiles(nbEtoiles);

        Nom = nom;
        Ville = ville;
        Adresse = adresse;
        NbEtoiles = nbEtoiles;
        Description = description;
        Telephone = telephone;
        Email = email ?? throw new ArgumentNullException(nameof(email));
        Images = images ?? new List<string>();
        Statut = StatutHotel.Actif;
    }

    /// <summary>
    /// Modifie les informations générales de l'hôtel.
    /// </summary>
    /// <param name="nom">Nouveau nom.</param>
    /// <param name="ville">Nouvelle ville.</param>
    /// <param name="adresse">Nouvelle adresse.</param>
    /// <param name="nbEtoiles">Nouveau nombre d'étoiles.</param>
    /// <param name="description">Nouvelle description.</param>
    /// <param name="telephone">Nouveau téléphone.</param>
    /// <param name="images">Nouvelle galerie d'images.</param>
    public void Modifier(
        string nom,
        string ville,
        string adresse,
        int nbEtoiles,
        string? description = null,
        string? telephone = null,
        List<string>? images = null)
    {
        ValiderNbEtoiles(nbEtoiles);

        Nom = nom;
        Ville = ville;
        Adresse = adresse;
        NbEtoiles = nbEtoiles;
        if (description != null) Description = description;
        if (telephone != null) Telephone = telephone;
        if (images != null) Images = images;
    }

    /// <summary>
    /// Active ou désactive l'hôtel.
    /// </summary>
    /// <param name="statut">Nouveau statut (Actif ou Inactif).</param>
    public void ActiverDesactiver(StatutHotel statut) => Statut = statut;

    /// <summary>
    /// Ajoute une nouvelle chambre à l'hôtel en vérifiant l'unicité du numéro au sein de l'établissement.
    /// </summary>
    /// <param name="numero">Numéro unique de la chambre.</param>
    /// <param name="type">Type de chambre.</param>
    /// <param name="etage">Étage.</param>
    /// <param name="prix">Tarif par nuit.</param>
    /// <returns>La chambre créée et rattachée à l'agrégat.</returns>
    /// <exception cref="DomainException">Levée si le numéro existe déjà dans cet hôtel.</exception>
    public Chambre AjouterChambre(string numero, TypeChambre type, int etage, Money prix, bool actif = true)
    {
        // RÈGLE MÉTIER : Unicité du numéro de chambre au sein d'un même établissement hôtelier
        if (Chambres.Any(c => string.Equals(c.Numero, numero.Trim(), StringComparison.OrdinalIgnoreCase)))
            throw new DomainException("Le numéro de chambre existe déjà dans cet hôtel.");

        var chambre = new Chambre(Guid.NewGuid(), Id, numero, type, etage, prix, actif);
        Chambres.Add(chambre);
        NbChambres++;
        return chambre;
    }

    /// <summary>
    /// Modifie les caractéristiques d'une chambre existante de cet hôtel.
    /// </summary>
    /// <param name="chambreId">Identifiant de la chambre.</param>
    /// <param name="type">Nouveau type de chambre.</param>
    /// <param name="etage">Nouvel étage.</param>
    /// <param name="prix">Nouveau tarif par nuit.</param>
    /// <exception cref="DomainException">Levée si la chambre n'appartient pas à cet hôtel.</exception>
    public void ModifierChambre(Guid chambreId, TypeChambre type, int etage, Money prix, bool? actif = null)
    {
        var chambre = Chambres.FirstOrDefault(c => c.Id == chambreId)
            ?? throw new DomainException("La chambre n'appartient pas à cet hôtel.");

        chambre.Modifier(type, etage, prix, actif);
    }

    /// <summary>
    /// Supprime une chambre de l'hôtel et décrémente le compteur total.
    /// </summary>
    /// <param name="chambreId">Identifiant de la chambre à supprimer.</param>
    /// <exception cref="DomainException">Levée si la chambre est introuvable.</exception>
    public void SupprimerChambre(Guid chambreId)
    {
        var chambre = Chambres.FirstOrDefault(c => c.Id == chambreId)
            ?? throw new DomainException("La chambre n'appartient pas à cet hôtel.");

        Chambres.Remove(chambre);
        NbChambres = Math.Max(0, NbChambres - 1);
    }

    /// <summary>
    /// Modifie le statut opérationnel d'une chambre de l'hôtel.
    /// </summary>
    /// <param name="chambreId">Identifiant de la chambre.</param>
    /// <param name="nouveauStatut">Nouveau statut (Libre, Occupee, Maintenance).</param>
    /// <exception cref="DomainException">Levée si la chambre n'appartient pas à cet hôtel.</exception>
    public void ChangerStatutChambre(Guid chambreId, StatutChambre nouveauStatut)
    {
        var chambre = Chambres.FirstOrDefault(c => c.Id == chambreId)
            ?? throw new DomainException("La chambre n'appartient pas à cet hôtel.");

        chambre.ChangerStatut(nouveauStatut);
    }

    /// <summary>
    /// Vérifie qu'une réservation est autorisée pour cet hôtel.
    /// </summary>
    /// <exception cref="DomainException">Levée si l'hôtel est inactif ou sans chambre disponible.</exception>
    public void ReserverUneChambre()
    {
        // RÈGLE MÉTIER : Aucune réservation n'est possible sur un hôtel inactif
        if (Statut != StatutHotel.Actif)
            throw new DomainException("Hôtel désactivé, réservation impossible.");
        if (!Chambres.Any(c => c.Actif && c.Statut == StatutChambre.Libre))
            throw new DomainException("Aucune chambre disponible.");
    }

    /// <summary>
    /// Libère une chambre de l'hôtel suite à un départ ou une fin de séjour.
    /// </summary>
    /// <param name="chambreId">Identifiant de la chambre à libérer.</param>
    /// <exception cref="DomainException">Levée si la chambre n'appartient pas à cet hôtel.</exception>
    public void LibererUneChambre(Guid chambreId)
    {
        var chambre = Chambres.FirstOrDefault(c => c.Id == chambreId)
            ?? throw new DomainException("La chambre n'appartient pas à cet hôtel.");

        if (chambre.Statut == StatutChambre.Occupee)
            chambre.ChangerStatut(StatutChambre.Libre);
    }

    private static void ValiderNbEtoiles(int nbEtoiles)
    {
        // RÈGLE MÉTIER : La classification hôtelière standard impose entre 1 et 5 étoiles
        if (nbEtoiles < 1 || nbEtoiles > 5)
            throw new DomainException("Le nombre d'étoiles doit être entre 1 et 5.");
    }
}
 