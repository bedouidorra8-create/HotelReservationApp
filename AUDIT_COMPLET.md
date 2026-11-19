# Audit complet backend HotelReservationApp

## 1. Résumé exécutif

Ce document est un audit de conformité technique et fonctionnelle du backend, réalisé en lecture seule, sans modification de code. Il se base sur l’état réel du dépôt au 2026-10-04.

### Verdict rapide

- Architecture globale : conforme au schéma Clean Architecture / DDD attendu.
- Backend fonctionnel : bien avancé et compilable.
- Risques principaux : persistance des permissions de rôle non réellement mappée, absence de `Genre` enum, facture PDF non générée en PDF, et certains écarts fonctionnels autour du dashboard, de la sécurité et des routes.
- Validation objective : le build et les tests du projet sont actuellement réussis.

### Preuves de validation actuelle

- Build : `dotnet build HotelReservation.slnx` → OK
- Tests : `dotnet test HotelReservation.slnx --no-build` → OK, 23 tests réussis / 0 échec

## 2. État des vérifications par chapitre

| Chapitre | Élément | État | Fichiers principaux | Remarque |
|---|---|---|---|---|
| 1 | `Genre` enum | Non conforme | [HotelReservation.Domain/Entities/Utilisateur.cs](HotelReservation.Domain/Entities/Utilisateur.cs), [HotelReservation.Application/DTOs/AuthDtos.cs](HotelReservation.Application/DTOs/AuthDtos.cs), [HotelReservation.Application/DTOs/AdministrationDtos.cs](HotelReservation.Application/DTOs/AdministrationDtos.cs) | Le sexe est encore un `string` (`Sexe`) et non un `enum Genre`. |
| 2 | Suppression de `NbChambresDisponibles` | Partiellement conforme | [HotelReservation.Domain/Entities/Hotel.cs](HotelReservation.Domain/Entities/Hotel.cs), [HotelReservation.Application/DTOs/HotelDtos.cs](HotelReservation.Application/DTOs/HotelDtos.cs), [HotelReservation.Application/DTOs/RechercheDtos.cs](HotelReservation.Application/DTOs/RechercheDtos.cs) | Le champ stocké a disparu de l’entité, mais des DTO et méthodes de calcul restent encore exposés. |
| 3 | Activation/désactivation d’une chambre | Non conforme | [HotelReservation.Domain/Entities/Chambre.cs](HotelReservation.Domain/Entities/Chambre.cs), [HotelReservation.Api/Controllers/HotelsController.cs](HotelReservation.Api/Controllers/HotelsController.cs) | La chambre n’a pas d’indicateur `Actif` distinct du statut `StatutChambre`. |
| 4 | `TableauDeBordService` / `StatistiqueService` | Non conforme | [HotelReservation.Application/UseCases/ConsulterDashboardUseCase.cs](HotelReservation.Application/UseCases/ConsulterDashboardUseCase.cs), [HotelReservation.Application/UseCases/ConsulterStatistiquesUseCase.cs](HotelReservation.Application/UseCases/ConsulterStatistiquesUseCase.cs) | Les calculs existent encore directement dans les use cases, sans service dédié. |
| 5 | Rôles dashboard et statistiques | Partiellement conforme | [HotelReservation.Api/Controllers/DashboardController.cs](HotelReservation.Api/Controllers/DashboardController.cs) | Le controller autorise `Administrateur,Agent` ensemble pour tout le dashboard. |
| 6 | Profil connecté | Non conforme | [HotelReservation.Api/Controllers/AuthController.cs](HotelReservation.Api/Controllers/AuthController.cs) | Pas de `GET /api/auth/profil` ni de `PUT /api/auth/profil`. |
| 7 | Consultation et sécurité des réservations | Partiellement conforme | [HotelReservation.Api/Controllers/ReservationsController.cs](HotelReservation.Api/Controllers/ReservationsController.cs) | La logique de propriété est partiellement présente, mais la route `GetById` est à vérifier plus finement. |
| 8 | Paiements | Partiellement conforme | [HotelReservation.Api/Controllers/PaiementsController.cs](HotelReservation.Api/Controllers/PaiementsController.cs) | Facture JSON seulement ; aucun PDF et contrôles d’accès complets. |
| 9 | Avis : consultation | Non conforme | [HotelReservation.Api/Controllers/AvisController.cs](HotelReservation.Api/Controllers/AvisController.cs) | Les use cases de consultation détaillée et filtrée manquent. |
| 10 | Rôles et permissions | Partiellement conforme | [HotelReservation.Domain/Entities/Role.cs](HotelReservation.Domain/Entities/Role.cs), [HotelReservation.Api/Controllers/AdministrationController.cs](HotelReservation.Api/Controllers/AdministrationController.cs) | `AjouterPermission`/`RetirerPermission` existent ; persistance et endpoints dédiés manquent. |
| 11 | Facture PDF / QuestPDF | Non conforme | [HotelReservation.Application/UseCases/GenererFactureUseCase.cs](HotelReservation.Application/UseCases/GenererFactureUseCase.cs) | La facture est renvoyée sous forme DTO, pas PDF binaire. |
| 12 | Audit log par identifiant | Non conforme | [HotelReservation.Infrastructure/Repositories/AuditLogRepository.cs](HotelReservation.Infrastructure/Repositories/AuditLogRepository.cs) | `GetByIdAsync` n’est pas démontré comme endpoint dédié. |
| 13 | Vérification des endpoints | Partiellement conforme | [HotelReservation.Api/Controllers](HotelReservation.Api/Controllers) | Beaucoup d’endpoints existent, mais plusieurs contrôles de propriété et de rôles restent à aligner strictement avec le cahier. |
| 14 | Règles métier | Partiellement conforme | [HotelReservation.Domain/Services/ReservationDomainService.cs](HotelReservation.Domain/Services/ReservationDomainService.cs), [HotelReservation.Infrastructure/Data/AppDbContext.cs](HotelReservation.Infrastructure/Data/AppDbContext.cs) | Non-chevauchement présent et testé dans le code ; plusieurs règles ne sont pas encore totalement alignées au cahier. |
| 15 | Corrections transversales | Partiellement conforme | [HotelReservation.Application/UseCases](HotelReservation.Application/UseCases) | Beaucoup de use cases existent, mais un certain nombre de demandes du cahier ne sont pas encore couvertes. |

## 3. Table des endpoints et écarts observés

