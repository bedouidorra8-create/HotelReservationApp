using HotelReservation.Application.DTOs;
using HotelReservation.Domain.Common;
using HotelReservation.Domain.Repositories;

namespace HotelReservation.Application.UseCases;

/// <summary>
/// Cas d'utilisation permettant de générer la facture détaillée d'une réservation avec l'historique des paiements.
/// </summary>
public class GenererFactureUseCase
{
    private readonly IReservationRepository _reservationRepository;
    private readonly IPaiementRepository _paiementRepository;
    private readonly IClientRepository _clientRepository;
    private readonly IHotelRepository _hotelRepository;

    /// <summary>
    /// Initialise une nouvelle instance de <see cref="GenererFactureUseCase"/>.
    /// </summary>
    /// <param name="reservationRepository">Repository des réservations.</param>
    /// <param name="paiementRepository">Repository des paiements.</param>
    public GenererFactureUseCase(
        IReservationRepository reservationRepository,
        IPaiementRepository paiementRepository,
        IClientRepository clientRepository,
        IHotelRepository hotelRepository)
    {
        _reservationRepository = reservationRepository;
        _paiementRepository = paiementRepository;
        _clientRepository = clientRepository;
        _hotelRepository = hotelRepository;
    }

    /// <summary>
    /// Génère le DTO de facture pour la réservation spécifiée.
    /// </summary>
    /// <param name="reservationId">Identifiant de la réservation.</param>
    /// <param name="cancellationToken">Jeton d'annulation asynchrone.</param>
    /// <returns>La facture consolidée avec les paiements.</returns>
    /// <exception cref="DomainException">Levée si la réservation est introuvable.</exception>
    public async Task<FactureDto> ExecuteAsync(Guid reservationId, CancellationToken cancellationToken = default)
    {
        var reservation = await _reservationRepository.GetByIdAsync(reservationId, cancellationToken)
            ?? throw new DomainException($"Réservation avec l'ID '{reservationId}' introuvable.");

        var paiements = (await _paiementRepository.GetByReservationIdAsync(reservationId, cancellationToken)).ToList();
        var client = await _clientRepository.GetByIdAsync(reservation.ClientId, cancellationToken);
        var hotel = await _hotelRepository.GetByIdAsync(reservation.HotelId, cancellationToken);
        var chambre = hotel?.Chambres.FirstOrDefault(c => c.Id == reservation.ChambreId);
        var paiementDtos = paiements.Select(p => new PaiementDto(
            p.Id,
            p.ReservationId,
            p.Montant.Amount,
            p.Montant.Currency,
            p.Date,
            p.ModePaiement,
            p.Reference,
            p.Statut,
            p.EnregistrePar
        )).ToList();

        var devise = reservation.Total?.Currency ?? paiements.FirstOrDefault()?.Montant.Currency ?? "EUR";
        var montantTotal = reservation.Total?.Amount ?? 0m;

        return new FactureDto(
            reservation.Id,
            DateTime.UtcNow,
            montantTotal,
            devise,
            paiementDtos,
            client == null ? null : new FactureClientDto(client.Id, client.Nom, client.Prenom, client.Email.Value),
            hotel == null ? null : new FactureHotelDto(hotel.Id, hotel.Nom, hotel.Adresse),
            chambre == null ? null : new FactureChambreDto(chambre.Id, chambre.Numero));
    }
}
