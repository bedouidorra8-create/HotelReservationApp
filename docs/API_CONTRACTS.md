# CONTRATS D'INTÉGRATION API — BACKEND ASP.NET CORE

**Version de l'API :** v1  
**Architecture :** RESTful / Clean Architecture & DDD  
**Format :** JSON (`application/json`) & PDF (`application/pdf`)  
**Authentification :** JWT Bearer Token (`Authorization: Bearer <token>`)

---

## 1. TABLEAU DE BORD DE L'ENSEMBLE DES ENDPOINTS

### 1.1 Authentification & Compte (`/api/auth`)

| Méthode | Route | Rôle Requis | Description | Payload Requête | Réponse (200 / 201) |
|---|---|---|---|---|---|
| `POST` | `/api/auth/inscription` | Public | Inscription nouveau client | `InscrireUtilisateurRequest` | `{ message: string }` |
| `POST` | `/api/auth/connexion` | Public | Connexion par email/mot de passe | `ConnexionRequest` | `ConnexionResponse` (Token, RefreshToken, User) |
| `POST` | `/api/auth/rafraichir-token` | Public | Renouvellement du JWT | `RafraichirTokenRequest` | `TokenResponse` |
| `POST` | `/api/auth/mot-de-passe-oublie` | Public | Demande de token de réinitialisation | `DemanderResetPasswordRequest` | `{ message: string }` |
| `POST` | `/api/auth/reinitialiser-mot-de-passe`| Public | Validation nouveau mot de passe | `ReinitialiserMotDePasseRequest` | `{ message: string }` |
| `GET` | `/api/auth/verifier-email` | Public | Validation de l'adresse email | Query `token` | `{ message: string }` |
| `PUT` | `/api/auth/changer-mot-de-passe` | Authentifié | Modification mot de passe connecté | `ChangerMotDePasseRequest` | `{ message: string }` |
| `POST` | `/api/auth/logout` | Authentifié | Révocation du Refresh Token | `DeconnexionRequest` | `{ message: string }` |
| `GET` | `/api/auth/profil` | Authentifié | Consultation du profil connecté | Aucun | `ProfilUtilisateurDto` |
| `PUT` | `/api/auth/profil` | Authentifié | Mise à jour du profil | `ModifierProfilRequest` | `ProfilUtilisateurDto` |

---

### 1.2 Hôtels (`/api/hotels`)

| Méthode | Route | Rôle Requis | Description | Paramètres / Payload | Réponse |
|---|---|---|---|---|---|
| `GET` | `/api/hotels` | Public | Liste complète des hôtels | Aucun | `List<HotelDto>` |
| `GET` | `/api/hotels/{id}` | Public | Détail complet d'un hôtel | Route `id` (GUID) | `HotelDto` |
| `GET` | `/api/hotels/recherche` | Public | Recherche multicritère | `ville`, `prixMin`, `prixMax`, `etoilesMin`, `tri`, etc. | `List<HotelResultatRechercheDto>` |
| `POST` | `/api/hotels` | Admin, Agent | Création d'un établissement | `CreerHotelRequest` | `HotelDto` |
| `PUT` | `/api/hotels/{id}` | Admin, Agent | Mise à jour d'un hôtel | `ModifierHotelRequest` | `HotelDto` |
| `PATCH` | `/api/hotels/{id}/actif` | Admin, Agent | Activer/Désactiver un hôtel | Query `actif` (bool) | `HotelDto` |
| `DELETE` | `/api/hotels/{id}` | Admin | Suppression d'un hôtel | Route `id` (GUID) | `204 NoContent` |

---

### 1.3 Chambres (`/api/chambres` & `/api/hotels/{hotelId}/chambres`)

