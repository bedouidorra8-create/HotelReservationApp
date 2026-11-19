# ANALYSE DE COMPATIBILITÉ BACKEND — HOTELI
## `docs/HOTELI_BACKEND_COMPATIBILITY.md`

**Projet :** HotelReservationApp $\rightarrow$ **HOTELI**  
**Date d'audit :** 17 Novembre 2026  
**Statut :** Diagnostic en lecture seule (Phase 1) — Aucune modification effectuée  

---

## 1. COMPATIBILITÉ DE L'ARCHITECTURE & DES COUCHES DDD

Le backend respecte une **Clean Architecture** stricte articulée en 4 couches :
1. **`HotelReservation.Domain` :** Modèles d'agrégats purs (`Hotel`, `Chambre`, `Reservation`, `Paiement`, `Avis`, `Favori`), Value Objects (`Money`, `Email`, `DateRange`, `NoteAvis`), Services de domaine et Règles invariantes sans dépendance technique.
2. **`HotelReservation.Application` :** Cas d'utilisation CQRS isolés, DTOs de contrats, validateurs FluentValidation, interfaces de services (`ITableauDeBordService`, `IStatistiqueService`, `IFacturePdfService`, `IEmailService`, `ITokenService`).
3. **`HotelReservation.Infrastructure` :** Implémentations Entity Framework Core 8/9 avec Pomelo MySQL (`AppDbContext`), Repositories, Seeder, génération PDF QuestPDF, services JWT et Email.
4. **`HotelReservation.Api` :** Contrôleurs ASP.NET Core légers, gestion du pipeline HTTP, middleware d'exception global et configuration Swagger OpenAPI.

---

## 2. ADAPTATION DU MODÈLE AUX SPÉCIFICITÉS TOURISTIQUES TUNISIENNES

### 2.1 Entité `Hotel` & Filtrage par Destination
- **Champ `Ville` :** Stocke directement le nom de la destination tunisienne (*"Sidi Bou Saïd"*, *"Tozeur"*, *"Djerba"*, *"Mahdia"*, *"Monastir"*, *"Hammamet"*, *"Tunis"*, *"Tabarka"*).
- **Moteur de recherche (`HotelRepository.RechercherAsync`) :**
  ```csharp
  if (!string.IsNullOrWhiteSpace(ville))
      query = query.Where(h => h.Ville.ToLower().Contains(ville.ToLower()));
  ```
  **Constat :** La recherche est insensible à la casse et fonctionne par sous-chaîne (`Contains`). Une recherche sur `"djerba"`, `"Tozeur"` ou `"sidi bou"` renvoie immédiatement les établissements associés sans aucune modification requise.

### 2.2 Champ `Description` & Storytelling Architectural
- Le champ `Description` (de type `longtext` en base MySQL) n'a pas de limite contraignante de taille.
- Il permet de structurer un contenu riche pour chaque hôtel :
  1. *Identité & Cadre architectural* (ex: façades blanchies à la chaux, moucharabiehs bleu indigo, patios intérieurs avec fontaines, briques pleines ocre de Tozeur, menzels traditionnels de Djerba).
  2. *Ambiance & Esprit des lieux* (ex: parfum de jasmin et de fleur d'oranger, brise marine méditerranéenne, quiétude de la palmeraie).
  3. *Expériences signatures* (ex: dégustation de dattes Deglet Nour et thés parfumés, initiation à la poterie de Guellala, rituels de hammam traditionnel et thalassothérapie).
- **Zéro impact de schéma :** Aucune migration de base de données n'est nécessaire.

### 2.3 Gestion Monétaire & Devise
- Le Value Object `Money` stocke le montant (`Amount`) et la devise (`Currency`).
- Le Seeder initialise déjà les chambres avec la devise `"TND"` (Dinar Tunisien).
- **Compatibilité :** Le contrat `ChambreDto` expose `PrixMontant: decimal` et `PrixDevise: string`, ce qui permet au frontend d'afficher automatiquement `"TND"` ou `"DT"` sans modifier les DTOs existants.

---

## 3. AUDIT DES CONTRATS D'API ET COHÉRENCE AVEC LE FRONTEND

| Endpoint API | Cas d'Utilisation appelé | DTO Retourné | Consommation Frontend Angular | Statut de Compatibilité |
|---|---|---|---|:---:|
| `GET /api/hotels` | `ConsulterHotelsUseCase` | `IEnumerable<HotelDto>` | `HotelService.getHotels()` | **100% Compatible** |
| `GET /api/hotels/{id}` | `ConsulterHotelsUseCase` | `HotelDto` | `HotelService.getHotelById()` | **100% Compatible** |
| `GET /api/hotels/recherche` | `RechercherHotelsUseCase` | `IEnumerable<HotelResultatRechercheDto>` | `HotelService.searchHotels()` | **100% Compatible** |
| `GET /api/hotels/{id}/chambres` | `ConsulterChambresUseCase` | `IEnumerable<ChambreDto>` | `ChambreService.getChambresByHotel()` | **100% Compatible** |
| `GET /api/hotels/{id}/disponibilites` | `ConsulterDisponibilitesUseCase` | `DisponibilitesHotelResponse` | `ChambreService.getDisponibilites()` | **100% Compatible** |
| `GET /api/avis/hotel/{hotelId}` | `ConsulterAvisHotelUseCase` | `IEnumerable<AvisDto>` | `AvisService.getHotelReviews()` | **100% Compatible** |

---

## 4. INVENTAIRE DES POINTS D'ATTENTION BACKEND (BE)

| ID | Description | Analyse technique | Recommandation |
|---|---|---|---|
| **BE-01** | **Tarif affiché sur la liste des hôtels (`PrixAffiche`)** | `HotelResultatRechercheDto` calcule le prix de la chambre la moins chère disponible via `chambresLibres.Min(c => c.Prix.Amount)`. Si aucune chambre n'est libre, il prend le minimum de toutes les chambres actives. | Parfaitement fonctionnel ; alimente fidèlement les cartes d'hôtels du frontend. |
| **BE-02** | **Support des accents dans la recherche par ville** | EF Core et MySQL avec la collation `utf8mb4_unicode_ci` ou `utf8mb4_general_ci` gèrent l'insensibilité aux accents de manière native (*Sidi Bou Said* trouve *Sidi Bou Saïd*). | Aucun changement de code nécessaire. |
| **BE-03** | **Enregistrement de paiement au guichet (`enregistrerPaiement`)** | Actuellement restreint au rôle `Client`. L'encaissement direct au comptoir par l'Agent nécessitera à terme d'enregistrer l'ID de l'agent dans `Paiement.EnregistrePar`. | Maintenir le comportement actuel pour la réservation en ligne et planifier l'extension guichet en Phase ultérieure. |
