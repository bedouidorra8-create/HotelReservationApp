# Rapport final d’audit backend — HotelReservationApp

**Date de l’audit :** 4 octobre 2026  
**Périmètre :** projets `HotelReservation.Api`, `HotelReservation.Application`, `HotelReservation.Domain`, `HotelReservation.Infrastructure` et `HotelReservation.Tests`. Le frontend Angular est hors périmètre, sauf mention explicite.  
**Nature :** état du code présent dans l’espace de travail. Ce document décrit les fonctionnalités et corrections recensables. Sans comparaison avec une version initiale ou un historique Git complet, il ne prétend pas prouver l’auteur ni la date de chaque ajout.

## 1. Synthèse

Le backend est organisé en quatre couches principales, avec un projet de tests. Il contient des modules d’authentification, utilisateurs, hôtels, chambres, réservations, paiements, avis, favoris, dashboard/statistiques et journalisation d’audit.

Le code possède **60 classes de cas d’utilisation** dans `HotelReservation.Application/UseCases`. Elles sont appelées directement par les contrôleurs au moyen de méthodes telles que `ExecuteAsync`. Malgré la présence de MediatR dans la solution et son utilisation pour la notification d’événements de domaine, les cas d’utilisation recensés ne sont pas des `IRequest`/`IRequestHandler` CQRS : le rapport précédent décrivait donc incorrectement un pipeline Command/Query/Handler généralisé.

La compilation et les tests ont été exécutés dans cette session : `dotnet build HotelReservation.slnx` puis `dotnet test HotelReservation.slnx --no-build`, avec code de sortie 0 et 23 tests réussis selon la sortie de cette exécution. Cela valide compilation et tests automatisés présents, mais ne constitue pas une preuve de conformité exhaustive au cahier des charges ni un test de charge/sécurité complet.

**Conclusion synthétique :** socle backend fonctionnel et compilable, mais audit à ne pas présenter comme « 95 % conforme » sans barème vérifiable. Les principaux points ouverts observés sont l’absence de génération PDF réelle, un contrôle d’accès insuffisant sur la facture, la protection contre les réservations concurrentes et la nécessité de resserrer les rôles de certains endpoints de réservation.

## 2. Architecture et technologies constatées

- **API** : contrôleurs ASP.NET Core, configuration JWT, injection de dépendances et middleware global d’exceptions.
- **Application** : DTOs, cas d’utilisation directs et validateurs FluentValidation présents pour plusieurs requêtes.
- **Domain** : entités, énumérations, Value Objects, exceptions métier, interfaces de repositories et service de domaine des réservations.
- **Infrastructure** : EF Core, `AppDbContext`, implémentations des repositories et services techniques.
- **Persistance configurée** : MySQL avec Pomelo, configuré par `UseMySql` dans `HotelReservation.Api/Program.cs`. Les migrations présentes sont des artefacts MySQL. Le rapport antérieur qui évoquait SQL Server est erroné. La présence des migrations ne prouve pas qu’elles ont été appliquées sur une base réelle.
- **MediatR** : présent et utilisé notamment dans le mécanisme de notifications d’événements de domaine. Les contrôleurs injectent cependant directement les classes `*UseCase` pour les opérations métier recensées.
- **Tests** : projet xUnit comprenant des tests d’entités et de cas d’utilisation. Le résultat final observé est 23 réussis, 0 échec.

## 3. Modifications et points d’alignement recensés

Les éléments ci-dessous correspondent aux corrections et ajouts suivis pendant la mission. Ils décrivent l’état actuel du code ; une comparaison de commits serait nécessaire pour produire un historique diff exact.