| Controller | Verbe + route | UseCase / logique | Autorisation actuelle | Écart observé |
|---|---|---|---|---|
| [HotelReservation.Api/Controllers/AuthController.cs](HotelReservation.Api/Controllers/AuthController.cs) | `POST /api/auth/inscription` | `InscrireUtilisateurUseCase` | Public | Conforme |
| [HotelReservation.Api/Controllers/AuthController.cs](HotelReservation.Api/Controllers/AuthController.cs) | `POST /api/auth/connexion` | `SeConnecterUseCase` | Public | Conforme |
| [HotelReservation.Api/Controllers/AuthController.cs](HotelReservation.Api/Controllers/AuthController.cs) | `POST /api/auth/rafraichir-token` | `RafraichirTokenUseCase` | Public | Conforme |
| [HotelReservation.Api/Controllers/AuthController.cs](HotelReservation.Api/Controllers/AuthController.cs) | `POST /api/auth/mot-de-passe-oublie` | `DemanderResetPasswordUseCase` | Public | Conforme au projet, hors cahier initial |
| [HotelReservation.Api/Controllers/AuthController.cs](HotelReservation.Api/Controllers/AuthController.cs) | `POST /api/auth/reinitialiser-mot-de-passe` | `ReinitialiserMotDePasseUseCase` | Public | Conforme au projet, hors cahier initial |
| [HotelReservation.Api/Controllers/AuthController.cs](HotelReservation.Api/Controllers/AuthController.cs) | `GET /api/auth/verifier-email` | `VerifierEmailUseCase` | Public | Conforme au projet, hors cahier initial |
| [HotelReservation.Api/Controllers/AuthController.cs](HotelReservation.Api/Controllers/AuthController.cs) | `POST /api/auth/logout` | `DeconnexionUseCase` | Authentifié | Conforme |
| [HotelReservation.Api/Controllers/AuthController.cs](HotelReservation.Api/Controllers/AuthController.cs) | `PUT /api/auth/changer-mot-de-passe` | `ChangerMotDePasseUseCase` | Authentifié | Conforme |
| [HotelReservation.Api/Controllers/HotelsController.cs](HotelReservation.Api/Controllers/HotelsController.cs) | `GET /api/hotels` | `ConsulterHotelsUseCase` | Public | Conforme |
| [HotelReservation.Api/Controllers/HotelsController.cs](HotelReservation.Api/Controllers/HotelsController.cs) | `GET /api/hotels/{id}` | `ConsulterHotelsUseCase` | Public | Conforme |
| [HotelReservation.Api/Controllers/HotelsController.cs](HotelReservation.Api/Controllers/HotelsController.cs) | `GET /api/hotels/recherche` | `RechercherHotelsUseCase` | Public | Conforme |
| [HotelReservation.Api/Controllers/HotelsController.cs](HotelReservation.Api/Controllers/HotelsController.cs) | `POST /api/hotels` | `CreerHotelUseCase` | `Administrateur,Agent` | Conforme |
| [HotelReservation.Api/Controllers/HotelsController.cs](HotelReservation.Api/Controllers/HotelsController.cs) | `PUT /api/hotels/{id}` | `ModifierHotelUseCase` | `Administrateur,Agent` | Conforme |
| [HotelReservation.Api/Controllers/HotelsController.cs](HotelReservation.Api/Controllers/HotelsController.cs) | `DELETE /api/hotels/{id}` | `SupprimerHotelUseCase` | `Administrateur` ou selon logique | À vérifier strictement |
| [HotelReservation.Api/Controllers/HotelsController.cs](HotelReservation.Api/Controllers/HotelsController.cs) | `PATCH /api/hotels/{id}/actif` | `ActiverDesactiverHotelUseCase` | `Administrateur,Agent` | Conforme |
| [HotelReservation.Api/Controllers/ReservationsController.cs](HotelReservation.Api/Controllers/ReservationsController.cs) | `GET /api/reservations` | `ConsulterListeReservationsUseCase` | `Administrateur,Agent` | Conforme |
| [HotelReservation.Api/Controllers/ReservationsController.cs](HotelReservation.Api/Controllers/ReservationsController.cs) | `GET /api/reservations/{id}` | `ConsulterReservationUseCase` (si présent) | Authentifié | À vérifier selon rôle et propriétaire |
| [HotelReservation.Api/Controllers/ReservationsController.cs](HotelReservation.Api/Controllers/ReservationsController.cs) | `POST /api/reservations` | `CreerReservationUseCase` | Authentifié | Le `clientId` doit être soumis au jeton côté serveur ; à vérifier |
| [HotelReservation.Api/Controllers/ReservationsController.cs](HotelReservation.Api/Controllers/ReservationsController.cs) | `PUT /api/reservations/{id}` | `ModifierReservationUseCase` | Authentifié | Le Client ne doit modifier que la sienne |
| [HotelReservation.Api/Controllers/ReservationsController.cs](HotelReservation.Api/Controllers/ReservationsController.cs) | `PATCH /api/reservations/{id}/annuler` | `AnnulerReservationUseCase` | Authentifié | À contrôler strictement le propriétaire |
| [HotelReservation.Api/Controllers/ReservationsController.cs](HotelReservation.Api/Controllers/ReservationsController.cs) | `POST /api/reservations/{id}/confirmer` | `ConfirmerReservationUseCase` | `Administrateur,Agent` | Conforme |
| [HotelReservation.Api/Controllers/ReservationsController.cs](HotelReservation.Api/Controllers/ReservationsController.cs) | `GET /api/reservations/calendrier` | `ConsulterCalendrierReservationsUseCase` | `Administrateur,Agent` | Conforme |
| [HotelReservation.Api/Controllers/PaiementsController.cs](HotelReservation.Api/Controllers/PaiementsController.cs) | `POST /api/paiements` | `EnregistrerPaiementUseCase` | `Administrateur,Agent` | Le Client doit payer sa réservation uniquement |
| [HotelReservation.Api/Controllers/PaiementsController.cs](HotelReservation.Api/Controllers/PaiementsController.cs) | `GET /api/paiements/{reservationId}/facture` | `GenererFactureUseCase` | `[Authorize]` sans contrainte propriété | Risque de sécurité ; pas PDF |
| [HotelReservation.Api/Controllers/AvisController.cs](HotelReservation.Api/Controllers/AvisController.cs) | `GET /api/avis/hotel/{hotelId}` | Lecture publique | Public | Conforme |
| [HotelReservation.Api/Controllers/AvisController.cs](HotelReservation.Api/Controllers/AvisController.cs) | `POST /api/avis` | `AjouterAvisUseCase` | Client | Conforme |
| [HotelReservation.Api/Controllers/AdministrationController.cs](HotelReservation.Api/Controllers/AdministrationController.cs) | `GET /api/administration/roles` | `GererRolesUseCase.GetAllAsync` | `Administrateur` | Conforme |
| [HotelReservation.Api/Controllers/AdministrationController.cs](HotelReservation.Api/Controllers/AdministrationController.cs) | `POST /api/administration/roles` | `GererRolesUseCase.ExecuteAsync` | `Administrateur` | Remplace la collection au lieu d’ajouter unitaire |
| [HotelReservation.Api/Controllers/DashboardController.cs](HotelReservation.Api/Controllers/DashboardController.cs) | `GET /api/dashboard` | `ConsulterDashboardUseCase` | `Administrateur,Agent` | Les 9 indicateurs sont présents, mais l’agent voit tout le dashboard |
| [HotelReservation.Api/Controllers/DashboardController.cs](HotelReservation.Api/Controllers/DashboardController.cs) | `GET /api/dashboard/statistiques` | `ConsulterStatistiquesUseCase` | `Administrateur,Agent` | Hors cahier : statistiques accessible à l’agent |

