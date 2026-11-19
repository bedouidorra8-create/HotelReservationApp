using HotelReservation.Application.DTOs;
using HotelReservation.Domain.Common;
using HotelReservation.Domain.Repositories;

namespace HotelReservation.Application.UseCases;

public class ActiverDesactiverChambreUseCase
{
    private readonly IHotelRepository _hotelRepository;

    public ActiverDesactiverChambreUseCase(IHotelRepository hotelRepository)
    {
        _hotelRepository = hotelRepository;
    }

    public async Task<ChambreDto> ExecuteAsync(Guid chambreId, bool actif, CancellationToken cancellationToken = default)
    {
        var hotels = await _hotelRepository.GetAllAsync(cancellationToken);
        var hotel = hotels.FirstOrDefault(h => h.Chambres.Any(c => c.Id == chambreId))
            ?? throw new DomainException($"Chambre avec l'ID '{chambreId}' introuvable.");

        var chambre = hotel.Chambres.First(c => c.Id == chambreId);
        if (actif) chambre.Activer();
        else chambre.Desactiver();
        await _hotelRepository.UpdateAsync(hotel, cancellationToken);

        return new ChambreDto(
            chambre.Id,
            chambre.HotelId,
            chambre.Numero,
            chambre.Type,
            chambre.Etage,
            chambre.Prix.Amount,
            chambre.Prix.Currency,
            chambre.Actif,
            chambre.Statut);
    }
}
