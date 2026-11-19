using HotelReservation.Domain.Enums;

namespace HotelReservation.Application.DTOs;

// ── Requetes ─────────────────────────────────────────────────────────────────

public record InscrireUtilisateurRequest(
    string Nom,
    string Prenom,
    string Email,
    string MotDePasse,
    string Telephone,
    DateTime DateNaissance,
    Genre? Sexe,
    string Adresse,
    string Role // "Client" | "Agent" | "Administrateur"
);

public record ConnexionRequest(
    string Email,
    string MotDePasse
);

public record RafraichirTokenRequest(
    string RefreshToken
);

public record DeconnexionRequest(
    string RefreshToken
);

public record DemanderResetPasswordRequest(
    string Email
);

public record ReinitialiserMotDePasseRequest(
    string Token,
    string NouveauMotDePasse
);

public record ChangerMotDePasseRequest(
    string AncienMotDePasse,
    string NouveauMotDePasse
);

public record ModifierProfilRequest(
    string Nom,
    string Prenom,
    string Telephone,
    string Adresse,
    string Photo = "",
    DateTime? DateNaissance = null,
    Genre? Sexe = null
);

// ── Reponses ──────────────────────────────────────────────────────────────────

public record ConnexionResponse(
    string Jwt,
    string RefreshToken,
    DateTime JwtExpiration,
    Guid UtilisateurId,
    string NomComplet,
    string Email,
    string Role
);

public record TokenResponse(
    string Jwt,
    string RefreshToken,
    DateTime JwtExpiration
);

public record MessageResponse(
    string Message
);

public record ProfilUtilisateurDto(
    Guid Id,
    string Nom,
    string Prenom,
    string Email,
    string Telephone,
    string Role,
    DateTime DateNaissance,
    Genre? Sexe,
    string Adresse,
    string Photo
);