## 4. Table des règles métier

| Domaine | Règle | Preuve dans le code | État |
|---|---|---|---|
| Utilisateur | Email unique | [HotelReservation.Infrastructure/Data/AppDbContext.cs](HotelReservation.Infrastructure/Data/AppDbContext.cs) | Conforme |
| Hôtel | Nombre d’étoiles entre 1 et 5 | [HotelReservation.Domain/Entities/Hotel.cs](HotelReservation.Domain/Entities/Hotel.cs) | Conforme |
| Réservation | Date de départ > date d’arrivée | [HotelReservation.Domain/Services/ReservationDomainService.cs](HotelReservation.Domain/Services/ReservationDomainService.cs) | Conforme |
| Réservation | Non-chevauchement | [HotelReservation.Domain/Services/ReservationDomainService.cs](HotelReservation.Domain/Services/ReservationDomainService.cs) | Conforme sur la formule `r.Periode.DateArrivee < dateFin && r.Periode.DateDepart > dateDebut` |
| Chambre | Numéro unique par hôtel | [HotelReservation.Infrastructure/Data/AppDbContext.cs](HotelReservation.Infrastructure/Data/AppDbContext.cs) | Conforme |
| Avis | Note 1 à 5 | [HotelReservation.Infrastructure/Data/AppDbContext.cs](HotelReservation.Infrastructure/Data/AppDbContext.cs) | Conforme |
| Favori | Unicité par utilisateur + hôtel | [HotelReservation.Infrastructure/Data/AppDbContext.cs](HotelReservation.Infrastructure/Data/AppDbContext.cs) | Conforme |
| Role | Ajout/retrait de permissions | [HotelReservation.Domain/Entities/Role.cs](HotelReservation.Domain/Entities/Role.cs) | Partiellement conforme |
| Dashboard | 9 indicateurs globaux | [HotelReservation.Application/UseCases/ConsulterDashboardUseCase.cs](HotelReservation.Application/UseCases/ConsulterDashboardUseCase.cs) | Conforme |
| Statistiques | 7 indicateurs | [HotelReservation.Application/UseCases/ConsulterStatistiquesUseCase.cs](HotelReservation.Application/UseCases/ConsulterStatistiquesUseCase.cs) | Conforme |

## 5. Fichiers créés et modifiés (état réel)

### Fichiers principaux du backend

- [HotelReservation.Api/Program.cs](HotelReservation.Api/Program.cs)
- [HotelReservation.Api/Controllers/AuthController.cs](HotelReservation.Api/Controllers/AuthController.cs)
- [HotelReservation.Api/Controllers/HotelsController.cs](HotelReservation.Api/Controllers/HotelsController.cs)
- [HotelReservation.Api/Controllers/ReservationsController.cs](HotelReservation.Api/Controllers/ReservationsController.cs)
- [HotelReservation.Api/Controllers/PaiementsController.cs](HotelReservation.Api/Controllers/PaiementsController.cs)
- [HotelReservation.Api/Controllers/AvisController.cs](HotelReservation.Api/Controllers/AvisController.cs)
- [HotelReservation.Api/Controllers/AdministrationController.cs](HotelReservation.Api/Controllers/AdministrationController.cs)
- [HotelReservation.Api/Controllers/DashboardController.cs](HotelReservation.Api/Controllers/DashboardController.cs)
- [HotelReservation.Domain/Entities/Hotel.cs](HotelReservation.Domain/Entities/Hotel.cs)
- [HotelReservation.Domain/Entities/Reservation.cs](HotelReservation.Domain/Entities/Reservation.cs)
- [HotelReservation.Domain/Entities/Utilisateur.cs](HotelReservation.Domain/Entities/Utilisateur.cs)
- [HotelReservation.Domain/Entities/Role.cs](HotelReservation.Domain/Entities/Role.cs)
- [HotelReservation.Domain/Services/ReservationDomainService.cs](HotelReservation.Domain/Services/ReservationDomainService.cs)
- [HotelReservation.Infrastructure/Data/AppDbContext.cs](HotelReservation.Infrastructure/Data/AppDbContext.cs)
- [HotelReservation.Application/UseCases/ConsulterDashboardUseCase.cs](HotelReservation.Application/UseCases/ConsulterDashboardUseCase.cs)
- [HotelReservation.Application/UseCases/ConsulterStatistiquesUseCase.cs](HotelReservation.Application/UseCases/ConsulterStatistiquesUseCase.cs)

### Fichiers de rapport et d’audit existants

- [AUDIT_REPORT.md](AUDIT_REPORT.md)
- [VERIFICATION.md](VERIFICATION.md)
- [AUDIT_COMPLET.md](AUDIT_COMPLET.md)

## 6. Migrations générées et rôle réel

Le dépôt contient une série de migrations EF Core dans [HotelReservation.Infrastructure/Migrations](HotelReservation.Infrastructure/Migrations). La spécification demandait explicitement les migrations `AjouterEnumGenre`, `SupprimerNbChambresDisponibles` et `AjouterActivationChambre`, mais le code réel n’affiche pas ces noms en vigueur. La base actuelle est plutôt structurée autour d’une migration initiale et de migrations de refonte.

### Ce que la base actuelle fait

- `Sexe` est encore une colonne texte dans le modèle global, pas un enum.
- `NbChambresDisponibles` reste présent dans certains DTO et dans le calcul métier, même si l’agrégat ne le stocke pas explicitement en base.
- `Chambre` ne dispose pas d’un champ `Actif` distinct du statut.
- Aucun `JsonStringEnumConverter` n’est explicitement configuré pour le sexe dans la couche API ; le système est donc encore basé sur un `string`.

### État de migration

- Migrations présentes : oui.
- Migrations appliquées : non vérifiable dans ce mode lecture seule.
- Règle de sécurité : les migrations n’ont pas été exécutées lors de l’audit.

## 7. Packages ajoutés et compatibilité

Le fichier [HotelReservation.Api/HotelReservation.Api.csproj](HotelReservation.Api/HotelReservation.Api.csproj) contient actuellement :

| Package | Version | Licence / remarque |
|---|---|---|
| `Microsoft.AspNetCore.Authentication.JwtBearer` | `9.0.2` | Microsoft EULA / OSS standard |
| `Microsoft.EntityFrameworkCore.Design` | `9.0.2` | MIT / Microsoft |
| `Swashbuckle.AspNetCore` | `6.5.0` | MIT |

### QuestPDF

