using HotelReservation.Domain.Enums;
using HotelReservation.Domain.ValueObjects;

namespace HotelReservation.Domain.Entities;

public class Administrateur : Personnel
{
    private Administrateur() : base() { }

    public Administrateur(
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
        : base(id, nom, prenom, email, motDePasseHash, telephone, dateNaissance, sexe, adresse, matricule, photo)
    {
    }

    public void ConsulterListeUtilisateurs() { }
    public void RechercherUtilisateur() { }
    public void ModifierAgent() { }
    public void SupprimerAgent() { }
    public void AjouterUtilisateur() { }
    public void ActiverDesactiverCompte() { }
    public void GererRoles() { }
    public void ValiderAvis() { }
    public void SupprimerAvis() { }
    public void SupprimerHotel() { }
    public void ActiverDesactiverHotel() { }
}
