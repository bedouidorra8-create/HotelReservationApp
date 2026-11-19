namespace HotelReservation.Domain.Enums;

/// <summary>
/// Représente les modes de paiement pris en charge par le système de réservation hôtelière.
/// Valeurs numériques fixées pour préserver l'intégrité des données en base.
/// </summary>
public enum ModePaiement
{
    /// <summary>Paiement par carte bancaire (débit ou crédit).</summary>
    Carte = 1,

    /// <summary>Paiement en espèces au comptoir de l'hôtel.</summary>
    Especes = 2,

    /// <summary>Paiement par virement bancaire.</summary>
    Virement = 3,

    /// <summary>Paiement par chèque bancaire.</summary>
    Cheque = 4
}