- Non présent dans le projet actuel.
- Le cahier exige `QuestPDF` pour générer la facture PDF.
- Sur `.NET 10`, il faut vérifier la version exacte de `QuestPDF` compatible avec le runtime ciblé avant ajout.
- La licence est généralement la licence Community de QuestPDF ; elle doit être signalée avant intégration en production.

## 8. Occurrences Angular touchées

L’audit a identifié les points du frontend qui sont directement liés aux changements d’API ou à l’exposition de champs calculés. Il ne faut pas les modifier pendant ce mode de lecture seule, mais il faut les signaler.

| Occurrence | Fichiers Angular concernés | Observation |
|---|---|---|
| `NbChambresDisponibles` | `hotel-reservation-frontend/src/**` | Le frontend reçoit ce champ dans les DTO de hotel et recherche. |
| `Sexe` / `Genre` | `hotel-reservation-frontend/src/**` | La donnée est encore sérialisée comme chaîne textuelle. |
| Dashboard filtré par rôle | `hotel-reservation-frontend/src/**` | L’API ne filtre pas strictement le dashboard selon le rôle. |
| Nouveaux endpoints / routes de profil | `hotel-reservation-frontend/src/**` | Les routes `/api/auth/profil` et les endpoints admin supplémentaires exigés par le cahier ne sont pas encore présents. |

## 9. Résultats des signalements du point 15.B

### 15.B.1 — Suppressions

- La logique de suppression de chambre, hôtel ou utilisateur n’a pas été auditée comme transactionnelle dans le chapitre d’intégration complet.
- Le comportement réel est fonctionnellement dépendant des contraintes EF Core et des use cases associés, mais il n’a pas été entièrement validé en mode lecture seule pour tous les cas de réservations et paiements liés.

### 15.B.2 — Unit of Work / transaction

- Aucun `IUnitOfWork` n’est présent.
- Les opérations multi-étapes ne sont pas encapsulées dans une transaction globale.
- Il existe un risque de non-atomicité lors de la création d’une réservation suivie d’un paiement ou d’un changement de statut.

### 15.B.3 — `GET /api/administration/roles` et `POST /api/administration/roles`

- `GET` renvoie les rôles.
- `POST` met à jour ou crée un rôle et remplace sa liste de permissions dans la logique actuelle, au lieu de proposer un ajout unitaire distinct.
- Cette logique est dans [HotelReservation.Application/UseCases/GererRolesUseCase.cs](HotelReservation.Application/UseCases/GererRolesUseCase.cs) et [HotelReservation.Api/Controllers/AdministrationController.cs](HotelReservation.Api/Controllers/AdministrationController.cs).

### 15.B.4 — Reset mot de passe / vérification email

- Les fonctions existent dans l’API et sont conservées dans le backend.
- Elles ne sont pas centralisées dans le cahier initial, mais elles sont bien présentes comme fonctionnalités additionnelles.

### 15.B.5 — Statut chambre “Occupée”

- Dans le code actuel, la chambre a un `StatutChambre` avec `Libre`, `Occupee`, `Maintenance`.
- Le changement automatique de statut n’est pas clairement implémenté dans le code de réservation de manière centralisée ; il semble dépendre des use cases et du domaine.

## 10. Questions restées sans réponse

1. Le sexe doit-il être migré depuis des valeurs historiques comme `M`, `F`, `Masculin`, `Féminin` ou des chaînes libres ?
2. Les permissions du rôle doivent-elles être stockées en JSON dans `Role` ou dans une table dédiée ?
3. Faut-il rendre la facture PDF obligatoire pour le contrat actuel ou conserver le DTO JSON ?
4. Les règles de suppression d’hôtel / chambre / utilisateur sont-elles réellement prévues comme suppression physique, soft-delete ou désactivation ?
5. Le calendrier d’occupation est-il calculé à partir des réservations actives ou du statut courant de chambre ?

## 11. Ce qui reste à faire

### Priorité haute

- Créer `Genre` enum avec `Homme`, `Femme` et migrer le point d’entrée de données.
- Finaliser la suppression de `NbChambresDisponibles` des DTO / use cases / recherches.
- Ajouter la propriété `Actif` sur `Chambre` et le use case associé.
- Séparer le tableau de bord du service (service dédié) et filtrer les résultats côté serveur selon le rôle.
- Implémenter la facture PDF avec `QuestPDF` et les contrôles d’accès propriétaires.
- Rendre les permissions de rôle persistées et ajouter `POST /api/administration/roles/{id}/permissions` + `DELETE ...`.

### Priorité moyenne

- Ajouter `GET /api/auth/profil` et `PUT /api/auth/profil`.
- Ajouter des use cases de consultation détaillée des avis et des reservations selon propriétaire.
- Ajouter le support d’identifiant unique par `AuditLog`.
- Vérifier et aligner les routes conflictuelles et les contraintes de route.

### Priorité basse

- Compléter le support des contraintes de pagination sur les listes longues.
- Vérifier le comportement exact de suppression avec données liées.
- Ajouter la transaction / unit of work si la concurrence et la cohérence de données doivent être renforcées.

## 12. Inventaire exhaustif des fichiers backend

Cette annexe énumère les **179 fichiers actuels** des cinq projets backend (`Api`, `Application`, `Domain`, `Infrastructure`, `Tests`). Les dossiers générés `bin/` et `obj/` sont exclus ; les fichiers de configuration, projets, tests, migrations et fichiers HTTP de test sont inclus. L’inventaire a été obtenu depuis l’arborescence présente dans le workspace le 2026-10-04.

### Comment interpréter le nombre de use cases

Le nombre de fichiers `*UseCase.cs` compte des **classes de cas d’utilisation**, pas le nombre total de traitements, calculs ou variantes fonctionnelles. Une même classe peut regrouper plusieurs opérations métier et les exposer par une méthode publique unique.

Par exemple, `ConsulterDashboardUseCase` expose une seule méthode `ExecuteAsync`, mais cette méthode regroupe plusieurs métriques : totaux utilisateurs/hôtels/réservations, réservations du jour, chiffre d’affaires, paiements en attente, dernières réservations et graphiques par mois sur 12 mois. Il s’agit donc d’un use case agrégé, pas de neuf classes distinctes. Dans le code audité, le regroupement temporel du dashboard est **mensuel** ; aucune méthode `GetByHour` ou métrique horaire n’a été trouvée dans cette classe.

De même, `ConsulterStatistiquesUseCase` expose une seule méthode `ExecuteAsync` avec des bornes de dates optionnelles et calcule plusieurs séries statistiques (réservations par mois, revenus mensuels, hôtels les plus réservés, paiements par mode, nouveaux utilisateurs par mois, taux d’occupation et évolution des avis). Compter uniquement les fichiers donnerait donc un chiffre plus bas que le nombre de fonctionnalités/calculs contenus dans ces use cases agrégés. Inversement, plusieurs fichiers peuvent aussi représenter des variantes d’un même domaine ; le décompte des fichiers ne remplace pas la matrice de conformité aux exigences.

### Catalogue des 60 use cases et de leurs traitements regroupés