- **Genre** : enum `Genre` utilisé au lieu d’un champ de sexe libre, avec les valeurs métier `Homme` et `Femme`.
- **Chambres** : propriété d’activation `Actif` et cas d’utilisation de bascule activation/désactivation présents. Le statut métier de chambre est également géré.
- **Contrats hôtel/recherche** : le champ calculé `NbChambresDisponibles` n’est pas exposé dans les DTOs de sortie recensés.
- **Profil et sécurité du compte** : routes de profil et de changement de mot de passe présentes dans le contrôleur d’authentification, ainsi que les cas d’utilisation d’authentification associés.
- **Dashboard** : l’accès au contrôleur est limité au rôle `Administrateur` par `[Authorize(Roles = "Administrateur")]`.
- **Autres modules existants** : cas d’utilisation pour hôtels, utilisateurs, réservations, paiements, factures sous forme de DTO, avis, favoris, statistiques, rôles et audit.

## 4. État par module

| Module | État constaté | Points à noter |
|---|---|---|
| Authentification | Login, inscription, JWT/refresh token, déconnexion, vérification d’email et parcours de réinitialisation du mot de passe présents. | Les contrôles de sécurité et le cycle complet des emails/tokens méritent des tests d’intégration dédiés. |
| Utilisateurs / administration | Gestion des clients, agents, administrateurs, comptes et rôles présente. | Vérifier les autorisations et l’élévation de privilèges endpoint par endpoint. |
| Hôtels | Création, modification, suppression, recherche, consultation et activation/désactivation présentes. | DTO sans `NbChambresDisponibles` dans le contrat audité. |
| Chambres | Création, modification, suppression, consultation, recherche, statut et activation présents. | Contrôler les règles de suppression quand des réservations existent et les droits par rôle. |
| Calendrier / disponibilité | Cas d’utilisation de calendrier et de disponibilité présents. | Aucun test de charge fourni pour les grands volumes. Aucune table Calendar n’est nécessaire au calendrier calculé. |
| Réservations | Création, modification, annulation, confirmation, changement de statut, recherche, listes, calendrier et historique client présents. | Risque de concurrence lors de créations simultanées ; certaines actions utilisent `[Authorize]` sans liste de rôles autorisés. |
| Paiements / facture | Enregistrement, consultation, annulation et endpoint de facture présents. | L’endpoint facture renvoie un `FactureDto` JSON, pas un fichier PDF. L’accès à la facture ne vérifie pas que le client connecté possède la réservation. |
| Avis | Ajout, modification, suppression, validation et rejet présents. | Les tests doivent couvrir les règles de propriété du client et les rôles d’administration. |
| Favoris | Ajout, retrait et consultation présents. | Des contrôles d’identité sont présents au contrôleur ; contrainte d’unicité à confirmer dans le schéma courant avant déploiement. |
| Dashboard / statistiques | DTOs et cas d’utilisation présents ; endpoints disponibles aux administrateurs. | Les agents ne disposent pas de l’accès actuellement, contrairement à ce que l’ancien tableau indiquait pour le dashboard. |
| Audit | Service, repository, DTO et endpoint de consultation présents. | Confirmer quelles opérations métier écrivent effectivement des événements d’audit et la politique de rétention. |

## 5. Inventaire exhaustif des cas d’utilisation

Le comptage a été vérifié à partir des fichiers `HotelReservation.Application/UseCases/*UseCase.cs` : **60 fichiers**.

