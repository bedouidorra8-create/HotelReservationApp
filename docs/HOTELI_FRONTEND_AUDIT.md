# AUDIT APPROFONDI DU FRONTEND ANGULAR — HOTELI
## `docs/HOTELI_FRONTEND_AUDIT.md`

**Projet :** HotelReservationApp $\rightarrow$ **HOTELI**  
**Date d'audit :** 17 Novembre 2026  
**Statut :** Diagnostic en lecture seule (Phase 1) — Aucune modification effectuée  

---

## 1. VUE D'ENSEMBLE DE L'ARCHITECTURE FRONTEND

L'application Angular 19 repose sur :
- **Composants Standalone** avec la nouvelle syntaxe de flux de contrôle (`@if`, `@for`, `@let`).
- **Signaux réactifs** (`signal`, `computed`) pour la gestion de l'état local et partagé (`AuthService`, `FavorisService`, `NotificationService`).
- **3 Espaces Layout distincts :**
  1. `PublicLayoutComponent` (Landing, Catalogue, Détail d'hôtel, Authentification).
  2. `ClientLayoutComponent` (Espace Voyageur : Dashboard, Réservations, Paiements, Factures, Favoris, Profil).
  3. `AdminLayoutComponent` (Espace Direction & Exploitation : Dashboard, Hôtels, Chambres, Réservations, Calendrier, Paiements, Modération des avis, Statistiques, Utilisateurs, Audit).
- **Guards & Sécurité :** `authGuard` (vérification token) et `roleGuard` (vérification des rôles JWT).
- **Intercepteur HTTP :** `authInterceptor` (injection Bearer + Refresh Token automatique sur 401).

---

## 2. INVENTAIRE DES LACUNES ET AMÉLIORATIONS FRONTEND

### A. CONTENU, LOCALISATION & IDENTITÉ TOURISTIQUE TUNISIENNE

| ID | Description | État actuel | Fichier & Ligne | Proposition d'amélioration | Bénéfice utilisateur | Dépendance Backend | Risque | Difficulté | Effort |
|---|---|---|---|---|---|---|---|---|---|
| **FE-01** | **Placeholders géographiques français** | Le champ de recherche affiche `"Paris, Nice, Chamonix..."` | `landing-page.component.ts:33`<br>`hotel-search.component.ts:37` | Remplacer par `"Sidi Bou Saïd, Djerba, Tozeur, Hammamet, Mahdia..."` | Clarté immédiate sur la vocation tunisienne de la plateforme | Aucune | Nul | Très faible | 5 min |
| **FE-02** | **Devise hardcodée en Euros (€)** | Les cartes d'hôtels et sliders affichent `€` ou `EUR` en dur | `landing-page.component.ts:57, 103`<br>`hotel-search.component.ts:61`<br>`hotel-detail-page.component.ts:110` | Utiliser la devise dynamique issue de l'API (`hotel.deviseAffichee || 'TND'`) et afficher le symbole `DT` ou `TND` | Tarification cohérente avec le marché hôtelier tunisien | Aucune (l'API renvoie déjà `TND`) | Faible | Faible | 20 min |
| **FE-03** | **Propriété de prix incorrecte sur la Landing Page** | Le template lit `hotel.prixLePlusBas \|\| 250` alors que le DTO expose `hotel.prixAffiche` | `landing-page.component.ts:103` | Remplacer par `hotel.prixAffiche` avec fallback dynamique | Affiche le tarif réel de la chambre la moins chère au lieu de 250 € par défaut | Aucune | Nul | Très faible | 5 min |
| **FE-04** | **Données mockées résiduelles (Paris/Riviera)** | `mock-data.ts` contient des hôtels français (*Le Grand Palais Royal - Paris*) | `src/app/core/mock-data/mock-data.ts:26-50` | Mettre à jour avec des hôtels de référence tunisiens (Sidi Bou Saïd, Tozeur, Djerba) pour les prévisualisations hors-ligne | Évite d'afficher accidentellement des hôtels français lors des coupures réseau | Aucune | Nul | Faible | 25 min |
| **FE-05** | **Absence de raccourcis rapides par destination tunisienne** | La landing page ne propose aucun filtre direct par région | `landing-page.component.ts:27-68` | Ajouter une rangée de badges/puces cliquables sous la barre de recherche : *Sidi Bou Saïd, Djerba, Tozeur, Mahdia, Monastir, Hammamet* redirigeant vers `/hotels?ville=...` | Découverte guidée et intuitive dès la première seconde | Aucune (le backend filtre déjà par ville) | Faible | Faible | 30 min |

---

### B. ÉTATS D'INTERFACE & EXPÉRIENCE UTILISATEUR (UX)

| ID | Description | État actuel | Fichier & Ligne | Proposition d'amélioration | Bénéfice utilisateur | Dépendance Backend | Risque | Difficulté | Effort |
|---|---|---|---|---|---|---|---|---|---|
| **UX-01** | **Protection contre les doubles soumissions de réservation** | Le bouton "Confirmer la réservation" ne passe pas en état `disabled` pendant l'envoi HTTP | `reservation-wizard.component.ts:180` | Lier `[disabled]="isSubmitting()"` et afficher un spinner de validation | Empêche la création accidentelle de deux réservations identiques en cas de double-clic | Aucune | Faible | Faible | 15 min |
| **UX-02** | **Gestion globale des erreurs 403 (Accès Refusé)** | L'intercepteur gère le rafraîchissement sur 401, mais laisse passer les 403 sans message global | `core/interceptors/auth.interceptor.ts:40-60` | Émettre une notification toast d'avertissement et/ou rediriger vers le dashboard approprié | L'utilisateur comprend pourquoi une action est bloquée plutôt que de subir un silence | Aucune | Faible | Faible | 20 min |
| **UX-03** | **Enrichissement des particularités régionales sur la fiche hôtel** | La fiche hôtel n'affiche que des puces génériques (*Conciergerie, Wi-Fi, Piscine*) | `hotel-detail-page.component.ts:74-83` | Segmenter l'affichage : *Architecture & Cadre*, *Prestations & Bien-être*, *Expériences locales* (ex: dégustation de dattes à Tozeur, poterie à Djerba, thé au jasmin à Sidi Bou Saïd) | Valorise l'identité unique de chaque établissement et de sa région | Utilise la description riche du backend | Faible | Modérée | 45 min |
| **UX-04** | **Indication des nuits et récapitulatif dynamique dans le wizard** | Le calcul du montant total dans le wizard ne détaille pas le nombre exact de nuitées sélectionnées | `reservation-wizard.component.ts:110` | Afficher clairement `Nombre de nuits : X` et `X nuitées × Y DT = Total DT` | Transparence totale du calcul du prix avant paiement | Aucune | Nul | Faible | 20 min |

---

### C. ACCESSIBILITÉ & PERFORMANCE

| ID | Description | État actuel | Fichier & Ligne | Proposition d'amélioration | Bénéfice utilisateur | Dépendance Backend | Risque | Difficulté | Effort |
|---|---|---|---|---|---|---|---|---|---|
| **FE-06** | **Attributs `alt` sur les images d'hôtels** | Certaines balises `<img>` utilisent des `alt` statiques ou manquants | `landing-page.component.ts:88`<br>`hotel-search.component.ts:102` | Renseigner systématiquement `[alt]="hotel.nom + ' - ' + hotel.ville"` | Accessibilité pour les lecteurs d'écran et SEO | Aucune | Nul | Très faible | 10 min |
| **FE-07** | **Optimisation du chargement des galeries photos** | Les images distantes n'ont pas d'attribut `loading="lazy"` | `hotel-search.component.ts:105` | Ajouter l'attribut natif `loading="lazy"` sur les grilles de résultats | Accélération notable du temps de chargement sur mobile | Aucune | Nul | Très faible | 5 min |
| **FE-08** | **Nettoyage des abonnements RxJS** | Quelques `.subscribe()` ne sont pas détruits via `takeUntilDestroyed()` | Divers composants | Appliquer `takeUntilDestroyed()` ou utiliser le pipe `async` / les signaux | Prévention des fuites de mémoire lors des navigations répétées | Aucune | Faible | Faible | 30 min |