| Méthode | Route | Rôle Requis | Description | Paramètres / Payload | Réponse |
|---|---|---|---|---|---|
| `GET` | `/api/hotels/{hotelId}/chambres` | Public | Liste des chambres d'un hôtel | Route `hotelId` (GUID) | `List<ChambreDto>` |
| `GET` | `/api/hotels/{hotelId}/chambres/disponibilites` | Public | Grille journalière de disponibilité | `dateDebut`, `dateFin` | `DisponibilitesHotelResponse` |
| `GET` | `/api/chambres/recherche` | Public | Recherche multicritère de chambres | `type`, `prixMin`, `prixMax`, `statut` | `List<ChambreDto>` |
| `POST` | `/api/hotels/{hotelId}/chambres` | Admin, Agent | Ajout d'une chambre à l'hôtel | `CreerChambreRequest` | `ChambreDto` |
| `PUT` | `/api/chambres/{id}` | Admin, Agent | Modification d'une chambre | `ModifierChambreRequest` | `ChambreDto` |
| `PATCH` | `/api/chambres/{id}/statut` | Admin, Agent | Changement statut opérationnel | Query `statut` (Libre, Occupee, Maintenance) | `ChambreDto` |
| `PATCH` | `/api/chambres/{id}/actif` | Admin, Agent | Activation/Désactivation chambre | Query `actif` (bool) | `ChambreDto` |
| `DELETE` | `/api/chambres/{id}` | Admin, Agent | Suppression d'une chambre libre | Route `id` (GUID) | `204 NoContent` |

---

### 1.4 Réservations (`/api/reservations`)

| Méthode | Route | Rôle Requis | Description | Paramètres / Payload | Réponse |
|---|---|---|---|---|---|
| `GET` | `/api/reservations` | Admin, Agent | Liste globale des réservations | Aucun | `List<ReservationDto>` |
| `GET` | `/api/reservations/{id}` | Authentifié | Consultation d'une réservation | Route `id` (Vérif IDOR Client / Personnel) | `ReservationDto` |
| `GET` | `/api/reservations/client/{clientId}` | Authentifié | Réservations d'un client | Route `clientId`, Query optionnel `statut` | `List<ReservationDto>` |
| `GET` | `/api/reservations/recherche` | Admin, Agent | Recherche multicritère | `clientId`, `hotelId`, `statut`, `dateDebut`, `dateFin` | `List<ReservationDto>` |
| `GET` | `/api/reservations/calendrier` | Admin, Agent | Données calendrier dérivées | `hotelId`, `dateDebut`, `dateFin` | `CalendrierReservationsResponse` |
| `POST` | `/api/reservations` | Client, Staff | Création d'une réservation | `CreerReservationRequest` | `ReservationDto` |
| `POST` | `/api/reservations/{id}/confirmer`| Admin, Agent | Confirmation d'une réservation | Route `id` | `ReservationDto` |
| `PUT` | `/api/reservations/{id}` | Client, Staff | Modification dates/occupants | `ModifierReservationRequest` | `ReservationDto` |
| `PATCH` | `/api/reservations/{id}/statut` | Admin, Agent | Changement forcé de statut | Query `statut` | `ReservationDto` |
| `POST` | `/api/reservations/{id}/annuler`| Authentifié | Annulation d'une réservation | Route `id` | `ReservationDto` |

---

### 1.5 Paiements & Facturation (`/api/paiements`)

| Méthode | Route | Rôle Requis | Description | Paramètres / Payload | Réponse |
|---|---|---|---|---|---|
| `GET` | `/api/paiements` | Admin, Agent | Liste globale des paiements | Query `statut`, `mode`, `dateDebut`, `dateFin` | `List<PaiementDto>` |
| `POST` | `/api/paiements` | Client | Règlement d'une réservation | `EnregistrerPaiementRequest` | `PaiementDto` |
| `GET` | `/api/paiements/me` | Client | Historique paiements client connecté | Aucun | `List<PaiementDto>` |
| `GET` | `/api/paiements/reservation/{reservationId}` | Admin, Agent | Paiements d'une réservation | Route `reservationId` | `List<PaiementDto>` |
| `POST` | `/api/paiements/{id}/annuler` | Admin, Agent | Annulation d'une transaction | Route `id` | `PaiementDto` |
| `GET` | `/api/paiements/{reservationId}/facture` | Authentifié | Facture consolidée JSON | Route `reservationId` (Vérif IDOR) | `FactureDto` |
| `GET` | `/api/paiements/{reservationId}/facture/pdf` | Authentifié | **Téléchargement Facture PDF** | Route `reservationId` (Vérif IDOR) | Binaire `application/pdf` |

---

### 1.6 Avis & Modération (`/api/avis` & `/api/administration/avis`)

