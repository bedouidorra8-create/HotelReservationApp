# FRONTEND_BACKEND_GAP_TRACKER.md
## Suivi des écarts et contrats d'intégration Frontend ↔ Backend

Ce document recense exhaustivement tous les besoins fonctionnels du frontend dont les endpoints, contrats ou comportements métier nécessitent une finalisation, un ajout ou un ajustement dans l'API ASP.NET Core lors de la phase suivante.

---

### Tableau de bord de synthèse des manques Backend

| ID | Espace Fonctionnel | Élément manquant ou à enrichir | Type | Priorité | Statut Backend | Solution / Contrat Frontend en place |
|---|---|---|---|---|---|---|
| **BG-01** | Factures & Paiements | Téléchargement binaire de facture en format PDF (`GET /api/paiements/{id}/facture/pdf`) | Endpoint / Service PDF | Haute | À faire | `PaiementService.telechargerFacturePdf()` génère un document PDF imprimable / téléchargeable via Blob et vue facture HTML/Print. |
| **BG-02** | Modération des avis | Endpoint de récupération globale des avis en attente de modération (`GET /api/administration/avis?statut=EnAttente`) | Endpoint / Query | Haute | À faire | `AdministrationService.getAvisEnAttente()` préparé ; intégration avec fallback gracieux et actions réelles `POST /api/administration/avis/{id}/valider` & `/rejeter` déjà opérationnelles côté backend. |
| **BG-03** | Gestion des paiements Admin | Endpoint de consultation paginée/filtrée de tous les paiements (`GET /api/paiements`) pour le personnel | Endpoint / Query | Moyenne | À faire | `PaiementService.getAllPaiements()` préparé ; l'admin consulte les paiements par réservation (`GET /api/paiements/reservation/{id}`) ou liste globale avec fallback démo si endpoint global absent. |
| **BG-04** | Consultation profil utilisateur par ID | Endpoint pour consulter un utilisateur individuel par son ID pour les admins (`GET /api/administration/utilisateurs/{id}`) | Endpoint / Query | Moyenne | À faire | Utilisation actuelle de `GET /api/administration/utilisateurs` filtré côté client en attendant la route dédiée. |
| **BG-05** | Calendrier de disponibilité global multi-hôtels | Agrégation des disponibilités pour plusieurs hôtels sans requêter hôtel par hôtel | Query / Optimisation | Faible | À faire | `CalendrierService` orchestre la requête sur l'endpoint existant `GET /api/reservations/calendrier?hotelId=...` ou `GET /api/hotels/{id}/chambres/disponibilites`. |
| **BG-06** | Réinitialisation mot de passe par email réel | Service SMTP / Envoi d'email réel du jeton de réinitialisation | Infrastructure | Moyenne | À faire | Endpoint `POST /api/auth/mot-de-passe-oublie` existant renvoie un message de succès ; interface utilisateur complète avec saisie token et nouveau mot de passe prête. |

---

### Fiches détaillées des écarts

#### BG-01 : Téléchargement PDF de Facture
- **Besoin métier** : Permettre au client et au personnel de télécharger la facture officielle au format PDF pour une réservation confirmée/payée.
- **Backend actuel** : `GET /api/paiements/{reservationId}/facture` retourne un objet JSON `FactureDto` (Total, DateEmission, Client, Hotel, Chambre, Paiements).
- **Évolution backend attendue** : Créer `GET /api/paiements/{reservationId}/facture/pdf` retournant un `FileContentResult` (MIME `application/pdf`) généré avec QuestPDF ou DinkToPdf.
- **Contrat Frontend mis en place** : Composant `InvoiceViewComponent` affichant la facture avec mise en page premium aux couleurs de la marque, impression directe (`window.print()`) et bouton de téléchargement prêt à brancher sur le flux binaire.

#### BG-02 : Liste des avis à modérer pour l'administration
- **Besoin métier** : Permettre aux administrateurs de visualiser la liste complète des avis soumis par les clients avec filtre par statut (EnAttente, Valide, Rejete).
- **Backend actuel** : 
  - `POST /api/administration/avis/{id}/valider` (existe)
  - `POST /api/administration/avis/{id}/rejeter` (existe)
  - `GET /api/avis/hotel/{hotelId}` (ne retourne que les avis validés pour un hôtel)
  - Manque : `GET /api/administration/avis` pour lister tous les avis soumis.
- **Évolution backend attendue** : Créer `ConsulterTousAvisUseCase` et exposer `GET /api/administration/avis`.
- **Contrat Frontend mis en place** : `AdministrationService.getAvisEnAttente()` avec modèle `AvisDto` typé, interface de modération avec badges et boutons de validation/rejet réels.

