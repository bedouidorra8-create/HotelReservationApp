import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { HotelService } from '../../../core/services/hotel.service';
import { HotelResultatRechercheDto } from '../../../core/models';
import { StarRatingComponent } from '../../../shared/components/star-rating.component';
import { LoadingSpinnerComponent } from '../../../shared/components/loading-spinner.component';

@Component({
  selector: 'app-landing-page',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule, StarRatingComponent, LoadingSpinnerComponent],
  template: `
    <div class="landing-page">
      <!-- Hero Section Prestigieuse -->
      <section class="hero-section">
        <div class="hero-overlay"></div>
        <div class="container hero-content">
          <span class="hero-badge">✦ VIBRATION & ÉLÉGANCE TUNISIENNE ✦</span>
          <h1 class="hero-title">Découvrez la Tunisie, Trouvez Votre Séjour</h1>
          <p class="hero-subtitle">
            Des palais de Sidi Bou Saïd aux oasis de Tozeur, vivez l'authenticité et le raffinement de l'hospitalité tunisienne.
          </p>

          <!-- Barre de Recherche Intégrée -->
          <div class="search-bar-card">
            <form (ngSubmit)="onSearch()" class="search-grid">
              <div class="search-field">
                <label class="field-label">Destination</label>
                <div class="input-with-icon">
                  <span class="icon">📍</span>
                  <input type="text" [(ngModel)]="searchVille" name="ville" placeholder="Sidi Bou Saïd, Djerba, Tozeur..." class="search-input" />
                </div>
              </div>

              <div class="search-field">
                <label class="field-label">Arrivée</label>
                <div class="input-with-icon">
                  <span class="icon">📅</span>
                  <input type="date" [(ngModel)]="searchDateDebut" name="dateDebut" class="search-input" />
                </div>
              </div>

              <div class="search-field">
                <label class="field-label">Départ</label>
                <div class="input-with-icon">
                  <span class="icon">📅</span>
                  <input type="date" [(ngModel)]="searchDateFin" name="dateFin" class="search-input" />
                </div>
              </div>

              <div class="search-field">
                <label class="field-label">Budget max / nuit</label>
                <div class="input-with-icon">
                  <span class="icon">🪙</span>
                  <input type="number" [(ngModel)]="searchPrixMax" name="prixMax" placeholder="Ex: 350 DT" class="search-input" />
                </div>
              </div>

              <div class="search-action">
                <button type="submit" class="btn btn-gold search-btn">
                  <span>Rechercher</span>
                  <span class="btn-arrow">→</span>
                </button>
              </div>
            </form>
          </div>

          <!-- Quick Discovery Chips Tunisiens (PROP-05) -->
          <div class="quick-destinations">
            <span class="quick-label">Destinations phares :</span>
            <div class="dest-chips-list">
              <button type="button" class="dest-chip" (click)="searchByCity('Sidi Bou Saïd')">📍 Sidi Bou Saïd</button>
              <button type="button" class="dest-chip" (click)="searchByCity('Djerba')">🏝️ Djerba</button>
              <button type="button" class="dest-chip" (click)="searchByCity('Tozeur')">🌴 Tozeur</button>
              <button type="button" class="dest-chip" (click)="searchByCity('Hammamet')">🌸 Hammamet</button>
              <button type="button" class="dest-chip" (click)="searchByCity('Mahdia')">🌊 Mahdia</button>
              <button type="button" class="dest-chip" (click)="searchByCity('Monastir')">⛵ Monastir</button>
              <button type="button" class="dest-chip" (click)="searchByCity('Tunis Médina')">🏛️ Tunis Médina</button>
              <button type="button" class="dest-chip" (click)="searchByCity('Tabarka')">🌲 Tabarka</button>
            </div>
          </div>
        </div>
      </section>

      <!-- Section Hôtels Mis en Avant -->
      <section class="featured-section">
        <div class="container">
          <div class="section-header">
            <span class="section-tag">SÉLECTION SIGNATURE</span>
            <h2 class="section-title">Nos Établissements Emblématiques</h2>
            <p class="section-subtitle">Chaque adresse incarne le summum du confort, de la discrétion et du service d'exception.</p>
          </div>

          @if (isLoading()) {
            <app-loading-spinner message="Chargement des établissements..."></app-loading-spinner>
          } @else {
            <div class="hotels-grid">
              @for (hotel of featuredHotels(); track hotel.id) {
                <div class="hotel-card card-luxury">
                  <div class="card-image-wrap">
                    <img [src]="hotel.images[0] || 'https://images.unsplash.com/photo-1566073771259-6a8506099945?auto=format&fit=crop&w=800&q=80'" [alt]="hotel.nom" class="hotel-img" />
                    <div class="image-overlay-badges">
                      <span class="city-badge">📍 {{ hotel.ville }}</span>
                      <app-star-rating [rating]="hotel.nbEtoiles"></app-star-rating>
                    </div>
                  </div>

                  <div class="card-body">
                    <h3 class="hotel-title">{{ hotel.nom }}</h3>
                    <p class="hotel-address">{{ hotel.adresse }}</p>
                    <p class="hotel-desc">{{ hotel.description }}</p>

                    <div class="hotel-footer-info">
                      <div class="price-box">
                        <span class="price-label">À partir de</span>
                        <span class="price-amount">{{ hotel.prixLePlusBas || 250 }} DT</span>
                        <span class="price-unit">/ nuit</span>
                      </div>
                      <a [routerLink]="['/hotels', hotel.id]" class="btn btn-primary btn-sm">
                        Découvrir & Réserver
                      </a>
                    </div>
                  </div>
                </div>
              }
            </div>
          }
        </div>
      </section>

      <!-- Section Services d'Exception -->
      <section class="services-section">
        <div class="container">
          <div class="section-header">
            <span class="section-tag">HOSPITALITÉ DE PRÉSTIGE</span>
            <h2 class="section-title">Une Expérience Pensée dans les Moindres Détails</h2>
          </div>

          <div class="services-grid">
            <div class="service-item">
              <div class="service-icon">👑</div>
              <h3 class="service-title">Conciergerie Privée Clés d'Or</h3>
              <p class="service-text">Disponibilité permanente pour orchestrer vos transferts héliportés, réservations de tables étoilées et demandes sur mesure.</p>
            </div>

            <div class="service-item">
              <div class="service-icon">🍷</div>
              <h3 class="service-title">Haute Gastronomie & Grands Crus</h3>
              <p class="service-text">Des chefs étoilés réinterprètent les terroirs d'exception sublimés par des caves aux millésimes légendaires.</p>
            </div>

            <div class="service-item">
              <div class="service-icon">🌿</div>
              <h3 class="service-title">Spas Holistiques & Bien-Être</h3>
              <p class="service-text">Havres de paix intimistes proposant soins personnalisés, piscines chauffées à débordement et rituels relaxants.</p>
            </div>

            <div class="service-item">
              <div class="service-icon">🛡️</div>
              <h3 class="service-title">Sérénité & Confidentialité Absolue</h3>
              <p class="service-text">Une discrétion exemplaire garantie par nos équipes formées aux standards les plus exigeants de l'hôtellerie mondiale.</p>
            </div>
          </div>
        </div>
      </section>

      <!-- Call to action adhésion -->
      <section class="cta-section">
        <div class="container cta-box">
          <div class="cta-content">
            <span class="cta-tag">AVANTAGES EXCLUSIFS</span>
            <h2 class="cta-title">Rejoignez le Club Voyageurs HOTELI</h2>
            <p class="cta-desc">
              Bénéficiez de surclassements prioritaires, d'attentions personnalisées à l'arrivée et de tarifs préférentiels réservés aux membres pour vos escapades en Tunisie.
            </p>
          </div>
          <div class="cta-actions">
            <a routerLink="/auth/register" class="btn btn-gold btn-lg">Créer un compte</a>
            <a routerLink="/auth/login" class="btn btn-outline-gold btn-lg">Se connecter</a>
          </div>
        </div>
      </section>
    </div>
  `,
  styles: [`
    .hero-section {
      position: relative;
      background: url('https://images.unsplash.com/photo-1542314831-068cd1dbfeeb?auto=format&fit=crop&w=1920&q=85') center/cover no-repeat;
      padding: 7rem 0 9rem;
      color: var(--white);
      text-align: center;
    }

    .hero-overlay {
      position: absolute;
      inset: 0;
      background: linear-gradient(180deg, rgba(15, 39, 32, 0.85) 0%, rgba(24, 61, 50, 0.92) 100%);
    }

    .hero-content {
      position: relative;
      z-index: 2;
      max-width: 950px;
      margin: 0 auto;
    }

    .hero-badge {
      display: inline-block;
      font-size: 0.75rem;
      letter-spacing: 0.25em;
      color: var(--soft-gold);
      font-weight: 700;
      margin-bottom: 1.25rem;
      border: 1px solid rgba(214, 183, 122, 0.4);
      padding: 0.35rem 1rem;
      border-radius: var(--radius-full);
      background: rgba(24, 61, 50, 0.6);
    }

    .hero-title {
      font-family: var(--font-serif);
      font-size: 3.25rem;
      line-height: 1.15;
      margin-bottom: 1.5rem;
      color: var(--ivory);
      letter-spacing: -0.01em;
    }

    .hero-subtitle {
      font-size: 1.15rem;
      color: #cbd8d1;
      max-width: 650px;
      margin: 0 auto 3rem;
      line-height: 1.6;
    }

    /* Barre de recherche intégrée */
    .search-bar-card {
      background: var(--white);
      border-radius: var(--radius-md);
      padding: 1.25rem 1.75rem;
      box-shadow: var(--shadow-xl);
      border: 1px solid var(--soft-gold-border);
    }

    .search-grid {
      display: grid;
      grid-template-columns: 1.5fr 1fr 1fr 1fr auto;
      gap: 1.25rem;
      align-items: flex-end;
      text-align: left;
    }

    .search-field {
      display: flex;
      flex-direction: column;
    }

    .field-label {
      font-size: 0.785rem;
      font-weight: 600;
      color: var(--forest-green);
      text-transform: uppercase;
      letter-spacing: 0.05em;
      margin-bottom: 0.35rem;
    }

    .input-with-icon {
      position: relative;
      display: flex;
      align-items: center;
    }

    .input-with-icon .icon {
      position: absolute;
      left: 0.65rem;
      font-size: 0.9rem;
      pointer-events: none;
    }

    .search-input {
      width: 100%;
      padding: 0.6rem 0.6rem 0.6rem 2.2rem;
      border: 1px solid var(--border-subtle);
      border-radius: var(--radius-sm);
      font-size: 0.885rem;
      font-family: inherit;
      color: var(--text-primary);
      outline: none;
      transition: border-color var(--transition-fast);
    }

    .search-input:focus {
      border-color: var(--secondary-green);
    }

    .search-btn {
      height: 42px;
      padding: 0 1.5rem;
    }

    .btn-arrow {
      font-size: 1.1rem;
    }

    /* Section Featured */
    .featured-section {
      padding: 5.5rem 0;
      background-color: var(--ivory);
    }

    .section-header {
      text-align: center;
      max-width: 650px;
      margin: 0 auto 3.5rem;
    }

    .section-tag {
      font-size: 0.75rem;
      letter-spacing: 0.2em;
      font-weight: 700;
      color: var(--secondary-green);
      display: block;
      margin-bottom: 0.5rem;
    }

    .section-title {
      font-family: var(--font-serif);
      font-size: 2.35rem;
      color: var(--forest-green);
      margin-bottom: 0.75rem;
    }

    .section-subtitle {
      color: var(--text-secondary);
      font-size: 0.95rem;
    }

    .hotels-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(320px, 1fr));
      gap: 2.25rem;
    }

    .hotel-card {
      display: flex;
      flex-direction: column;
    }

    .card-image-wrap {
      position: relative;
      height: 240px;
      overflow: hidden;
    }

    .hotel-img {
      width: 100%;
      height: 100%;
      object-fit: cover;
      transition: transform var(--transition-slow);
    }

    .hotel-card:hover .hotel-img {
      transform: scale(1.05);
    }

    .image-overlay-badges {
      position: absolute;
      top: 1rem;
      left: 1rem;
      right: 1rem;
      display: flex;
      justify-content: space-between;
      align-items: center;
      background: rgba(15, 39, 32, 0.7);
      backdrop-filter: blur(4px);
      padding: 0.35rem 0.75rem;
      border-radius: var(--radius-sm);
    }

    .city-badge {
      color: var(--ivory);
      font-size: 0.8rem;
      font-weight: 600;
    }

    .hotel-title {
      font-family: var(--font-serif);
      font-size: 1.35rem;
      color: var(--forest-green);
      margin-bottom: 0.35rem;
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
      -webkit-line-clamp: 3;
      -webkit-box-orient: vertical;
      overflow: hidden;
      flex: 1;
    }

    .hotel-footer-info {
      display: flex;
      align-items: center;
      justify-content: space-between;
      border-top: 1px solid var(--border-subtle);
      padding-top: 1rem;
    }

    .price-box {
      display: flex;
      flex-direction: column;
    }

    .price-label {
      font-size: 0.725rem;
      color: var(--text-muted);
    }

    .price-amount {
      font-size: 1.35rem;
      font-weight: 700;
      color: var(--forest-green);
    }

    .price-unit {
      font-size: 0.75rem;
      color: var(--text-muted);
    }

    /* Section Services */
    .services-section {
      background-color: var(--white);
      padding: 5.5rem 0;
      border-top: 1px solid var(--border-subtle);
      border-bottom: 1px solid var(--border-subtle);
    }

    .services-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(240px, 1fr));
      gap: 2.5rem;
    }

    .service-item {
      text-align: center;
      padding: 2rem 1.5rem;
      background: var(--ivory-light);
      border-radius: var(--radius-md);
      border: 1px solid var(--border-subtle);
      transition: transform var(--transition-normal);
    }

    .service-item:hover {
      transform: translateY(-5px);
      box-shadow: var(--shadow-md);
    }

    .service-icon {
      font-size: 2.5rem;
      margin-bottom: 1.25rem;
    }

    .service-title {
      font-family: var(--font-serif);
      font-size: 1.15rem;
      color: var(--forest-green);
      margin-bottom: 0.75rem;
    }

    .service-text {
      font-size: 0.885rem;
      color: var(--text-secondary);
      line-height: 1.6;
    }

    /* CTA Box */
    .cta-section {
      padding: 5rem 0;
      background-color: var(--ivory);
    }

    .cta-box {
      background: linear-gradient(135deg, var(--forest-green) 0%, var(--forest-green-dark) 100%);
      border-radius: var(--radius-lg);
      padding: 3.5rem;
      color: var(--white);
      display: flex;
      align-items: center;
      justify-content: space-between;
      gap: 3rem;
      border: 1px solid var(--soft-gold-border);
      box-shadow: var(--shadow-lg);
    }

    .cta-tag {
      font-size: 0.75rem;
      letter-spacing: 0.2em;
      color: var(--soft-gold);
      font-weight: 700;
      display: block;
      margin-bottom: 0.5rem;
    }

    .cta-title {
      font-family: var(--font-serif);
      font-size: 2.2rem;
      margin-bottom: 0.75rem;
      color: var(--ivory);
    }

    .cta-desc {
      color: #b7c7be;
      font-size: 0.95rem;
      max-width: 550px;
      line-height: 1.6;
    }

    .cta-actions {
      display: flex;
      gap: 1rem;
      flex-shrink: 0;
    }

    @media (max-width: 992px) {
      .search-grid {
        grid-template-columns: 1fr 1fr;
      }
      .hero-title {
        font-size: 2.4rem;
      }
      .cta-box {
        flex-direction: column;
        text-align: center;
        padding: 2.5rem 1.5rem;
      }
      .cta-actions {
        justify-content: center;
      }
    }

    @media (max-width: 600px) {
      .search-grid {
        grid-template-columns: 1fr;
      }
    }

    .quick-destinations {
      margin-top: 1.5rem;
      display: flex;
      align-items: center;
      justify-content: center;
      gap: 0.75rem;
      flex-wrap: wrap;
    }

    .quick-label {
      font-size: 0.85rem;
      color: rgba(255, 255, 255, 0.85);
      font-weight: 500;
      letter-spacing: 0.05em;
    }

    .dest-chips-list {
      display: flex;
      gap: 0.5rem;
      flex-wrap: wrap;
      justify-content: center;
    }

    .dest-chip {
      background: rgba(255, 255, 255, 0.15);
      backdrop-filter: blur(8px);
      border: 1px solid rgba(214, 183, 122, 0.35);
      color: var(--ivory);
      padding: 0.35rem 0.85rem;
      border-radius: var(--radius-full);
      font-size: 0.82rem;
      cursor: pointer;
      transition: all 0.2s ease;
      font-weight: 500;
    }

    .dest-chip:hover {
      background: var(--soft-gold);
      color: var(--forest-green);
      border-color: var(--soft-gold);
      transform: translateY(-2px);
    }
  `]
})
export class LandingPageComponent implements OnInit {
  private hotelService = inject(HotelService);
  private router = inject(Router);

  featuredHotels = signal<HotelResultatRechercheDto[]>([]);
  isLoading = signal<boolean>(true);

  searchVille: string = '';
  searchDateDebut: string = '';
  searchDateFin: string = '';
  searchPrixMax?: number;

  ngOnInit(): void {
    this.hotelService.searchHotels({}).subscribe({
      next: (res) => {
        this.featuredHotels.set(res.slice(0, 3));
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
  }

  searchByCity(city: string): void {
    this.searchVille = city;
    this.onSearch();
  }

  onSearch(): void {
    this.router.navigate(['/hotels'], {
      queryParams: {
        ville: this.searchVille || undefined,
        dateDebut: this.searchDateDebut || undefined,
        dateFin: this.searchDateFin || undefined,
        prixMax: this.searchPrixMax || undefined
      }
    });
  }
}
