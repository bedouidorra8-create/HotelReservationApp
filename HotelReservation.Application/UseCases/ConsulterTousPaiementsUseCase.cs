using HotelReservation.Application.DTOs;
using HotelReservation.Domain.Enums;
using HotelReservation.Domain.Repositories;

namespace HotelReservation.Application.UseCases;

public class ConsulterTousPaiementsUseCase
{
    private readonly IPaiementRepository _paiementRepository;

    public ConsulterTousPaiementsUseCase(IPaiementRepository paiementRepository)
    {
        _paiementRepository = paiementRepository;
    }

    public async Task<IEnumerable<PaiementDto>> ExecuteAsync(
        StatutPaiement? statut = null,
        ModePaiement? mode = null,
        DateTime? dateDebut = null,
        DateTime? dateFin = null,
        CancellationToken cancellationToken = default)
    {
        var paiements = await _paiementRepository.GetAllAsync(cancellationToken);

        if (statut.HasValue)
            paiements = paiements.Where(p => p.Statut == statut.Value);

        if (mode.HasValue)
            paiements = paiements.Where(p => p.ModePaiement == mode.Value);

        if (dateDebut.HasValue)
            paiements = paiements.Where(p => p.Date >= dateDebut.Value);

        if (dateFin.HasValue)
            paiements = paiements.Where(p => p.Date <= dateFin.Value);

        return paiements
            .OrderByDescending(p => p.Date)
            .Select(p => new PaiementDto(
                p.Id,
                p.ReservationId,
                p.Montant.Amount,
                p.Montant.Currency,
                p.Date,
                p.ModePaiement,
                p.Reference,
                p.Statut,
                p.EnregistrePar
            ));
    }
}
