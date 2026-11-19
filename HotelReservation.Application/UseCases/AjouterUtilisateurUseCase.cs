using HotelReservation.Application.DTOs;
using HotelReservation.Domain.Common;
using HotelReservation.Domain.Entities;
using HotelReservation.Domain.Repositories;
using HotelReservation.Domain.ValueObjects;

namespace HotelReservation.Application.UseCases;

public class AjouterUtilisateurUseCase
{
    private readonly IUtilisateurRepository _repository;

    public AjouterUtilisateurUseCase(IUtilisateurRepository repository)
    {
        _repository = repository;
    }

    public async Task<UtilisateurDto> ExecuteAsync(AjouterUtilisateurRequest request, CancellationToken cancellationToken = default)
    {
        var email = new Email(request.Email);
        if (await _repository.GetByEmailAsync(email, cancellationToken) != null)
            throw new DomainException("Un compte avec cet email existe déjà.");

        var hash = BCrypt.Net.BCrypt.HashPassword(request.MotDePasse);
        Utilisateur utilisateur;
        switch (request.Role.ToLowerInvariant())
        {
            case "client":
                utilisateur = new Client(Guid.NewGuid(), request.Nom, request.Prenom, email, hash, request.Telephone, request.DateNaissance, request.Sexe, request.Adresse);
                break;
            case "agent":
                utilisateur = new Agent(Guid.NewGuid(), request.Nom, request.Prenom, email, hash, request.Telephone, request.DateNaissance, request.Sexe, request.Adresse, request.Matricule);
                break;
            case "administrateur":
                utilisateur = new Administrateur(Guid.NewGuid(), request.Nom, request.Prenom, email, hash, request.Telephone, request.DateNaissance, request.Sexe, request.Adresse, request.Matricule);
                break;
            default:
                throw new DomainException("Le rôle doit être Client, Agent ou Administrateur.");
        }

        await _repository.AddAsync(utilisateur, cancellationToken);
        return ConsulterListeUtilisateursUseCase.ToDto(utilisateur);
    }
}
