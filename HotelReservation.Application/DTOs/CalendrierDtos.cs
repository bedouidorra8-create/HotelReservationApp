using HotelReservation.Domain.Enums;

namespace HotelReservation.Application.DTOs;

public record ReservationCalendrierDto(
    Guid ReservationId,
    Guid HotelId,
    string HotelNom,
    Guid ChambreId,
    string ChambreNumero,
    Guid ClientId,
    DateTime DateDebut,
    DateTime DateFin,
    StatutReservation Statut,
    int NbPersonnes
);

public record CalendrierReservationsResponse(
    Guid HotelId,
    string HotelNom,
    DateTime DateDebut,
    DateTime DateFin,
    List<ReservationCalendrierDto> Reservations
);