1. `ActiverDesactiverChambreUseCase`
2. `ActiverDesactiverCompteUseCase`
3. `ActiverDesactiverHotelUseCase`
4. `AjouterAvisUseCase`
5. `AjouterFavoriUseCase`
6. `AjouterUtilisateurUseCase`
7. `AnnulerPaiementUseCase`
8. `AnnulerReservationUseCase`
9. `ChangerMotDePasseUseCase`
10. `ChangerStatutChambreUseCase`
11. `ChangerStatutReservationUseCase`
12. `ConfirmerReservationUseCase`
13. `ConsulterAuditLogsUseCase`
14. `ConsulterCalendrierReservationsUseCase`
15. `ConsulterChambresUseCase`
16. `ConsulterDashboardUseCase`
17. `ConsulterDisponibilitesUseCase`
18. `ConsulterFavorisUseCase`
19. `ConsulterHotelsUseCase`
20. `ConsulterListeClientsUseCase`
21. `ConsulterListeReservationsUseCase`
22. `ConsulterListeUtilisateursUseCase`
23. `ConsulterMesReservationsUseCase`
24. `ConsulterPaiementsUseCase`
25. `ConsulterStatistiquesUseCase`
26. `CreerChambreUseCase`
27. `CreerClientUseCase`
28. `CreerHotelUseCase`
29. `CreerReservationUseCase`
30. `DeconnexionUseCase`
31. `DemanderResetPasswordUseCase`
32. `EnregistrerPaiementUseCase`
33. `GenererFactureUseCase`
34. `GererRolesUseCase`
35. `InscrireUtilisateurUseCase`
36. `ModifierAdministrateurUseCase`
37. `ModifierAgentUseCase`
38. `ModifierAvisUseCase`
39. `ModifierChambreUseCase`
40. `ModifierClientUseCase`
41. `ModifierHotelUseCase`
42. `ModifierReservationUseCase`
43. `RafraichirTokenUseCase`
44. `RechercherChambreUseCase`
45. `RechercherClientUseCase`
46. `RechercherHotelsUseCase`
47. `RechercherReservationUseCase`
48. `RechercherUtilisateurUseCase`
49. `ReinitialiserMotDePasseUseCase`
50. `RejeterAvisUseCase`
51. `SeConnecterUseCase`
52. `SupprimerAdministrateurUseCase`
53. `SupprimerAgentUseCase`
54. `SupprimerAvisUseCase`
55. `SupprimerChambreUseCase`
56. `SupprimerClientUseCase`
57. `SupprimerFavoriUseCase`
58. `SupprimerHotelUseCase`
59. `ValiderAvisUseCase`
60. `VerifierEmailUseCase`

**Important :** ces 60 classes constituent un inventaire technique, pas une preuve que 60 exigences d’un cahier des charges distinct sont toutes satisfaites. Le total attendu, les correspondances et le score de conformité exigent la version de référence du cahier des charges et une méthode de comptage explicite.

## 6. Modèle de données et persistance

- `AppDbContext` centralise les entités et leur configuration EF Core.
- Les entités principales comprennent les utilisateurs et leurs types, hôtels, chambres, réservations, paiements, avis, favoris, rôles, refresh tokens et logs d’audit.
- Le modèle utilise notamment des enums et Value Objects pour certaines données, et les migrations EF Core sont présentes.
- Le fournisseur configuré dans l’application est MySQL/Pomelo, pas SQL Server.
- Les contraintes décrites dans une version précédente du rapport (indexes uniques, contraintes `CHECK`, règles de suppression et mapping TPH) doivent être considérées comme à revalider contre le `AppDbContextModelSnapshot` et les migrations courantes avant d’être certifiées dans ce rapport. Une migration présente ne prouve pas le schéma réellement déployé.

## 7. Écarts et risques confirmés

### P1 — Autorisation de la facture

`GET /api/paiements/{reservationId}/facture` exige une authentification, mais le contrôleur transmet seulement l’identifiant de réservation au cas d’utilisation. Le contrôleur et `GenererFactureUseCase` ne comparent pas le propriétaire de la réservation au `NameIdentifier` du client connecté. Un utilisateur authentifié qui connaît un identifiant peut donc accéder au DTO de facture de cette réservation, sous réserve que celle-ci existe.

**Correction recommandée :** pour un rôle Client, extraire l’identifiant utilisateur du JWT, charger la réservation et vérifier `reservation.ClientId == clientId` avant de générer la facture. Restreindre également explicitement le endpoint aux rôles attendus (par exemple Client, Agent, Administrateur) et ajouter des tests d’autorisation.

### P1 — Concurrence lors de la création de réservation

La disponibilité est vérifiée avant la sauvegarde. Aucun verrou, transaction d’isolation `Serializable`, token de concurrence ou autre mécanisme atomique n’a été identifié dans le chemin de création audité. Deux demandes simultanées peuvent donc toutes deux lire une chambre disponible puis enregistrer des périodes qui se chevauchent.

**Correction recommandée :** implémenter une stratégie de concurrence cohérente avec le fournisseur MySQL, avec transaction/locking adapté ou contrôle optimiste et reprise en cas de conflit. Une simple vérification préalable en mémoire ou un test unitaire ne règle pas ce risque.

