using HotelReservation.Domain.Enums;

namespace HotelReservation.Application.DTOs;

public record PaiementDto(
    Guid Id,
    Guid ReservationId,
    decimal Montant,
    string Devise,
    DateTime Date,
    ModePaiement ModePaiement,
    string Reference,
    StatutPaiement Statut,
    Guid? EnregistrePar
);

public record EnregistrerPaiementRequest(
    Guid ReservationId,
    decimal Montant,
    string Devise,
    ModePaiement ModePaiement,
    string Reference
);

public record FactureDto(
    Guid ReservationId,
    DateTime DateEmission,
    decimal Total,
    string Devise,
    List<PaiementDto> Paiements,
    FactureClientDto? Client = null,
    FactureHotelDto? Hotel = null,
    FactureChambreDto? Chambre = null
);

public record FactureClientDto(Guid Id, string Nom, string Prenom, string Email);

public record FactureHotelDto(Guid Id, string Nom, string Adresse);

public record FactureChambreDto(Guid Id, string Numero);
