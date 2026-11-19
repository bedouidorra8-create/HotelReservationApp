using HotelReservation.Domain.Enums;

namespace HotelReservation.Application.DTOs;

public record ClientDto(
    Guid Id,
    string Nom,
    string Prenom,
    string Email,
    string Telephone,
    DateTime DateNaissance,
    Genre? Sexe,
    string Adresse,
    string Photo,
    StatutUtilisateur Statut,
    DateTime DateCreation
);

public record CreerClientRequest(
    string Nom,
    string Prenom,
    string Email,
    string MotDePasse,
    string Telephone,
    DateTime DateNaissance,
    Genre? Sexe,
    string Adresse,
    string Photo = ""
);

public record ModifierClientRequest(
    string Nom,
    string Prenom,
    string Telephone,
    string Adresse,
    string Photo = ""
);

public record RechercherClientRequest(string? Terme);
