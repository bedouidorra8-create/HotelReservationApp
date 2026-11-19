# PLAN DE RENOMMAGE COMMERCIAL — HOTELI
## `docs/HOTELI_BRAND_RENAME_PLAN.md`

**Projet :** HotelReservationApp $\rightarrow$ **HOTELI**  
**Slogan & Positionnement :** *HOTELI — Discover Tunisia, Find Your Stay*  
**Date d'audit :** 17 Novembre 2026  
**Statut :** Diagnostic en lecture seule (Phase 1) — Aucune modification effectuée  

---

## 1. VISION & POSITIONNEMENT DE LA NOUVELLE MARQUE

L'ancienne identité visuelle du projet alternait entre des dénominations génériques de maquette (*"L'Écrin Hôtelier"*, *"Collection Prestige"*, *"Club Écrin"*, *"HotelReservationApp"*), avec des mentions résiduelles françaises (*"Paris"*, *"Nice"*, *"Chamonix"*, *"art de vivre français"*).

La nouvelle marque **HOTELI** incarne :
- **L'hospitalité tunisienne authentique :** Chaleur de l'accueil, élégance méditerranéenne et oasienne.
- **La découverte culturelle et touristique :** Sidi Bou Saïd, Tozeur, Djerba, Mahdia, Monastir, Hammamet, etc.
- **La clarté et la modernité :** Une interface fluide, lisible, inspirée des grands standards internationaux (Booking.com) tout en préservant son âme tunisienne.

---

## 2. INVENTAIRE ET CLASSIFICATION DES OCCURRENCES

### GROUPE 1 : AFFICHAGE UTILISATEUR (Visibilité directe à renommer après validation)
*Éléments purement textuels et graphiques dans le frontend et les documents de sortie sans aucun impact sur les identifiants techniques.*

| Réf | Fichier concerné | Ligne(s) | Texte actuel | Remplacement proposé par HOTELI |
|---|---|---|---|---|
| **BRAND-01** | `src/index.html` | L.5 | `<title>L'Écrin Hôtelier — Collection Prestige</title>` | `<title>HOTELI — Réservez votre séjour d'exception en Tunisie</title>` |
| **BRAND-02** | `src/app/layout/public-layout/public-layout.component.ts` | L.20-21 | `<span class="brand-title">L'ÉCRIN HÔTELIER</span>`<br>`<span class="brand-tagline">COLLECTION PRESTIGE</span>` | `<span class="brand-title">HOTELI</span>`<br>`<span class="brand-tagline">DISCOVER TUNISIA</span>` |
| **BRAND-03** | `src/app/layout/public-layout/public-layout.component.ts` | L.44 | `Rejoindre le Club` | `Rejoindre HOTELI Club` |
| **BRAND-04** | `src/app/layout/public-layout/public-layout.component.ts` | L.61-65 | `L'ÉCRIN HÔTELIER`<br>`art de vivre français...` | `HOTELI`<br>`La plateforme de référence pour découvrir et réserver les plus beaux hôtels et demeures de charme en Tunisie.` |
| **BRAND-05** | `src/app/layout/public-layout/public-layout.component.ts` | L.72-74 | `Paris`, `Nice`, `Chamonix` | `Sidi Bou Saïd`, `Djerba`, `Tozeur`, `Mahdia`, `Hammamet` |
| **BRAND-06** | `src/app/layout/public-layout/public-layout.component.ts` | L.91-92 | `contact@ecrinhotelier.com`<br>`+33 (0)1 42 68 00 00` | `contact@hoteli.tn`<br>`+216 71 000 100` |
| **BRAND-07** | `src/app/layout/public-layout/public-layout.component.ts` | L.97 | `© 2026 L'Écrin Hôtelier` | `© 2026 HOTELI — L'Hospitalité Tunisienne d'Exception` |
| **BRAND-08** | `src/app/layout/client-layout/client-layout.component.ts` | L.20-21 | `L'ÉCRIN HÔTELIER`<br>`ESPACE VOYAGEUR PRIVILÈGE` | `HOTELI`<br>`ESPACE VOYAGEUR PRIVILÈGE` |
| **BRAND-09** | `src/app/layout/admin-layout/admin-layout.component.ts` | L.20-21 | `L'ÉCRIN HÔTELIER`<br>`CONSOLE DIRECTION` | `HOTELI`<br>`ESPACE GESTION & DIRECTION` |
| **BRAND-10** | `src/app/features/public/landing-page/landing-page.component.ts` | L.20-24 | `COLLECTION HÔTELIÈRE D'EXCEPTION`<br>`L'Art de Vivre & le Raffinement Suprême`<br>`villas privées et retraites alpines...` | `VIBRATION & ÉLÉGANCE TUNISIENNE`<br>`Découvrez la Tunisie, Trouvez Votre Séjour`<br>`Des palais de Sidi Bou Saïd aux oasis de Tozeur, vivez l'authenticité de l'hospitalité tunisienne.` |
| **BRAND-11** | `src/app/features/public/landing-page/landing-page.component.ts` | L.33 | `placeholder="Paris, Nice, Chamonix..."` | `placeholder="Sidi Bou Saïd, Djerba, Tozeur, Hammamet..."` |
| **BRAND-12** | `src/app/features/public/landing-page/landing-page.component.ts` | L.159 | `Rejoignez le Cercle Privilège de L'Écrin` | `Rejoignez le Club Voyageurs HOTELI` |
| **BRAND-13** | `src/app/features/client/dashboard/client-dashboard.component.ts` | L.53 | `Statut Club Écrin` | `Statut Club HOTELI` |
| **BRAND-14** | `src/app/features/hotels/hotel-search/hotel-search.component.ts` | L.37 | `placeholder="Paris, Nice, Chamonix..."` | `placeholder="Sidi Bou Saïd, Djerba, Tozeur..."` |
| **BRAND-15** | `HotelReservation.Infrastructure/Services/FacturePdfService.cs` | L.120 | `"Système de Gestion des Réservations Hôtelières"` | `"HOTELI Tunisie — Plateforme Officielle de Réservation"` |
| **BRAND-16** | `HotelReservation.Api/Program.cs` | L.25 | `Title = "HotelReservation API"` | `Title = "HOTELI API — Services de Réservation"` |

