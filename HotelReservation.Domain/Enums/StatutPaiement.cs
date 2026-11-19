namespace HotelReservation.Domain.Enums;

/// <summary>
/// Représente les différents statuts d'un paiement au sein du système.
/// Alignement avec le cahier des charges :
/// Note : La valeur Paye (valeur numérique 2) correspond à l'ancienne valeur Valide.
/// La valeur numérique 2 est strictement conservée pour maintenir la compatibilité avec les données en base.
/// </summary>
public enum StatutPaiement
{
    /// <summary>Paiement initié ou en attente d'encaissement.</summary>
    EnAttente = 1,

    /// <summary>Paiement validé et encaissé avec succès (anciennement Valide).</summary>
    Paye = 2,

    /// <summary>Paiement remboursé au client.</summary>
    Rembourse = 3,

    /// <summary>Paiement ayant échoué lors de la transaction.</summary>
    Echoue = 4,

    /// <summary>Paiement annulé avant finalisation.</summary>
    Annule = 5
}
