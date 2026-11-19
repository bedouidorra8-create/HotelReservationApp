using HotelReservation.Domain.Enums;
using HotelReservation.Domain.ValueObjects;

namespace HotelReservation.Domain.Entities;

public class Client : Utilisateur
{
    private Client() : base() { }

    public Client(
        Guid id,
        string nom,
        string prenom,
        Email email,
        string motDePasseHash,
        string telephone,
        DateTime dateNaissance,
        Genre? sexe,
        string adresse,
        string photo = "")
        : base(id, nom, prenom, email, motDePasseHash, telephone, dateNaissance, sexe, adresse, photo)
    {
    }

    
}
