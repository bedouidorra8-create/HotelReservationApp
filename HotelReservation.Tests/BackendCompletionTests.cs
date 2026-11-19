using System.Text;
using HotelReservation.Application.DTOs;
using HotelReservation.Application.Services;
using HotelReservation.Application.UseCases;
using HotelReservation.Domain.Common;
using HotelReservation.Domain.Entities;
using HotelReservation.Domain.Enums;
using HotelReservation.Domain.Repositories;
using HotelReservation.Domain.Services;
using HotelReservation.Domain.ValueObjects;
using HotelReservation.Infrastructure.Services;
using Xunit;

namespace HotelReservation.Tests;

public class TestRoleRepository : IRoleRepository
{
    private readonly List<Role> _roles = new();
    public Task<Role?> GetByIdAsync(Guid id, CancellationToken ct = default) => Task.FromResult(_roles.FirstOrDefault(r => r.Id == id));
    public Task<Role?> GetByNomAsync(string nom, CancellationToken ct = default) => Task.FromResult(_roles.FirstOrDefault(r => r.Nom.Equals(nom, StringComparison.OrdinalIgnoreCase)));
    public Task<IEnumerable<Role>> GetAllAsync(CancellationToken ct = default) => Task.FromResult<IEnumerable<Role>>(_roles);
    public Task AddAsync(Role role, CancellationToken ct = default) { _roles.Add(role); return Task.CompletedTask; }
    public Task UpdateAsync(Role role, CancellationToken ct = default) => Task.CompletedTask;
    public Task DeleteAsync(Role role, CancellationToken ct = default) => Task.CompletedTask;
}

public class TestClientRepository : IClientRepository
{
    private readonly List<Client> _clients = new();
    public Task<Client?> GetByIdAsync(Guid id, CancellationToken ct = default) => Task.FromResult(_clients.FirstOrDefault(c => c.Id == id));
    public Task<Client?> GetByEmailAsync(Email email, CancellationToken ct = default) => Task.FromResult(_clients.FirstOrDefault(c => c.Email.Value == email.Value));
    public Task<IEnumerable<Client>> GetAllAsync(CancellationToken ct = default) => Task.FromResult<IEnumerable<Client>>(_clients);
    public Task AddAsync(Client client, CancellationToken ct = default) { _clients.Add(client); return Task.CompletedTask; }
    public Task UpdateAsync(Client client, CancellationToken ct = default) => Task.CompletedTask;
    public Task DeleteAsync(Client client, CancellationToken ct = default) => Task.CompletedTask;
}

public class FakeCurrentUserService : ICurrentUserService
{
    public Guid? UserId { get; set; }
    public string? Role { get; set; }
    public bool IsAuthenticated => UserId.HasValue;
    public bool IsInRole(string role) => string.Equals(Role, role, StringComparison.OrdinalIgnoreCase);
}

public class BackendCompletionTests
{
    [Fact]
    public async Task FacturePdfService_Generates_Valid_Pdf_Stream()
    {
        // Arrange
        var service = new FacturePdfService();
        var facture = new FactureDto(
            ReservationId: Guid.NewGuid(),
            DateEmission: DateTime.UtcNow,
            Total: 780.00m,
            Devise: "TND",
            Paiements: new List<PaiementDto>
            {
                new(Guid.NewGuid(), Guid.NewGuid(), 780.00m, "TND", DateTime.UtcNow, ModePaiement.Carte, "REF-12345", StatutPaiement.Paye, null)
            },
            Client: new FactureClientDto(Guid.NewGuid(), "Ben Ali", "Amine", "amine@test.com"),
            Hotel: new FactureHotelDto(Guid.NewGuid(), "The Residence Tunis", "Gammarth, Tunis"),
            Chambre: new FactureChambreDto(Guid.NewGuid(), "101")
        );

        // Act
        var pdfBytes = await service.GenererPdfAsync(facture);

        // Assert
        Assert.NotNull(pdfBytes);
        Assert.True(pdfBytes.Length > 500, "Le PDF doit avoir une taille binaire significative.");
        var header = Encoding.ASCII.GetString(pdfBytes.Take(5).ToArray());
        Assert.Equal("%PDF-", header);
    }

    [Fact]
    public void Role_Permissions_Can_Be_Added_And_Removed_Without_Duplicates()
    {
        // Arrange
        var role = new Role(Guid.NewGuid(), "Superviseur", "Rôle de supervision opérationnelle");

        // Act
        role.AjouterPermission("LIRE_RESERVATIONS");
        role.AjouterPermission("LIRE_RESERVATIONS"); // Doublon intentionnel
        role.AjouterPermission("MODIFIER_CHAMBRE");

        // Assert
        Assert.Equal(2, role.Permissions.Count);
        Assert.Contains("LIRE_RESERVATIONS", role.Permissions);
        Assert.Contains("MODIFIER_CHAMBRE", role.Permissions);

        // Act remove
        role.RetirerPermission("LIRE_RESERVATIONS");

        // Assert after remove
        Assert.Single(role.Permissions);
        Assert.DoesNotContain("LIRE_RESERVATIONS", role.Permissions);
    }

