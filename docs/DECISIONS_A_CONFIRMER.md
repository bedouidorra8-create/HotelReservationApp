# DÉCISIONS À CONFIRMER & POINTS D'ARBITRAGE

**Projet :** Système de gestion des réservations hôtelières  
**Document de référence :** `docs/DECISIONS_A_CONFIRMER.md`  

---

## 1. DÉCISIONS TECHNIQUES VALIDÉES & APPLIQUÉES

### D-01 : Persistance des Permissions de l'entité Role
- **Constat :** L'entité `Role` gère une liste interne `_permissions` via les méthodes `AjouterPermission` et `RetirerPermission`, mais la table SQL `Roles` ne possède aucune colonne correspondante dans EF Core.
- **Décision conservatrice appliquée :** Mappage de la collection `_permissions` via une colonne JSON `Permissions` dans la table `Roles` avec un Value Converter / Value Comparer EF Core, accompagné d'une migration incrémentale dédiée `AjouterPermissionsRole`.
- **Règle de sécurité :** Les permissions stockées dans l'entité `Role` restent purement descriptives et informatives ; l'autorisation applicative reste strictement garantie par les rôles JWT (`[Authorize(Roles = "...")]`).

### D-02 : Facture PDF via QuestPDF (Licence Community)
- **Constat :** Le frontend Angular attend un flux binaire PDF sur `GET /api/paiements/{reservationId}/facture/pdf`.
- **Décision appliquée :** Utilisation de `QuestPDF` configuré explicitement avec `QuestPDF.Settings.License = LicenseType.Community;`. Création d'une interface `IFacturePdfService` dans `HotelReservation.Application` et de son implémentation `FacturePdfService` dans `HotelReservation.Infrastructure`.

### D-03 : Écart Dashboard - Indicateurs visibles par l'Agent
- **Constat :** Le cahier des charges ne mentionne que deux indicateurs pour l'Agent, alors que la checklist technique en spécifie trois (Réservations du jour, Paiements en attente, Dernières réservations).
- **Décision appliquée :** Conformément à la directive prioritaire de la Section 1, la règle de la checklist est retenue : l'Agent a accès aux 3 indicateurs susmentionnés. Tous les indicateurs globaux, statistiques financières et graphiques mensuels restent strictement restreints à l'Administrateur.

### D-04 : Découplage complet des Contrôleurs (Clean Architecture)
- **Constat :** Plusieurs contrôleurs (`AvisController`, `ClientsController`, `PaiementsController`) effectuaient des accès directs aux repositories EF Core ou géraient des contrôles d'autorisation métier dans le corps des actions HTTP.
- **Décision appliquée :** Tout accès passe par un Cas d'Utilisation dédié (`ConsulterAvisHotelUseCase`, `ConsulterAvisClientUseCase`, `ConsulterTousAvisUseCase`, `ConsulterClientUseCase`, `ConsulterTousPaiementsUseCase`, `GenererFacturePdfUseCase`). L'autorisation d'accès aux données propres du client (prévention IDOR) est confiée à la couche Application.

### D-05 : Injection du temps via `IDateTimeProvider`
- **Constat :** Les entités et cas d'utilisation utilisaient `DateTime.UtcNow` en dur, rendant les tests temporels non déterministes.
- **Décision appliquée :** Introduction de `IDateTimeProvider` dans `HotelReservation.Application` avec son implémentation `DateTimeProvider` dans `HotelReservation.Infrastructure` (renvoyant `DateTime.UtcNow`).

### D-06 : Environnement système & Absence de l'outil CLI Git
- **Constat :** La commande `git` n'est pas installée dans les variables d'environnement de la machine hôte.
- **Décision appliquée :** Le jalonnement et la validation de chaque phase sont garantis par des compilations rigoureuses (`dotnet build --no-restore`), le passage systématique des tests xUnit (`dotnet test`), et le traçage complet des livrables documentaires.


## 2. NOUVEAUX POINTS D'ARBITRAGE ISSUS DE L'AUDIT DE SÉCURITÉ (PHASE 1)

### D-07 : Droit de suppression d'un client par l'Agent (`supprimerClient`)
- **Diagramme :** Autorise l'Agent à supprimer un client (`supprimerClient` sur cibles `Client`).
- **Backend actuel :** `DELETE /api/clients/{id}` est restreint à `[Authorize(Roles = "Administrateur")]`.
- **Proposition :** Maintenir la restriction à l'Administrateur par sécurité de gouvernance, ou ouvrir à l'Agent en vérifiant strictement que la cible est bien un `Client` (et non un `Agent` ou `Administrateur`).

### D-08 : Droit de suppression d'un hôtel par l'Agent (`supprimerHotel`)
- **Diagramme :** Le tableau des droits du diagramme cite `supprimerHotel` pour `Agent` et `Admin`.
- **Backend actuel :** `DELETE /api/hotels/{id}` est restreint à `[Authorize(Roles = "Administrateur")]`.
- **Recommandation :** Conserver la suppression d'hôtel réservée à l'Administrateur (impact destructeur majeur sur le parc hôtelier).

### D-09 : Écran Angular de Modération des Avis pour l'Agent
- **Constat :** Dans Angular, la page `/admin/reviews` et le menu "⭐ Modération des Avis" sont visibles pour l'Agent, mais tous les endpoints backend (`valider`, `rejeter`) renvoient 403.
- **Correction recommandée (Phase 2) :** Masquer le lien de menu dans `admin-layout.component.ts` via `@if (authService.userRole() === 'Administrateur')` et protéger la route `/admin/reviews` dans `app.routes.ts` avec le `roleGuard: ['Administrateur']`.

### D-10 : Faille IDOR Facture PDF (`telechargerFacture`)
- **Constat :** `GET /api/paiements/{reservationId}/facture/pdf` ne vérifie pas la propriété de la réservation pour les utilisateurs ayant le rôle `Client`.
- **Correction recommandée (Phase 2) :** Ajouter la vérification dans `GenererFacturePdfUseCase` ou `PaiementsController` : si l'utilisateur est un `Client`, lever un accès refusé (403) si `reservation.ClientId != tokenUserId`.
