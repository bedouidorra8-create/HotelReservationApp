# RAPPORT COMPLET D'AUDIT DES RÔLES & PERMISSIONS (PHASES 0 & 1)

**Projet :** HotelReservationApp (ASP.NET Core / EF Core / Angular 19)  
**Date :** 17 Novembre 2026  
**Statut :** Phase 0 (Inventaire) & Phase 1 (Matrice & Preuves d'écarts) terminées en LECTURE SEULE  
**Fichier de référence des preuves :** `docs/ROLE_PERMISSION_MATRIX.md`

---

## 1. CONTEXTE & PROTOCOLE DE SÉCURITÉ

- **Environnement hôte :** Windows (sans outil CLI `git` installé sur le système hôte).
- **Règle absolue :** Aucune modification de code durant les Phases 0 et 1.
- **Principe d'exécution :** Chaque constatation est étayée par l'inspection exhaustive des contrôleurs, des cas d'utilisation, des entités de domaine, du pipeline d'authentification JWT et des guards/layouts Angular.

---

## 2. INVENTAIRE TECHNIQUE EXHAUSTIF (PHASE 0)

### 2.1 Backend ASP.NET Core

1. **Pipeline d'Authentification & Rôles :**
   - Schéma : `JwtBearerDefaults.AuthenticationScheme`.
   - Émission du claim de rôle dans `TokenService.GenererJwt()` :
     `new Claim(ClaimTypes.Role, role)` où `role` $\in \{$ `"Client"`, `"Agent"`, `"Administrateur"` $\}$.
   - Aucun fallback global `[Authorize]` par défaut dans `Program.cs` $\rightarrow$ chaque contrôleur ou endpoint doit impérativement définir ses propres attributs.

2. **Revue Contrôleur par Contrôleur :**
   - **`AuthController` :** 10 endpoints. 6 anonymes (inscription, connexion, refresh, mot de passe oublié/reset, vérification email), 4 protégés par `[Authorize]` (logout, profil GET/PUT, changer mot de passe). Tous basés sur l'identité propre du token (`sub`).
   - **`HotelsController` :** 9 endpoints. Lecture publique (liste, recherche, détail, disponibilités). Écriture/Modification réservées à `[Authorize(Roles = "Administrateur,Agent")]`. Suppression réservée à `[Authorize(Roles = "Administrateur")]`.
   - **`ChambresController` :** 8 endpoints. Lecture publique (liste, recherche, disponibilités). Gestion réservée à `[Authorize(Roles = "Administrateur,Agent")]`.
   - **`ReservationsController` :** 10 endpoints. Aucun endpoint anonyme. Gestion opérationnelle réservée au personnel (`Administrateur,Agent`). Endpoints client (`/client/{clientId}`, annuler, modifier, détail) vérifient la propriété de la réservation.
   - **`PaiementsController` :** 7 endpoints. Paiement par carte réservé à `[Authorize(Roles = "Client")]` avec vérification stricte `reservation.ClientId == clientId`. Annulation et consultation globale réservées au personnel. **Faille critique sur `GET /api/paiements/{reservationId}/facture/pdf` : aucun contrôle de propriété.**
   - **`AvisController` :** 8 endpoints. Consultation par hôtel et par ID publique. Ajout/modification réservés au Client. Suppression autorisée au Client (le sien) et à l'Administrateur (tous). **Manque de contrôle de propriété sur `GET /api/avis/client/{clientId}`.**
   - **`FavorisController` :** 3 endpoints couverts par `[Authorize(Roles = "Client")]` au niveau classe. Contrôle strict `utilisateurId == clientId` sur la consultation.
   - **`ClientsController` :** 6 endpoints. Tous réservés à `[Authorize(Roles = "Administrateur,Agent")]` sauf `DELETE` réservé à l'Admin.
   - **`AdministrationController` :** 15 endpoints protégés au niveau classe par `[Authorize(Roles = "Administrateur")]`. Couvre la gestion des utilisateurs, agents, admins, rôles et la modération des avis.
   - **`DashboardController` :** 2 endpoints. Statistiques avancées réservées à l'Admin. Dashboard opérationnel ouvert à `Administrateur,Agent` avec projection différenciée selon le rôle dans `ConsulterDashboardUseCase`.
   - **`AuditLogsController` :** 1 endpoint réservé à `[Authorize(Roles = "Administrateur")]`.

---

### 2.2 Frontend Angular 19

1. **Guards & Gestion des Rôles :**
   - `authGuard` : vérifie la présence du token JWT et l'état authentifié.
   - `roleGuard` : lit le rôle actif via `authService.userRole()` (signal alimenté au login) et compare avec `route.data['roles']`. Redirige vers `/client/dashboard` ou `/admin/dashboard` en cas de non-concordance.
   - `authInterceptor` : injecte le header `Authorization: Bearer <token>`, gère le refresh token automatique sur erreur 401, mais n'intercepte pas globalement le statut 403 (délégué aux composants).

2. **Layouts & Menus :**
   - **`PublicLayoutComponent` :** Affiche "Mon Espace" pour les Clients et "Administration" pour le Personnel.
   - **`ClientLayoutComponent` :** Espace voyageur (Réservations, Paiements, Favoris, Profil).
   - **`AdminLayoutComponent` :** Sidebar avec sections dédiées. La section "DIRECTION & GOUVERNANCE" (Statistiques, Utilisateurs, Audit) est masquée pour l'Agent via `@if (authService.userRole() === 'Administrateur')`.
   - **Incohérence détectée :** Le lien `⭐ Modération des Avis` est affiché à l'Agent dans la section "FINANCES & CONFIANCE", alors que les endpoints backend répondent 403.

---

## 3. SYNTHÈSE DES ÉCARTS PROUVÉS (PHASE 1)

### 3.1 Écarts SUR-PERMISSIFS (Failles de sécurité - Priorité Haute)

| Réf | Composant affecté | Risque de sécurité | Cause racine | Statut attendu vs Réel |
|---|---|---|---|---|
| **SUR-01** | `GET /api/paiements/{reservationId}/facture/pdf` | **IDOR Critique :** N'importe quel client connecté peut télécharger la facture PDF nominative d'un autre client en fournissant son `reservationId`. | Omission du test `reservation.ClientId == tokenUserId` présent sur la route JSON. | Attendu : 403 Forbidden<br>Réel : 200 OK avec le PDF |
| **SUR-02** | `GET /api/avis/client/{clientId}` | **Fuite de données privées :** Un client ou tiers connecté peut lister tous les avis d'un client ciblé. | Absence de condition de propriété `clientId == tokenUserId` si l'appelant est un Client. | Attendu : 403 Forbidden<br>Réel : 200 OK |
| **SUR-03** | `PATCH /api/administration/utilisateurs/{id}/actif` | **Auto-verrouillage :** Un administrateur peut se désactiver lui-même par inadvertance. | Absence de validation `adminId != targetId` dans `ActiverDesactiverCompteUseCase`. | Attendu : 400 Bad Request / 403<br>Réel : 200 OK |

---

### 3.2 Écarts INCOHÉRENTS (Désalignement Frontend / Backend)

| Réf | Écran / Route Angular | Comportement Backend | Impact Utilisateur |
|---|---|---|---|
| **INC-01** | `/admin/reviews` (`ReviewsModerationComponent`) | Endpoints `/api/administration/avis/*` protégés `[Authorize(Roles = "Administrateur")]` | L'Agent a le lien dans son menu et accède à la page. Lorsqu'il clique sur "Valider" ou "Rejeter", l'API renvoie une erreur 403. |

---

### 3.3 Écarts SOUS-PERMISSIFS (Droits du diagramme non accordés à l'Agent)

| Réf | Opération du diagramme | Configuration Backend actuelle | Décision recommandée |
|---|---|---|---|
| **SOUS-01** | `supprimerClient` (Agent) | `DELETE /api/clients/{id}` annoté `[Authorize(Roles = "Administrateur")]` | Arbitrage D-07 : Confirmer si la suppression définitive de compte client doit être ouverte à l'Agent ou réservée à l'Admin. |
| **SOUS-02** | `supprimerHotel` (Agent) | `DELETE /api/hotels/{id}` annoté `[Authorize(Roles = "Administrateur")]` | Arbitrage D-08 : La suppression physique d'un établissement entier est très lourde de conséquences ; la réservation à l'Admin est une mesure conservatrice prudente. |
| **SOUS-03** | `enregistrerPaiement` (Agent) | `POST /api/paiements` restreint à `[Authorize(Roles = "Client")]` | L'enregistrement d'un encaissement guichet/comptoir par le personnel nécessite de renseigner `enregistrePar`. |

---

## 4. TABLEAU DE BORD AGENT : VÉRIFICATION SPÉCIFIQUE (SECTION 1)

Le use case `ConsulterDashboardUseCase.cs` applique fidèlement la règle conservatrice :
- **Agent :** Reçoit uniquement `ReservationsDuJour`, `PaiementsEnAttente` et `DernieresReservations`. Les champs `NombreTotalUtilisateurs`, `NombreHotels`, `NombreReservations`, `ChiffreAffaires`, `GraphiqueReservationsParMois` et `GraphiqueRevenusParMois` sont forcés à `null`.
- **Admin :** Reçoit l'ensemble des 9 métriques et graphiques.
- **Frontend :** Les cartes KPI et graphiques du template `admin-dashboard.component.ts` sont conditionnés par `@if (data()!.chiffreAffaires)` ou `@if (data()!.graphiqueReservationsParMois)`, masquant automatiquement les données financières aux Agents.

---

## 5. STATUT DU PROTOCOLE & TRANSITION VERS LA PHASE 2

- **Phase 0 & Phase 1 :** 100% exécutées en lecture seule sans aucune altération de code.
- **Fichiers produits :** `docs/ROLE_PERMISSION_MATRIX.md` et `docs/ROLE_AUDIT_REPORT.md`.
- **Arrêt de sécurité :** Conformément à la consigne de la Section 3 du prompt (« Arrête-toi après cette phase et résume »), aucune correction de Phase 2 n'est entreprise avant confirmation de ces constats par l'utilisateur.