    [Fact]
    public async Task GenererFacturePdfUseCase_Throws_DomainException_On_IDOR_Attempt()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var hackerClientId = Guid.NewGuid();
        var hotelId = Guid.NewGuid();
        var chambreId = Guid.NewGuid();

        var domainService = new ReservationDomainService();
        var reservation = domainService.CreerReservation(
            chambreId,
            clientId,
            hotelId,
            new DateRange(DateTime.UtcNow.AddDays(2), DateTime.UtcNow.AddDays(5)),
            2,
            Enumerable.Empty<Reservation>(),
            new Money(200m, "EUR")
        );

        var resRepo = new TestReservationRepository();
        await resRepo.AddAsync(reservation);

        var paiementRepo = new TestPaiementRepository();
        var clientRepo = new TestClientRepository();
        var hotelRepo = new TestHotelRepository();

        var genererFactureUseCase = new GenererFactureUseCase(resRepo, paiementRepo, clientRepo, hotelRepo);
        var pdfService = new FacturePdfService();

        var currentUser = new FakeCurrentUserService
        {
            UserId = hackerClientId, // Un autre client qui tente d'accéder à la réservation
            Role = "Client"
        };

        var useCase = new GenererFacturePdfUseCase(genererFactureUseCase, pdfService, resRepo, currentUser);

        // Act & Assert
        await Assert.ThrowsAsync<DomainException>(() => useCase.ExecuteAsync(reservation.Id));
    }

    [Fact]
    public async Task ConsulterAvisHotelUseCase_Returns_Only_Validated_Reviews()
    {
        // Arrange
        var hotelId = Guid.NewGuid();
        var avisRepo = new TestAvisRepository();

        var avisValide = new Avis(Guid.NewGuid(), Guid.NewGuid(), hotelId, Guid.NewGuid(), new NoteAvis(5), "Excellent séjour !");
        avisValide.Valider();

        var avisEnAttente = new Avis(Guid.NewGuid(), Guid.NewGuid(), hotelId, Guid.NewGuid(), new NoteAvis(3), "Moyen.");

        await avisRepo.AddAsync(avisValide);
        await avisRepo.AddAsync(avisEnAttente);

        var useCase = new ConsulterAvisHotelUseCase(avisRepo);

        // Act
        var result = (await useCase.ExecuteAsync(hotelId)).ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal(StatutAvis.Valide, result[0].Statut);
        Assert.Equal("Excellent séjour !", result[0].Commentaire);
    }

    [Fact]
    public async Task ConsulterTousPaiementsUseCase_Filters_Properly_By_Status_And_Mode()
    {
        // Arrange
        var paiementRepo = new TestPaiementRepository();
        var p1 = new Paiement(Guid.NewGuid(), Guid.NewGuid(), new Money(100m, "EUR"), ModePaiement.Carte, "REF-1");
        var p2 = new Paiement(Guid.NewGuid(), Guid.NewGuid(), new Money(200m, "EUR"), ModePaiement.Virement, "REF-2");
        p2.Annuler();

        await paiementRepo.AddAsync(p1);
        await paiementRepo.AddAsync(p2);

        var useCase = new ConsulterTousPaiementsUseCase(paiementRepo);

        // Act
        var resultPaye = (await useCase.ExecuteAsync(statut: StatutPaiement.Paye)).ToList();
        var resultVirement = (await useCase.ExecuteAsync(mode: ModePaiement.Virement)).ToList();

        // Assert
        Assert.Single(resultPaye);
        Assert.Equal("REF-1", resultPaye[0].Reference);

        Assert.Single(resultVirement);
        Assert.Equal("REF-2", resultVirement[0].Reference);
    }

    [Fact]
    public async Task ConsulterUtilisateurUseCase_Maps_Role_Correctly()
    {
        // Arrange
        var userRepo = new TestUtilisateurRepository();
        var admin = new Administrateur(
            Guid.NewGuid(),
            "Dupont",
            "Jean",
            new Email("admin@test.com"),
            "hash",
            "0123456789",
            DateTime.UtcNow.AddYears(-30),
            Genre.Homme,
            "Paris",
            "photo.jpg",
            "MAT-100"
        );
        await userRepo.AddAsync(admin);

        var useCase = new ConsulterUtilisateurUseCase(userRepo);

        // Act
        var dto = await useCase.ExecuteAsync(admin.Id);

        // Assert
        Assert.NotNull(dto);
        Assert.Equal("Administrateur", dto.Role);
        Assert.Equal("Dupont", dto.Nom);
        Assert.Equal("admin@test.com", dto.Email);
    }
}
