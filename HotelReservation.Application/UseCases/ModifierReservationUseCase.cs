using HotelReservation.Application.DTOs;
using HotelReservation.Application.Services;
using HotelReservation.Domain.Common;
using HotelReservation.Domain.Repositories;
using HotelReservation.Domain.Services;
using System.Text.Json;

namespace HotelReservation.Application.UseCases;

/// <summary>
/// Cas d'utilisation permettant de modifier les dates ou le nombre de personnes d'une réservation existante.
/// Vérifie que la nouvelle période ne chevauche pas d'autres réservations actives de la même chambre.
/// </summary>
public class ModifierReservationUseCase
{
    private readonly IReservationRepository _repository;
    private readonly IHotelRepository _hotelRepository;
    private readonly ReservationDomainService _domainService;
    private readonly IAuditLogService _auditLogService;

    public ModifierReservationUseCase(IReservationRepository repository, IHotelRepository hotelRepository, ReservationDomainService domainService, IAuditLogService auditLogService)
    {
        _repository = repository;
        _hotelRepository = hotelRepository;
        _domainService = domainService;
        _auditLogService = auditLogService;
    }

    public async Task<ReservationDto> ExecuteAsync(Guid reservationId, ModifierReservationRequest request, CancellationToken cancellationToken = default, Guid? utilisateurId = null, bool isPersonnel = true)
    {
        var reservation = await _repository.GetByIdAsync(reservationId, cancellationToken)
            ?? throw new DomainException($"Réservation avec l'ID '{reservationId}' introuvable.");

        if (!isPersonnel && utilisateurId.HasValue && reservation.ClientId != utilisateurId.Value)
            throw new DomainException("Accès refusé : vous n'êtes pas propriétaire de cette réservation.");

        var hotels = await _hotelRepository.GetAllAsync(cancellationToken);
        var hotel = hotels.FirstOrDefault(h => h.Chambres.Any(c => c.Id == reservation.ChambreId));
        if (hotel != null)
        {
            var chambre = hotel.Chambres.First(c => c.Id == reservation.ChambreId);
            if (!chambre.Actif)
                throw new DomainException("La chambre est désactivée et ne peut pas être réservée.");
        }

        var ancienneValeur = JsonSerializer.Serialize(new { reservation.Periode, reservation.NbPersonnes, reservation.Statut });

        await _domainService.VerifierDisponibilite(
            _repository,
            reservation.ChambreId,
            request.DateArrivee,
            request.DateDepart,
            reservation.Id,
            cancellationToken);

        // RÈGLE MÉTIER : Vérification de non-chevauchement pour le même client
        var nouvellePeriode = new HotelReservation.Domain.ValueObjects.DateRange(request.DateArrivee, request.DateDepart);
        var reservationsClient = await _repository.GetByClientIdAsync(reservation.ClientId, cancellationToken);
        var chevauchementClient = reservationsClient.FirstOrDefault(r =>
            r.Id != reservation.Id
            && (r.Statut == Domain.Enums.StatutReservation.EnAttente || r.Statut == Domain.Enums.StatutReservation.Confirmee)
            && nouvellePeriode.ChevaucheAvec(r.Periode));

        if (chevauchementClient != null)
        {
            throw new DomainException($"Le voyageur possède déjà une autre réservation active sur cette période (du {chevauchementClient.Periode.DateArrivee:dd/MM/yyyy} au {chevauchementClient.Periode.DateDepart:dd/MM/yyyy}).");
        }

        reservation.Modifier(request.DateArrivee, request.DateDepart, request.NbPersonnes);
        await _repository.UpdateAsync(reservation, cancellationToken);
        await _auditLogService.EnregistrerAsync(utilisateurId, "Modification", "Reservation", reservation.Id, ancienneValeur,
            JsonSerializer.Serialize(new { reservation.Periode, reservation.NbPersonnes, reservation.Statut }), cancellationToken);
        return ConsulterListeReservationsUseCase.ToDto(reservation);
    }
}