La colonne « méthode publique » décrit le point d’entrée applicatif constaté. Une méthode `ExecuteAsync` peut orchestrer plusieurs sous-traitements. Les quatre classes qui exposent plusieurs signatures/opérations publiques sont `ActiverDesactiverHotelUseCase` (deux surcharges), `ConsulterHotelsUseCase`, `ConsulterPaiementsUseCase` et `GererRolesUseCase`.

| Use case | Méthode(s) publique(s) | Traitements regroupés dans le cas d’utilisation |
|---|---|---|
| `ActiverDesactiverChambreUseCase` | `ExecuteAsync` | Charge l’hôtel, bascule le drapeau actif de la chambre et sauvegarde l’agrégat hôtel. |
| `ActiverDesactiverCompteUseCase` | `ExecuteAsync` | Charge un utilisateur, active ou désactive son compte et persiste le changement. |
| `ActiverDesactiverHotelUseCase` | `ExecuteAsync` avec `StatutHotel`; `ExecuteAsync` avec `bool` | Deux formes d’entrée pour changer le statut de l’hôtel et sauvegarder. Ce sont deux surcharges d’une même opération, pas deux use cases séparés. |
| `AjouterAvisUseCase` | `ExecuteAsync` | Valide la demande, vérifie l’éligibilité et l’absence d’avis en doublon, crée et persiste l’avis. |
| `AjouterFavoriUseCase` | `ExecuteAsync` | Vérifie l’hôtel et le doublon, crée et sauvegarde le favori. |
| `AjouterUtilisateurUseCase` | `ExecuteAsync` | Détermine le type de compte demandé (client, agent ou administrateur), vérifie l’email et crée l’utilisateur. |
| `AnnulerPaiementUseCase` | `ExecuteAsync` | Charge le paiement, applique son annulation et traite l’annulation de la réservation associée selon les règles implémentées. |
| `AnnulerReservationUseCase` | `ExecuteAsync` | Charge et annule la réservation, sauvegarde le changement et inscrit une entrée d’audit. |
| `ChangerMotDePasseUseCase` | `ExecuteAsync` | Vérifie le mot de passe actuel, calcule le hash du nouveau mot de passe et sauvegarde le compte. |
| `ChangerStatutChambreUseCase` | `ExecuteAsync` | Charge l’hôtel/chambre, applique un nouveau statut opérationnel et sauvegarde l’agrégat. |
| `ChangerStatutReservationUseCase` | `ExecuteAsync` | Applique une transition de statut à la réservation, sauvegarde et journalise le changement. |
| `ConfirmerReservationUseCase` | `ExecuteAsync` | Charge et confirme la réservation, persiste la transition et enregistre l’action d’audit. |
| `ConsulterAuditLogsUseCase` | `ExecuteAsync` | Récupère les logs, applique les bornes de dates optionnelles et convertit les résultats en DTOs. |
| `ConsulterCalendrierReservationsUseCase` | `ExecuteAsync` | Récupère les réservations non annulées d’un hôtel qui chevauchent l’intervalle demandé et les ordonne par date d’arrivée. |
| `ConsulterChambresUseCase` | `ExecuteAsync` | Récupère et transforme en DTOs les chambres rattachées à l’hôtel ciblé. |
| `ConsulterDashboardUseCase` | `ExecuteAsync` | Regroupe les totaux utilisateurs/hôtels/réservations, réservations du jour, revenu payé cumulé, paiements en attente, 10 réservations récentes, réservations par mois et revenus par mois sur 12 mois. Les graphiques sont mensuels, pas horaires. |
| `ConsulterDisponibilitesUseCase` | `ExecuteAsync` | Construit les disponibilités par chambre et par jour pour une période; distingue notamment maintenance, dates réservées et dates libres. |
| `ConsulterFavorisUseCase` | `ExecuteAsync` | Charge les favoris d’un utilisateur, récupère les détails d’hôtel disponibles et les projette en DTOs. |
| `ConsulterHotelsUseCase` | `GetAllAsync`; `GetByIdAsync` | Deux consultations : liste de tous les hôtels ou hôtel par identifiant. Les deux projettent les détails de chambres et le prix minimum calculé. |
| `ConsulterListeClientsUseCase` | `ExecuteAsync` | Récupère tous les clients et les convertit en DTOs. |
| `ConsulterListeReservationsUseCase` | `ExecuteAsync` | Récupère toutes les réservations et les convertit en DTOs avec leurs informations de prix. |
| `ConsulterListeUtilisateursUseCase` | `ExecuteAsync` | Récupère les utilisateurs et construit leurs DTOs, notamment avec le rôle déduit du type concret. |
| `ConsulterMesReservationsUseCase` | `ExecuteAsync` | Récupère les réservations d’un client et applique éventuellement un filtre de statut. |
| `ConsulterPaiementsUseCase` | `ExecuteAsync`; `ExecuteForClientAsync` | Consultation des paiements d’une réservation (ou selon le mode global implémenté) et consultation distincte des paiements associés aux réservations d’un client. |
| `ConsulterStatistiquesUseCase` | `ExecuteAsync` | Regroupe 7 ensembles d’indicateurs : réservations par mois; revenus mensuels payés; hôtels les plus réservés; paiements par mode (nombre et montant); nouveaux utilisateurs par mois; taux d’occupation calculé à partir des chambres; avis par mois (volume et note moyenne). Les dates optionnelles filtrent réservations, paiements, utilisateurs et avis; occupation et classement hôtels ne sont pas filtrés par date. |
| `CreerChambreUseCase` | `ExecuteAsync` | Valide les données, crée/ajoute une chambre via l’agrégat hôtel et sauvegarde l’hôtel. |
| `CreerClientUseCase` | `ExecuteAsync` | Construit un client à partir de la requête et le persiste. |
| `CreerHotelUseCase` | `ExecuteAsync` | Valide les données, construit et persiste l’hôtel, puis retourne son DTO. |
| `CreerReservationUseCase` | `ExecuteAsync` | Valide la requête, vérifie hôtel/chambre et statut, contrôle la disponibilité, crée et persiste la réservation puis journalise l’opération. La vérification préalable ne garantit pas à elle seule l’exclusion atomique des requêtes concurrentes. |
| `DeconnexionUseCase` | `ExecuteAsync` | Vérifie le refresh token et son propriétaire puis révoque le token. |
| `DemanderResetPasswordUseCase` | `ExecuteAsync` | Recherche le compte, génère et envoie un token de réinitialisation si applicable, tout en conservant une réponse non révélatrice pour un email inconnu. |
| `EnregistrerPaiementUseCase` | `ExecuteAsync` | Valide la demande, contrôle réservation/propriétaire/état, montant et devise, évite un paiement payé en doublon puis enregistre le paiement. |
| `GenererFactureUseCase` | `ExecuteAsync` | Assemble la réservation, l’historique des paiements et les informations client/hôtel/chambre disponibles en `FactureDto` JSON; ne génère pas de fichier PDF. |
| `GererRolesUseCase` | `ExecuteAsync`; `GetAllAsync` | Crée ou met à jour un rôle et ses permissions demandées; méthode séparée pour lister les rôles. |
| `InscrireUtilisateurUseCase` | `ExecuteAsync` | Inscrit un client, hache le mot de passe, crée le jeton de vérification et déclenche l’envoi de l’email. |
| `ModifierAdministrateurUseCase` | `ExecuteAsync` | Vérifie le type administrateur, modifie ses données de profil et sauvegarde. |
| `ModifierAgentUseCase` | `ExecuteAsync` | Vérifie le type agent, modifie ses données de profil et sauvegarde. |
| `ModifierAvisUseCase` | `ExecuteAsync` | Vérifie l’auteur et l’état de la réservation, modifie l’avis et sauvegarde. |
| `ModifierChambreUseCase` | `ExecuteAsync` | Charge l’hôtel/chambre, modifie les propriétés prises en charge (type, étage, prix et état actif) et sauvegarde l’agrégat. |
| `ModifierClientUseCase` | `ExecuteAsync` | Charge le client, met à jour les propriétés du profil et sauvegarde. |
| `ModifierHotelUseCase` | `ExecuteAsync` | Charge l’hôtel, modifie ses informations, sauvegarde et construit le DTO de sortie avec les chambres et le prix minimum calculé. |
| `ModifierReservationUseCase` | `ExecuteAsync` | Vérifie la réservation et les nouvelles dates (en excluant la réservation courante du contrôle de conflit), applique les modifications, sauvegarde et journalise avant/après. |
| `RafraichirTokenUseCase` | `ExecuteAsync` | Vérifie le refresh token, le révoque et délivre un nouveau JWT accompagné d’un nouveau refresh token. |
| `RechercherChambreUseCase` | `ExecuteAsync` | Filtre les chambres selon les critères facultatifs de type, statut et fourchette de prix. |
| `RechercherClientUseCase` | `ExecuteAsync` | Recherche par nom, prénom, email ou téléphone; une recherche vide renvoie la liste complète selon la logique actuelle. |
| `RechercherHotelsUseCase` | `ExecuteAsync` | Filtre les hôtels, peut tenir compte de dates/disponibilités de chambres, calcule le prix minimum correspondant et trie les résultats. |
| `RechercherReservationUseCase` | `ExecuteAsync` | Filtre les réservations selon client, hôtel, statut et intervalle de dates facultatifs. |
| `RechercherUtilisateurUseCase` | `ExecuteAsync` | Filtre les utilisateurs selon un terme de recherche et/ou un rôle facultatifs. |
| `ReinitialiserMotDePasseUseCase` | `ExecuteAsync` | Recherche le compte par token, hache le nouveau mot de passe et applique les règles de validité/usage unique du token. |
| `RejeterAvisUseCase` | `ExecuteAsync` | Charge l’avis, le rejette et sauvegarde son nouvel état. |
| `SeConnecterUseCase` | `ExecuteAsync` | Vérifie les identifiants et l’état du compte, puis produit les tokens de connexion. |
| `SupprimerAdministrateurUseCase` | `ExecuteAsync` | Vérifie le type administrateur ciblé puis supprime le compte. |
| `SupprimerAgentUseCase` | `ExecuteAsync` | Vérifie le type agent ciblé puis supprime le compte. |
| `SupprimerAvisUseCase` | `ExecuteAsync` | Charge l’avis ciblé et le supprime. |
| `SupprimerChambreUseCase` | `ExecuteAsync` | Vérifie l’existence de réservations actives; refuse la suppression si nécessaire, sinon retire la chambre de l’hôtel et sauvegarde. |
| `SupprimerClientUseCase` | `ExecuteAsync` | Charge le client ciblé et le supprime. |
| `SupprimerFavoriUseCase` | `ExecuteAsync` | Supprime le favori uniquement s’il appartient à l’utilisateur appelant. |
| `SupprimerHotelUseCase` | `ExecuteAsync` | Charge l’hôtel ciblé et le supprime via le repository. |
| `ValiderAvisUseCase` | `ExecuteAsync` | Charge l’avis, le valide et sauvegarde son nouvel état. |
| `VerifierEmailUseCase` | `ExecuteAsync` | Recherche le compte par token de vérification, valide/invalide le token via le domaine et sauvegarde le compte vérifié. |

