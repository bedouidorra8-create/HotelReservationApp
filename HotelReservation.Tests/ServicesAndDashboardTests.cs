using HotelReservation.Application.DTOs;
using HotelReservation.Application.Services;
using HotelReservation.Application.UseCases;
using HotelReservation.Infrastructure.Services;
using Xunit;

namespace HotelReservation.Tests;

public class ServicesAndDashboardTests
{
    private readonly TestUtilisateurRepository _utilisateurRepo = new();
    private readonly TestHotelRepository _hotelRepo = new();
    private readonly TestReservationRepository _reservationRepo = new();
    private readonly TestPaiementRepository _paiementRepo = new();
    private readonly TestAvisRepository _avisRepo = new();

    private TableauDeBordService CreateDashboardService() =>
        new TableauDeBordService(_utilisateurRepo, _hotelRepo, _reservationRepo, _paiementRepo);

    private StatistiqueService CreateStatistiqueService() =>
        new StatistiqueService(_utilisateurRepo, _hotelRepo, _reservationRepo, _paiementRepo, _avisRepo);

    [Fact]
    public async Task TableauDeBordService_Methods_Return_Expected_Metrics()
    {
        var service = CreateDashboardService();

        var nbUsers = await service.ObtenirNombreUtilisateursAsync();
        var nbHotels = await service.ObtenirNombreHotelsAsync();
        var nbResa = await service.ObtenirNombreReservationsAsync();
        var nbResaJour = await service.ObtenirReservationsDuJourAsync();
        var dernieresResa = await service.ObtenirDernieresReservationsAsync();
        var paiementsAttente = await service.ObtenirPaiementsEnAttenteAsync();
        var ca = await service.ObtenirChiffreAffairesAsync();
        var graphResa = await service.ObtenirGraphiqueReservationsParMoisAsync();
        var graphRev = await service.ObtenirGraphiqueRevenusParMoisAsync();

        Assert.Equal(0, nbUsers);
        Assert.Equal(0, nbHotels);
        Assert.Equal(0, nbResa);
        Assert.Equal(0, nbResaJour);
        Assert.Empty(dernieresResa);
        Assert.Equal(0, paiementsAttente);
        Assert.Equal(0, ca);
        Assert.Equal(12, graphResa.Count);
        Assert.Equal(12, graphRev.Count);
    }

    [Fact]
    public async Task Agent_Role_On_Dashboard_Hides_Admin_Only_Metrics()
    {
        var dbService = CreateDashboardService();
        var useCase = new ConsulterDashboardUseCase(dbService);

        var dtoAgent = await useCase.ExecuteAsync("Agent");

        Assert.Null(dtoAgent.NombreTotalUtilisateurs);
        Assert.Null(dtoAgent.NombreHotels);
        Assert.Null(dtoAgent.NombreReservations);
        Assert.Null(dtoAgent.ChiffreAffaires);
        Assert.Null(dtoAgent.GraphiqueReservationsParMois);
        Assert.Null(dtoAgent.GraphiqueRevenusParMois);

        Assert.True(dtoAgent.ReservationsDuJour >= 0);
        Assert.True(dtoAgent.PaiementsEnAttente >= 0);
        Assert.NotNull(dtoAgent.DernieresReservations);
    }

    [Fact]
    public async Task Admin_Role_On_Dashboard_Shows_All_9_Metrics()
    {
        var dbService = CreateDashboardService();
        var useCase = new ConsulterDashboardUseCase(dbService);

        var dtoAdmin = await useCase.ExecuteAsync("Administrateur");

        Assert.NotNull(dtoAdmin.NombreTotalUtilisateurs);
        Assert.NotNull(dtoAdmin.NombreHotels);
        Assert.NotNull(dtoAdmin.NombreReservations);
        Assert.NotNull(dtoAdmin.ChiffreAffaires);
        Assert.NotNull(dtoAdmin.GraphiqueReservationsParMois);
        Assert.NotNull(dtoAdmin.GraphiqueRevenusParMois);
    }

    [Fact]
    public async Task StatistiqueService_Methods_Work_Correctly()
    {
        var service = CreateStatistiqueService();

        var resMois = await service.ObtenirReservationsParMoisAsync();
        var caMensuel = await service.ObtenirChiffreAffairesMensuelAsync();
        var topHotels = await service.ObtenirHotelsLesPlusReservesAsync();
        var modePaiements = await service.ObtenirRepartitionPaiementsParModeAsync();
        var newUsers = await service.ObtenirNombreNouveauxUtilisateursAsync();
        var tauxOcc = await service.ObtenirTauxOccupationHotelsAsync();
        var evolAvis = await service.ObtenirEvolutionAvisAsync();

        Assert.NotNull(resMois);
        Assert.NotNull(caMensuel);
        Assert.NotNull(topHotels);
        Assert.NotNull(modePaiements);
        Assert.NotNull(newUsers);
        Assert.NotNull(tauxOcc);
        Assert.NotNull(evolAvis);
    }
}