| Méthode | Route | Rôle Requis | Description | Paramètres / Payload | Réponse |
|---|---|---|---|---|---|
| `GET` | `/api/avis/hotel/{hotelId}` | Public | Avis validés d'un hôtel | Route `hotelId` | `List<AvisDto>` |
| `GET` | `/api/avis/client/{clientId}` | Authentifié | Avis soumis par un client | Route `clientId` (Vérif IDOR) | `List<AvisDto>` |
| `GET` | `/api/avis/{id}` | Public | Consultation d'un avis par ID | Route `id` | `AvisDto` |
| `GET` | `/api/avis/en-attente` | Admin | Avis en attente de modération | Aucun | `List<AvisDto>` |
| `GET` | `/api/administration/avis` | Admin | Tous les avis (avec filtre statut) | Query optionnel `statut` | `List<AvisDto>` |
| `POST` | `/api/avis` | Client | Déposer un avis | `AjouterAvisRequest` | `AvisDto` |
| `PUT` | `/api/avis/{id}` | Client | Modifier son avis | `ModifierAvisRequest` | `AvisDto` |
| `DELETE` | `/api/avis/{id}` | Client, Admin | Supprimer un avis | Route `id` | `204 NoContent` |
| `POST` | `/api/administration/avis/{id}/valider` | Admin | Valider et publier un avis | Route `id` | `AvisDto` |
| `POST` | `/api/administration/avis/{id}/rejeter` | Admin | Rejeter un avis | Route `id` | `AvisDto` |

---

### 1.7 Favoris (`/api/favoris`)

| Méthode | Route | Rôle Requis | Description | Paramètres / Payload | Réponse |
|---|---|---|---|---|---|
| `GET` | `/api/favoris/client/{clientId}` | Client | Liste des hôtels favoris | Route `clientId` | `List<HotelDto>` |
| `POST` | `/api/favoris` | Client | Ajouter un hôtel aux favoris | `AjouterFavoriRequest(Guid HotelId)` | `FavoriDto` |
| `DELETE` | `/api/favoris/{id}` | Client | Retirer des favoris | Route `id` (GUID du favori ou hotelId) | `204 NoContent` |

---

### 1.8 Tableau de Bord & Statistiques (`/api/dashboard`)

| Méthode | Route | Rôle Requis | Description | Réponse |
|---|---|---|---|---|
| `GET` | `/api/dashboard` | Admin, Agent | Métriques consolidées selon rôle | `DashboardDto` (Agent : 3 indicateurs restreints ; Admin : complet) |
| `GET` | `/api/dashboard/statistiques` | Admin | 7 métriques statistiques et graphiques | `StatistiquesDto` |

---

### 1.9 Administration & Rôles (`/api/administration`)

| Méthode | Route | Rôle Requis | Description | Payload | Réponse |
|---|---|---|---|---|---|
| `GET` | `/api/administration/utilisateurs` | Admin | Liste de tous les utilisateurs | Aucun | `List<UtilisateurDto>` |
| `GET` | `/api/administration/utilisateurs/{id}` | Admin | Fiche utilisateur par ID | Route `id` | `UtilisateurDto` |
| `GET` | `/api/administration/utilisateurs/recherche`| Admin | Recherche utilisateurs | Query `terme`, `role` | `List<UtilisateurDto>` |
| `POST` | `/api/administration/utilisateurs` | Admin | Création Agent ou Admin | `AjouterUtilisateurRequest` | `UtilisateurDto` |
| `PUT` | `/api/administration/agents/{id}` | Admin | Modification fiche agent | `ModifierAgentRequest` | `UtilisateurDto` |
| `DELETE` | `/api/administration/agents/{id}` | Admin | Suppression d'un agent | Route `id` | `204 NoContent` |
| `PUT` | `/api/administration/administrateurs/{id}` | Admin | Modification fiche admin | `ModifierAdministrateurRequest` | `UtilisateurDto` |
| `DELETE` | `/api/administration/administrateurs/{id}` | Admin | Suppression d'un admin | Route `id` | `204 NoContent` |
| `PATCH` | `/api/administration/utilisateurs/{id}/actif` | Admin | Activer/Désactiver compte | `ActiverDesactiverCompteRequest` | `UtilisateurDto` |
| `GET` | `/api/administration/roles` | Admin | Liste des rôles configurés | Aucun | `List<RoleDto>` |
| `GET` | `/api/administration/roles/{id}` | Admin | Détail d'un rôle par ID | Route `id` | `RoleDto` |
| `POST` | `/api/administration/roles` | Admin | Création ou mise à jour rôle | `GererRolesRequest` | `RoleDto` |
| `GET` | `/api/audit-logs` | Admin | Journalisation des audits | Query `dateDebut`, `dateFin` | `List<AuditLogDto>` |
