using HotelReservation.Application.DTOs;

namespace HotelReservation.Application.Services;

/// <summary>
/// Service de génération de documents PDF pour les factures de réservations.
/// </summary>
public interface IFacturePdfService
{
    /// <summary>
    /// Génère le flux binaire d'un document PDF représentant la facture.
    /// </summary>
    /// <param name="facture">Données consolidées de la facture.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Tableau d'octets du fichier PDF.</returns>
    Task<byte[]> GenererPdfAsync(FactureDto facture, CancellationToken cancellationToken = default);
}
