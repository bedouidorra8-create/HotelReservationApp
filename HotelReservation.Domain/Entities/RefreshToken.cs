using HotelReservation.Domain.Common;

namespace HotelReservation.Domain.Entities;

/// <summary>
/// Entite stockant les Refresh Tokens JWT.
/// Table : RefreshTokens (Id, UtilisateurId, Token, DateExpiration, Revoque)
/// </summary>
public class RefreshToken : Entity<Guid>
{
    public Guid UtilisateurId { get; private set; }
    public string Token { get; private set; } = string.Empty;
    public DateTime DateExpiration { get; private set; }
    public bool Revoque { get; private set; } = false;
    public DateTime DateCreation { get; private set; } = DateTime.UtcNow;

    private RefreshToken() { }

    public RefreshToken(Guid id, Guid utilisateurId, string token, DateTime dateExpiration)
        : base(id)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new DomainException("Le refresh token ne peut pas etre vide.");

        UtilisateurId = utilisateurId;
        Token = token;
        DateExpiration = dateExpiration;
        Revoque = false;
        DateCreation = DateTime.UtcNow;
    }

    public bool EstValide() => !Revoque && DateExpiration > DateTime.UtcNow;

    public void Revoquer()
    {
        Revoque = true;
    }
}
