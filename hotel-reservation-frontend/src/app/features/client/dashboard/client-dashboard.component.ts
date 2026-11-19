import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { AuthService } from '../../../core/auth/auth.service';
import { ReservationService } from '../../../core/services/reservation.service';
import { FavorisService } from '../../../core/services/favoris.service';
import { Reservation, Favori, StatutReservation } from '../../../core/models';
import { LoadingSpinnerComponent } from '../../../shared/components/loading-spinner.component';
import { StatusBadgePipe } from '../../../shared/pipes/status-badge.pipe';

@Component({
  selector: 'app-client-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule, LoadingSpinnerComponent],
  template: `
    <div class="client-dashboard">
      <!-- Welcome Hero -->
      <section class="welcome-banner card-luxury">
        <div class="welcome-text">
          <span class="welcome-tag">MEMBRE PRIVILÈGE</span>
          <h1 class="welcome-title">Bienvenue, {{ authService.currentUser()?.nomComplet || 'Cher Voyageur' }}</h1>
          <p class="welcome-desc">
            Retrouvez tous vos séjours à venir, vos résidences favorites et vos avantages exclusifs.
          </p>
        </div>
        <div class="welcome-action">
          <a routerLink="/hotels" class="btn btn-gold btn-lg">Planifier un nouveau séjour →</a>
        </div>
      </section>

      <!-- Grille d'Indicateurs Rapides -->
      <div class="quick-stats-grid">
        <div class="stat-card card-luxury">
          <span class="stat-icon">📅</span>
          <div>
            <span class="stat-val">{{ upcomingReservations().length }}</span>
            <span class="stat-label">Séjour(s) à venir</span>
          </div>
        </div>

        <div class="stat-card card-luxury">
          <span class="stat-icon">♥</span>
          <div>
            <span class="stat-val">{{ favorites().length }}</span>
            <span class="stat-label">Hôtels en favoris</span>
          </div>
        </div>

        <div class="stat-card card-luxury">
          <span class="stat-icon">👑</span>
          <div>
            <span class="stat-val">VIP Gold</span>
            <span class="stat-label">Statut Club Écrin</span>
          </div>
        </div>
      </div>

      <!-- Prochaines Réservations -->
      <section class="dashboard-section">
        <div class="section-title-row">
          <h2 class="sub-heading">Vos Prochains Séjours</h2>
          <a routerLink="/client/reservations" class="view-all-link">Consulter tout l'historique →</a>
        </div>

        @if (isLoading()) {
          <app-loading-spinner message="Chargement de vos données..."></app-loading-spinner>
        } @else if (upcomingReservations().length === 0) {
          <div class="empty-banner card-luxury">
            <span class="empty-icon">🧳</span>
            <div>
              <h3 class="empty-title">Aucun départ prévu prochainement</h3>
              <p class="empty-desc">Envie d'une parenthèse d'évasion ? Explorez notre sélection d'hôtels prestigieux.</p>
            </div>
            <a routerLink="/hotels" class="btn btn-primary btn-sm">Réserver maintenant</a>
          </div>
        } @else {
          <div class="upcoming-list">
            @for (res of upcomingReservations().slice(0, 2); track res.id) {
              <div class="upcoming-card card-luxury">
                <div class="upcoming-left">
                  <span class="res-badge">CONFIRMÉ</span>
                  <h3 class="up-hotel-title">{{ res.hotelNom || 'Établissement Haut de Gamme' }}</h3>
                  <p class="up-dates">
                    Du <strong>{{ res.dateArrivee | date:'dd MMM yyyy' }}</strong> au <strong>{{ res.dateDepart | date:'dd MMM yyyy' }}</strong> ({{ res.nbNuits }} nuits)
                  </p>
                  <span class="up-room">Chambre n° {{ res.chambreNumero || '101' }} • {{ res.nbPersonnes }} personnes</span>
                </div>
                <div class="upcoming-right">
                  <span class="up-price">{{ res.totalMontant || 560 }} DT</span>
                  <a [routerLink]="['/client/invoice', res.id]" class="btn btn-outline btn-sm">Facture</a>
                </div>
              </div>
            }
          </div>
        }
      </section>

      <!-- Établissements Favoris -->
      @if (favorites().length > 0) {
        <section class="dashboard-section">
          <div class="section-title-row">
            <h2 class="sub-heading">Vos Adresses Préférées</h2>
            <a routerLink="/client/favorites" class="view-all-link">Gérer mes favoris →</a>
          </div>

          <div class="fav-grid">
            @for (fav of favorites().slice(0, 3); track fav.id) {
              <div class="fav-mini-card card-luxury">
                <div class="fav-mini-info">
                  <h4 class="fav-hotel-name">{{ fav.hotelNom || 'Hôtel de Prestige' }}</h4>
                  <span class="fav-city">📍 {{ fav.hotelVille || 'France' }}</span>
                </div>
                <a [routerLink]="['/hotels', fav.hotelId]" class="btn btn-gold btn-sm">Voir l'hôtel</a>
              </div>
            }
          </div>
        </section>
      }
    </div>
  `,
  styles: [`
    .client-dashboard {
      max-width: 1040px;
      margin: 0 auto;
    }

    .welcome-banner {
      background: linear-gradient(135deg, var(--forest-green) 0%, var(--forest-green-dark) 100%);
      color: var(--white);
      padding: 2.5rem;
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 2rem;
      border: 1px solid var(--soft-gold-border);
    }

    .welcome-tag {
      font-size: 0.725rem;
      letter-spacing: 0.2em;
      color: var(--soft-gold);
      font-weight: 700;
      display: block;
      margin-bottom: 0.35rem;
    }

    .welcome-title {
      font-family: var(--font-serif);
      font-size: 2rem;
      color: var(--ivory);
      margin-bottom: 0.4rem;
    }

    .welcome-desc {
      color: #b7c7be;
      font-size: 0.95rem;
      max-width: 550px;
    }

    /* Quick stats */
    .quick-stats-grid {
      display: grid;
      grid-template-columns: repeat(3, 1fr);
      gap: 1.5rem;
      margin-bottom: 2.5rem;
    }

    .stat-card {
      padding: 1.5rem;
      display: flex;
      align-items: center;
      gap: 1.25rem;
    }

    .stat-icon {
      font-size: 2.2rem;
      color: var(--soft-gold);
    }

    .stat-val {
      font-size: 1.6rem;
      font-weight: 700;
      color: var(--forest-green);
      display: block;
      line-height: 1.1;
    }

    .stat-label {
      font-size: 0.8rem;
      color: var(--text-muted);
    }

    /* Sections */
    .dashboard-section {
      margin-bottom: 3rem;
    }

    .section-title-row {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 1.25rem;
    }

    .sub-heading {
      font-family: var(--font-serif);
      font-size: 1.35rem;
      color: var(--forest-green);
    }

    .view-all-link {
      font-size: 0.85rem;
      color: var(--secondary-green);
      font-weight: 500;
    }

    .view-all-link:hover {
      text-decoration: underline;
    }

    .empty-banner {
      padding: 2rem;
      display: flex;
      align-items: center;
      gap: 1.5rem;
      background: var(--white);
    }

    .empty-icon {
      font-size: 2.5rem;
    }

    .empty-title {
      font-size: 1.1rem;
      color: var(--forest-green);
      margin-bottom: 0.25rem;
    }

    .empty-desc {
      font-size: 0.885rem;
      color: var(--text-secondary);
    }

    .upcoming-list {
      display: flex;
      flex-direction: column;
      gap: 1.25rem;
    }

    .upcoming-card {
      padding: 1.5rem;
      display: flex;
      justify-content: space-between;
      align-items: center;
      border-left: 4px solid var(--forest-green);
    }

    .res-badge {
      font-size: 0.7rem;
      font-weight: 700;
      color: var(--color-success);
      letter-spacing: 0.08em;
    }

    .up-hotel-title {
      font-family: var(--font-serif);
      font-size: 1.25rem;
      color: var(--forest-green);
      margin: 0.2rem 0;
    }

    .up-dates {
      font-size: 0.885rem;
      color: var(--text-secondary);
    }

    .up-room {
      font-size: 0.8rem;
      color: var(--text-muted);
    }

    .upcoming-right {
      display: flex;
      flex-direction: column;
      align-items: flex-end;
      gap: 0.75rem;
    }

    .up-price {
      font-size: 1.3rem;
      font-weight: 700;
      color: var(--forest-green);
    }

    /* Fav mini grid */
    .fav-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(280px, 1fr));
      gap: 1.25rem;
    }

    .fav-mini-card {
      padding: 1.25rem;
      display: flex;
      justify-content: space-between;
      align-items: center;
    }

    .fav-hotel-name {
      font-size: 0.95rem;
      color: var(--forest-green);
      margin-bottom: 0.2rem;
    }

    .fav-city {
      font-size: 0.8rem;
      color: var(--text-muted);
    }

    @media (max-width: 768px) {
      .welcome-banner {
        flex-direction: column;
        align-items: flex-start;
        gap: 1.5rem;
      }
      .quick-stats-grid {
        grid-template-columns: 1fr;
      }
      .upcoming-card {
        flex-direction: column;
        align-items: flex-start;
        gap: 1rem;
      }
      .upcoming-right {
        align-items: flex-start;
      }
    }
  `]
})
export class ClientDashboardComponent implements OnInit {
  authService = inject(AuthService);
  private reservationService = inject(ReservationService);
  private favorisService = inject(FavorisService);

  reservations = signal<Reservation[]>([]);
  favorites = signal<Favori[]>([]);
  isLoading = signal<boolean>(true);

  upcomingReservations = signal<Reservation[]>([]);

  ngOnInit(): void {
    const user = this.authService.currentUser();
    if (!user?.utilisateurId) return;

    this.reservationService.getMyReservations(user.utilisateurId).subscribe({
      next: (res) => {
        this.reservations.set(res);
        this.upcomingReservations.set(
          res.filter(r => r.statut === StatutReservation.EnAttente || r.statut === StatutReservation.Confirmee)
        );
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });

    this.favorisService.getFavorites(user.utilisateurId).subscribe({
      next: (favs) => this.favorites.set(favs)
    });
  }
}
