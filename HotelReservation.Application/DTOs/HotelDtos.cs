using HotelReservation.Domain.Enums;

namespace HotelReservation.Application.DTOs;

/// <summary>
/// DTO représentant la fiche détaillée d'un hôtel pour le frontend.
/// Préserve la compatibilité avec le contrat existant (champ booléen Actif).
/// </summary>
/// <param name="Id">Identifiant unique de l'hôtel.</param>
/// <param name="Nom">Nom de l'établissement.</param>
/// <param name="Ville">Ville.</param>
/// <param name="Adresse">Adresse complète.</param>
/// <param name="NbEtoiles">Nombre d'étoiles (1 à 5).</param>
/// <param name="Description">Description.</param>
/// <param name="Telephone">Téléphone.</param>
/// <param name="Email">Adresse email.</param>
/// <param name="Images">Galerie de photos.</param>
/// <param name="Actif">Statut d'activation de l'hôtel.</param>
/// <param name="Chambres">Liste des chambres associées.</param>
/// <param name="PrixMinParNuit">Tarif minimum indicatif calculé dynamiquement à partir des chambres (non stocké en base).</param>
public record HotelDto(
    Guid Id,
    string Nom,
    string Ville,
    string Adresse,
    int NbEtoiles,
    string Description,
    string Telephone,
    string Email,
    List<string> Images,
    bool Actif,
    List<ChambreDto> Chambres,
    decimal? PrixMinParNuit = null,
    double? NoteMoyenne = null,
    int? NbAvis = null
);

/// <summary>
/// DTO représentant une chambre d'hôtel.
/// </summary>
public record ChambreDto(
    Guid Id,
    Guid HotelId,
    string Numero,
    TypeChambre Type,
    int Etage,
    decimal PrixMontant,
    string PrixDevise,
    bool Actif,
    StatutChambre Statut
);

/// <summary>
/// Requête de création d'un nouvel hôtel.
/// </summary>
public record CreerHotelRequest(
    string Nom,
    string Ville,
    string Adresse,
    int NbEtoiles,
    string Description,
    string Telephone,
    string Email,
    List<string>? Images
);

/// <summary>
/// Requête de modification d'un hôtel existant.
/// Note : Le champ PrixParNuit a été retiré conformément aux spécifications (prix géré au niveau Chambre).
/// </summary>
public record ModifierHotelRequest(
    string Nom,
    string Ville,
    string Adresse,
    int NbEtoiles,
    string? Description = null,
    string? Telephone = null,
    List<string>? Images = null
);

/// <summary>
/// Requête de création d'une nouvelle chambre pour un hôtel.
/// </summary>
public record CreerChambreRequest(
    string Numero,
    TypeChambre Type,
    int Etage,
    decimal PrixMontant,
    string PrixDevise = "EUR",
    bool Actif = true
);

/// <summary>
/// Requête de modification des caractéristiques d'une chambre existante.
/// </summary>
public record ModifierChambreRequest(
    TypeChambre Type,
    int Etage,
    decimal PrixMontant,
    string PrixDevise = "EUR",
    bool? Actif = null
);

/// <summary>
/// Requête de modification du statut d'une chambre.
/// </summary>
public record ChangerStatutChambreRequest(StatutChambre Statut);

/// <summary>
/// Paramètres de recherche multicritère de chambres.
/// </summary>
public record RechercherChambreRequest(
    TypeChambre? Type = null,
    decimal? PrixMin = null,
    decimal? PrixMax = null,
    StatutChambre? Statut = null
);

/// <summary>
/// DTO représentant la disponibilité journalière d'une chambre sur une période donnée.
/// </summary>
public record DisponibiliteChambreDto(
    Guid ChambreId,
    string Numero,
    TypeChambre Type,
    int Etage,
    decimal PrixMontant,
    string PrixDevise,
    Dictionary<string, string> StatutParJour // Clé: YYYY-MM-DD, Valeur: "Libre", "Occupee", "Maintenance"
);

/// <summary>
/// Réponse consolidée des disponibilités des chambres d'un hôtel sur une période.
/// </summary>
public record DisponibilitesHotelResponse(
    Guid HotelId,
    string HotelNom,
    DateTime DateDebut,
    DateTime DateFin,
    List<DisponibiliteChambreDto> Chambres
);
