using HotelReservation.Application.DTOs;
using HotelReservation.Domain.Enums;
using HotelReservation.Domain.Repositories;
using HotelReservation.Domain.Services;
using HotelReservation.Domain.ValueObjects;

namespace HotelReservation.Application.UseCases;

public class RechercherHotelsUseCase
{
    private readonly IHotelRepository _hotelRepository;
    private readonly IReservationRepository _reservationRepository;
    private readonly ReservationDomainService _reservationDomainService;
    private readonly IAvisRepository _avisRepository;

    public RechercherHotelsUseCase(
        IHotelRepository hotelRepository,
        IReservationRepository reservationRepository,
        ReservationDomainService reservationDomainService,
        IAvisRepository avisRepository)
    {
        _hotelRepository = hotelRepository;
        _reservationRepository = reservationRepository;
        _reservationDomainService = reservationDomainService;
        _avisRepository = avisRepository;
    }

    public async Task<IEnumerable<HotelResultatRechercheDto>> ExecuteAsync(
        RechercherHotelsRequest request,
        CancellationToken ct = default)
    {
        // 1. Filtrer via repository : ville, prix min/max, etoiles min
        var hotels = await _hotelRepository.RechercherAsync(
            request.Ville,
            request.PrixMin,
            request.PrixMax,
            request.EtoilesMin,
            ct);

        var resultats = new List<HotelResultatRechercheDto>();

        foreach (var hotel in hotels)
        {
            // Chambres disponibles (actives, pas en Maintenance)
            var chambresLibres = hotel.Chambres
                .Where(c => c.Actif && c.Statut != StatutChambre.Maintenance)
                .ToList();

            if (request.DisponibleUniquement && (request.DateDebut == null || request.DateFin == null))
                throw new ArgumentException("dateDebut et dateFin sont obligatoires quand disponibleUniquement=true.");

            if (request.DisponibleUniquement && request.DateDebut.HasValue && request.DateFin.HasValue)
            {
                var periode = new DateRange(request.DateDebut.Value, request.DateFin.Value);

                // Recuperer toutes les reservations de cet hotel
                var reservationsHotel = await _reservationRepository.GetByHotelIdAsync(hotel.Id, ct);

                // Reutilisation STRICTE de ReservationDomainService.PeutReserver()
                chambresLibres = chambresLibres
                    .Where(chambre => _reservationDomainService.PeutReserver(
                        chambre.Id,
                        periode,
                        reservationsHotel))
                    .ToList();

                if (!chambresLibres.Any())
                    continue; // cet hotel n'a aucune chambre libre sur la periode
            }

            if (!chambresLibres.Any())
                continue; // aucune chambre Libre (statut)

            // Prix affiche = chambre la moins chere disponible
            var chambreLaMoinsChere = chambresLibres
                .OrderBy(c => c.Prix.Amount)
                .First();

            // Calcul Note moyenne et nombre d'avis validés
            var avisValides = (await _avisRepository.GetByHotelIdAsync(hotel.Id, ct))
                .Where(a => a.Statut == StatutAvis.Valide)
                .ToList();

            double? noteMoyenne = avisValides.Any()
                ? Math.Round(avisValides.Average(a => a.Note.Valeur), 1)
                : null;
            int? nbAvis = avisValides.Count;

            resultats.Add(new HotelResultatRechercheDto(
                hotel.Id,
                hotel.Nom,
                hotel.Ville,
                hotel.Adresse,
                hotel.NbEtoiles,
                hotel.Description,
                chambreLaMoinsChere.Prix.Amount,
                chambreLaMoinsChere.Prix.Currency,
                hotel.Images,
                noteMoyenne,
                nbAvis
            ));
        }

        // 2. Appliquer le tri
        resultats = request.Tri switch
        {
            "prixDesc" => resultats.OrderByDescending(r => r.PrixAffiche).ToList(),
            "noteDesc" => resultats.OrderByDescending(r => r.NoteMoyenne ?? 0).ThenByDescending(r => r.NbEtoiles).ToList(),
            _ => resultats.OrderBy(r => r.PrixAffiche).ToList() // prixAsc par defaut
        };

        return resultats;
    }
}
