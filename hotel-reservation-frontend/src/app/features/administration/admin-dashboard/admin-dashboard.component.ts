import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { DashboardService } from '../../../core/services/dashboard.service';
import { AuthService } from '../../../core/auth/auth.service';
import { DashboardDto } from '../../../core/models';
import { LoadingSpinnerComponent } from '../../../shared/components/loading-spinner.component';
import { StatusBadgePipe } from '../../../shared/pipes/status-badge.pipe';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule, LoadingSpinnerComponent, StatusBadgePipe],
  template: `
    <div class="admin-dashboard-page">
      <div class="page-header">
        <h1 class="page-title">Tableau de Bord de Direction</h1>
        <p class="page-subtitle">Indicateurs clés de performance et activité en temps réel du parc hôtelier</p>
      </div>

      @if (isLoading()) {
        <app-loading-spinner message="Chargement des indicateurs décisionnels..."></app-loading-spinner>
      } @else if (data()) {
        <!-- Ligne des KPIs -->
        <div class="kpi-grid">
          @if (data()!.chiffreAffaires !== null && data()!.chiffreAffaires !== undefined) {
            <div class="kpi-card card-luxury border-gold">
              <span class="kpi-tag">CHIFFRE D'AFFAIRES</span>
              <div class="kpi-val">{{ data()!.chiffreAffaires | number:'1.2-2' }} DT</div>
              <span class="kpi-sub">Encaissements consolidés</span>
            </div>
          }

          <div class="kpi-card card-luxury">
            <span class="kpi-tag">RÉSERVATIONS DU JOUR</span>
            <div class="kpi-val">{{ data()!.reservationsDuJour }}</div>
            <span class="kpi-sub">Nouvelles arrivées & réservations</span>
          </div>

          <div class="kpi-card card-luxury">
            <span class="kpi-tag">PAIEMENTS EN ATTENTE</span>
            <div class="kpi-val" [style.color]="data()!.paiementsEnAttente > 0 ? 'var(--color-warning)' : 'inherit'">
              {{ data()!.paiementsEnAttente }}
            </div>
            <span class="kpi-sub">Dossiers à régulariser</span>
          </div>

          @if (data()!.nombreHotels !== null && data()!.nombreHotels !== undefined) {
            <div class="kpi-card card-luxury">
              <span class="kpi-tag">PARC HÔTELIER</span>
              <div class="kpi-val">{{ data()!.nombreHotels }}</div>
              <span class="kpi-sub">Établissements en exploitation</span>
            </div>
          }

          @if (data()!.nombreTotalUtilisateurs !== null && data()!.nombreTotalUtilisateurs !== undefined) {
            <div class="kpi-card card-luxury">
              <span class="kpi-tag">MEMBRES INSCRITS</span>
              <div class="kpi-val">{{ data()!.nombreTotalUtilisateurs }}</div>
              <span class="kpi-sub">Comptes actifs enregistrés</span>
            </div>
          }
        </div>

        <!-- Section Graphiques & Performance Mensuelle -->
        @if (data()!.graphiqueReservationsParMois) {
          <div class="charts-row">
            <div class="card-luxury chart-card">
              <div class="chart-header">
                <h3 class="chart-title">Évolution des Réservations</h3>
                <span class="chart-badge">6 derniers mois</span>
              </div>
              <div class="bar-chart-visual">
                @for (entry of getEntries(data()!.graphiqueReservationsParMois); track entry.key) {
                  <div class="bar-col">
                    <div class="bar-fill" [style.height.%]="(entry.value / 35) * 100">
                      <span class="bar-tooltip">{{ entry.value }}</span>
                    </div>
                    <span class="bar-label">{{ entry.key }}</span>
                  </div>
                }
              </div>
            </div>

            @if (data()!.graphiqueRevenusParMois) {
              <div class="card-luxury chart-card">
                <div class="chart-header">
                  <h3 class="chart-title">Revenus Mensuels (DT)</h3>
                  <span class="chart-badge">6 derniers mois</span>
                </div>
                <div class="bar-chart-visual revenue-bars">
                  @for (entry of getEntries(data()!.graphiqueRevenusParMois); track entry.key) {
                    <div class="bar-col">
                      <div class="bar-fill gold-fill" [style.height.%]="(entry.value / 18000) * 100">
                        <span class="bar-tooltip">{{ entry.value | number:'1.0-0' }} DT</span>
                      </div>
                      <span class="bar-label">{{ entry.key }}</span>
                    </div>
                  }
                </div>
              </div>
            }
          </div>
        }

        <!-- Dernières Réservations Enregistrées -->
        <div class="card-luxury recents-card">
          <div class="card-header">
            <h3 class="table-block-title">Dernières Réservations Enregistrées</h3>
            <a routerLink="/admin/reservations" class="btn btn-outline btn-sm">Voir toutes les réservations →</a>
          </div>

          <div class="table-responsive">
            <table class="custom-table">
              <thead>
                <tr>
                  <th>Réf</th>
                  <th>Établissement</th>
                  <th>Chambre</th>
                  <th>Dates Séjour</th>
                  <th>Invités</th>
                  <th>Statut</th>
                  <th style="text-align: right;">Montant</th>
                </tr>
              </thead>
              <tbody>
                @for (res of data()!.dernieresReservations; track res.id) {
                  <tr>
                    <td>
                      <strong style="font-family: monospace; color: var(--forest-green);">
                        #{{ res.id.substring(0, 8) }}
                      </strong>
                    </td>
                    <td>{{ res.hotelNom || 'Hôtel de Prestige' }}</td>
                    <td>Chambre {{ res.chambreNumero || '101' }}</td>
                    <td>{{ res.dateArrivee | date:'dd/MM' }} → {{ res.dateDepart | date:'dd/MM/yyyy' }}</td>
                    <td>{{ res.nbPersonnes }} pers.</td>
                    <td>
                      @let b = (res.statut | statusBadge:'reservation');
                      <span class="badge" [ngClass]="b.cssClass">{{ b.label }}</span>
                    </td>
                    <td style="text-align: right;">
                      <strong>{{ res.totalMontant || 560 }} {{ res.totalDevise || 'DT' }}</strong>
                    </td>
                  </tr>
                }
              </tbody>
            </table>
          </div>
        </div>
      }
    </div>
  `,
  styles: [`
    .admin-dashboard-page {
      display: flex;
      flex-direction: column;
      gap: 2rem;
    }

    .kpi-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
      gap: 1.5rem;
    }

    .kpi-card {
      padding: 1.5rem;
      display: flex;
      flex-direction: column;
      border-left: 4px solid var(--forest-green);
    }

    .kpi-card.border-gold {
      border-left-color: var(--soft-gold);
    }

    .kpi-tag {
      font-size: 0.675rem;
      letter-spacing: 0.12em;
      color: var(--text-muted);
      font-weight: 700;
      margin-bottom: 0.4rem;
    }

    .kpi-val {
      font-size: 1.85rem;
      font-weight: 700;
      color: var(--forest-green);
      line-height: 1.1;
      margin-bottom: 0.35rem;
    }

    .kpi-sub {
      font-size: 0.785rem;
      color: var(--text-secondary);
    }

    /* Graphiques */
    .charts-row {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 2rem;
    }

    .chart-card {
      padding: 1.5rem;
    }

    .chart-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 1.5rem;
    }

    .chart-title {
      font-family: var(--font-serif);
      font-size: 1.15rem;
      color: var(--forest-green);
    }

    .chart-badge {
      font-size: 0.75rem;
      color: var(--text-muted);
      background: var(--ivory-light);
      padding: 0.2rem 0.55rem;
      border-radius: var(--radius-xs);
    }

    .bar-chart-visual {
      display: flex;
      align-items: flex-end;
      justify-content: space-between;
      height: 180px;
      padding-top: 1.5rem;
      gap: 1rem;
      border-bottom: 1px solid var(--border-subtle);
    }

    .bar-col {
      flex: 1;
      height: 100%;
      display: flex;
      flex-direction: column;
      justify-content: flex-end;
      align-items: center;
    }

    .bar-fill {
      width: 100%;
      max-width: 36px;
      background: var(--forest-green);
      border-radius: 4px 4px 0 0;
      position: relative;
      transition: height 400ms ease;
    }

    .bar-fill.gold-fill {
      background: var(--soft-gold);
    }

    .bar-tooltip {
      position: absolute;
      top: -24px;
      left: 50%;
      transform: translateX(-50%);
      font-size: 0.7rem;
      font-weight: 700;
      color: var(--forest-green);
      white-space: nowrap;
    }

    .bar-label {
      font-size: 0.75rem;
      color: var(--text-muted);
      margin-top: 0.5rem;
    }

    /* Récents */
    .recents-card {
      overflow: hidden;
    }

    .table-block-title {
      font-family: var(--font-serif);
      font-size: 1.15rem;
      color: var(--forest-green);
    }

    @media (max-width: 900px) {
      .charts-row {
        grid-template-columns: 1fr;
      }
    }
  `]
})
export class AdminDashboardComponent implements OnInit {
  private dashboardService = inject(DashboardService);
  authService = inject(AuthService);

  data = signal<DashboardDto | null>(null);
  isLoading = signal<boolean>(true);

  ngOnInit(): void {
    this.dashboardService.getDashboard().subscribe({
      next: (d) => {
        this.data.set(d);
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
  }

  getEntries(dict: any): { key: string; value: number }[] {
    if (!dict) return [];
    return Object.keys(dict).map(k => ({ key: k, value: dict[k] }));
  }
}
