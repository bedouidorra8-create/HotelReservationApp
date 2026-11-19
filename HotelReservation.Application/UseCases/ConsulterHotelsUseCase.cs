using HotelReservation.Application.DTOs;
using HotelReservation.Domain.Common;
using HotelReservation.Domain.Entities;
using HotelReservation.Domain.Enums;
using HotelReservation.Domain.Repositories;

namespace HotelReservation.Application.UseCases;

/// <summary>
/// Cas d'utilisation permettant la consultation unitaire ou globale des établissements hôteliers.
/// </summary>
public class ConsulterHotelsUseCase
{
    private readonly IHotelRepository _hotelRepository;
    private readonly IAvisRepository? _avisRepository;

    /// <summary>
    /// Initialise une nouvelle instance de <see cref="ConsulterHotelsUseCase"/>.
    /// </summary>
    public ConsulterHotelsUseCase(IHotelRepository hotelRepository, IAvisRepository? avisRepository = null)
    {
        _hotelRepository = hotelRepository;
        _avisRepository = avisRepository;
    }

    /// <summary>
    /// Récupère l'ensemble des hôtels enregistrés avec leurs chambres et le tarif minimum indicatif.
    /// </summary>
    public async Task<IEnumerable<HotelDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var hotels = await _hotelRepository.GetAllAsync(cancellationToken);
        var dtos = new List<HotelDto>();
        foreach (var hotel in hotels)
        {
            dtos.Add(await ToDtoAsync(hotel, cancellationToken));
        }
        return dtos;
    }

    /// <summary>
    /// Récupère un hôtel spécifique par son identifiant unique.
    /// </summary>
    public async Task<HotelDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var h = await _hotelRepository.GetByIdAsync(id, cancellationToken);
        if (h == null)
            throw new DomainException($"Hôtel avec l'ID '{id}' introuvable.");

        return await ToDtoAsync(h, cancellationToken);
    }

    private async Task<HotelDto> ToDtoAsync(Hotel h, CancellationToken cancellationToken)
    {
        decimal? prixMin = h.Chambres.Any() ? h.Chambres.Min(c => c.Prix.Amount) : null;
        double? noteMoyenne = null;
        int? nbAvis = null;

        if (_avisRepository != null)
        {
            var avis = (await _avisRepository.GetByHotelIdAsync(h.Id, cancellationToken))
                .Where(a => a.Statut == StatutAvis.Valide)
                .ToList();
            if (avis.Any())
            {
                noteMoyenne = Math.Round(avis.Average(a => a.Note.Valeur), 1);
                nbAvis = avis.Count;
            }
        }

        return new HotelDto(
            h.Id,
            h.Nom,
            h.Ville,
            h.Adresse,
            h.NbEtoiles,
            h.Description,
            h.Telephone,
            h.Email.Value,
            h.Images,
            h.Statut == StatutHotel.Actif,
            h.Chambres.Select(c => new ChambreDto(
                c.Id,
                c.HotelId,
                c.Numero,
                c.Type,
                c.Etage,
                c.Prix.Amount,
                c.Prix.Currency,
                c.Actif,
                c.Statut
            )).ToList(),
            prixMin,
            noteMoyenne,
            nbAvis
        );
    }
}
