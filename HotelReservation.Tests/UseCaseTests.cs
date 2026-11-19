using FluentValidation;
using HotelReservation.Application.DTOs;
using HotelReservation.Application.Services;
using HotelReservation.Application.UseCases;
using HotelReservation.Api.Controllers;
using HotelReservation.Domain.Common;
using HotelReservation.Domain.Entities;
using HotelReservation.Domain.Enums;
using HotelReservation.Domain.Repositories;
using HotelReservation.Domain.Services;
using HotelReservation.Domain.ValueObjects;
using Xunit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HotelReservation.Tests;

public class UseCaseTests
{
    [Fact]
    public async Task Exact_payment_amount_is_accepted()
    {
        var reservation = CreateReservation();
        var payments = new FakePaymentRepository();
        var useCase = CreatePaymentUseCase(reservation, payments);

        var result = await useCase.ExecuteAsync(reservation.ClientId, PaymentRequest(reservation));

        Assert.Equal(reservation.Total.Amount, result.Montant);
    }

    [Fact]
    public async Task Lower_payment_amount_is_rejected()
    {
        var reservation = CreateReservation();
        var useCase = CreatePaymentUseCase(reservation, new FakePaymentRepository());

        await Assert.ThrowsAsync<DomainException>(() => useCase.ExecuteAsync(
            reservation.ClientId, PaymentRequest(reservation, reservation.Total.Amount - 1)));
    }

    [Fact]
    public async Task Higher_payment_amount_is_rejected()
    {
        var reservation = CreateReservation();
        var useCase = CreatePaymentUseCase(reservation, new FakePaymentRepository());

        await Assert.ThrowsAsync<DomainException>(() => useCase.ExecuteAsync(
            reservation.ClientId, PaymentRequest(reservation, reservation.Total.Amount + 1)));
    }

    [Fact]
    public async Task Different_payment_currency_is_rejected()
    {
        var reservation = CreateReservation();
        var useCase = CreatePaymentUseCase(reservation, new FakePaymentRepository());

        await Assert.ThrowsAsync<DomainException>(() => useCase.ExecuteAsync(
            reservation.ClientId, PaymentRequest(reservation, devise: "USD")));
    }

    [Fact]
    public async Task Second_payment_is_rejected()
    {
        var reservation = CreateReservation();
        var payments = new FakePaymentRepository();
        var useCase = CreatePaymentUseCase(reservation, payments);
        await useCase.ExecuteAsync(reservation.ClientId, PaymentRequest(reservation));

        await Assert.ThrowsAsync<DomainException>(() => useCase.ExecuteAsync(
            reservation.ClientId, PaymentRequest(reservation)));
    }

    [Fact]
    public async Task Payment_for_cancelled_reservation_is_rejected()
    {
        var reservation = CreateReservation();
        reservation.Annuler();
        var useCase = CreatePaymentUseCase(reservation, new FakePaymentRepository());

        await Assert.ThrowsAsync<DomainException>(() => useCase.ExecuteAsync(
            reservation.ClientId, PaymentRequest(reservation)));
    }

    [Fact]
    public async Task Payment_for_another_client_is_rejected()
    {
        var reservation = CreateReservation();
        var useCase = CreatePaymentUseCase(reservation, new FakePaymentRepository());

        await Assert.ThrowsAsync<DomainException>(() => useCase.ExecuteAsync(
            Guid.NewGuid(), PaymentRequest(reservation)));
    }

