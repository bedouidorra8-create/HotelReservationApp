import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, ActivatedRoute, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { HotelService } from '../../../core/services/hotel.service';
import { FavorisService } from '../../../core/services/favoris.service';
import { AuthService } from '../../../core/auth/auth.service';
import { HotelResultatRechercheDto } from '../../../core/models';
import { StarRatingComponent } from '../../../shared/components/star-rating.component';
import { LoadingSpinnerComponent } from '../../../shared/components/loading-spinner.component';
import { EmptyStateComponent } from '../../../shared/components/empty-state.component';

@Component({
  selector: 'app-hotel-search',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule, StarRatingComponent, LoadingSpinnerComponent, EmptyStateComponent],
  template: `
    <div class="search-page">
      <div class="container">
        <!-- En-tête -->
        <div class="search-header">
          <span class="subheading">COLLECTION EXCLUSIVE</span>
          <h1 class="page-title">Découvrez nos Palais & Résidences</h1>
          <p class="page-subtitle">Trouvez le cadre d'exception idéal pour votre prochain séjour.</p>
        </div>

        <div class="search-layout">
          <!-- Panneau des Filtres Latéraux -->
          <aside class="filters-sidebar card-luxury">
            <div class="filters-head">
              <h3 class="filters-title">Filtres de recherche</h3>
              <button class="btn-reset" (click)="resetFilters()">Réinitialiser</button>
            </div>

            <div class="filter-group">
              <label class="filter-label">Destination (Ville)</label>
              <input type="text" [(ngModel)]="filterVille" (ngModelChange)="applyFilters()" placeholder="Sidi Bou Saïd, Djerba, Tozeur..." class="form-control form-control-sm" />
            </div>

            <div class="filter-group">
              <label class="filter-label">Date d'arrivée</label>
              <input type="date" [(ngModel)]="filterDateDebut" (ngModelChange)="applyFilters()" class="form-control form-control-sm" />
            </div>

            <div class="filter-group">
              <label class="filter-label">Date de départ</label>
              <input type="date" [(ngModel)]="filterDateFin" (ngModelChange)="applyFilters()" class="form-control form-control-sm" />
            </div>

            <div class="filter-group">
              <label class="filter-label">Standing minimum</label>
              <select [(ngModel)]="filterEtoiles" (ngModelChange)="applyFilters()" class="form-select form-control-sm">
                <option [ngValue]="null">Toutes les catégories</option>
                <option [ngValue]="5">5 étoiles (Luxe Suprême)</option>
                <option [ngValue]="4">4 étoiles (Grand Confort)</option>
                <option [ngValue]="3">3 étoiles (Élégance)</option>
              </select>
            </div>

            <div class="filter-group">
              <label class="filter-label">Budget max / nuit : {{ filterPrixMax || 1500 }} DT</label>
              <input type="range" min="150" max="1500" step="50" [(ngModel)]="filterPrixMax" (ngModelChange)="applyFilters()" class="range-slider" />
            </div>

            <div class="filter-group checkbox-group">
              <label class="checkbox-label">
                <input type="checkbox" [(ngModel)]="filterDispoOnly" (ngModelChange)="applyFilters()" />
                <span>Chambres disponibles uniquement</span>
              </label>
            </div>

            <div class="filter-group">
              <label class="filter-label">Trier par</label>
              <select [(ngModel)]="filterTri" (ngModelChange)="applyFilters()" class="form-select form-control-sm">
                <option value="prixAsc">Prix croissant</option>
                <option value="prixDesc">Prix décroissant</option>
                <option value="etoilesDesc">Nombre d'étoiles</option>
              </select>
            </div>
          </aside>

          <!-- Liste des Résultats -->
          <main class="results-main">
            @if (isLoading()) {
              <app-loading-spinner message="Recherche des disponibilités en cours..."></app-loading-spinner>
            } @else if (hotels().length === 0) {
              <app-empty-state
                icon="🏨"
                title="Aucun établissement trouvé"
                description="Aucun hôtel ne correspond exactement à vos critères. Essayez d'ajuster votre budget ou vos dates.">
                <button class="btn btn-outline btn-sm" (click)="resetFilters()">Réinitialiser les filtres</button>
              </app-empty-state>
            } @else {
              <div class="results-count-bar">
                <span>{{ hotels().length }} établissement(s) d'exception trouvé(s)</span>
              </div>

              <div class="results-grid">
                @for (hotel of hotels(); track hotel.id) {
                  <div class="hotel-search-card card-luxury">
                    <div class="card-thumb-wrap">
                      <img [src]="hotel.images[0] || 'https://images.unsplash.com/photo-1566073771259-6a8506099945?auto=format&fit=crop&w=800&q=80'" [alt]="hotel.nom" class="thumb-img" />
                      <button 
                        class="favorite-heart" 
                        [class.active]="favorisService.isFavorite(hotel.id)"
                        (click)="toggleFavorite(hotel.id, $event)"
                        title="Ajouter aux favoris">
                        ♥
                      </button>
                      <span class="city-pill">📍 {{ hotel.ville }}</span>
                    </div>

                    <div class="card-content">
                      <div class="card-top-row">
                        <app-star-rating [rating]="hotel.nbEtoiles"></app-star-rating>
                        <span class="avail-tag">
                          {{ hotel.chambresDisponibles > 0 ? hotel.chambresDisponibles + ' chambre(s) libre(s)' : 'Sur demande' }}
                        </span>
                      </div>

                      <h2 class="hotel-title">
                        <a [routerLink]="['/hotels', hotel.id]">{{ hotel.nom }}</a>
                      </h2>
                      <p class="hotel-address">{{ hotel.adresse }}</p>
                      <p class="hotel-desc">{{ hotel.description }}</p>

                      <div class="card-bottom-row">
                        <div class="price-details">
                          <span class="price-from">Tarif indicatif dès</span>
                          <div class="price-val">
                            <span class="val">{{ hotel.prixLePlusBas }} DT</span>
                            <span class="period">/ nuit</span>
                          </div>
                        </div>

                        <a [routerLink]="['/hotels', hotel.id]" class="btn btn-gold btn-sm">
                          Sélectionner les chambres →
                        </a>
                      </div>
                    </div>
                  </div>
                }
              </div>
            }
          </main>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .search-page {
      padding: 3.5rem 0 5rem;
    }

    .search-header {
      margin-bottom: 2.5rem;
    }

    .subheading {
      font-size: 0.75rem;
      letter-spacing: 0.2em;
      color: var(--secondary-green);
      font-weight: 700;
      display: block;
      margin-bottom: 0.35rem;
    }

    .search-layout {
      display: grid;
      grid-template-columns: 290px 1fr;
      gap: 2.5rem;
      align-items: flex-start;
    }

    /* Sidebar des Filtres */
    .filters-sidebar {
      padding: 1.5rem;
      position: sticky;
      top: 90px;
    }

    .filters-head {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 1.5rem;
      border-bottom: 1px solid var(--border-subtle);
      padding-bottom: 0.75rem;
    }

    .filters-title {
      font-size: 1.05rem;
      color: var(--forest-green);
      font-weight: 600;
    }

    .btn-reset {
      background: none;
      border: none;
      color: var(--secondary-green);
      font-size: 0.8rem;
      cursor: pointer;
    }

    .btn-reset:hover {
      text-decoration: underline;
    }

    .filter-group {
      margin-bottom: 1.25rem;
    }

    .filter-label {
      font-size: 0.825rem;
      font-weight: 600;
      color: var(--text-primary);
      margin-bottom: 0.4rem;
      display: block;
    }

    .range-slider {
      width: 100%;
      accent-color: var(--forest-green);
    }

    .checkbox-label {
      display: flex;
      align-items: center;
      gap: 0.5rem;
      font-size: 0.825rem;
      cursor: pointer;
      color: var(--text-primary);
    }

    /* Résultats */
    .results-count-bar {
      margin-bottom: 1.25rem;
      font-size: 0.875rem;
      color: var(--text-secondary);
      font-weight: 500;
    }

    .results-grid {
      display: flex;
      flex-direction: column;
      gap: 1.75rem;
    }

    .hotel-search-card {
      display: grid;
      grid-template-columns: 320px 1fr;
      min-height: 240px;
    }

    .card-thumb-wrap {
      position: relative;
      height: 100%;
      min-height: 220px;
    }

    .thumb-img {
      width: 100%;
      height: 100%;
      object-fit: cover;
    }

    .favorite-heart {
      position: absolute;
      top: 0.75rem;
      right: 0.75rem;
      background: rgba(255, 255, 255, 0.85);
      border: none;
      width: 34px;
      height: 34px;
      border-radius: 50%;
      display: flex;
      align-items: center;
      justify-content: center;
      cursor: pointer;
      color: #999;
      font-size: 1.2rem;
      transition: all var(--transition-fast);
    }

    .favorite-heart.active, .favorite-heart:hover {
      color: #e53e3e;
      background: var(--white);
      transform: scale(1.1);
    }

    .city-pill {
      position: absolute;
      bottom: 0.75rem;
      left: 0.75rem;
      background: rgba(15, 39, 32, 0.75);
      backdrop-filter: blur(4px);
      color: var(--white);
      font-size: 0.75rem;
      font-weight: 600;
      padding: 0.25rem 0.65rem;
      border-radius: var(--radius-xs);
    }

    .card-content {
      padding: 1.5rem;
      display: flex;
      flex-direction: column;
    }

    .card-top-row {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 0.5rem;
    }

    .avail-tag {
      font-size: 0.75rem;
      color: var(--color-success);
      font-weight: 600;
      background: var(--color-success-bg);
      padding: 0.2rem 0.6rem;
      border-radius: var(--radius-full);
    }

    .hotel-title {
      font-family: var(--font-serif);
      font-size: 1.4rem;
      color: var(--forest-green);
      margin-bottom: 0.25rem;
    }

    .hotel-title a:hover {
      color: var(--secondary-green);
    }

    .hotel-address {
      font-size: 0.825rem;
      color: var(--text-muted);
      margin-bottom: 0.75rem;
    }

    .hotel-desc {
      font-size: 0.885rem;
      color: var(--text-secondary);
      line-height: 1.5;
      margin-bottom: 1.5rem;
      display: -webkit-box;
      -webkit-line-clamp: 2;
      -webkit-box-orient: vertical;
      overflow: hidden;
      flex: 1;
    }

    .card-bottom-row {
      display: flex;
      align-items: flex-end;
      justify-content: space-between;
      border-top: 1px solid var(--border-subtle);
      padding-top: 1rem;
    }

    .price-from {
      font-size: 0.725rem;
      color: var(--text-muted);
      display: block;
    }

    .val {
      font-size: 1.5rem;
      font-weight: 700;
      color: var(--forest-green);
    }

    .period {
      font-size: 0.785rem;
      color: var(--text-muted);
      margin-left: 0.25rem;
    }

    @media (max-width: 900px) {
      .search-layout {
        grid-template-columns: 1fr;
      }
      .hotel-search-card {
        grid-template-columns: 1fr;
      }
      .card-thumb-wrap {
        height: 200px;
      }
    }
  `]
})
export class HotelSearchComponent implements OnInit {
  private hotelService = inject(HotelService);
  favorisService = inject(FavorisService);
  private authService = inject(AuthService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);

