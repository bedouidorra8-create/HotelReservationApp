namespace HotelReservation.Application.DTOs;

public record AuditLogDto(
    Guid Id,
    Guid? UtilisateurId,
    string Action,
    string Entite,
    Guid? EntiteId,
    string? AncienneValeur,
    string? NouvelleValeur,
    DateTime DateHeure
);
