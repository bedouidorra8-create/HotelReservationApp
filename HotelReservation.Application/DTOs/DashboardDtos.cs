namespace HotelReservation.Application.DTOs;

/// <summary>
/// DTO consolidé représentant le tableau de bord pour l'administration et les agents.
/// Les indicateurs confidentiels sont nulls pour le rôle Agent.
/// </summary>
public record DashboardDto(
    int? NombreTotalUtilisateurs,
    int? NombreHotels,
    int? NombreReservations,
    int ReservationsDuJour,
    decimal? ChiffreAffaires,
    int PaiementsEnAttente,
    List<ReservationDto> DernieresReservations,
    Dictionary<string, int>? GraphiqueReservationsParMois = null,
    Dictionary<string, decimal>? GraphiqueRevenusParMois = null
);