---

### GROUPE 2 : IDENTIFIANTS TECHNIQUES (À NE PAS RENOMMER)
*Ces éléments constituent l'ossature architecturale du projet. Conformément à la règle absolue 3.1 et 12, ils doivent rester inchangés afin d'éviter tout bris de contrat d'API, de compilation ou de migration.*

| Type d'identifiant | Valeur conservée | Justification technique |
|---|---|---|
| **Solution C#** | `HotelReservation.slnx` | Clé de build MSBuild / dotnet. |
| **Projets C#** | `HotelReservation.Api.csproj`, `HotelReservation.Application.csproj`, etc. | Liens inter-projets et dépendances de packages NuGet. |
| **Namespaces C#** | `HotelReservation.Domain.*`, `HotelReservation.Application.*`, etc. | Utilisés dans 120+ fichiers C#. |
| **Base de Données MySQL** | `HotelReservationDb` | Chaîne de connexion et catalogue SQL existant. |
| **Migrations EF Core** | `HotelReservation.Infrastructure/Migrations/*` | Historique de migration immuable. |
| **Routes API REST** | `/api/hotels`, `/api/reservations`, `/api/auth`, etc. | Contrat avec le client Angular. |
| **JWT Configuration** | `Jwt:Issuer = "HotelReservationApi"`, `Audience = "HotelReservationClient"` | Validation des claims de jetons existants. |
| **Package Angular** | `hotel-reservation-frontend` dans `package.json` | Configuration Angular CLI et npm. |

---

### GROUPE 3 : ÉLÉMENTS SENSIBLES (Stockage client & Sessions)

| Élément | Emplacement | Comportement actuel | Recommandation |
|---|---|---|---|
| Clés LocalStorage | `src/app/core/auth/token.storage.ts` | Utilise les clés `'access_token'`, `'refresh_token'`, `'current_user'` | **Conserver strictement** ces noms de clés pour préserver les sessions des utilisateurs connectés sans provoquer de déconnexion inopinée. |
| URLs de partage | QueryParams (`?ville=...`, `?dateDebut=...`) | Paramètres standardisés | **Conserver** la nomenclature actuelle pour maintenir les signets et l'historique de navigation. |

---

## 3. STRATÉGIE DE CENTRALISATION PROPOSÉE

Pour éviter de dupliquer la chaîne `"HOTELI"` en dur dans les 15 composants de templates :
1. Définir une constante de marque dans `src/environments/environment.ts` et `environment.development.ts` :
   ```typescript
   export const environment = {
     production: false,
     apiUrl: 'http://localhost:5000/api',
     brand: {
       name: 'HOTELI',
       tagline: 'Discover Tunisia, Find Your Stay',
       contactEmail: 'contact@hoteli.tn',
       contactPhone: '+216 71 000 100',
       currencyDefault: 'TND',
       currencySymbol: 'DT'
     }
   };
   ```
2. Injecter un service léger `BrandService` ou exposer `environment.brand` dans les composants layout pour un affichage dynamique et unifié.
