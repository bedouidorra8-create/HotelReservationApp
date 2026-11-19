namespace HotelReservation.Domain.Enums;

/// <summary>
/// Représente le statut opérationnel d'un établissement hôtelier (Actif ou Inactif).
/// </summary>
public enum StatutHotel
{
    /// <summary>L'hôtel est actif, visible et ouvert aux réservations.</summary>
    Actif = 1,

    /// <summary>L'hôtel est désactivé (temporairement fermé ou archivé), les réservations y sont bloquées.</summary>
    Inactif = 2
}