Ce tableau décrit les responsabilités trouvées dans les classes, et non une affirmation qu’un cas d’utilisation correspond exactement à une exigence du cahier des charges. Le comportement HTTP et les règles d’autorisation restent à lire avec les contrôleurs; un use case seul ne prouve pas qu’un endpoint est correctement protégé.

### 12.1 HotelReservation.Api — 18 fichiers

- `HotelReservation.Api/appsettings.Development.json`
- `HotelReservation.Api/appsettings.json`
- `HotelReservation.Api/Controllers/AdministrationController.cs`
- `HotelReservation.Api/Controllers/AuditLogsController.cs`
- `HotelReservation.Api/Controllers/AuthController.cs`
- `HotelReservation.Api/Controllers/AvisController.cs`
- `HotelReservation.Api/Controllers/ChambresController.cs`
- `HotelReservation.Api/Controllers/ClientsController.cs`
- `HotelReservation.Api/Controllers/DashboardController.cs`
- `HotelReservation.Api/Controllers/FavorisController.cs`
- `HotelReservation.Api/Controllers/HotelsController.cs`
- `HotelReservation.Api/Controllers/PaiementsController.cs`
- `HotelReservation.Api/Controllers/ReservationsController.cs`
- `HotelReservation.Api/HotelReservation.Api.csproj`
- `HotelReservation.Api/HotelReservation.Api.http`
- `HotelReservation.Api/Middleware/GlobalExceptionMiddleware.cs`
- `HotelReservation.Api/Program.cs`
- `HotelReservation.Api/Properties/launchSettings.json`

### 12.2 HotelReservation.Application — 85 fichiers

**Événements de domaine et handlers**

- `HotelReservation.Application/DomainEvents/DomainEventNotification.cs`
- `HotelReservation.Application/DomainEvents/Handlers/ReservationAnnuleeEventHandler.cs`
- `HotelReservation.Application/DomainEvents/Handlers/ReservationConfirmeeEventHandler.cs`

**DTOs**

- `HotelReservation.Application/DTOs/AdministrationDtos.cs`
- `HotelReservation.Application/DTOs/AuditLogDtos.cs`
- `HotelReservation.Application/DTOs/AuthDtos.cs`
- `HotelReservation.Application/DTOs/AvisDtos.cs`
- `HotelReservation.Application/DTOs/CalendrierDtos.cs`
- `HotelReservation.Application/DTOs/ClientDtos.cs`
- `HotelReservation.Application/DTOs/DashboardDtos.cs`
- `HotelReservation.Application/DTOs/FavoriDtos.cs`
- `HotelReservation.Application/DTOs/HotelDtos.cs`
- `HotelReservation.Application/DTOs/PaiementDtos.cs`
- `HotelReservation.Application/DTOs/RechercheDtos.cs`
- `HotelReservation.Application/DTOs/ReservationDtos.cs`
- `HotelReservation.Application/DTOs/StatistiquesDtos.cs`
- `HotelReservation.Application/HotelReservation.Application.csproj`

