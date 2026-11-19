# FEUILLE DE ROUTE D'ÉVOLUTION & ARBITRAGE — HOTELI
## `docs/HOTELI_IMPROVEMENT_ROADMAP.md`

**Projet :** HotelReservationApp $\rightarrow$ **HOTELI**  
**Date d'audit :** 17 Novembre 2026  
**Statut :** Diagnostic en lecture seule (Phase 1) — Soumis pour décision utilisateur (Phase 2)  

---

## 1. TABLEAU RÉCAPITULATIF DES AMÉLIORATIONS ÉVALUÉES

*Ce tableau classe l'ensemble des propositions par ordre de ratio Bénéfice / Effort pour vous permettre d'arbitrer facilement.*

| ID | Proposition | Existant ? | Difficulté | Backend Requis ? | Risque | Effort | Décision |
|---|---|---|---|---|---|---|:---:|
| **PROP-01** | **Renommage commercial HOTELI** (Titres, logos, index.html, textes vitrine, footer, factures PDF) | Partiel (ancien nom) | Très faible | Non (affichage seul) | Nul | 30 min | [ ] Oui  [ ] Non  [ ] Plus tard |
| **PROP-02** | **Enrichissement du Seeder en Hôtels Tunisiens** (8 régions : Sidi Bou Saïd, Tozeur, Djerba, Mahdia, Monastir, Hammamet, Tunis, Tabarka) | Partiel (4 hôtels seulement) | Faible | Oui (`DbSeeder.cs`) | Nul (idempotent) | 45 min | [ ] Oui  [ ] Non  [ ] Plus tard |
| **PROP-03** | **Remplacement des références françaises par les destinations tunisiennes** (*Paris, Nice, Chamonix* $\rightarrow$ *Sidi Bou Saïd, Tozeur, Djerba...*) | Présent | Très faible | Non | Nul | 15 min | [ ] Oui  [ ] Non  [ ] Plus tard |
| **PROP-04** | **Affichage de la devise officielle Dinar Tunisien (DT / TND)** sur les fiches et la recherche | Partiel (Euros affichés en dur) | Faible | Non (déjà dans DTO) | Nul | 20 min | [ ] Oui  [ ] Non  [ ] Plus tard |
| **PROP-05** | **Raccourcis rapides par destination sur la Landing Page** (Badges cliquables : *Sidi Bou Saïd*, *Tozeur*, *Djerba*...) | Non | Faible | Non (filtre existant) | Très faible | 25 min | [ ] Oui  [ ] Non  [ ] Plus tard |
| **PROP-06** | **Enrichissement des fiches hôtels avec l'identité régionale** (Architecture, Ambiance, Expériences locales) | Basique | Faible | Non (via `Description`) | Très faible | 30 min | [ ] Oui  [ ] Non  [ ] Plus tard |
| **PROP-07** | **Protection anti double-clic sur la réservation** (`isSubmitting` + spinner sur confirmation) | Partiel | Très faible | Non | Nul | 15 min | [ ] Oui  [ ] Non  [ ] Plus tard |
| **PROP-08** | **Génération et téléchargement du fichier iCalendar (.ics)** pour ajouter son séjour à Google / Apple Calendar | Non | Faible | Non (génération JS côté client) | Nul | 30 min | [ ] Oui  [ ] Non  [ ] Plus tard |
| **PROP-09** | **Export CSV des réservations et des paiements** dans l'espace Administration | Non | Faible | Non (export Blob client) | Nul | 30 min | [ ] Oui  [ ] Non  [ ] Plus tard |
| **PROP-10** | **Bouton de partage direct d'une fiche hôtel** (Copie du lien avec confirmation toast) | Non | Très faible | Non | Nul | 15 min | [ ] Oui  [ ] Non  [ ] Plus tard |
| **PROP-11** | **Bandeau indicatif Bi-Devise (DT / EUR)** pour les voyageurs internationaux | Non | Faible | Non (taux fixe indicatif) | Nul | 25 min | [ ] Oui  [ ] Non  [ ] Plus tard |
| **PROP-12** | **Palette de commandes rapide (Ctrl+K)** dans la console d'administration | Non | Modérée | Non | Faible | 1h 15 | [ ] Oui  [ ] Non  [ ] Plus tard |

---

## 2. ÉVALUATION DÉTAILLÉE : FONCTIONNALITÉS CLIENT (SECTION 8.1)

