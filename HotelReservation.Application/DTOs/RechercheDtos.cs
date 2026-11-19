using HotelReservation.Domain.Enums;

namespace HotelReservation.Application.DTOs;

public record RechercherHotelsRequest(
    string? Ville,
    decimal? PrixMin,
    decimal? PrixMax,
    int? EtoilesMin,
    DateTime? DateDebut,
    DateTime? DateFin,
    bool DisponibleUniquement,
    string? Tri // "prixAsc" | "prixDesc" | "noteDesc"
);

public record HotelResultatRechercheDto(
    Guid Id,
    string Nom,
    string Ville,
    string Adresse,
    int NbEtoiles,
    string Description,
    decimal PrixAffiche, // prix chambre la moins chere disponible
    string DeviseAffichee,
    List<string>? Images = null,
    double? NoteMoyenne = null,
    int? NbAvis = null
);
