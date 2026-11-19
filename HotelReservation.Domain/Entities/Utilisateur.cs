using HotelReservation.Domain.Common;
using HotelReservation.Domain.Enums;
using HotelReservation.Domain.ValueObjects;

namespace HotelReservation.Domain.Entities;

public abstract class Utilisateur : AggregateRoot<Guid>
{
    public string Nom { get; protected set; } = string.Empty;
    public string Prenom { get; protected set; } = string.Empty;
    public Email Email { get; protected set; } = null!;
    public string MotDePasseHash { get; protected set; } = string.Empty;
    public string Telephone { get; protected set; } = string.Empty;
    public DateTime DateNaissance { get; protected set; }
    public Genre? Sexe { get; protected set; }
    public string Adresse { get; protected set; } = string.Empty;
    public string Photo { get; protected set; } = string.Empty;
    public StatutUtilisateur Statut { get; protected set; } = StatutUtilisateur.Actif;
    public DateTime DateCreation { get; protected set; } = DateTime.UtcNow;

    // ── Authentification ──────────────────────────────────────────────────────
    public bool EmailVerifie { get; protected set; } = false;
    public string? TokenVerificationEmail { get; protected set; }
    public DateTime? TokenVerificationEmailExpiration { get; protected set; }
    public string? TokenResetMotDePasse { get; protected set; }
    public DateTime? TokenResetMotDePasseExpiration { get; protected set; }

    protected Utilisateur() { }

    protected Utilisateur(
        Guid id,
        string nom,
        string prenom,
        Email email,
        string motDePasseHash,
        string telephone,
        DateTime dateNaissance,
        Genre? sexe,
        string adresse,
        string photo) : base(id)
    {
        if (string.IsNullOrWhiteSpace(nom)) throw new DomainException("Le nom est obligatoire.");
        if (string.IsNullOrWhiteSpace(prenom)) throw new DomainException("Le prénom est obligatoire.");

        Nom = nom.Trim();
        Prenom = prenom.Trim();
        Email = email ?? throw new ArgumentNullException(nameof(email));
        MotDePasseHash = motDePasseHash;
        Telephone = telephone;
        DateNaissance = dateNaissance;
        Sexe = sexe;
        Adresse = adresse;
        Photo = photo;
        Statut = StatutUtilisateur.Actif;
        DateCreation = DateTime.UtcNow;
        EmailVerifie = false;
    }

    public virtual void ModifierProfil(string nom, string prenom, string telephone, string adresse, string photo, DateTime? dateNaissance = null, Genre? sexe = null)
    {
        if (string.IsNullOrWhiteSpace(nom)) throw new DomainException("Le nom ne peut pas être vide.");
        if (string.IsNullOrWhiteSpace(prenom)) throw new DomainException("Le prénom ne peut pas être vide.");

        Nom = nom.Trim();
        Prenom = prenom.Trim();
        Telephone = telephone;
        Adresse = adresse;
        Photo = photo;
        if (dateNaissance.HasValue) DateNaissance = dateNaissance.Value;
        if (sexe.HasValue) Sexe = sexe.Value;
    }

    public virtual void Desactiver()
    {
        if (Statut == StatutUtilisateur.Desactive)
            throw new DomainException("Le compte utilisateur est déjà désactivé.");

        Statut = StatutUtilisateur.Desactive;
    }

    public virtual void Activer()
    {
        Statut = StatutUtilisateur.Actif;
    }

    /// <summary>Définit le token de vérification d'email (durée de vie : 24h).</summary>
    public void DefinirTokenVerificationEmail(string token)
    {
        TokenVerificationEmail = token;
        TokenVerificationEmailExpiration = DateTime.UtcNow.AddHours(24);
    }

    /// <summary>Marque l'email comme vérifié et invalide le token.</summary>
    public void VerifierEmail(string token)
    {
        if (TokenVerificationEmail != token)
            throw new DomainException("Token de vérification d'email invalide.");
        if (TokenVerificationEmailExpiration < DateTime.UtcNow)
            throw new DomainException("Le token de vérification d'email a expiré.");

        EmailVerifie = true;
        TokenVerificationEmail = null;
        TokenVerificationEmailExpiration = null;
    }

    /// <summary>Marque l'email comme vérifié sans token (seeding / création par un admin).</summary>
    public void ConfirmerEmailDirectement()
    {
        EmailVerifie = true;
        TokenVerificationEmail = null;
        TokenVerificationEmailExpiration = null;
    }

    /// <summary>Définit le token de reset de mot de passe (durée de vie : 1h).</summary>
    public void DefinirTokenResetMotDePasse(string token)
    {
        TokenResetMotDePasse = token;
        TokenResetMotDePasseExpiration = DateTime.UtcNow.AddHours(1);
    }

    /// <summary>Réinitialise le mot de passe via le token (usage unique).</summary>
    public void ReinitialiserMotDePasse(string token, string nouveauHash)
    {
        if (TokenResetMotDePasse != token)
            throw new DomainException("Token de réinitialisation invalide.");
        if (TokenResetMotDePasseExpiration < DateTime.UtcNow)
            throw new DomainException("Le token de réinitialisation a expiré.");
        if (string.IsNullOrWhiteSpace(nouveauHash))
            throw new DomainException("Le nouveau mot de passe est obligatoire.");

        MotDePasseHash = nouveauHash;
        // Invalidation immédiate : usage unique
        TokenResetMotDePasse = null;
        TokenResetMotDePasseExpiration = null;
    }

    /// <summary>Change le mot de passe d'un utilisateur déjà connecté (vérifie l'ancien).</summary>
    public void ChangerMotDePasse(string ancienHash, string nouveauHash, Func<string, string, bool> verifier)
    {
        if (!verifier(ancienHash, MotDePasseHash))
            throw new DomainException("L'ancien mot de passe est incorrect.");
        if (string.IsNullOrWhiteSpace(nouveauHash))
            throw new DomainException("Le nouveau mot de passe est obligatoire.");

        MotDePasseHash = nouveauHash;
    }

    /// <summary>Permet de définir la date d'inscription lors du seeding ou de l'import d'historique.</summary>
    public void DefinirDateCreation(DateTime date)
    {
        DateCreation = date;
    }
}