### P2 — Rôles d’endpoints de réservation trop larges

Les routes de modification et d’annulation utilisent `[Authorize]` puis vérifient la propriété seulement lorsque l’appelant est dans le rôle `Client`. Tout autre rôle authentifié qui n’est ni Client ni explicitement interdit peut passer par cette branche et atteindre le cas d’utilisation.

**Correction recommandée :** déclarer précisément les rôles admis (Client, Agent, Administrateur si c’est la règle métier) et conserver le contrôle de propriété pour les clients. Ajouter des tests HTTP pour client propriétaire, autre client, agent, administrateur et utilisateur authentifié d’un autre rôle.

### P2 — Facture PDF non implémentée

Le contrôleur renvoie `ActionResult<FactureDto>` avec `Ok(...)`. Le cas d’utilisation assemble les informations de facture sous forme de DTO. Aucun moteur PDF ni réponse `application/pdf` n’a été constaté dans le code audité.

**Correction recommandée :** si le cahier des charges exige réellement un PDF téléchargeable, ajouter un service de génération PDF, retourner un flux/fichier HTTP avec le type MIME approprié, puis tester le contenu et les autorisations. Sinon, renommer l’exigence en consultation de facture structurée.

### P3 — Comptage et conformité annoncés sans méthode vérifiable

Le score précédent de 95 %, le total attendu de 58 et l’affirmation « zéro manquant » ne sont pas démontrables avec les seuls fichiers du backend. De même, les intitulés de handlers et validators de plusieurs tableaux ne reflètent pas nécessairement les types réellement présents.

**Correction recommandée :** établir une matrice exigence ↔ fichier ↔ endpoint ↔ test à partir du cahier des charges source, puis calculer le score sur cette base.

## 8. Limites de l’audit et validation

- Le rapport décrit l’état du code source disponible, pas une comparaison exhaustive avant/après.
- Le build et les tests ont été exécutés et ont terminé avec exit code 0 dans la session : `dotnet build HotelReservation.slnx` et `dotnet test HotelReservation.slnx --no-build`. Résultat communiqué par la sortie : 23 tests réussis, aucun échec.
- Ces tests ne couvrent pas intégralement les autorisations HTTP, la fuite de facture, la concurrence réelle en base, les migrations sur une base propre ni les performances en charge.
- Le cahier des charges original n’est pas inclus dans les éléments analysés ici ; aucune certification chiffrée globale n’est donc revendiquée.

## 9. Priorités recommandées

1. Corriger et tester l’autorisation de facture par propriétaire de réservation.
2. Fermer explicitement les rôles autorisés sur les endpoints de modification/annulation des réservations.
3. Protéger atomiquement la création de réservation contre les chevauchements concurrents sur MySQL.
4. Implémenter la génération PDF si le livrable attendu est un document téléchargeable.
5. Comparer chaque exigence du cahier des charges aux endpoints/use cases/tests, puis recalculer le taux de conformité.
6. Ajouter des tests d’intégration API et base pour les autorisations, migrations et scénarios concurrents.

## 10. Références de code principales

- API/configuration : `HotelReservation.Api/Program.cs`
- Authentification et profil : `HotelReservation.Api/Controllers/AuthController.cs`
- Réservations : `HotelReservation.Api/Controllers/ReservationsController.cs`
- Paiements et facture : `HotelReservation.Api/Controllers/PaiementsController.cs`, `HotelReservation.Application/UseCases/GenererFactureUseCase.cs`
- Dashboard/statistiques : `HotelReservation.Api/Controllers/DashboardController.cs`
- Création et disponibilité de réservation : `HotelReservation.Application/UseCases/CreerReservationUseCase.cs`, `HotelReservation.Domain/Services/ReservationDomainService.cs`
- Mapping EF Core : `HotelReservation.Infrastructure/Data/AppDbContext.cs`
- Projet de tests : `HotelReservation.Tests/HotelReservation.Tests.csproj`
