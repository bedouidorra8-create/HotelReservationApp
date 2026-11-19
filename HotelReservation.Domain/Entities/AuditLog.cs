using HotelReservation.Domain.Common;

namespace HotelReservation.Domain.Entities;

public class AuditLog : Entity<Guid>
{
    public Guid? UtilisateurId { get; private set; }
    public string Action { get; private set; } = string.Empty;
    public string Entite { get; private set; } = string.Empty;
    public Guid? EntiteId { get; private set; }
    public string? AncienneValeur { get; private set; }
    public string? NouvelleValeur { get; private set; }
    public DateTime DateHeure { get; private set; }

    private AuditLog() { }

    public AuditLog(
        Guid id,
        Guid? utilisateurId,
        string action,
        string entite,
        Guid? entiteId,
        string? ancienneValeur,
        string? nouvelleValeur)
        : base(id)
    {
        if (string.IsNullOrWhiteSpace(action))
            throw new DomainException("L'action d'audit est obligatoire.");
        if (string.IsNullOrWhiteSpace(entite))
            throw new DomainException("L'entité auditée est obligatoire.");

        UtilisateurId = utilisateurId;
        Action = action.Trim();
        Entite = entite.Trim();
        EntiteId = entiteId;
        AncienneValeur = ancienneValeur;
        NouvelleValeur = nouvelleValeur;
        DateHeure = DateTime.UtcNow;
    }
}
