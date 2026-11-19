using HotelReservation.Domain.Enums;
using HotelReservation.Domain.ValueObjects;

namespace HotelReservation.Domain.Entities;

public abstract class Personnel : Utilisateur
{
    public string Matricule { get; protected set; } = string.Empty;

    protected Personnel() : base() { }

    protected Personnel(
        Guid id,
        string nom,
        string prenom,
        Email email,
        string motDePasseHash,
        string telephone,
        DateTime dateNaissance,
        Genre? sexe,
        string adresse,
        string matricule,
        string photo = "")
        : base(id, nom, prenom, email, motDePasseHash, telephone, dateNaissance, sexe, adresse, photo)
    {
        Matricule = matricule;
    }

}