using HotelReservation.Domain.Enums;

namespace HotelReservation.Application.DTOs;

/// <summary>
/// DTO représentant un utilisateur du système (Client, Agent ou Administrateur).
/// </summary>
public record UtilisateurDto(
    Guid Id,
    string Nom,
    string Prenom,
    string Email,
    string Telephone,
    string Role,
    StatutUtilisateur Statut,
    DateTime DateCreation
);

/// <summary>
/// Requête d'ajout d'un nouvel utilisateur (Agent, Administrateur, etc.).
/// </summary>
public record AjouterUtilisateurRequest(
    string Nom,
    string Prenom,
    string Email,
    string MotDePasse,
    string Telephone,
    DateTime DateNaissance,
    Genre? Sexe,
    string Adresse,
    string Role,
    string Matricule = ""
);

/// <summary>
/// Requête de modification des coordonnées d'un agent.
/// </summary>
public record ModifierAgentRequest(
    string Nom,
    string Prenom,
    string Telephone,
    string Adresse,
    string Photo = ""
);

/// <summary>
/// Requête de modification des coordonnées d'un administrateur.
/// </summary>
public record ModifierAdministrateurRequest(
    string Nom,
    string Prenom,
    string Telephone,
    string Adresse,
    string Photo = ""
);

/// <summary>
/// Paramètres de recherche d'utilisateurs.
/// </summary>
public record RechercherUtilisateurRequest(string? Terme, string? Role = null);

/// <summary>
/// Requête d'activation ou désactivation d'un compte utilisateur.
/// </summary>
public record ActiverDesactiverCompteRequest(bool Actif);

/// <summary>
/// Requête de gestion ou assignation d'un rôle.
/// </summary>
public record GererRolesRequest(string Nom, string Description, List<string> Permissions, Guid? RoleId = null);

/// <summary>
/// DTO représentant un rôle et ses permissions associées.
/// </summary>
public record RoleDto(Guid Id, string Nom, string Description, IReadOnlyCollection<string> Permissions);