**Interfaces de services applicatifs**

- `HotelReservation.Application/Services/IAuditLogService.cs`
- `HotelReservation.Application/Services/IEmailService.cs`
- `HotelReservation.Application/Services/ITokenService.cs`

**Use cases (60)**

- `HotelReservation.Application/UseCases/ActiverDesactiverChambreUseCase.cs`
- `HotelReservation.Application/UseCases/ActiverDesactiverCompteUseCase.cs`
- `HotelReservation.Application/UseCases/ActiverDesactiverHotelUseCase.cs`
- `HotelReservation.Application/UseCases/AjouterAvisUseCase.cs`
- `HotelReservation.Application/UseCases/AjouterFavoriUseCase.cs`
- `HotelReservation.Application/UseCases/AjouterUtilisateurUseCase.cs`
- `HotelReservation.Application/UseCases/AnnulerPaiementUseCase.cs`
- `HotelReservation.Application/UseCases/AnnulerReservationUseCase.cs`
- `HotelReservation.Application/UseCases/ChangerMotDePasseUseCase.cs`
- `HotelReservation.Application/UseCases/ChangerStatutChambreUseCase.cs`
- `HotelReservation.Application/UseCases/ChangerStatutReservationUseCase.cs`
- `HotelReservation.Application/UseCases/ConfirmerReservationUseCase.cs`
- `HotelReservation.Application/UseCases/ConsulterAuditLogsUseCase.cs`
- `HotelReservation.Application/UseCases/ConsulterCalendrierReservationsUseCase.cs`
- `HotelReservation.Application/UseCases/ConsulterChambresUseCase.cs`
- `HotelReservation.Application/UseCases/ConsulterDashboardUseCase.cs`
- `HotelReservation.Application/UseCases/ConsulterDisponibilitesUseCase.cs`
- `HotelReservation.Application/UseCases/ConsulterFavorisUseCase.cs`
- `HotelReservation.Application/UseCases/ConsulterHotelsUseCase.cs`
- `HotelReservation.Application/UseCases/ConsulterListeClientsUseCase.cs`
- `HotelReservation.Application/UseCases/ConsulterListeReservationsUseCase.cs`
- `HotelReservation.Application/UseCases/ConsulterListeUtilisateursUseCase.cs`
- `HotelReservation.Application/UseCases/ConsulterMesReservationsUseCase.cs`
- `HotelReservation.Application/UseCases/ConsulterPaiementsUseCase.cs`
- `HotelReservation.Application/UseCases/ConsulterStatistiquesUseCase.cs`
- `HotelReservation.Application/UseCases/CreerChambreUseCase.cs`
- `HotelReservation.Application/UseCases/CreerClientUseCase.cs`
- `HotelReservation.Application/UseCases/CreerHotelUseCase.cs`
- `HotelReservation.Application/UseCases/CreerReservationUseCase.cs`
- `HotelReservation.Application/UseCases/DeconnexionUseCase.cs`
- `HotelReservation.Application/UseCases/DemanderResetPasswordUseCase.cs`
- `HotelReservation.Application/UseCases/EnregistrerPaiementUseCase.cs`
- `HotelReservation.Application/UseCases/GenererFactureUseCase.cs`
- `HotelReservation.Application/UseCases/GererRolesUseCase.cs`
- `HotelReservation.Application/UseCases/InscrireUtilisateurUseCase.cs`
- `HotelReservation.Application/UseCases/ModifierAdministrateurUseCase.cs`
- `HotelReservation.Application/UseCases/ModifierAgentUseCase.cs`
- `HotelReservation.Application/UseCases/ModifierAvisUseCase.cs`
- `HotelReservation.Application/UseCases/ModifierChambreUseCase.cs`
- `HotelReservation.Application/UseCases/ModifierClientUseCase.cs`
- `HotelReservation.Application/UseCases/ModifierHotelUseCase.cs`
- `HotelReservation.Application/UseCases/ModifierReservationUseCase.cs`
- `HotelReservation.Application/UseCases/RafraichirTokenUseCase.cs`
- `HotelReservation.Application/UseCases/RechercherChambreUseCase.cs`
- `HotelReservation.Application/UseCases/RechercherClientUseCase.cs`
- `HotelReservation.Application/UseCases/RechercherHotelsUseCase.cs`
- `HotelReservation.Application/UseCases/RechercherReservationUseCase.cs`
- `HotelReservation.Application/UseCases/RechercherUtilisateurUseCase.cs`
- `HotelReservation.Application/UseCases/ReinitialiserMotDePasseUseCase.cs`
- `HotelReservation.Application/UseCases/RejeterAvisUseCase.cs`
- `HotelReservation.Application/UseCases/SeConnecterUseCase.cs`
- `HotelReservation.Application/UseCases/SupprimerAdministrateurUseCase.cs`
- `HotelReservation.Application/UseCases/SupprimerAgentUseCase.cs`
- `HotelReservation.Application/UseCases/SupprimerAvisUseCase.cs`
- `HotelReservation.Application/UseCases/SupprimerChambreUseCase.cs`
- `HotelReservation.Application/UseCases/SupprimerClientUseCase.cs`
- `HotelReservation.Application/UseCases/SupprimerFavoriUseCase.cs`
- `HotelReservation.Application/UseCases/SupprimerHotelUseCase.cs`
- `HotelReservation.Application/UseCases/ValiderAvisUseCase.cs`
- `HotelReservation.Application/UseCases/VerifierEmailUseCase.cs`

**Validators**

- `HotelReservation.Application/Validators/AjouterAvisRequestValidator.cs`
- `HotelReservation.Application/Validators/CreerChambreRequestValidator.cs`
- `HotelReservation.Application/Validators/CreerHotelRequestValidator.cs`
- `HotelReservation.Application/Validators/CreerReservationRequestValidator.cs`
- `HotelReservation.Application/Validators/EnregistrerPaiementRequestValidator.cs`

### 12.3 HotelReservation.Domain — 48 fichiers

**Common**

- `HotelReservation.Domain/Common/AggregateRoot.cs`
- `HotelReservation.Domain/Common/DomainException.cs`
- `HotelReservation.Domain/Common/Entity.cs`
- `HotelReservation.Domain/Common/IDomainEvent.cs`
- `HotelReservation.Domain/Common/ValueObject.cs`

**Entités**

