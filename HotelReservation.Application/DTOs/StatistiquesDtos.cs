using HotelReservation.Domain.Enums;

namespace HotelReservation.Application.DTOs;

/// <summary>
/// DTO représentant le nombre de réservations enregistrées pour un mois donné.
/// </summary>
public record ReservationsParMoisDto(int Annee, int Mois, int NombreReservations);

/// <summary>
/// DTO représentant le chiffre d'affaires cumulé des paiements validés pour un mois donné.
/// </summary>
public record ChiffreAffairesMensuelDto(int Annee, int Mois, decimal Montant);

/// <summary>
/// DTO représentant un hôtel parmi les plus réservés.
/// </summary>
public record HotelPlusReserveDto(Guid HotelId, string HotelNom, int NombreReservations);

/// <summary>
/// DTO représentant la répartition des paiements pour un mode donné (Carte, Espèces, etc.).
/// </summary>
public record PaiementsParModeDto(ModePaiement Mode, string ModeNom, int NombrePaiements, decimal MontantTotal);

/// <summary>
/// DTO représentant le nombre de nouveaux utilisateurs inscrits par mois.
/// </summary>
public record NouveauxUtilisateursParMoisDto(int Annee, int Mois, int NombreUtilisateurs);

/// <summary>
/// DTO représentant le taux d'occupation des chambres d'un hôtel (chambres occupées / total chambres).
/// </summary>
public record TauxOccupationHotelDto(
    Guid HotelId,
    string HotelNom,
    int TotalChambres,
    int ChambresOccupees,
    double TauxOccupationPourcentage
);

/// <summary>
/// DTO représentant l'évolution mensuelle des avis clients et de la note moyenne.
/// </summary>
public record EvolutionAvisDto(int Annee, int Mois, int NombreAvis, double NoteMoyenne);

/// <summary>
/// DTO consolidé des statistiques de l'application intégrant les 7 indicateurs métiers exigés par le cahier des charges.
/// </summary>
public record StatistiquesDto(
    List<ReservationsParMoisDto> ReservationsParMois,
    List<ChiffreAffairesMensuelDto> ChiffreAffairesMensuel,
    List<HotelPlusReserveDto> HotelsLesPlusReserves,
    List<PaiementsParModeDto> RepartitionPaiementsParMode,
    List<NouveauxUtilisateursParMoisDto> NouveauxUtilisateursParMois,
    List<TauxOccupationHotelDto> TauxOccupationHotels,
    List<EvolutionAvisDto> EvolutionAvis,
    DateTime? DateDebut,
    DateTime? DateFin
);

