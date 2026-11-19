using HotelReservation.Domain.Common;
using HotelReservation.Domain.Entities;
using HotelReservation.Domain.Enums;
using HotelReservation.Domain.ValueObjects;
using Xunit;

namespace HotelReservation.Tests;

public class EntityTests
{
    [Fact]
    public void Payment_cannot_have_negative_amount()
    {
        Assert.Throws<DomainException>(() => new Money(-1));
    }

    [Fact]
    public void Payment_cancellation_changes_status()
    {
        var payment = new Paiement(Guid.NewGuid(), Guid.NewGuid(), new Money(10), ModePaiement.Carte, "REF");

        payment.Annuler();

        Assert.Equal(StatutPaiement.Annule, payment.Statut);
    }

    [Fact]
    public void Revoked_refresh_token_is_invalid()
    {
        var token = new RefreshToken(Guid.NewGuid(), Guid.NewGuid(), "token", DateTime.UtcNow.AddDays(1));

        token.Revoquer();

        Assert.False(token.EstValide());
    }

    [Fact]
    public void Avis_can_be_added_only_with_a_valid_note()
    {
        Assert.Throws<DomainException>(() => new NoteAvis(6));
    }

    [Fact]
    public void Validated_avis_cannot_be_modified()
    {
        var avis = new Avis(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), new NoteAvis(4), "Très bien");
        avis.Valider();

        Assert.Throws<DomainException>(() => avis.Modifier(new NoteAvis(5), "Excellent"));
    }

    [Fact]
    public void Maintenance_room_cannot_become_occupied_directly()
    {
        var room = new Chambre(Guid.NewGuid(), Guid.NewGuid(), "101", TypeChambre.Simple, 1, new Money(50));
        room.ChangerStatut(StatutChambre.Maintenance);

        Assert.Throws<DomainException>(() => room.ChangerStatut(StatutChambre.Occupee));
    }

    [Fact]
    public void Reservation_validator_rejects_invalid_dates()
    {
        var validator = new HotelReservation.Application.Validators.CreerReservationRequestValidator();
        var result = validator.Validate(new HotelReservation.Application.DTOs.CreerReservationRequest(
            Guid.NewGuid(), DateTime.UtcNow.Date.AddDays(2), DateTime.UtcNow.Date.AddDays(1), 1));

        Assert.False(result.IsValid);
    }
}
