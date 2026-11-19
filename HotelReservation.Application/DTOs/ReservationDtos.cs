using HotelReservation.Domain.Enums;

namespace HotelReservation.Application.DTOs;

/// <summary>
/// DTO représentant une réservation hôtelière complète, incluant les conditions financières figées.
/// </summary>
public record ReservationDto(
    Guid Id,
    Guid ChambreId,
    Guid ClientId,
    DateTime DateArrivee,
    DateTime DateDepart,
    int NbNuits,
    int NbPersonnes,
    StatutReservation Statut,
    DateTime DateCreation,
    decimal PrixUnitaireMontant = 0,
    string PrixUnitaireDevise = "EUR",
    decimal TotalMontant = 0,
    string TotalDevise = "EUR"
);

/// <summary>
/// Requête de création d'une nouvelle réservation.
/// </summary>
public record CreerReservationRequest(
    Guid ChambreId,
    DateTime DateArrivee,
    DateTime DateDepart,
    int NbPersonnes,
    Guid? ClientId = null
);

/// <summary>
/// Requête de modification des dates ou du nombre de personnes d'une réservation.
/// </summary>
public record ModifierReservationRequest(
    DateTime DateArrivee,
    DateTime DateDepart,
    int NbPersonnes
);

/// <summary>
/// Paramètres de recherche multicritère de réservations.
/// </summary>
public record RechercherReservationRequest(
    Guid? ClientId = null,
    Guid? HotelId = null,
    StatutReservation? Statut = null,
    DateTime? DateDebut = null,
    DateTime? DateFin = null
);

