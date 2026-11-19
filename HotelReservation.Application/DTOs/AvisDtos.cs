using HotelReservation.Domain.Enums;

namespace HotelReservation.Application.DTOs;

public record AvisDto(
    Guid Id,
    Guid ClientId,
    Guid HotelId,
    Guid ReservationId,
    int Note,
    string Commentaire,
    DateTime Date,
    StatutAvis Statut
);

public record AjouterAvisRequest(
    Guid HotelId,
    Guid ReservationId,
    int Note,
    string Commentaire
);

public record ModifierAvisRequest(int Note, string Commentaire);
