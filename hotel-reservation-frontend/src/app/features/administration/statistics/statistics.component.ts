import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DashboardService } from '../../../core/services/dashboard.service';
import { StatistiquesDto } from '../../../core/models';
import { LoadingSpinnerComponent } from '../../../shared/components/loading-spinner.component';
import { StarRatingComponent } from '../../../shared/components/star-rating.component';

@Component({
  selector: 'app-statistics',
  standalone: true,
  imports: [CommonModule, LoadingSpinnerComponent, StarRatingComponent],
  template: `
    <div class="statistics-page">
      <div class="page-header">
        <h1 class="page-title">Statistiques Décisionnelles & Audit Métier</h1>
        <p class="page-subtitle">Les 7 indicateurs stratégiques de performance consolidés</p>
      </div>

      @if (isLoading()) {
        <app-loading-spinner message="Calcul des 7 indicateurs stratégiques..."></app-loading-spinner>
      } @else if (stats()) {
        <div class="stats-grid">
          <!-- 1. Réservations par mois -->
          <div class="card-luxury stat-card">
            <h3 class="stat-title">1. Réservations Mensuelles</h3>
            <p class="stat-desc">Nombre de séjours enregistrés par période</p>
            <div class="metric-list">
              @for (item of stats()!.reservationsParMois; track item.mois) {
                <div class="metric-row">
                  <span class="m-label">{{ getMonthName(item.mois) }} {{ item.annee }}</span>
                  <div class="m-bar-wrap">
                    <div class="m-bar" [style.width.%]="(item.nombreReservations / 40) * 100"></div>
                  </div>
                  <strong class="m-val">{{ item.nombreReservations }}</strong>
                </div>
              }
            </div>
          </div>

          <!-- 2. Chiffre d'affaires mensuel -->
          <div class="card-luxury stat-card">
            <h3 class="stat-title">2. Chiffre d'Affaires Mensuel</h3>
            <p class="stat-desc">Encaissements TTC consolidés</p>
            <div class="metric-list">
              @for (item of stats()!.chiffreAffairesMensuel; track item.mois) {
                <div class="metric-row">
                  <span class="m-label">{{ getMonthName(item.mois) }} {{ item.annee }}</span>
                  <div class="m-bar-wrap">
                    <div class="m-bar gold" [style.width.%]="(item.montant / 20000) * 100"></div>
                  </div>
                  <strong class="m-val">{{ item.montant | number:'1.0-0' }} DT</strong>
                </div>
              }
            </div>
          </div>

          <!-- 3. Hôtels les plus réservés -->
          <div class="card-luxury stat-card">
            <h3 class="stat-title">3. Établissements Phares</h3>
            <p class="stat-desc">Volume de réservations par hôtel</p>
            <div class="metric-list">
              @for (item of stats()!.hotelsLesPlusReserves; track item.hotelId) {
                <div class="metric-row">
                  <span class="m-label">{{ item.hotelNom }}</span>
                  <div class="m-bar-wrap">
                    <div class="m-bar" [style.width.%]="(item.nombreReservations / 50) * 100"></div>
                  </div>
                  <strong class="m-val">{{ item.nombreReservations }} rés.</strong>
                </div>
              }
            </div>
          </div>

          <!-- 4. Répartition des paiements par mode -->
          <div class="card-luxury stat-card">
            <h3 class="stat-title">4. Répartition des Moyens de Paiement</h3>
            <p class="stat-desc">Transactions par méthode de règlement</p>
            <div class="payment-methods-breakdown">
              @for (item of stats()!.repartitionPaiementsParMode; track item.mode) {
                <div class="pm-box">
                  <span class="pm-name">{{ item.modeNom }}</span>
                  <span class="pm-count">{{ item.nombrePaiements }} règlements</span>
                  <strong class="pm-total">{{ item.montantTotal | number:'1.2-2' }} DT</strong>
                </div>
              }
            </div>
          </div>

          <!-- 5. Nouveaux utilisateurs par mois -->
          <div class="card-luxury stat-card">
            <h3 class="stat-title">5. Croissance de la Communauté</h3>
            <p class="stat-desc">Nouveaux membres et clients inscrits</p>
            <div class="metric-list">
              @for (item of stats()!.nouveauxUtilisateursParMois; track item.mois) {
                <div class="metric-row">
                  <span class="m-label">{{ getMonthName(item.mois) }} {{ item.annee }}</span>
                  <div class="m-bar-wrap">
                    <div class="m-bar green" [style.width.%]="(item.nombreUtilisateurs / 50) * 100"></div>
                  </div>
                  <strong class="m-val">+{{ item.nombreUtilisateurs }}</strong>
                </div>
              }
            </div>
          </div>

          <!-- 6. Taux d'occupation par hôtel -->
          <div class="card-luxury stat-card">
            <h3 class="stat-title">6. Taux d'Occupation Opérationnel</h3>
            <p class="stat-desc">Chambres occupées / capacité totale</p>
            <div class="metric-list">
              @for (item of stats()!.tauxOccupationHotels; track item.hotelId) {
                <div class="metric-row">
                  <span class="m-label">{{ item.hotelNom }}</span>
                  <div class="m-bar-wrap">
                    <div class="m-bar gold" [style.width.%]="item.tauxOccupationPourcentage"></div>
                  </div>
                  <strong class="m-val">{{ item.tauxOccupationPourcentage | number:'1.1-1' }} %</strong>
                </div>
              }
            </div>
          </div>

          <!-- 7. Évolution des avis et note moyenne -->
          <div class="card-luxury stat-card full-span">
            <h3 class="stat-title">7. Baromètre Qualité & Satisfaction Client</h3>
            <p class="stat-desc">Évolution mensuelle des notations et volume des avis déposés</p>
            <div class="reviews-trend-grid">
              @for (item of stats()!.evolutionAvis; track item.mois) {
                <div class="trend-item">
                  <span class="trend-month">{{ getMonthName(item.mois) }} {{ item.annee }}</span>
                  <div class="trend-rating">
                    <app-star-rating [rating]="item.noteMoyenne"></app-star-rating>
                    <span class="trend-score">{{ item.noteMoyenne | number:'1.1-1' }} / 5</span>
                  </div>
                  <span class="trend-count">{{ item.nombreAvis }} avis vérifiés</span>
                </div>
              }
            </div>
          </div>
        </div>
      }
    </div>
  `,
  styles: [`
    .stats-grid {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 2rem;
    }

    .stat-card {
      padding: 1.75rem;
    }

    .stat-card.full-span {
      grid-column: 1 / -1;
    }

    .stat-title {
      font-family: var(--font-serif);
      font-size: 1.15rem;
      color: var(--forest-green);
      margin-bottom: 0.25rem;
    }

    .stat-desc {
      font-size: 0.8rem;
      color: var(--text-muted);
      margin-bottom: 1.25rem;
    }

    .metric-list {
      display: flex;
      flex-direction: column;
      gap: 0.75rem;
    }

    .metric-row {
      display: flex;
      align-items: center;
      gap: 1rem;
      font-size: 0.85rem;
    }

    .m-label {
      width: 140px;
      color: var(--text-secondary);
      font-weight: 500;
      white-space: nowrap;
      overflow: hidden;
      text-overflow: ellipsis;
    }

    .m-bar-wrap {
      flex: 1;
      height: 10px;
      background: var(--ivory-light);
      border-radius: var(--radius-full);
      overflow: hidden;
    }

    .m-bar {
      height: 100%;
      background: var(--forest-green);
      border-radius: var(--radius-full);
    }

    .m-bar.gold { background: var(--soft-gold); }
    .m-bar.green { background: var(--secondary-green); }

    .m-val {
      min-width: 65px;
      text-align: right;
      color: var(--forest-green);
    }

    /* Moyens de paiement */
    .payment-methods-breakdown {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 1rem;
    }

    .pm-box {
      background: var(--ivory-light);
      padding: 1rem;
      border-radius: var(--radius-sm);
      display: flex;
      flex-direction: column;
      gap: 0.2rem;
    }

    .pm-name {
      font-weight: 600;
      color: var(--forest-green);
      font-size: 0.85rem;
    }

    .pm-count {
      font-size: 0.75rem;
      color: var(--text-muted);
    }

    .pm-total {
      font-size: 1.05rem;
      color: var(--forest-green);
      margin-top: 0.25rem;
    }

    /* Trend avis */
    .reviews-trend-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(130px, 1fr));
      gap: 1rem;
    }

    .trend-item {
      background: var(--ivory-light);
      padding: 1rem;
      border-radius: var(--radius-sm);
      text-align: center;
      display: flex;
      flex-direction: column;
      gap: 0.35rem;
    }

    .trend-month {
      font-weight: 600;
      font-size: 0.825rem;
      color: var(--forest-green);
    }

    .trend-score {
      font-weight: 700;
      font-size: 0.85rem;
      color: var(--forest-green);
      display: block;
    }

    .trend-count {
      font-size: 0.75rem;
      color: var(--text-muted);
    }

    @media (max-width: 900px) {
      .stats-grid {
        grid-template-columns: 1fr;
      }
    }
  `]
})
export class StatisticsComponent implements OnInit {
  private dashboardService = inject(DashboardService);

  stats = signal<StatistiquesDto | null>(null);
  isLoading = signal<boolean>(true);

  ngOnInit(): void {
    this.dashboardService.getStatistiques().subscribe({
      next: (s) => {
        this.stats.set(s);
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
  }

  getMonthName(monthNum: number): string {
    const months = ['Jan', 'Fév', 'Mar', 'Avr', 'Mai', 'Juin', 'Juil', 'Août', 'Sept', 'Oct', 'Nov', 'Déc'];
    return months[monthNum - 1] || `Mois ${monthNum}`;
  }
}
