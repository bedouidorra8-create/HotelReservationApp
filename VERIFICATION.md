# VERIFICATION.md — Audit Backend HotelReservationApp
> Généré le 2026-10-03 · Lecture seule · Exclus : module Images, Résumé IA

---

## VÉRIFICATION 1 : Rôles et permissions

| # | Point vérifié | Statut | Fichier + méthode | Remarque |
|---|---|---|---|---|
| 1.1 | Classe `Role` avec collection de permissions | ✅ conforme | `Role.cs` · `IReadOnlyCollection<string> Permissions` (L12) | Les permissions sont des strings libres (non un enum). Liste stockée dans `_permissions` (L7) |
| 1.2 | Entité ou enum `Permission` | ❌ absent | NON TROUVÉ | Aucune entité `Permission`, aucun enum `Permission`. La table `Roles` en BDD (migration `InitialCreate`) ne contient PAS de colonne Permissions ni de table de liaison `RolePermission` |
| 1.2b | Table `RolePermission` (FK, unicité) en migration | ❌ absent | NON TROUVÉ dans migrations | La collection `_permissions` n'est **pas mappée** dans `AppDbContext.cs` (L280-287) : aucun `OwnsMany` ni colonne JSON. **Bug critique : les permissions ne sont pas persistées en base.** |
| 1.3 | Action « ajouter permission à un rôle » (controller + use case + validator) | ⚠️ partiel | `AdministrationController.cs` `ManageRole` (L138) · `GererRolesUseCase.cs` `ExecuteAsync` (L17) · appelle `role.AjouterPermission()` (L33) | `POST /api/administration/roles` crée ou met à jour un rôle **en remplaçant les permissions** (pas un ajout unitaire). Pas de validator dédié `GererRolesValidator`. |
| 1.4 | Action « supprimer une permission d'un rôle » | ⚠️ partiel | `Role.cs` méthode `RetirerPermission` (L37) | Méthode domain existe. Mais **aucun use case** `SupprimerPermissionUseCase` ni endpoint dédié `DELETE /roles/{id}/permissions/{perm}`. |
| 1.5 | Seed des 3 rôles (Administrateur, Agent, Client) | ❌ absent | NON TROUVÉ | Aucun `HasData`, `InsertData` ni appel de seed dans les 4 migrations. Les 3 rôles ne sont pas seedés en base. |
| 1.6 | Rôle dans le JWT (`TokenService`, `ClaimTypes.Role`) | ✅ conforme | `TokenService.cs` `GenererJwt` (L20-44) · `new Claim(ClaimTypes.Role, role)` (L33) | Seul le **rôle** est dans le JWT. Les permissions **ne sont pas** dans le JWT (impossible puisqu'elles ne sont pas persistées). |
| 1.7a | Actions SANS `[Authorize]` — AuthController | ✅ conforme (voulu) | `AuthController.cs` | `inscription`, `connexion`, `rafraichir-token`, `mot-de-passe-oublie`, `reinitialiser-mot-de-passe`, `verifier-email` sans `[Authorize]` — normal. |
| 1.7b | Actions publiques — HotelsController | ⚠️ à documenter | `HotelsController.cs` L50, L60, L70, L143 | `GET /hotels`, `GET /hotels/{id}`, `GET /hotels/recherche`, `GET /hotels/{id}/disponibilites` sont anonymes. Choix intentionnel (vitrine publique), mais non documenté. |
| 1.7c | Actions publiques — ChambresController | ⚠️ à vérifier | `ChambresController.cs` L69, L86, L161 | `GET /hotels/{id}/chambres`, `GET /hotels/{id}/chambres/disponibilites`, `GET /chambres/recherche` publiques sans `[Authorize]`. |
| 1.7d | Actions publiques — AvisController | ✅ conforme | `AvisController.cs` L107 | `GET /avis/hotel/{id}` public — conforme (lecture publique des avis validés). |
| 1.7e | Incohérence — PaiementsController GetFacture | ❌ problème | `PaiementsController.cs` `GetFacture` L77 | `[Authorize]` (sans rôle) → n'importe quel utilisateur connecté peut demander `GET /paiements/{reservationId}/facture`. **Aucun contrôle d'appartenance** dans cet endpoint. |
| 1.8 | Statistiques avancées réservées à l'Admin uniquement | ❌ non conforme | `DashboardController.cs` L13 | `[Authorize(Roles = "Administrateur,Agent")]` sur tout le controller. Les 7 statistiques avancées sont accessibles aux Agents aussi, pas réservées à l'Admin seul comme demandé. |

---

## VÉRIFICATION 2 : Calendrier de disponibilité

| # | Point vérifié | Statut | Fichier + méthode | Remarque |
|---|---|---|---|---|
| 2.1 | Table / DbSet / entité Calendar, Calendrier, Disponibilité | ✅ conforme | `AppDbContext.cs` | Aucune table Calendar. Conforme au cahier. |
| 2.2 | Endpoint renvoyant chambres + réservations (arrivée, départ, statut) pour un hôtel et une période | ✅ conforme | `ReservationsController.cs` `GetCalendrier` (L87) · route `GET /api/reservations/calendrier?hotelId=&dateDebut=&dateFin=` · DTO : `CalendrierReservationsResponse` (`CalendrierDtos.cs`) | `[Authorize(Roles = "Administrateur,Agent")]`. Retourne `ReservationCalendrierDto` (ChambreId, ChambreNumero, DateDebut, DateFin, Statut, NbPersonnes). |
| 2.3 | Endpoint disponibilités journalières (grille chambre × jours) | ✅ conforme | `ChambresController.cs` `GetDisponibilites` (L86) et `HotelsController.cs` `GetDisponibilites` (L143) · `ConsulterDisponibilitesUseCase.cs` | Retourne `DisponibilitesHotelResponse` avec `Dictionary<string, string> StatutParJour` (Libre/Occupee/Maintenance). **Doublon de route** entre les deux controllers. |
| 2.4 | Calcul état Occupée/Disponible : Angular ou backend | ✅ conforme (backend) | `ConsulterDisponibilitesUseCase.cs` L48-60 | L'état est calculé côté **backend** (attendu). Le frontend Angular consomme la grille déjà calculée. |

---

## VÉRIFICATION 3 : Statistiques

| # | Statistique | Statut | Fichier + méthode | Remarque |
|---|---|---|---|---|
| 3.1 | Réservations par mois | ✅ conforme | `ConsulterStatistiquesUseCase.cs` L77-81 | GroupBy Year/Month → `ReservationsParMoisDto` |
| 3.2 | Chiffre d'affaires mensuel | ✅ conforme | `ConsulterStatistiquesUseCase.cs` L84-89 | Filtre `StatutPaiement.Paye` → `ChiffreAffairesMensuelDto` |
| 3.3 | Hôtels les plus réservés | ✅ conforme | `ConsulterStatistiquesUseCase.cs` L92-101 | Jointure via chambreToHotelMap → `HotelPlusReserveDto` |
| 3.4 | Répartition des paiements par mode | ✅ conforme | `ConsulterStatistiquesUseCase.cs` L104-108 | GroupBy ModePaiement → `PaiementsParModeDto` |
| 3.5 | Nombre de nouveaux utilisateurs | ✅ conforme | `ConsulterStatistiquesUseCase.cs` L111-115 | GroupBy DateCreation.Year/Month → `NouveauxUtilisateursParMoisDto` |
| 3.6 | Taux d'occupation des hôtels | ✅ conforme | `ConsulterStatistiquesUseCase.cs` L118-126 | (chambresOccupees / totalChambres) * 100 → `TauxOccupationHotelDto`. ⚠️ Basé sur `StatutChambre.Occupee` (statut courant), pas sur les réservations actives. |
| 3.7 | Évolution des avis | ✅ conforme | `ConsulterStatistiquesUseCase.cs` L129-137 | Volume + note moyenne par mois → `EvolutionAvisDto` |
| 3.8 | Emplacement des statistiques | ✅ conforme | `DashboardController.cs` `GetStatistiques` (L48) | Les 7 stats dans `ConsulterStatistiquesUseCase`, via `GET /api/dashboard/statistiques`. Pas de `StatistiqueService` distinct (le use case fait le calcul directement). |
| 3.9 | `StatistiquesDto` contient les 7 indicateurs | ✅ conforme | `StatistiquesDtos.cs` L49-59 | 7 listes + DateDebut + DateFin. |
| 3.10 | Dashboard : 9 indicateurs présents dans `DashboardDto` | ✅ conforme | `DashboardDtos.cs` | `NombreTotalUtilisateurs`, `NombreHotels`, `NombreReservations`, `ReservationsDuJour`, `ChiffreAffaires`, `PaiementsEnAttente`, `DernieresReservations`, `GraphiqueReservationsParMois`, `GraphiqueRevenusParMois` — les 9 sont présents. |
| 3.11 | Performance : chargement total en mémoire pour les stats | ⚠️ alerte | `ConsulterStatistiquesUseCase.cs` L54-58 | `GetAllAsync()` charge toutes les entités en mémoire. Non scalable sur un vrai volume de données. |

---

## VÉRIFICATION 4 : Éléments possiblement manquants

| Élément recherché | Statut | Fichier + méthode | Remarque |
|---|---|---|---|
| Repository des chambres (`IChambreRepository`) | ✅ trouvé (via agrégat) | `Chambre.cs` L9 · `IHotelRepository.cs` | Par design DDD : `Chambre` est entité dans l'agrégat `Hotel`. Toutes les opérations passent par `IHotelRepository.GetByIdAsync` puis manipulation de `hotel.Chambres`. Conforme. |
| Unit of Work / gestion de transaction | ❌ absent | NON TROUVÉ | Aucun `IUnitOfWork`, aucun `BeginTransaction`, aucun `UseTransaction`. EF Core `SaveChangesAsync` dans chaque repository appelé individuellement. Réservation + paiement = 2 `SaveChanges` séparés sans transaction englobante. **Risque d'incohérence.** |
| Génération facture PDF (bibliothèque + endpoint + contrôle accès) | ⚠️ partiel | `GenererFactureUseCase.cs` · `PaiementsController.cs` `GetFacture` (L76) | La facture est un DTO JSON (`FactureDto`), pas un PDF binaire. **Aucune bibliothèque PDF** (iText, QuestPDF…). `[Authorize]` sans contrôle d'appartenance. |
| Récupération utilisateur connecté (`ICurrentUser` ou `User.FindFirst`) | ✅ trouvé | `AuthController.cs` L116, `ReservationsController.cs` L66, `PaiementsController.cs` L39 | Pattern `User.FindFirstValue(ClaimTypes.NameIdentifier)` dans chaque controller. Pas de service `ICurrentUser` centralisé (duplication). Le filtrage « mes réservations » est fait **côté backend** dans `ConsulterMesReservationsUseCase` (L16). |
| Hash des mots de passe | ✅ conforme | `InscrireUtilisateurUseCase.cs` L38 · `SeConnecterUseCase.cs` L32 | `BCrypt.Net.BCrypt.HashPassword` + `BCrypt.Net.BCrypt.Verify`. |
| CRUD Agents + Administrateurs (controller) | ✅ conforme | `AdministrationController.cs` | `POST /agents`, `PUT /agents/{id}`, `DELETE /agents/{id}`, `PUT /administrateurs/{id}`, `DELETE /administrateurs/{id}`, `POST /utilisateurs`. |
| Recherche multicritère utilisateurs | ✅ conforme | `AdministrationController.cs` `SearchUsers` (L65) · `RechercherUtilisateurUseCase.cs` | Par `terme` (nom/email) + `role`. |
| Activer / désactiver un compte | ✅ conforme | `AdministrationController.cs` `SetAccountStatus` (L121) · `ActiverDesactiverCompteUseCase.cs` | `PATCH /api/administration/utilisateurs/{id}/actif`. |
| Référence unique de paiement | ⚠️ partiel | `Paiement.cs` L25 · `EnregistrerPaiementUseCase.cs` L56 | La référence est fournie par le client dans `EnregistrerPaiementRequest.Reference`. **Non générée automatiquement** côté serveur. Pas de contrainte d'unicité en base sur `Paiement.Reference`. |
| Pagination côté serveur | ❌ absent | NON TROUVÉ | Aucune classe `PagedResult`, aucun paramètre `page`/`pageSize` dans les controllers ou repositories. Tous les `GetAllAsync()` chargent en mémoire. |

---

## VÉRIFICATION 5 : Use Cases (couche Application)

### 5.1 — Organisation des use cases

Les use cases sont des **classes applicatives simples** dans `HotelReservation.Application/UseCases/`. **Pas de MediatR Commands/Queries/Handlers** : chaque use case est une classe avec une méthode `ExecuteAsync()`. MediatR est présent dans les dépendances mais utilisé uniquement pour les **Domain Events** (`DomainEventNotification<T>`, `AppDbContext.cs`).

### 5.2 — Inventaire réel par module (59 use cases)

| Module | Use Case | Fichier |
|---|---|---|
| **Auth** | InscrireUtilisateur | `InscrireUtilisateurUseCase.cs` |
| **Auth** | SeConnecter | `SeConnecterUseCase.cs` |
| **Auth** | RafraichirToken | `RafraichirTokenUseCase.cs` |
| **Auth** | Deconnexion | `DeconnexionUseCase.cs` |
| **Auth** | ChangerMotDePasse | `ChangerMotDePasseUseCase.cs` |
| **Auth** | DemanderResetPassword | `DemanderResetPasswordUseCase.cs` |
| **Auth** | ReinitialiserMotDePasse | `ReinitialiserMotDePasseUseCase.cs` |
| **Auth** | VerifierEmail | `VerifierEmailUseCase.cs` |
| **Rôles** | GererRoles (GET all + POST create/update) | `GererRolesUseCase.cs` |
| **Utilisateurs** | AjouterUtilisateur | `AjouterUtilisateurUseCase.cs` |
| **Utilisateurs** | ModifierAgent | `ModifierAgentUseCase.cs` |
| **Utilisateurs** | SupprimerAgent | `SupprimerAgentUseCase.cs` |
| **Utilisateurs** | ModifierAdministrateur | `ModifierAdministrateurUseCase.cs` |
| **Utilisateurs** | SupprimerAdministrateur | `SupprimerAdministrateurUseCase.cs` |
| **Utilisateurs** | ActiverDesactiverCompte | `ActiverDesactiverCompteUseCase.cs` |
| **Utilisateurs** | ConsulterListeUtilisateurs | `ConsulterListeUtilisateursUseCase.cs` |
| **Utilisateurs** | RechercherUtilisateur | `RechercherUtilisateurUseCase.cs` |
| **Clients** | CreerClient | `CreerClientUseCase.cs` |
| **Clients** | ModifierClient | `ModifierClientUseCase.cs` |
| **Clients** | SupprimerClient | `SupprimerClientUseCase.cs` |
| **Clients** | ConsulterListeClients | `ConsulterListeClientsUseCase.cs` |
| **Clients** | RechercherClient | `RechercherClientUseCase.cs` |
| **Hôtels** | CreerHotel | `CreerHotelUseCase.cs` |
| **Hôtels** | ModifierHotel | `ModifierHotelUseCase.cs` |
| **Hôtels** | SupprimerHotel | `SupprimerHotelUseCase.cs` |
| **Hôtels** | ConsulterHotels (GetAll + GetById) | `ConsulterHotelsUseCase.cs` |
| **Hôtels** | RechercherHotels | `RechercherHotelsUseCase.cs` |
| **Hôtels** | ActiverDesactiverHotel | `ActiverDesactiverHotelUseCase.cs` |
| **Chambres** | CreerChambre | `CreerChambreUseCase.cs` |
| **Chambres** | ModifierChambre | `ModifierChambreUseCase.cs` |
| **Chambres** | SupprimerChambre | `SupprimerChambreUseCase.cs` |
| **Chambres** | ConsulterChambres | `ConsulterChambresUseCase.cs` |
| **Chambres** | RechercherChambre | `RechercherChambreUseCase.cs` |
| **Chambres** | ChangerStatutChambre | `ChangerStatutChambreUseCase.cs` |
| **Chambres** | ConsulterDisponibilites | `ConsulterDisponibilitesUseCase.cs` |
| **Calendrier** | ConsulterCalendrierReservations | `ConsulterCalendrierReservationsUseCase.cs` |
| **Réservations** | CreerReservation | `CreerReservationUseCase.cs` |
| **Réservations** | ModifierReservation | `ModifierReservationUseCase.cs` |
| **Réservations** | AnnulerReservation | `AnnulerReservationUseCase.cs` |
| **Réservations** | ConfirmerReservation | `ConfirmerReservationUseCase.cs` |
| **Réservations** | ChangerStatutReservation | `ChangerStatutReservationUseCase.cs` |
| **Réservations** | ConsulterListeReservations | `ConsulterListeReservationsUseCase.cs` |
| **Réservations** | RechercherReservation | `RechercherReservationUseCase.cs` |
| **Historique** | ConsulterMesReservations | `ConsulterMesReservationsUseCase.cs` |
| **Favoris** | AjouterFavori | `AjouterFavoriUseCase.cs` |
| **Favoris** | SupprimerFavori | `SupprimerFavoriUseCase.cs` |
| **Favoris** | ConsulterFavoris | `ConsulterFavorisUseCase.cs` |
| **Paiements** | EnregistrerPaiement | `EnregistrerPaiementUseCase.cs` |
| **Paiements** | AnnulerPaiement | `AnnulerPaiementUseCase.cs` |
| **Paiements** | ConsulterPaiements | `ConsulterPaiementsUseCase.cs` |
| **Paiements** | GenererFacture (JSON, pas PDF) | `GenererFactureUseCase.cs` |
| **Avis** | AjouterAvis | `AjouterAvisUseCase.cs` |
| **Avis** | ModifierAvis | `ModifierAvisUseCase.cs` |
| **Avis** | SupprimerAvis | `SupprimerAvisUseCase.cs` |
| **Avis** | ValiderAvis | `ValiderAvisUseCase.cs` |
| **Avis** | RejeterAvis | `RejeterAvisUseCase.cs` |
| **Dashboard** | ConsulterDashboard | `ConsulterDashboardUseCase.cs` |
| **Statistiques** | ConsulterStatistiques | `ConsulterStatistiquesUseCase.cs` |
| **Audit Log** | ConsulterAuditLogs | `ConsulterAuditLogsUseCase.cs` |

### 5.3 — Comparaison avec les 75 use cases attendus

| Module | Use cases attendus | Statut |
|---|---|---|
| **Auth** | Register ✅ · Login ✅ · RefreshToken ✅ · Logout ✅ · **GetMyProfile ❌** · **UpdateMyProfile ❌** · ChangePassword ✅ | ⚠️ 2 manquants |
| **Rôles** | **AddPermissionToRole ❌** · **RemovePermissionFromRole ❌** | ❌ 2 manquants (GererRoles remplace en bloc) |
| **Utilisateurs** | Create ✅ · Update ✅ · Delete ✅ · GetById ✅ · Search ✅ · Activate ✅ · Deactivate ✅ · **ChangeRole ❌** | ⚠️ 1 manquant |
| **Hôtels** | Create ✅ · Update ✅ · Delete ✅ · GetById ✅ · Search ✅ · Activate ✅ · Deactivate ✅ | ✅ complet |
| **Chambres** | Create ✅ · Update ✅ · Delete ✅ · GetById ✅ · Search ✅ · GetAvailable ✅ · ChangeStatus ✅ · **Activate ❌** · **Deactivate ❌** | ⚠️ 2 manquants (pas de statut Actif/Inactif chambre) |
| **Calendrier** | GetRoomsCalendar ✅ | ✅ complet |
| **Réservations** | Create ✅ · Update ✅ · Cancel ✅ · GetById ✅ · Search ✅ · ChangeStatus ✅ | ✅ complet |
| **Historique** | GetMyReservations ✅ (avec filtre statut) · **GetMyReservationDetails ❌** | ⚠️ 1 manquant |
| **Favoris** | Add ✅ · Remove ✅ · GetMyFavorites ✅ | ✅ complet |
| **Paiements** | Record ✅ · Cancel ✅ · GetById ✅ · Search ✅ · GetMyPayments ✅ · **DownloadInvoicePdf ❌** (JSON seulement) | ⚠️ 1 manquant |
| **Avis** | Add ✅ · Update ✅ · Delete ✅ · **GetById ❌** · GetHotelReviews ✅ · **GetMyReviews ❌** · **GetPending ❌** · Approve ✅ · Reject ✅ | ⚠️ 3 manquants |
| **Dashboard** | Summary ✅ · TodayReservations ✅ · PendingPayments ✅ · LatestReservations ✅ · MonthlyReservationsChart ✅ · MonthlyRevenueChart ✅ | ✅ complet (1 endpoint consolidé) |
| **Statistiques** | 7 indicateurs ✅ | ✅ complet |
| **Audit Log** | GetAuditLogs ✅ · **GetAuditLogById ❌** | ⚠️ 1 manquant |

**Use cases manquants (liste consolidée) : 13**

`GetMyProfile`, `UpdateMyProfile`, `AddPermissionToRole`, `RemovePermissionFromRole`, `ChangeRole`, `ActivateChambre`, `DeactivateChambre`, `GetMyReservationDetails`, `DownloadInvoicePdf`, `GetAvisById`, `GetMyReviews`, `GetPendingReviews`, `GetAuditLogById`

**Use cases en trop (non demandés) : 4**

`DemanderResetPassword`, `ReinitialiserMotDePasse`, `VerifierEmail` (utiles mais hors spec initiale), `ConsulterDisponibilites` (doublon de `GetRoomsCalendar`).

---

## VÉRIFICATION 6 : Enums et entités

### 6.1 — Inventaire des enums réels

| Enum | Valeurs réelles | Conformité vs cahier |
|---|---|---|
| `StatutReservation` | `EnAttente=1, Confirmee=2, Annulee=3, Terminee=4` | ✅ conforme |
| `StatutChambre` | `Libre=1, Occupee=2, Maintenance=3` | ⚠️ partiel — cahier dit « Disponible » mais code dit `Libre` |
| `ModePaiement` | `Carte=1, Especes=2, Virement=3, Cheque=4` | ⚠️ partiel — cahier liste Carte/Espèces/Virement. `Cheque=4` est **en plus** |
| `StatutPaiement` | `EnAttente=1, Paye=2, Rembourse=3, Echoue=4, Annule=5` | ⚠️ partiel — cahier dit (En attente, Payé, Remboursé). `Echoue=4` et `Annule=5` **en plus** |
| `StatutAvis` | `EnAttente=1, Valide=2, Rejete=3` | ✅ conforme |
| `TypeChambre` | `Simple=1, Double=2, Suite=3, Familiale=4` | ✅ conforme |
| `StatutUtilisateur` | `Actif=1, Desactive=2` | ✅ conforme |
| `StatutHotel` | `Actif=1, Inactif=2` | ✅ conforme |
| `Gender` / `Sexe` | NON TROUVÉ comme enum | ❌ absent — `Sexe` est `string` dans `Utilisateur.cs` (L15) |
| `AuditAction` | NON TROUVÉ comme enum | ❌ absent — `Action` dans `AuditLog.cs` (L8) est `string` |

### 6.2 — Hiérarchie des entités du Domain

| Entité | Statut | Fichier | Remarque |
|---|---|---|---|
| `Utilisateur` (abstrait) | ✅ conforme | `Utilisateur.cs` | `abstract class Utilisateur : AggregateRoot<Guid>` |
| `Client : Utilisateur` | ✅ conforme | `Client.cs` | — |
| `Personnel : Utilisateur` (abstrait) | ✅ conforme | `Personnel.cs` | Propriété `Matricule` |
| `Agent : Personnel` | ✅ conforme | `Agent.cs` | — |
| `Administrateur : Personnel` | ✅ conforme | `Administrateur.cs` | — |
| `Role` | ✅ conforme | `Role.cs` | `AggregateRoot<Guid>`, `_permissions: List<string>` **non persistée** |
| `Hotel` | ✅ conforme | `Hotel.cs` | `AggregateRoot<Guid>`, contient `Chambres` |
| `Chambre` | ✅ conforme | `Chambre.cs` | `Entity<Guid>` dans agrégat Hotel |
| `Reservation` | ✅ conforme | `Reservation.cs` | `AggregateRoot<Guid>`, prix figé à la création |
| `Paiement` | ✅ conforme | `Paiement.cs` | `AggregateRoot<Guid>` |
| `Avis` | ✅ conforme | `Avis.cs` | UNIQUE(ClientId, ReservationId) en BDD (`AppDbContext.cs` L265) |
| `Favori` | ✅ conforme | `Favori.cs` | `Entity<Guid>`, UNIQUE(UtilisateurId, HotelId) en BDD (`AppDbContext.cs` L309) |
| `AuditLog` | ✅ conforme | `AuditLog.cs` | `Entity<Guid>` |
| `RefreshToken` | ✅ conforme | `RefreshToken.cs` | — |

---

## SYNTHÈSE FINALE

### A) Ce qui MANQUE réellement

| # | Manque | Sévérité |
|---|---|---|
| M1 | **Persistance des permissions du Role** : `_permissions` non mappée en base (ni JSON, ni table). Les permissions ne sont **jamais sauvegardées**. | 🔴 Critique |
| M2 | **Use cases `AddPermissionToRole` / `RemovePermissionFromRole`** distincts (GererRoles remplace en bloc, sans endpoint unitaire). | 🔴 Critique |
| M3 | **Seed des 3 rôles** (Administrateur, Agent, Client) : aucune migration de données initiales. | 🔴 Critique |
| M4 | **Unit of Work** : pas de transaction englobante. Réservation + paiement = 2 `SaveChangesAsync` sans garantie atomique. | 🔴 Critique |
| M5 | **`GetMyProfile` / `UpdateMyProfile`** : aucun endpoint `/api/auth/profil`. | 🟠 Important |
| M6 | **`DownloadInvoicePdf`** : la facture est un JSON, pas un PDF. Aucune bibliothèque PDF intégrée. | 🟠 Important |
| M7 | **Contrôle d'accès sur `GetFacture`** : `[Authorize]` sans vérification d'appartenance au client. | 🟠 Important |
| M8 | **`ChangeRole`** : pas de use case pour changer le type (rôle) d'un utilisateur existant. | 🟠 Important |
| M9 | **Pagination côté serveur** : aucun `PagedResult`. Tous les `GetAllAsync()` chargent en mémoire. | 🟠 Important |
| M10 | **Enum `Gender`** : `Sexe` est un `string` libre, pas un enum typé. | 🟡 Mineur |
| M11 | **Enum `AuditAction`** : `Action` dans `AuditLog` est un `string`, pas un enum. | 🟡 Mineur |
| M12 | **`GetMyReservationDetails`** : pas de use case pour détail d'une réservation du client. | 🟡 Mineur |
| M13 | **`GetAvisById`, `GetMyReviews`, `GetPendingReviews`** : 3 use cases manquants. | 🟡 Mineur |
| M14 | **`GetAuditLogById`** : pas de `GET /api/audit-logs/{id}`. | 🟡 Mineur |
| M15 | **Référence unique paiement** : fournie par le client, non générée serveur, pas de contrainte d'unicité en BDD. | 🟡 Mineur |
| M16 | **`ICurrentUser` centralisé** : `User.FindFirstValue(ClaimTypes.NameIdentifier)` dupliqué dans 5+ controllers. | 🟡 Mineur |
| M17 | **Performance statistiques** : `GetAllAsync()` en mémoire sur toutes les entités. | 🟡 Mineur |

### B) Ce qui est EN TROP (non demandé dans le cahier)

| # | Élément en trop | Remarque |
|---|---|---|
| E1 | `DemanderResetPassword` + `ReinitialiserMotDePasse` : flux reset email hors spec initiale | Acceptable |
| E2 | `VerifierEmail` : vérification email hors spec | Acceptable |
| E3 | `ModePaiement.Cheque = 4` : le cahier liste Carte/Espèces/Virement seulement | Mineur |
| E4 | `StatutPaiement.Echoue` et `Annule` : le cahier liste En attente / Payé / Remboursé | Mineur |
| E5 | Doublon de routes disponibilités : `GET /hotels/{id}/chambres/disponibilites` et `GET /hotels/{id}/disponibilites` font la même chose | Refactoring conseillé |
| E6 | CRUD client dans `ClientsController` partiellement en doublon avec `AdministrationController` | Refactoring conseillé |

### C) Plan de correction priorisé

#### 🔴 Priorité 1 — Bloquant / Critique

1. **M1** — Persister les permissions du Role : ajouter `OwnsMany<string>` ou une table `RolePermissions` dans `AppDbContext.OnModelCreating`, créer une migration.
2. **M2** — Use cases unitaires `AddPermissionToRole` / `RemovePermissionFromRole` avec endpoints `POST /roles/{id}/permissions` et `DELETE /roles/{id}/permissions/{perm}`.
3. **M3** — Seed des rôles : ajouter une migration de données ou `HasData` pour les 3 rôles avec leurs permissions par défaut.
4. **M4** — Unit of Work : implémenter `IUnitOfWork` ou `IDbContextTransaction` pour envelopper Réservation + Paiement dans une transaction atomique.

#### 🟠 Priorité 2 — Important / Fonctionnel

5. **M5** — `GetMyProfile` / `UpdateMyProfile` : endpoints `GET /api/auth/profil` et `PUT /api/auth/profil`.
6. **M6** — Facture PDF : intégrer QuestPDF ou iText7, endpoint `GET /api/paiements/{id}/facture/pdf` retournant `application/pdf`.
7. **M7** — Contrôle accès facture : vérifier que `reservation.ClientId == userId` ou que le demandeur est Agent/Admin.
8. **M8** — `ChangeRole` : use case `ChangerRoleUtilisateurUseCase` + `PATCH /api/administration/utilisateurs/{id}/role`.
9. **M9** — Pagination : créer `PagedResult<T>` + `PaginationParams`, ajouter `page`/`pageSize` aux endpoints de liste.

#### 🟡 Priorité 3 — Amélioration / Qualité

10. **M10** — Enum `Genre` : remplacer `string Sexe` par `enum Genre { Homme, Femme, NonBinaire, NonRenseigne }`.
11. **M11** — Enum `AuditAction` : créer `enum AuditAction { Connexion, Creation, Modification, Suppression, Validation, ... }` et typer `AuditLog.Action`.
12. **M12/M13/M14** — Use cases manquants : `GetMyReservationDetails`, `GetAvisById`, `GetMyReviews`, `GetPendingReviews`, `GetAuditLogById`.
13. **M15** — Référence paiement : générer automatiquement `Reference = $"PAY-{DateTime.UtcNow:yyyyMMddHHmmss}-{shortGuid}"` + index unique en base.
14. **M16** — `ICurrentUser` : créer `ICurrentUserService` injecté en Scoped pour centraliser `CurrentUserId`.
15. **M17** — Performance stats : migrer les statistiques vers des requêtes LINQ-to-SQL (`GroupBy` EF Core) au lieu de charger toutes les entités en mémoire.
16. **E5** — Doublon routes disponibilités : supprimer l'une des deux routes.