  hotels = signal<HotelResultatRechercheDto[]>([]);
  isLoading = signal<boolean>(true);

  filterVille: string = '';
  filterDateDebut: string = '';
  filterDateFin: string = '';
  filterEtoiles: number | null = null;
  filterPrixMax: number = 1000;
  filterDispoOnly: boolean = false;
  filterTri: string = 'prixAsc';

  ngOnInit(): void {
    this.route.queryParams.subscribe(params => {
      if (params['ville']) this.filterVille = params['ville'];
      if (params['dateDebut']) this.filterDateDebut = params['dateDebut'];
      if (params['dateFin']) this.filterDateFin = params['dateFin'];
      if (params['prixMax']) this.filterPrixMax = Number(params['prixMax']);
      if (params['etoiles']) this.filterEtoiles = Number(params['etoiles']);
      this.applyFilters();
    });

    const user = this.authService.currentUser();
    if (user && user.utilisateurId) {
      this.favorisService.getFavorites(user.utilisateurId).subscribe();
    }
  }

  applyFilters(): void {
    this.isLoading.set(true);
    this.hotelService.searchHotels({
      ville: this.filterVille || undefined,
      prixMax: this.filterPrixMax || undefined,
      etoilesMin: this.filterEtoiles || undefined,
      dateDebut: this.filterDateDebut || undefined,
      dateFin: this.filterDateFin || undefined,
      disponibleUniquement: this.filterDispoOnly,
      tri: this.filterTri
    }).subscribe({
      next: (res) => {
        this.hotels.set(res);
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
  }

  resetFilters(): void {
    this.filterVille = '';
    this.filterDateDebut = '';
    this.filterDateFin = '';
    this.filterEtoiles = null;
    this.filterPrixMax = 1000;
    this.filterDispoOnly = false;
    this.filterTri = 'prixAsc';
    this.applyFilters();
  }

  toggleFavorite(hotelId: string, event: Event): void {
    event.stopPropagation();
    if (!this.authService.isAuthenticated()) {
      this.router.navigate(['/auth/login']);
      return;
    }

    if (this.favorisService.isFavorite(hotelId)) {
      this.favorisService.removeFavorite(hotelId).subscribe();
    } else {
      this.favorisService.addFavorite(hotelId).subscribe();
    }
  }
}