1. **Recherche avancée :** **DÉJÀ EXISTANTE** dans `hotel-search.component.ts` (Ville, Dates, Étoiles, Budget, Disponibilité). Nécessite seulement le remplacement des libellés français par les destinations tunisiennes.
2. **Filtres par destination :** **DÉJÀ EXISTANT** (champ `Ville` avec liaison queryParams).
3. **Filtres par prix :** **DÉJÀ EXISTANT** (slider de prix fonctionnel). À caler sur des échelles en DT (ex: 100 DT à 1200 DT).
4. **Tri des hôtels :** **DÉJÀ EXISTANT** (Prix croissant, Prix décroissant, Nombre d'étoiles).
5. **Favoris instantanés :** **DÉJÀ EXISTANT** (géré via `FavorisService` avec signal réactif et persistance en base).
6. **Factures PDF :** **DÉJÀ EXISTANT & SÉCURISÉ** (téléchargement PDF QuestPDF avec contrôle IDOR validé en Phase 1).
7. **Calendrier de disponibilité :** **DÉJÀ EXISTANT** dans l'administration (`/admin/calendar`) et sur l'endpoint `/api/chambres/disponibilites`.
8. **Historique des réservations :** **DÉJÀ EXISTANT** dans `/client/reservations` (onglets *À venir*, *Terminées*, *Annulées*).

---

## 3. ÉVALUATION DÉTAILLÉE : FONCTIONNALITÉS ADMINISTRATION (SECTION 8.2)

1. **Dashboard enrichi :** **DÉJÀ EXISTANT** avec projection selon le rôle (l'Agent voit 3 métriques opérationnelles, l'Admin voit les 9 métriques et graphiques).
2. **Évolution des statistiques & Chiffre d'Affaires :** **DÉJÀ EXISTANT** sur `/admin/statistics` avec les 7 indicateurs décisionnels.
3. **Modales de confirmation avant suppression :** **DÉJÀ EXISTANT** (`ConfirmModalComponent` utilisé sur les hôtels, chambres et annulations).
4. **Export CSV (Proposition PROP-09) :** Ajout recommandé d'un bouton d'export tabulaire simple et sans dépendance externe sur les tables Réservations et Paiements.

---

## 4. CINQ PROPOSITIONS ORIGINALES ET AUTHENTIQUES (SECTION 8.3)

### Idée 1 : Puces de Découverte Touristique Rapide (Landing Page)
- **Concept :** Sous la barre de recherche principale, afficher les 6 destinations vedettes (*Sidi Bou Saïd*, *Tozeur*, *Djerba*, *Mahdia*, *Monastir*, *Hammamet*) avec leur icône identitaire (ex: 🏺 Djerba, 🌴 Tozeur, 🏛️ Sidi Bou Saïd). Un clic filtre instantanément le catalogue.
- **Faisabilité :** 100% réalisable sans toucher au backend.
- **Effort :** 25 min | **Risque :** Nul.

### Idée 2 : Badge d'Authenticité & Typologie d'Établissement
- **Concept :** Sur les cartes d'hôtels, ajouter un tag visuel discret caractérisant le type de séjour : *Demeure d'Oasis*, *Maison d'Hôtes Arabo-Andalouse*, *Palace Balnéaire & Thalasso*, *Hôtel Historique*.
- **Faisabilité :** Déduit naturellement de la description ou du nom de l'hôtel.
- **Effort :** 20 min | **Risque :** Nul.

### Idée 3 : Ajout du séjour au calendrier (.ics)
- **Concept :** Sur l'écran de confirmation de réservation et dans "Mes réservations", ajouter un bouton *"Ajouter à mon agenda"*. Un clic télécharge un fichier `.ics` standardisé qui s'intègre automatiquement dans Google Calendar, Outlook ou Apple Calendar avec le nom de l'hôtel, l'adresse et les dates de check-in/check-out.
- **Faisabilité :** Génération côté client pure en TypeScript.
- **Effort :** 30 min | **Risque :** Nul.

### Idée 4 : Affichage Bi-Devise (Dinar Tunisien & Euro)
- **Concept :** Afficher le tarif principal en Dinars Tunisiens (ex: `320 DT / nuit`) accompagné d'une mention discrète du montant indicatif en Euros (ex: `~ 95 €`) pour les touristes internationaux et la diaspora.
- **Faisabilité :** Pipe simple Angular avec taux de change indicatif.
- **Effort :** 25 min | **Risque :** Nul.

### Idée 5 : Bouton de Partage d'Hôtel ("Partager cette adresse")
- **Concept :** Sur la fiche hôtel, un bouton élégant qui copie l'adresse URL directe dans le presse-papiers avec une notification toast *"Lien copié ! Partagez cette adresse avec vos proches."*.
- **Faisabilité :** API Clipboard standard du navigateur.
- **Effort :** 15 min | **Risque :** Nul.