    [Fact]
    public async Task Payment_for_another_client_returns_forbidden()
    {
        var reservation = CreateReservation();
        var clientId = Guid.NewGuid();
        var paymentUseCase = CreatePaymentUseCase(reservation, new FakePaymentRepository());
        var controller = new PaiementsController(
            paymentUseCase,
            null!,
            null!,
            null!,
            new FakeReservationRepository(reservation));
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity([
                    new Claim(ClaimTypes.NameIdentifier, clientId.ToString()),
                    new Claim(ClaimTypes.Role, "Client")
                ], "test"))
            }
        };

        var result = await controller.Create(PaymentRequest(reservation));

        Assert.IsType<ForbidResult>(result.Result);
    }

    [Fact]
    public async Task Avis_creation_is_accepted_for_completed_reservation()
    {
        var reservation = CreateReservation();
        reservation.ChangerStatut(StatutReservation.Terminee);
        var avisRepository = new FakeAvisRepository();
        var useCase = new AjouterAvisUseCase(avisRepository, new FakeReservationRepository(reservation),
            new HotelReservation.Application.Validators.AjouterAvisRequestValidator());

        var result = await useCase.ExecuteAsync(reservation.ClientId,
            new AjouterAvisRequest(reservation.HotelId, reservation.Id, 5, "Excellent"));

        Assert.Equal(StatutAvis.EnAttente, result.Statut);
        Assert.NotNull(avisRepository.Added);
    }

    [Fact]
    public async Task Avis_creation_is_rejected_before_completion()
    {
        var reservation = CreateReservation();
        var useCase = new AjouterAvisUseCase(new FakeAvisRepository(), new FakeReservationRepository(reservation),
            new HotelReservation.Application.Validators.AjouterAvisRequestValidator());

        await Assert.ThrowsAsync<DomainException>(() => useCase.ExecuteAsync(reservation.ClientId,
            new AjouterAvisRequest(reservation.HotelId, reservation.Id, 5, "Excellent")));
    }

    [Fact]
    public async Task Revoked_token_is_rejected_by_refresh_use_case()
    {
        var token = new RefreshToken(Guid.NewGuid(), Guid.NewGuid(), "revoked", DateTime.UtcNow.AddDays(1));
        token.Revoquer();
        var useCase = new RafraichirTokenUseCase(new FakeRefreshTokenRepository(token), new FakeUtilisateurRepository(), new FakeTokenService());

        await Assert.ThrowsAsync<DomainException>(() => useCase.ExecuteAsync(new RafraichirTokenRequest("revoked")));
    }

    [Fact]
    public async Task Client_payment_query_returns_only_owned_reservations()
    {
        var owner = Guid.NewGuid();
        var other = CreateReservation();
        var owned = CreateReservation(clientId: owner);
        var payments = new FakePaymentRepository();
        payments.Items.Add(new Paiement(Guid.NewGuid(), owned.Id, owned.Total, ModePaiement.Carte, "OWNED"));
        payments.Items.Add(new Paiement(Guid.NewGuid(), other.Id, other.Total, ModePaiement.Carte, "OTHER"));

        var useCase = new ConsulterPaiementsUseCase(payments,
            new FakeReservationRepository(owned, other));
        var result = await useCase.ExecuteForClientAsync(owner);

        var payment = Assert.Single(result);
        Assert.Equal("OWNED", payment.Reference);
    }

    private static EnregistrerPaiementUseCase CreatePaymentUseCase(Reservation reservation, FakePaymentRepository payments)
        => new(payments, new FakeReservationRepository(reservation),
            new HotelReservation.Application.Validators.EnregistrerPaiementRequestValidator());

    private static EnregistrerPaiementRequest PaymentRequest(Reservation reservation, decimal? montant = null, string devise = "EUR")
        => new(reservation.Id, montant ?? reservation.Total.Amount, devise, ModePaiement.Carte, Guid.NewGuid().ToString());

    private static Reservation CreateReservation(Guid? clientId = null)
        => new ReservationDomainService().CreerReservation(Guid.NewGuid(), clientId ?? Guid.NewGuid(), Guid.NewGuid(),
            new DateRange(DateTime.UtcNow.Date.AddDays(1), DateTime.UtcNow.Date.AddDays(3)), 1,
            [], new Money(100, "EUR"));

    private sealed class FakeReservationRepository : IReservationRepository
    {
        private readonly List<Reservation> _items;
        public FakeReservationRepository(params Reservation[] items) => _items = items.ToList();
        public Task<Reservation?> GetByIdAsync(Guid id, CancellationToken ct = default) => Task.FromResult(_items.FirstOrDefault(x => x.Id == id));
        public Task<IEnumerable<Reservation>> GetReservationsActivesPourChambre(Guid id, CancellationToken ct = default) => Task.FromResult<IEnumerable<Reservation>>(_items.Where(x => x.ChambreId == id));
        public Task<IEnumerable<Reservation>> GetByClientIdAsync(Guid id, CancellationToken ct = default) => Task.FromResult<IEnumerable<Reservation>>(_items.Where(x => x.ClientId == id));
        public Task<IEnumerable<Reservation>> GetByHotelIdAsync(Guid id, CancellationToken ct = default) => Task.FromResult<IEnumerable<Reservation>>(_items.Where(x => x.HotelId == id));
        public Task<IEnumerable<Reservation>> GetAllAsync(CancellationToken ct = default) => Task.FromResult<IEnumerable<Reservation>>(_items);
        public Task AddAsync(Reservation item, CancellationToken ct = default) { _items.Add(item); return Task.CompletedTask; }
        public Task UpdateAsync(Reservation item, CancellationToken ct = default) => Task.CompletedTask;
    }

    private sealed class FakePaymentRepository : IPaiementRepository
    {
        public List<Paiement> Items { get; } = [];
        public Task<Paiement?> GetByIdAsync(Guid id, CancellationToken ct = default) => Task.FromResult(Items.FirstOrDefault(x => x.Id == id));
        public Task<IEnumerable<Paiement>> GetByReservationIdAsync(Guid id, CancellationToken ct = default) => Task.FromResult<IEnumerable<Paiement>>(Items.Where(x => x.ReservationId == id));
        public Task<IEnumerable<Paiement>> GetAllAsync(CancellationToken ct = default) => Task.FromResult<IEnumerable<Paiement>>(Items);
        public Task AddAsync(Paiement item, CancellationToken ct = default) { Items.Add(item); return Task.CompletedTask; }
        public Task UpdateAsync(Paiement item, CancellationToken ct = default) => Task.CompletedTask;
    }

    private sealed class FakeAvisRepository : IAvisRepository
    {
        public Avis? Added { get; private set; }
        public Task<Avis?> GetByIdAsync(Guid id, CancellationToken ct = default) => Task.FromResult<Avis?>(null);
        public Task<IEnumerable<Avis>> GetByHotelIdAsync(Guid id, CancellationToken ct = default) => Task.FromResult<IEnumerable<Avis>>([]);
        public Task<IEnumerable<Avis>> GetByClientIdAsync(Guid id, CancellationToken ct = default) => Task.FromResult<IEnumerable<Avis>>([]);
        public Task<IEnumerable<Avis>> GetAllAsync(CancellationToken ct = default) => Task.FromResult<IEnumerable<Avis>>([]);
        public Task AddAsync(Avis avis, CancellationToken ct = default) { Added = avis; return Task.CompletedTask; }
        public Task UpdateAsync(Avis avis, CancellationToken ct = default) => Task.CompletedTask;
        public Task DeleteAsync(Avis avis, CancellationToken ct = default) => Task.CompletedTask;
    }

    private sealed class FakeRefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly RefreshToken _token;
        public FakeRefreshTokenRepository(RefreshToken token) => _token = token;
        public Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken ct = default) => Task.FromResult<RefreshToken?>(_token.Token == token ? _token : null);
        public Task<IEnumerable<RefreshToken>> GetByUtilisateurIdAsync(Guid id, CancellationToken ct = default) => Task.FromResult<IEnumerable<RefreshToken>>([]);
        public Task AddAsync(RefreshToken token, CancellationToken ct = default) => Task.CompletedTask;
        public Task UpdateAsync(RefreshToken token, CancellationToken ct = default) => Task.CompletedTask;
        public Task RevoquerTousLesTokensAsync(Guid id, CancellationToken ct = default) => Task.CompletedTask;
    }

    private sealed class FakeUtilisateurRepository : IUtilisateurRepository
    {
        public Task<Utilisateur?> GetByIdAsync(Guid id, CancellationToken ct = default) => Task.FromResult<Utilisateur?>(null);
        public Task<Utilisateur?> GetByEmailAsync(Email email, CancellationToken ct = default) => Task.FromResult<Utilisateur?>(null);
        public Task<IEnumerable<Utilisateur>> GetAllAsync(CancellationToken ct = default) => Task.FromResult<IEnumerable<Utilisateur>>([]);
        public Task AddAsync(Utilisateur item, CancellationToken ct = default) => Task.CompletedTask;
        public Task UpdateAsync(Utilisateur item, CancellationToken ct = default) => Task.CompletedTask;
        public Task DeleteAsync(Utilisateur item, CancellationToken ct = default) => Task.CompletedTask;
    }

    private sealed class FakeTokenService : ITokenService
    {
        public string GenererJwt(Guid utilisateurId, string email, string role) => "jwt";
        public string GenererRefreshToken() => "refresh";
        public string GenererTokenSecurise() => "secure";
    }
}
