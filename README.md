# Hotel Reservation App

Application web de réservation d'hôtels, avec une API backend en ASP.NET Core et une interface frontend en Angular.

## Fonctionnalités

- Inscription, connexion et gestion du profil (authentification JWT avec refresh token)
- Recherche d'hôtels et de chambres, consultation des disponibilités
- Création, modification, confirmation et annulation de réservations
- Paiements et génération de factures PDF
- Avis clients avec modération par l'administration
- Favoris
- Tableau de bord et statistiques
- Gestion des utilisateurs, des rôles et des permissions
- Journal d'audit des actions

## Technologies

**Backend**
- C# / ASP.NET Core Web API
- Entity Framework Core (migrations)
- Architecture en couches : Domain, Application, Infrastructure, Api
- Tests unitaires (projet HotelReservation.Tests)

**Frontend**
- Angular
- TypeScript, SCSS

## Structure du projet

```
HotelReservationApp/
├── HotelReservation.Api/             # Contrôleurs et configuration de l'API
├── HotelReservation.Application/     # Cas d'utilisation, DTOs, validateurs
├── HotelReservation.Domain/          # Entités, règles métier, événements
├── HotelReservation.Infrastructure/  # Base de données, dépôts, services
├── HotelReservation.Tests/           # Tests unitaires
├── hotel-reservation-frontend/       # Application Angular
└── docs/                             # Documentation et contrats d'API
```

## Lancer le projet

### Prérequis
- .NET SDK [version, par exemple 9]
- Node.js [version, par exemple 20] et npm
- [Base de données utilisée, par exemple SQL Server ou SQLite]

### Backend
```
cd HotelReservation.Api
dotnet restore
dotnet ef database update
dotnet run
```

### Frontend
```
cd hotel-reservation-frontend
npm install
npm start
```

L'application est ensuite disponible sur `http://localhost:4200`.

## Documentation

Le dossier `docs/` contient les contrats d'API (`openapi.json`), la matrice des rôles et permissions, et les rapports d'audit.

## Auteur

**Dorra Bedoui**
GitHub : [bedouidorra8-create](https://github.com/bedouidorra8-create)
