using HotelReservation.Application.DTOs;
using HotelReservation.Application.Services;
using HotelReservation.Domain.Common;
using HotelReservation.Domain.Repositories;
using HotelReservation.Domain.Services;
using HotelReservation.Domain.ValueObjects;
using System.Text.Json;
using FluentValidation;

namespace HotelReservation.Application.UseCases;

/// <summary>
/// Cas d'utilisation permettant à un client de créer une réservation pour une chambre donnée.
/// Vérifie la disponibilité de la chambre et l'absence de chevauchement de dates.
/// </summary>
public class CreerReservationUseCase
{
    private readonly IReservationRepository _reservationRepository;
    private readonly IHotelRepository _hotelRepository;
    private readonly ReservationDomainService _reservationDomainService;
    private readonly IAuditLogService _auditLogService;
    private readonly IValidator<CreerReservationRequest> _validator;

    /// <summary>
    /// Initialise une nouvelle instance de <see cref="CreerReservationUseCase"/>.
    /// </summary>
    /// <param name="reservationRepository">Repository pour la persistance des réservations.</param>
    /// <param name="hotelRepository">Repository pour l'accès aux hôtels et leurs chambres.</param>
    /// <param name="reservationDomainService">Service de domaine pour les règles de réservation.</param>
    public CreerReservationUseCase(
        IReservationRepository reservationRepository,
        IHotelRepository hotelRepository,
        ReservationDomainService reservationDomainService,
        IAuditLogService auditLogService,
        IValidator<CreerReservationRequest> validator)
    {
        _reservationRepository = reservationRepository;
        _hotelRepository = hotelRepository;
        _reservationDomainService = reservationDomainService;
        _auditLogService = auditLogService;
        _validator = validator;
    }

    /// <summary>
    /// Exécute la création d'une réservation après validation métier et vérification de non-chevauchement.
    /// </summary>
    /// <param name="clientId">Identifiant du client créant la réservation.</param>
    /// <param name="request">Données de la demande de réservation (chambre, dates, nombre de personnes).</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Le DTO de la réservation créée.</returns>
    /// <exception cref="DomainException">Levée si la chambre est introuvable, indisponible ou déjà occupée sur la période.</exception>
    public async Task<ReservationDto> ExecuteAsync(Guid clientId, CreerReservationRequest request, CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);
        var hotels = await _hotelRepository.GetAllAsync(cancellationToken);
        var hotel = hotels.FirstOrDefault(h => h.Chambres.Any(c => c.Id == request.ChambreId));
        if (hotel == null)
            throw new DomainException($"Chambre avec l'ID '{request.ChambreId}' introuvable.");

        var chambre = hotel.Chambres.First(c => c.Id == request.ChambreId);
        if (!chambre.Actif)
            throw new DomainException("La chambre est désactivée et ne peut pas être réservée.");
        if (chambre.Statut == Domain.Enums.StatutChambre.Maintenance)
            throw new DomainException("La chambre est en maintenance et ne peut pas être réservée.");

        // RÈGLE MÉTIER CRITIQUE : Vérification de non-chevauchement des dates avec les réservations existantes actives de la chambre
        await _reservationDomainService.VerifierDisponibilite(
            _reservationRepository,
            request.ChambreId,
            request.DateArrivee,
            request.DateDepart,
            null,
            cancellationToken);

        var periode = new DateRange(request.DateArrivee, request.DateDepart);

        // RÈGLE MÉTIER : Empêcher un même client d'avoir deux réservations qui se chevauchent dans le temps
        var reservationsClient = await _reservationRepository.GetByClientIdAsync(clientId, cancellationToken);
        var chevauchementClient = reservationsClient.FirstOrDefault(r =>
            (r.Statut == Domain.Enums.StatutReservation.EnAttente || r.Statut == Domain.Enums.StatutReservation.Confirmee)
            && periode.ChevaucheAvec(r.Periode));

        if (chevauchementClient != null)
        {
            throw new DomainException($"Vous possédez déjà une réservation active sur cette période (du {chevauchementClient.Periode.DateArrivee:dd/MM/yyyy} au {chevauchementClient.Periode.DateDepart:dd/MM/yyyy}). Un voyageur ne peut pas cumuler deux séjours simultanés.");
        }

        var reservationsActives = await _reservationRepository.GetReservationsActivesPourChambre(request.ChambreId, cancellationToken);

        var reservation = _reservationDomainService.CreerReservation(
            request.ChambreId,
            clientId,
            hotel.Id,
            periode,
            request.NbPersonnes,
            reservationsActives,
            chambre.Prix);

        await _reservationRepository.AddAsync(reservation, cancellationToken);
        await _auditLogService.EnregistrerAsync(clientId, "Création", "Reservation", reservation.Id, null,
            JsonSerializer.Serialize(new { reservation.Id, reservation.ClientId, reservation.ChambreId, reservation.Periode }), cancellationToken);

        return ConsulterListeReservationsUseCase.ToDto(reservation);
    }
}
