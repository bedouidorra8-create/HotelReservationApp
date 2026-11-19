using HotelReservation.Application.DTOs;
using HotelReservation.Application.Services;
using HotelReservation.Domain.Common;
using HotelReservation.Domain.Repositories;

namespace HotelReservation.Application.UseCases;

public class ConsulterAvisClientUseCase
{
    private readonly IAvisRepository _avisRepository;
    private readonly ICurrentUserService _currentUserService;

    public ConsulterAvisClientUseCase(IAvisRepository avisRepository, ICurrentUserService currentUserService)
    {
        _avisRepository = avisRepository;
        _currentUserService = currentUserService;
    }

    public async Task<IEnumerable<AvisDto>> ExecuteAsync(Guid clientId, CancellationToken cancellationToken = default)
    {
        if (_currentUserService.IsInRole("Client") && _currentUserService.UserId != clientId)
            throw new DomainException("Accès non autorisé aux avis d'un autre client.");

        var allAvis = await _avisRepository.GetAllAsync(cancellationToken);
        return allAvis
            .Where(a => a.ClientId == clientId)
            .OrderByDescending(a => a.Date)
            .Select(a => new AvisDto(
                a.Id,
                a.ClientId,
                a.HotelId,
                a.ReservationId,
                a.Note.Valeur,
                a.Commentaire,
                a.Date,
                a.Statut
            ));
    }
}