#### BG-03 : Liste globale des paiements pour l'administration
- **Besoin métier** : Permettre au gestionnaire de consulter l'historique complet des transactions financières de l'ensemble des établissements.
- **Backend actuel** : 
  - `GET /api/paiements/me` (uniquement réservations du client connecté)
  - `GET /api/paiements/reservation/{reservationId}` (paiement d'une réservation précise)
  - `POST /api/paiements/{id}/annuler` (existe)
- **Évolution backend attendue** : Créer `GET /api/paiements` accessible aux rôles `Administrateur, Agent`.
- **Contrat Frontend mis en place** : `PaiementService.getAllPaiements()` avec filtres par mode de paiement, statut et dates.

---

### Cartographie des Endpoints Backend réellement connectés

| Entité | Endpoint Backend | Méthode HTTP | Rôle requis | État d'intégration Frontend |
|---|---|---|---|---|
| **Auth** | `/api/auth/connexion` | POST | Anonyme | Connecté & géré via `AuthService.login()` |
| **Auth** | `/api/auth/inscription` | POST | Anonyme | Connecté via `AuthService.register()` |
| **Auth** | `/api/auth/rafraichir-token` | POST | Anonyme | Connecté via `AuthInterceptor` automatique |
| **Auth** | `/api/auth/profil` | GET / PUT | Authentifié | Connecté via `AuthService.getProfil()` et `updateProfil()` |
| **Auth** | `/api/auth/changer-mot-de-passe` | PUT | Authentifié | Connecté via `AuthService.changePassword()` |
| **Hotels** | `/api/hotels` | GET | Public | Connecté via `HotelService.getHotels()` |
| **Hotels** | `/api/hotels/{id}` | GET | Public | Connecté via `HotelService.getHotelById()` |
| **Hotels** | `/api/hotels/recherche` | GET | Public | Connecté via `HotelService.searchHotels()` avec critères |
| **Hotels** | `/api/hotels` | POST | Admin/Agent | Connecté via `HotelService.createHotel()` |
| **Hotels** | `/api/hotels/{id}` | PUT | Admin/Agent | Connecté via `HotelService.updateHotel()` |
| **Hotels** | `/api/hotels/{id}/actif` | PATCH | Admin/Agent | Connecté via `HotelService.toggleActive()` |
| **Chambres** | `/api/chambres/recherche` | GET | Public | Connecté via `ChambreService.searchChambres()` |
| **Chambres** | `/api/hotels/{hotelId}/chambres` | GET | Public | Connecté via `ChambreService.getChambresByHotel()` |
| **Chambres** | `/api/hotels/{hotelId}/chambres` | POST | Admin/Agent | Connecté via `ChambreService.createChambre()` |
| **Chambres** | `/api/chambres/{id}/statut` | PATCH | Admin/Agent | Connecté via `ChambreService.changeStatus()` |
| **Chambres** | `/api/chambres/{id}` | PUT / DELETE | Admin/Agent | Connecté via `ChambreService.updateChambre()` / `deleteChambre()` |
| **Réservations**| `/api/reservations` | POST | Client/Staff | Connecté via `ReservationService.createReservation()` |
| **Réservations**| `/api/reservations/client/{clientId}` | GET | Authentifié | Connecté via `ReservationService.getMyReservations()` |
| **Réservations**| `/api/reservations/recherche` | GET | Admin/Agent | Connecté via `ReservationService.searchReservations()` |
| **Réservations**| `/api/reservations/{id}/confirmer`| POST | Admin/Agent | Connecté via `ReservationService.confirmReservation()` |
| **Réservations**| `/api/reservations/{id}/annuler`| POST | Authentifié | Connecté via `ReservationService.cancelReservation()` |
| **Réservations**| `/api/reservations/calendrier` | GET | Admin/Agent | Connecté via `ReservationService.getCalendrier()` |
| **Paiements** | `/api/paiements` | POST | Client | Connecté via `PaiementService.createPaiement()` |
| **Paiements** | `/api/paiements/me` | GET | Client | Connecté via `PaiementService.getMyPaiements()` |
| **Paiements** | `/api/paiements/{reservationId}/facture` | GET | Authentifié | Connecté via `PaiementService.getFacture()` |
| **Favoris** | `/api/favoris/client/{clientId}` | GET | Client | Connecté via `FavorisService.getFavorites()` |
| **Favoris** | `/api/favoris` | POST | Client | Connecté via `FavorisService.addFavorite()` |
| **Favoris** | `/api/favoris/{id}` | DELETE | Client | Connecté via `FavorisService.removeFavorite()` |
| **Avis** | `/api/avis/hotel/{hotelId}` | GET | Public | Connecté via `AvisService.getHotelReviews()` |
| **Avis** | `/api/avis` | POST | Client | Connecté via `AvisService.createReview()` |
| **Dashboard** | `/api/dashboard` | GET | Admin/Agent | Connecté via `DashboardService.getDashboard()` |
| **Dashboard** | `/api/dashboard/statistiques` | GET | Admin | Connecté via `DashboardService.getStatistiques()` |
| **Admin** | `/api/administration/utilisateurs` | GET | Admin | Connecté via `AdministrationService.getUsers()` |
| **Admin** | `/api/administration/utilisateurs` | POST | Admin | Connecté via `AdministrationService.addUser()` |
| **Admin** | `/api/administration/utilisateurs/{id}/actif` | PATCH | Admin | Connecté via `AdministrationService.toggleUserStatus()` |
| **Audit** | `/api/audit-logs` | GET | Admin | Connecté via `AuditService.getLogs()` |
