# MATRICE DES DROITS ET RÔLES (POST-CORRECTIONS)

**Date :** 17 Novembre 2026  
**Référentiel :** Diagramme de classes & Alignement Backend/Frontend  
**Statut :** Corrections appliquées & compilées avec succès  

---

## 1. TABLEAU DE VÉRIFICATION APRÈS CORRECTIONS

| # | Opération | Rôles Attendus | Endpoint Backend | Sécurité Backend | Angular (Menu & Route) | Verdict Final |
|---|---|---|---|---|---|---|
| **1** | `modifierSonProfil` | Client, Agent, Admin (siens) | `PUT /api/auth/profil` | `[Authorize]` (extrait `sub` JWT) | Menu profil client + Topbar admin | **OK** |
| **2** | `reserver` | Client (siens), Agent, Admin | `POST /api/reservations` | `[Authorize(Roles = "Client,Administrateur,Agent")]` | Bouton "Réserver" fiche hôtel | **OK** |
| **3** | `modifierReservation` | Client (siens), Agent, Admin | `PUT /api/reservations/{id}` | `[Authorize]` (propriété vérifiée) | Modale Réservation | **OK** |
| **4** | `annulerReservation` | Client (siens), Agent, Admin | `POST /api/reservations/{id}/annuler` | `[Authorize]` (propriété vérifiée) | Bouton "Annuler" | **OK** |
| **5** | `consulterSesReservations` | Client (siens) | `GET /api/reservations/client/{clientId}` | `[Authorize]` (vérifie `clientId == sub`) | Menu "Mes réservations" | **OK** |
| **6** | `payer` | Client (siens) | `POST /api/paiements` | `[Authorize(Roles = "Client")]` (vérif IDOR) | `/client/payments` | **OK** |
| **7** | `telechargerFacture` (JSON) | Client (siens) | `GET /api/paiements/{reservationId}/facture` | `[Authorize]` (vérif IDOR) | `/client/invoice/:id` | **OK** |
| **8** | `telechargerFacture` (PDF) | Client (siens) | `GET /api/paiements/{reservationId}/facture/pdf` | `[Authorize]` + **Vérification IDOR ajoutée** | `/client/invoice/:id` | **OK (Sécurisé)** |
| **9** | `ajouterAvis` | Client (siens) | `POST /api/avis` | `[Authorize(Roles = "Client")]` | Modale Avis | **OK** |
| **10** | `modifierAvis` | Client (siens) | `PUT /api/avis/{id}` | `[Authorize(Roles = "Client")]` (vérifie auteur) | Modale Avis | **OK** |
| **11** | `supprimerAvis` (le sien) | Client (siens) | `DELETE /api/avis/{id}` | `[Authorize(Roles = "Client,Administrateur")]` | Modale Avis | **OK** |
| **12** | `ajouterAuxFavoris` | Client (siens) | `POST /api/favoris` | `[Authorize(Roles = "Client")]` (classe) | Bouton Coeur | **OK** |
| **13** | `creerReservation` (guichet) | Agent, Admin | `POST /api/reservations` | `[Authorize(Roles = "Client,Administrateur,Agent")]` | `/admin/reservations` | **OK** |
| **14** | `changerStatutReservation` | Agent, Admin | `PATCH /api/reservations/{id}/statut` | `[Authorize(Roles = "Administrateur,Agent")]` | `/admin/reservations` | **OK** |
| **15** | `consulterListeReservations` | Agent, Admin | `GET /api/reservations` | `[Authorize(Roles = "Administrateur,Agent")]` | `/admin/reservations` | **OK** |
| **16** | `rechercherReservation` | Agent, Admin | `GET /api/reservations/recherche` | `[Authorize(Roles = "Administrateur,Agent")]` | Filtres recherche | **OK** |
| **17** | `annulerPaiement` | Agent, Admin | `POST /api/paiements/{id}/annuler` | `[Authorize(Roles = "Administrateur,Agent")]` | `/admin/payments` | **OK** |
| **18** | `consulterPaiements` | Agent, Admin | `GET /api/paiements` | `[Authorize(Roles = "Administrateur,Agent")]` | `/admin/payments` | **OK** |
| **19** | `genererFacture` | Agent, Admin | `GET /api/paiements/{reservationId}/facture` | `[Authorize]` (Personnel exempté IDOR) | `/admin/payments` | **OK** |
| **20** | `ajouterHotel` | Agent, Admin | `POST /api/hotels` | `[Authorize(Roles = "Administrateur,Agent")]` | `/admin/hotels` | **OK** |
| **21** | `modifierHotel` | Agent, Admin | `PUT /api/hotels/{id}` | `[Authorize(Roles = "Administrateur,Agent")]` | `/admin/hotels` | **OK** |
| **22** | `activerDesactiverHotel` | Agent, Admin | `PATCH /api/hotels/{id}/actif` | `[Authorize(Roles = "Administrateur,Agent")]` | `/admin/hotels` | **OK** |
| **23** | `gererChambres` | Agent, Admin | `POST/PUT/DELETE /api/chambres` | `[Authorize(Roles = "Administrateur,Agent")]` | `/admin/rooms` | **OK** |
| **24** | `consulterListeClients` | Agent, Admin | `GET /api/clients` | `[Authorize(Roles = "Administrateur,Agent")]` | `/admin/users` | **OK** |
| **25** | `rechercherClient` | Agent, Admin | `GET /api/clients/recherche` | `[Authorize(Roles = "Administrateur,Agent")]` | `/admin/users` | **OK** |
| **26** | `modifierClient` | Agent, Admin | `PUT /api/clients/{id}` | `[Authorize(Roles = "Administrateur,Agent")]` | `/admin/users` | **OK** |
| **27** | `consulterListeUtilisateurs` | Admin | `GET /api/administration/utilisateurs` | `[Authorize(Roles = "Administrateur")]` | Masqué à l'Agent | **OK** |
| **28** | `rechercherUtilisateur` | Admin | `GET /api/administration/utilisateurs/recherche` | `[Authorize(Roles = "Administrateur")]` | Masqué à l'Agent | **OK** |
| **29** | `modifierAgent` | Admin | `PUT /api/administration/agents/{id}` | `[Authorize(Roles = "Administrateur")]` | Masqué à l'Agent | **OK** |
| **30** | `supprimerAgent` | Admin | `DELETE /api/administration/agents/{id}` | `[Authorize(Roles = "Administrateur")]` | Masqué à l'Agent | **OK** |
| **31** | `ajouterUtilisateur` | Admin | `POST /api/administration/utilisateurs` | `[Authorize(Roles = "Administrateur")]` | Masqué à l'Agent | **OK** |
| **32** | `activerDesactiverCompte` | Admin | `PATCH /api/administration/utilisateurs/{id}/actif` | `[Authorize(Roles = "Administrateur")]` + **Anti-auto-désactivation** | Écran Utilisateurs | **OK (Sécurisé)** |
| **33** | `gererRoles` | Admin | `GET/POST /api/administration/roles` | `[Authorize(Roles = "Administrateur")]` | Masqué à l'Agent | **OK** |
| **34** | `validerAvis` | Admin | `POST /api/administration/avis/{id}/valider` | `[Authorize(Roles = "Administrateur")]` | **Menu masqué à l'Agent & Route protégée par roleGuard Admin** | **OK (Cohérent 100%)** |
| **35** | `supprimerAvis` (arbitraire) | Admin | `DELETE /api/avis/{id}` | `[Authorize(Roles = "Client,Administrateur")]` | Écran Modération Admin | **OK** |
| **36** | `StatistiqueService` (7 stats) | Admin | `GET /api/dashboard/statistiques` | `[Authorize(Roles = "Administrateur")]` | Masqué à l'Agent | **OK** |
| **37** | `TableauDeBordService` (jour, attente) | Agent, Admin | `GET /api/dashboard` | `[Authorize(Roles = "Administrateur,Agent")]` | `/admin/dashboard` | **OK** |
| **38** | `TableauDeBordService` (CA, graphiques) | Admin | `GET /api/dashboard` | `[Authorize(Roles = "Administrateur,Agent")]` (`null` pour Agent) | `@if` Angular masquant à l'Agent | **OK** |