- `HotelReservation.Domain/Entities/Administrateur.cs`
- `HotelReservation.Domain/Entities/Agent.cs`
- `HotelReservation.Domain/Entities/AuditLog.cs`
- `HotelReservation.Domain/Entities/Avis.cs`
- `HotelReservation.Domain/Entities/Chambre.cs`
- `HotelReservation.Domain/Entities/Client.cs`
- `HotelReservation.Domain/Entities/Favori.cs`
- `HotelReservation.Domain/Entities/Hotel.cs`
- `HotelReservation.Domain/Entities/Paiement.cs`
- `HotelReservation.Domain/Entities/Personnel.cs`
- `HotelReservation.Domain/Entities/RefreshToken.cs`
- `HotelReservation.Domain/Entities/Reservation.cs`
- `HotelReservation.Domain/Entities/Role.cs`
- `HotelReservation.Domain/Entities/Utilisateur.cs`

**Enums**

- `HotelReservation.Domain/Enums/Genre.cs`
- `HotelReservation.Domain/Enums/ModePaiement.cs`
- `HotelReservation.Domain/Enums/StatutAvis.cs`
- `HotelReservation.Domain/Enums/StatutChambre.cs`
- `HotelReservation.Domain/Enums/StatutHotel.cs`
- `HotelReservation.Domain/Enums/StatutPaiement.cs`
- `HotelReservation.Domain/Enums/StatutReservation.cs`
- `HotelReservation.Domain/Enums/StatutUtilisateur.cs`
- `HotelReservation.Domain/Enums/TypeChambre.cs`

**Événements de domaine**

- `HotelReservation.Domain/Events/AvisModifieEvent.cs`
- `HotelReservation.Domain/Events/DemandeAvisCreeeEvent.cs`
- `HotelReservation.Domain/Events/ReservationAnnuleeEvent.cs`
- `HotelReservation.Domain/Events/ReservationConfirmeeEvent.cs`
- `HotelReservation.Domain/HotelReservation.Domain.csproj`

**Interfaces de repositories**

- `HotelReservation.Domain/Repositories/IAuditLogRepository.cs`
- `HotelReservation.Domain/Repositories/IAvisRepository.cs`
- `HotelReservation.Domain/Repositories/IClientRepository.cs`
- `HotelReservation.Domain/Repositories/IFavoriRepository.cs`
- `HotelReservation.Domain/Repositories/IHotelRepository.cs`
- `HotelReservation.Domain/Repositories/IPaiementRepository.cs`
- `HotelReservation.Domain/Repositories/IRefreshTokenRepository.cs`
- `HotelReservation.Domain/Repositories/IReservationRepository.cs`
- `HotelReservation.Domain/Repositories/IRoleRepository.cs`
- `HotelReservation.Domain/Repositories/IUtilisateurRepository.cs`

**Service de domaine et Value Objects**

- `HotelReservation.Domain/Services/ReservationDomainService.cs`
- `HotelReservation.Domain/ValueObjects/DateRange.cs`
- `HotelReservation.Domain/ValueObjects/Email.cs`
- `HotelReservation.Domain/ValueObjects/Money.cs`
- `HotelReservation.Domain/ValueObjects/NoteAvis.cs`

### 12.4 HotelReservation.Infrastructure — 24 fichiers

- `HotelReservation.Infrastructure/Data/AppDbContext.cs`
- `HotelReservation.Infrastructure/HotelReservation.Infrastructure.csproj`
- `HotelReservation.Infrastructure/Migrations/20260813215834_InitialCreate.cs`
- `HotelReservation.Infrastructure/Migrations/20260813215834_InitialCreate.Designer.cs`
- `HotelReservation.Infrastructure/Migrations/20260819125541_FinaliserBackend.cs`
- `HotelReservation.Infrastructure/Migrations/20260819125541_FinaliserBackend.Designer.cs`
- `HotelReservation.Infrastructure/Migrations/20260822160001_HarmonisationDomaineComplete.cs`
- `HotelReservation.Infrastructure/Migrations/20260822160001_HarmonisationDomaineComplete.Designer.cs`
- `HotelReservation.Infrastructure/Migrations/20260822180047_VerifierFavoris.cs`
- `HotelReservation.Infrastructure/Migrations/20260822180047_VerifierFavoris.Designer.cs`
- `HotelReservation.Infrastructure/Migrations/AppDbContextModelSnapshot.cs`
- `HotelReservation.Infrastructure/Repositories/AuditLogRepository.cs`
- `HotelReservation.Infrastructure/Repositories/AvisRepository.cs`
- `HotelReservation.Infrastructure/Repositories/ClientRepository.cs`
- `HotelReservation.Infrastructure/Repositories/FavoriRepository.cs`
- `HotelReservation.Infrastructure/Repositories/HotelRepository.cs`
- `HotelReservation.Infrastructure/Repositories/PaiementRepository.cs`
- `HotelReservation.Infrastructure/Repositories/RefreshTokenRepository.cs`
- `HotelReservation.Infrastructure/Repositories/ReservationRepository.cs`
- `HotelReservation.Infrastructure/Repositories/RoleRepository.cs`
- `HotelReservation.Infrastructure/Repositories/UtilisateurRepository.cs`
- `HotelReservation.Infrastructure/Services/AuditLogService.cs`
- `HotelReservation.Infrastructure/Services/EmailService.cs`
- `HotelReservation.Infrastructure/Services/TokenService.cs`

### 12.5 HotelReservation.Tests — 4 fichiers

- `HotelReservation.Tests/EntityTests.cs`
- `HotelReservation.Tests/HotelReservation.Tests.csproj`
- `HotelReservation.Tests/ReservationTests.cs`
- `HotelReservation.Tests/UseCaseTests.cs`

**Total contrôlé : 18 API + 85 Application + 48 Domain + 24 Infrastructure + 4 Tests = 179 fichiers.**

**Exclusions :** la liste ne contient pas les fichiers de compilation/restauration générés sous `bin/` et `obj/`, ni le frontend, ni les documents racine tels que `AUDIT_REPORT.md` et `VERIFICATION.md`. Elle décrit l’inventaire présent au moment de la commande ; elle ne constitue pas à elle seule une liste des fichiers ajoutés par la mission. Pour prouver ceux-ci, il faut comparer avec une révision Git de départ.

## 12. Conclusion

Le backend HotelReservation est globalement solide, bien structuré et validé au niveau build/tests. La base fonctionnelle est présente, avec une vraie architecture DDD et un bon niveau de logique métier dans le domaine et les use cases.

En revanche, plusieurs écarts substantiels demeurent par rapport au cahier de charge complet :

- l’absence d’`enum Genre` ;
- les permissions de rôle non réellement persistées ;
- le dashboard non filtré selon le rôle ;
- la facture non générée en PDF ;
- la gestion de profil connecté absente ;
- les points de sécurité de propriété à vérifier sur plusieurs endpoints.

Le projet est donc en état de développement avancé, mais il n’est pas encore entièrement aligné avec le cahier de charge détaillé transmis dans la mission.
