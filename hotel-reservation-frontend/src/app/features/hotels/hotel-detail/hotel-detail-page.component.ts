import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule, Router } from '@angular/router';
import { HotelService } from '../../../core/services/hotel.service';
import { ChambreService } from '../../../core/services/chambre.service';
import { AvisService } from '../../../core/services/avis.service';
import { FavorisService } from '../../../core/services/favoris.service';
import { AuthService } from '../../../core/auth/auth.service';
import { NotificationService } from '../../../core/services/notification.service';
import { Hotel, Chambre, Avis, StatutChambre } from '../../../core/models';
import { StarRatingComponent } from '../../../shared/components/star-rating.component';
import { LoadingSpinnerComponent } from '../../../shared/components/loading-spinner.component';
import { RoomTypePipe } from '../../../shared/pipes/room-type.pipe';

@Component({
  selector: 'app-hotel-detail-page',
  standalone: true,
  imports: [CommonModule, RouterModule, StarRatingComponent, LoadingSpinnerComponent, RoomTypePipe],
  template: `
    @if (isLoading()) {
      <app-loading-spinner message="Chargement des détails de l'établissement..."></app-loading-spinner>
    } @else if (hotel()) {
      <div class="hotel-detail-page">
        <!-- Hero Gallery -->
        <section class="hotel-hero">
          <div class="container">
            <div class="hero-top-nav">
              <a routerLink="/hotels" class="back-link">← Retour aux hôtels</a>
              <div class="hero-actions-group">
                <button 
                  class="btn btn-outline-gold btn-sm btn-share" 
                  (click)="shareHotel()" 
                  title="Partager cette adresse">
                  🔗 Partager cette adresse
                </button>
                <button 
                  class="btn btn-outline-gold btn-sm btn-fav" 
                  [class.active]="favorisService.isFavorite(hotel()!.id)"
                  (click)="toggleFavorite()">
                  ♥ {{ favorisService.isFavorite(hotel()!.id) ? 'Dans vos favoris' : 'Ajouter aux favoris' }}
                </button>
              </div>
            </div>

            <div class="hotel-title-block">
              <div class="title-left">
                <div class="badge-row">
                  <span class="city-pill">📍 {{ hotel()?.ville }}</span>
                  <app-star-rating [rating]="hotel()!.nbEtoiles"></app-star-rating>
                </div>
                <h1 class="hotel-name">{{ hotel()?.nom }}</h1>
                <p class="hotel-address">{{ hotel()?.adresse }}</p>
              </div>

              <div class="contact-box">
                <span class="contact-item">📞 {{ hotel()?.telephone }}</span>
                <span class="contact-item">✉️ {{ hotel()?.email }}</span>
              </div>
            </div>

            <!-- Galerie d'images -->
            <div class="gallery-grid">
              <div class="main-image-wrap">
                <img [src]="hotel()?.images?.[0] || 'https://images.unsplash.com/photo-1566073771259-6a8506099945?auto=format&fit=crop&w=1200&q=80'" [alt]="hotel()?.nom" class="main-img" />
              </div>
              <div class="side-images-wrap">
                <img [src]="hotel()?.images?.[1] || 'https://images.unsplash.com/photo-1582719478250-c89cae4dc85b?auto=format&fit=crop&w=600&q=80'" alt="Ambiance" class="side-img" />
                <img [src]="hotel()?.images?.[0] || 'https://images.unsplash.com/photo-1520250497591-112f2f40a3f4?auto=format&fit=crop&w=600&q=80'" alt="Suite" class="side-img" />
              </div>
            </div>
          </div>
        </section>

        <!-- Description & Commodités -->
        <section class="details-body">
          <div class="container details-grid">
            <div class="details-left">
              <div class="card-luxury p-card">
                <h2 class="sub-title">À propos de l'établissement</h2>
                <p class="hotel-long-desc">{{ hotel()?.description }}</p>

                <div class="amenities-wrap">
                  <h3 class="amenities-title">Prestations & Identité du Lieu</h3>
                  <div class="amenities-chips">
                    <span class="chip">✨ Conciergerie 24/7</span>
                    <span class="chip">📶 Wi-Fi Très Haut Débit</span>
                    <span class="chip">🏊 Piscine & Espace Spa</span>
                    <span class="chip">☕ Petit-déjeuner du terroir tunisien</span>
                    <span class="chip">🏛️ Architecture & Artisanat local</span>
                    <span class="chip">🌿 Patios & Jardins méditerranéens</span>
                    <span class="chip">🍸 Salon de thé & Lounge</span>
                    <span class="chip">🚗 Service de voiturier</span>
                  </div>
                </div>
              </div>

              <!-- Liste des Chambres Disponibles -->
              <div class="rooms-section">
                <div class="rooms-header">
                  <h2 class="sub-title">Chambres & Suites Disponibles</h2>
                  <p class="rooms-sub">Sélectionnez la chambre de votre choix pour démarrer votre réservation.</p>
                </div>

                <div class="rooms-list">
                  @for (room of chambres(); track room.id) {
                    <div class="room-card card-luxury" [class.occupied]="room.statut !== 1">
                      <div class="room-info">
                        <div class="room-header-row">
                          <span class="room-num">Chambre n° {{ room.numero }}</span>
                          <span class="room-floor">Étage {{ room.etage }}</span>
                          <span class="badge" [ngClass]="room.statut === 1 ? 'badge-success' : 'badge-warning'">
                            {{ room.statut === 1 ? 'Disponible' : 'Indisponible' }}
                          </span>
                        </div>
                        <h3 class="room-type-title">{{ room.type | roomType }}</h3>
                        <p class="room-amenities">Lit King Size • Salle de bain en marbre • Insonorisation haute performance • Vue panoramique</p>
                      </div>

                      <div class="room-booking-action">
                        <div class="room-price-wrap">
                          <span class="price-val">{{ room.prixMontant }} {{ room.prixDevise || 'DT' }}</span>
                          <span class="price-sub">/ nuitée</span>
                        </div>
                        <button 
                          class="btn btn-gold btn-sm" 
                          [disabled]="room.statut !== 1"
                          (click)="bookRoom(room)">
                          {{ room.statut === 1 ? 'Réserver ce séjour' : 'Chambre occupée' }}
                        </button>
                      </div>
                    </div>
                  }
                </div>
              </div>

              <!-- Section Avis Clients -->
              <div class="reviews-section card-luxury p-card">
                <div class="reviews-header">
                  <div>
                    <h2 class="sub-title">Témoignages & Avis Clients</h2>
                    <p class="reviews-sub">{{ reviews().length }} retour(s) d'expérience certifié(s)</p>
                  </div>
                  @if (authService.isAuthenticated()) {
                    <a routerLink="/client/reservations" class="btn btn-outline btn-sm">Partager mon avis</a>
                  }
                </div>

                @if (reviews().length === 0) {
                  <p class="no-reviews">Aucun avis publié pour l'instant. Soyez le premier à partager votre séjour !</p>
                } @else {
                  <div class="reviews-list">
                    @for (rev of reviews(); track rev.id) {
                      <div class="review-item">
                        <div class="review-meta">
                          <span class="review-author">{{ rev.clientNom || 'Hôte Privilège' }}</span>
                          <app-star-rating [rating]="rev.note"></app-star-rating>
                          <span class="review-date">{{ rev.date | date:'dd MMMM yyyy' }}</span>
                        </div>
                        <p class="review-comment">"{{ rev.commentaire }}"</p>
                      </div>
                    }
                  </div>
                }
              </div>
            </div>

            <!-- Colonne Latérale Réservation Rapide -->
            <aside class="details-sidebar">
              <div class="card-luxury p-card sticky-box">
                <h3 class="side-box-title">Votre Séjour d'Exception</h3>
                <p class="side-box-desc">Réservez en direct au meilleur tarif garanti avec accueil personnalisé.</p>

                <div class="guarantee-list">
                  <div class="g-item">
                    <span class="g-icon">🔒</span>
                    <div>
                      <strong>Paiement 100% Sécurisé</strong>
                      <p>Règlement par carte ou virement direct.</p>
                    </div>
                  </div>
                  <div class="g-item">
                    <span class="g-icon">↩️</span>
                    <div>
                      <strong>Annulation Flexible</strong>
                      <p>Selon conditions contractuelles en vigueur.</p>
                    </div>
                  </div>
                  <div class="g-item">
                    <span class="g-icon">⭐</span>
                    <div>
                      <strong>Points Fidélité</strong>
                      <p>Cumulez vos avantages Club HOTELI.</p>
                    </div>
                  </div>
                </div>

                @if (chambres().length > 0) {
                  <button class="btn btn-gold btn-block" (click)="bookRoom(chambres()[0])">
                    Choisir ma chambre
                  </button>
                }
              </div>
            </aside>
          </div>
        </section>
      </div>
    }
  `,
  styles: [`
    .hotel-hero {
      padding: 2rem 0;
      background: var(--white);
      border-bottom: 1px solid var(--border-subtle);
    }

    .hero-top-nav {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 1.5rem;
    }

    .hero-actions-group {
      display: flex;
      gap: 0.75rem;
      align-items: center;
      flex-wrap: wrap;
    }

    .back-link {
      color: var(--secondary-green);
      font-weight: 500;
      font-size: 0.9rem;
    }

    .back-link:hover {
      text-decoration: underline;
    }

    .hotel-title-block {
      display: flex;
      justify-content: space-between;
      align-items: flex-start;
      margin-bottom: 1.75rem;
    }

    .badge-row {
      display: flex;
      align-items: center;
      gap: 0.75rem;
      margin-bottom: 0.4rem;
    }

    .city-pill {
      font-size: 0.8rem;
      font-weight: 600;
      color: var(--secondary-green);
      background: var(--secondary-green-subtle);
      padding: 0.2rem 0.65rem;
      border-radius: var(--radius-xs);
    }

    .hotel-name {
      font-family: var(--font-serif);
      font-size: 2.35rem;
      color: var(--forest-green);
      line-height: 1.2;
    }

    .hotel-address {
      color: var(--text-muted);
      font-size: 0.9rem;
      margin-top: 0.25rem;
    }

    .contact-box {
      display: flex;
      flex-direction: column;
      gap: 0.35rem;
      text-align: right;
      font-size: 0.85rem;
      color: var(--text-secondary);
    }

    /* Galerie */
    .gallery-grid {
      display: grid;
      grid-template-columns: 2fr 1fr;
      gap: 1rem;
      height: 420px;
      border-radius: var(--radius-md);
      overflow: hidden;
    }

    .main-image-wrap {
      height: 100%;
    }

    .main-img, .side-img {
      width: 100%;
      height: 100%;
      object-fit: cover;
    }

    .side-images-wrap {
      display: grid;
      grid-template-rows: 1fr 1fr;
      gap: 1rem;
      height: 100%;
    }

    /* Corps de la page */
    .details-body {
      padding: 3rem 0 5rem;
    }

    .details-grid {
      display: grid;
      grid-template-columns: 1fr 340px;
      gap: 2.5rem;
      align-items: flex-start;
    }

    .p-card {
      padding: 2rem;
      margin-bottom: 2rem;
    }

    .sub-title {
      font-family: var(--font-serif);
      font-size: 1.5rem;
      color: var(--forest-green);
      margin-bottom: 1rem;
    }

    .hotel-long-desc {
      color: var(--text-secondary);
      font-size: 0.95rem;
      line-height: 1.7;
      margin-bottom: 2rem;
    }

    .amenities-title {
      font-size: 1.05rem;
      color: var(--forest-green);
      margin-bottom: 1rem;
    }

    .amenities-chips {
      display: flex;
      flex-wrap: wrap;
      gap: 0.75rem;
    }

    .chip {
      background: var(--ivory-light);
      border: 1px solid var(--border-subtle);
      padding: 0.45rem 0.85rem;
      border-radius: var(--radius-sm);
      font-size: 0.85rem;
      color: var(--forest-green);
      font-weight: 500;
    }

    /* Chambres */
    .rooms-section {
      margin-bottom: 2.5rem;
    }

    .rooms-header {
      margin-bottom: 1.25rem;
    }

    .rooms-sub {
      color: var(--text-secondary);
      font-size: 0.9rem;
    }

    .rooms-list {
      display: flex;
      flex-direction: column;
      gap: 1.25rem;
    }

    .room-card {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 1.5rem;
      border-left: 4px solid var(--secondary-green);
    }

    .room-card.occupied {
      opacity: 0.7;
      border-left-color: var(--color-warning);
    }

    .room-header-row {
      display: flex;
      align-items: center;
      gap: 0.75rem;
      margin-bottom: 0.35rem;
    }

    .room-num {
      font-weight: 700;
      color: var(--forest-green);
      font-size: 0.9rem;
    }

    .room-floor {
      font-size: 0.8rem;
      color: var(--text-muted);
    }

    .room-type-title {
      font-family: var(--font-serif);
      font-size: 1.3rem;
      color: var(--forest-green);
      margin-bottom: 0.35rem;
    }

    .room-amenities {
      font-size: 0.825rem;
      color: var(--text-secondary);
    }

    .room-booking-action {
      display: flex;
      flex-direction: column;
      align-items: flex-end;
      gap: 0.75rem;
    }

    .price-val {
      font-size: 1.45rem;
      font-weight: 700;
      color: var(--forest-green);
    }

    .price-sub {
      font-size: 0.75rem;
      color: var(--text-muted);
      margin-left: 0.25rem;
    }

    /* Avis */
    .reviews-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 1.5rem;
      border-bottom: 1px solid var(--border-subtle);
      padding-bottom: 1rem;
    }

    .reviews-sub {
      color: var(--text-muted);
      font-size: 0.85rem;
    }

    .reviews-list {
      display: flex;
      flex-direction: column;
      gap: 1.25rem;
    }

    .review-item {
      padding-bottom: 1.25rem;
      border-bottom: 1px solid var(--border-subtle);
    }

    .review-item:last-child {
      border-bottom: none;
      padding-bottom: 0;
    }

    .review-meta {
      display: flex;
      align-items: center;
      gap: 0.75rem;
      margin-bottom: 0.4rem;
    }

    .review-author {
      font-weight: 600;
      color: var(--forest-green);
      font-size: 0.9rem;
    }

    .review-date {
      font-size: 0.785rem;
      color: var(--text-muted);
      margin-left: auto;
    }

    .review-comment {
      color: var(--text-secondary);
      font-size: 0.9rem;
      font-style: italic;
    }

    .no-reviews {
      color: var(--text-muted);
      font-size: 0.9rem;
      font-style: italic;
    }

    /* Sidebar */
    .sticky-box {
      position: sticky;
      top: 90px;
    }

    .side-box-title {
      font-family: var(--font-serif);
      font-size: 1.25rem;
      color: var(--forest-green);
      margin-bottom: 0.5rem;
    }

    .side-box-desc {
      font-size: 0.85rem;
      color: var(--text-secondary);
      line-height: 1.5;
      margin-bottom: 1.5rem;
    }

    .guarantee-list {
      display: flex;
      flex-direction: column;
      gap: 1rem;
      margin-bottom: 1.75rem;
    }

    .g-item {
      display: flex;
      gap: 0.75rem;
      font-size: 0.825rem;
    }

    .g-icon {
      font-size: 1.2rem;
    }

    .g-item strong {
      color: var(--forest-green);
      display: block;
    }

    .g-item p {
      color: var(--text-muted);
      margin-top: 0.15rem;
    }

    .btn-block {
      width: 100%;
      padding: 0.8rem;
    }

    @media (max-width: 900px) {
      .details-grid {
        grid-template-columns: 1fr;
      }
      .gallery-grid {
        grid-template-columns: 1fr;
        height: auto;
      }
      .side-images-wrap {
        display: none;
      }
      .hotel-title-block {
        flex-direction: column;
        gap: 1rem;
      }
      .contact-box {
        text-align: left;
      }
    }
  `]
})
export class HotelDetailPageComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private hotelService = inject(HotelService);
  private chambreService = inject(ChambreService);
  private avisService = inject(AvisService);
  favorisService = inject(FavorisService);
  authService = inject(AuthService);
  notificationService = inject(NotificationService);

  hotel = signal<Hotel | null>(null);
  chambres = signal<Chambre[]>([]);
  reviews = signal<Avis[]>([]);
  isLoading = signal<boolean>(true);

  ngOnInit(): void {
    this.route.params.subscribe(params => {
      const id = params['id'];
      if (id) {
        this.loadHotel(id);
      }
    });
  }

  loadHotel(id: string): void {
    this.isLoading.set(true);
    this.hotelService.getHotelById(id).subscribe({
      next: (h) => {
        this.hotel.set(h);
        this.chambreService.getChambresByHotel(id).subscribe(ch => this.chambres.set(ch));
        this.avisService.getHotelReviews(id).subscribe(r => this.reviews.set(r));
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
  }

  toggleFavorite(): void {
    const h = this.hotel();
    if (!h) return;

    if (!this.authService.isAuthenticated()) {
      this.router.navigate(['/auth/login']);
      return;
    }

    if (this.favorisService.isFavorite(h.id)) {
      this.favorisService.removeFavorite(h.id).subscribe();
    } else {
      this.favorisService.addFavorite(h.id).subscribe();
    }
  }

  shareHotel(): void {
    const url = window.location.href;
    if (navigator.clipboard) {
      navigator.clipboard.writeText(url).then(() => {
        this.notificationService.success('Lien copié dans votre presse-papier ! Partagez cette belle adresse tunisienne.');
      }).catch(() => {
        this.notificationService.info(`Lien : ${url}`);
      });
    } else {
      this.notificationService.info(`Lien : ${url}`);
    }
  }

  bookRoom(chambre: Chambre): void {
    this.router.navigate(['/reservations/wizard'], {
      queryParams: {
        hotelId: this.hotel()?.id,
        chambreId: chambre.id
      }
    });
  }
}
