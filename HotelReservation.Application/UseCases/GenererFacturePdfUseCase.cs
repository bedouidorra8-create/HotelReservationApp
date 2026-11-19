using HotelReservation.Application.Services;
using HotelReservation.Domain.Common;
using HotelReservation.Domain.Repositories;

namespace HotelReservation.Application.UseCases;

public class GenererFacturePdfUseCase
{
    private readonly GenererFactureUseCase _genererFactureUseCase;
    private readonly IFacturePdfService _facturePdfService;
    private readonly IReservationRepository _reservationRepository;
    private readonly ICurrentUserService _currentUserService;

    public GenererFacturePdfUseCase(
        GenererFactureUseCase genererFactureUseCase,
        IFacturePdfService facturePdfService,
        IReservationRepository reservationRepository,
        ICurrentUserService currentUserService)
    {
        _genererFactureUseCase = genererFactureUseCase;
        _facturePdfService = facturePdfService;
        _reservationRepository = reservationRepository;
        _currentUserService = currentUserService;
    }

    public async Task<byte[]> ExecuteAsync(Guid reservationId, CancellationToken cancellationToken = default)
    {
        var reservation = await _reservationRepository.GetByIdAsync(reservationId, cancellationToken)
            ?? throw new DomainException($"Réservation avec l'ID '{reservationId}' introuvable.");

        // Sécurité IDOR : un client ne peut télécharger que sa propre facture
        if (_currentUserService.IsInRole("Client"))
        {
            if (_currentUserService.UserId != reservation.ClientId)
                throw new DomainException("Accès non autorisé à cette facture.");
        }

        var factureDto = await _genererFactureUseCase.ExecuteAsync(reservationId, cancellationToken);
        return await _facturePdfService.GenererPdfAsync(factureDto, cancellationToken);
    }
}
