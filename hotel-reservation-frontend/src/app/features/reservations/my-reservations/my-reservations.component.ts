import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ReservationService } from '../../../core/services/reservation.service';
import { AuthService } from '../../../core/auth/auth.service';
import { NotificationService } from '../../../core/services/notification.service';
import { Reservation, StatutReservation } from '../../../core/models';
import { StatusBadgePipe } from '../../../shared/pipes/status-badge.pipe';
import { LoadingSpinnerComponent } from '../../../shared/components/loading-spinner.component';
import { EmptyStateComponent } from '../../../shared/components/empty-state.component';
import { ConfirmModalComponent } from '../../../shared/components/confirm-modal.component';
import { ReviewModalComponent } from '../../reviews/review-modal/review-modal.component';

@Component({
  selector: 'app-my-reservations',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    StatusBadgePipe,
    LoadingSpinnerComponent,
    EmptyStateComponent,
    ConfirmModalComponent,
    ReviewModalComponent
  ],
  template: `
    <div class="my-reservations-page">
      <div class="page-header">
        <h1 class="page-title">Mes Réservations</h1>
        <p class="page-subtitle">Consultez l'historique et les détails de vos séjours d'exception</p>
      </div>

      <!-- Onglets Statuts -->
      <div class="tabs-nav">
        <button 
          class="tab-btn" 
          [class.active]="activeTab() === 'upcoming'"
          (click)="activeTab.set('upcoming')">
          À venir ({{ upcomingReservations().length }})
        </button>
        <button 
          class="tab-btn" 
          [class.active]="activeTab() === 'completed'"
          (click)="activeTab.set('completed')">
          Terminées ({{ completedReservations().length }})
        </button>
        <button 
          class="tab-btn" 
          [class.active]="activeTab() === 'cancelled'"
          (click)="activeTab.set('cancelled')">
          Annulées ({{ cancelledReservations().length }})
        </button>
      </div>

      @if (isLoading()) {
        <app-loading-spinner message="Chargement de vos séjours..."></app-loading-spinner>
      } @else if (currentList().length === 0) {
        <app-empty-state
          icon="📅"
          title="Aucun séjour dans cette catégorie"
          description="Vous n'avez aucune réservation correspondant à cet onglet actuellement.">
          <a routerLink="/hotels" class="btn btn-gold btn-sm">Explorer nos hôtels</a>
        </app-empty-state>
      } @else {
        <div class="reservations-cards-list">
          @for (res of currentList(); track res.id) {
            <div class="reservation-card card-luxury">
              <div class="res-header-bar">
                <div class="res-num-badge">
                  <span class="ref-label">RÉFÉRENCE</span>
                  <span class="ref-val">#{{ res.id.substring(0, 8) }}</span>
                </div>
                @let badgeInfo = (res.statut | statusBadge:'reservation');
                <span class="badge" [ngClass]="badgeInfo.cssClass">
                  {{ badgeInfo.label }}
                </span>
              </div>

              <div class="res-body-grid">
                <div class="res-details-col">
                  <h3 class="hotel-title">{{ res.hotelNom || 'Établissement de Prestige' }}</h3>
                  <p class="room-spec">Chambre n° {{ res.chambreNumero || 'N/A' }} • {{ res.nbPersonnes }} voyageur(s)</p>
                  
                  <div class="dates-range">
                    <span class="date-point">Arrivée : <strong>{{ res.dateArrivee | date:'dd MMM yyyy' }}</strong></span>
                    <span class="arrow-sep">→</span>
                    <span class="date-point">Départ : <strong>{{ res.dateDepart | date:'dd MMM yyyy' }}</strong></span>
                    <span class="nights-chip">{{ res.nbNuits }} nuitée(s)</span>
                  </div>
                </div>

                <div class="res-pricing-col">
                  <span class="price-title">Montant Total</span>
                  <span class="price-figure">{{ res.totalMontant || (res.nbNuits * 280) }} {{ res.totalDevise || 'EUR' }}</span>
                </div>
              </div>

              <div class="res-actions-bar">
                <!-- Voir Facture -->
                @if (res.statut === 2 || res.statut === 4) {
                  <a [routerLink]="['/client/invoice', res.id]" class="btn btn-outline btn-sm">
                    📄 Facture du séjour
                  </a>
                }

                <!-- Laisser un Avis si terminé -->
                @if (res.statut === 4) {
                  <button class="btn btn-gold btn-sm" (click)="openReviewModal(res)">
                    ⭐ Évaluer ce séjour
                  </button>
                }

                <!-- Annuler si à venir -->
                @if (res.statut === 1 || res.statut === 2) {
                  <button class="btn btn-danger btn-sm" (click)="askCancel(res.id)">
                    Annuler la réservation
                  </button>
                }
              </div>
            </div>
          }
        </div>
      }

      <!-- Modale de Confirmation Annulation -->
      <app-confirm-modal
        [isOpen]="isCancelModalOpen()"
        title="Annuler cette réservation ?"
        message="Attention : l'annulation de votre réservation libérera la chambre pour d'autres voyageurs. Souhaitez-vous continuer ?"
        confirmText="Oui, Annuler"
        [isDanger]="true"
        (confirmed)="executeCancel()"
        (cancelled)="isCancelModalOpen.set(false)">
      </app-confirm-modal>

      <!-- Modale de Dépôt d'Avis -->
      @if (reviewTarget()) {
        <app-review-modal
          [isOpen]="isReviewModalOpen()"
          [hotelId]="reviewTarget()!.chambreId || '11111111-1111-1111-1111-111111111111'"
          [reservationId]="reviewTarget()!.id"
          (closed)="isReviewModalOpen.set(false)"
          (submitted)="loadReservations()">
        </app-review-modal>
      }
    </div>
  `,
  styles: [`
    .my-reservations-page {
      max-width: 960px;
      margin: 0 auto;
    }

    .tabs-nav {
      display: flex;
      gap: 0.5rem;
      border-bottom: 2px solid var(--border-subtle);
      margin-bottom: 2rem;
    }

    .tab-btn {
      padding: 0.75rem 1.5rem;
      background: none;
      border: none;
      font-size: 0.95rem;
      font-weight: 600;
      color: var(--text-secondary);
      cursor: pointer;
      border-bottom: 3px solid transparent;
      margin-bottom: -2px;
      transition: all var(--transition-fast);
    }

    .tab-btn.active {
      color: var(--forest-green);
      border-bottom-color: var(--soft-gold);
    }

    .reservations-cards-list {
      display: flex;
      flex-direction: column;
      gap: 1.5rem;
    }

    .reservation-card {
      padding: 1.75rem;
    }

    .res-header-bar {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 1.25rem;
      border-bottom: 1px solid var(--border-subtle);
      padding-bottom: 0.75rem;
    }

    .res-num-badge {
      display: flex;
      align-items: center;
      gap: 0.5rem;
    }

    .ref-label {
      font-size: 0.7rem;
      letter-spacing: 0.1em;
      color: var(--text-muted);
      font-weight: 700;
    }

    .ref-val {
      font-family: monospace;
      font-size: 0.95rem;
      font-weight: 700;
      color: var(--forest-green);
    }

    .res-body-grid {
      display: grid;
      grid-template-columns: 1fr auto;
      gap: 1.5rem;
      margin-bottom: 1.5rem;
    }

    .hotel-title {
      font-family: var(--font-serif);
      font-size: 1.35rem;
      color: var(--forest-green);
      margin-bottom: 0.25rem;
    }

    .room-spec {
      font-size: 0.85rem;
      color: var(--text-secondary);
      margin-bottom: 0.75rem;
    }

    .dates-range {
      display: flex;
      align-items: center;
      flex-wrap: wrap;
      gap: 0.75rem;
      font-size: 0.885rem;
      color: var(--text-primary);
    }

    .nights-chip {
      background: var(--secondary-green-subtle);
      color: var(--forest-green);
      font-size: 0.75rem;
      font-weight: 600;
      padding: 0.2rem 0.55rem;
      border-radius: var(--radius-xs);
    }

    .res-pricing-col {
      text-align: right;
      display: flex;
      flex-direction: column;
      justify-content: center;
      padding-left: 1.5rem;
      border-left: 1px solid var(--border-subtle);
    }

    .price-title {
      font-size: 0.75rem;
      color: var(--text-muted);
    }

    .price-figure {
      font-size: 1.45rem;
      font-weight: 700;
      color: var(--forest-green);
    }

    .res-actions-bar {
      display: flex;
      justify-content: flex-end;
      gap: 0.75rem;
      border-top: 1px solid var(--border-subtle);
      padding-top: 1rem;
    }

    @media (max-width: 600px) {
      .res-body-grid {
        grid-template-columns: 1fr;
      }
      .res-pricing-col {
        text-align: left;
        border-left: none;
        padding-left: 0;
        border-top: 1px solid var(--border-subtle);
        padding-top: 0.75rem;
      }
    }
  `]
})
export class MyReservationsComponent implements OnInit {
  private reservationService = inject(ReservationService);
  private authService = inject(AuthService);
  private notification = inject(NotificationService);

  reservations = signal<Reservation[]>([]);
  isLoading = signal<boolean>(true);
  activeTab = signal<'upcoming' | 'completed' | 'cancelled'>('upcoming');

  isCancelModalOpen = signal<boolean>(false);
  targetCancelId = signal<string | null>(null);

  isReviewModalOpen = signal<boolean>(false);
  reviewTarget = signal<Reservation | null>(null);

  upcomingReservations = computed(() => 
    this.reservations().filter(r => r.statut === StatutReservation.EnAttente || r.statut === StatutReservation.Confirmee)
  );

  completedReservations = computed(() => 
    this.reservations().filter(r => r.statut === StatutReservation.Terminee)
  );

  cancelledReservations = computed(() => 
    this.reservations().filter(r => r.statut === StatutReservation.Annulee)
  );

  currentList = computed(() => {
    switch (this.activeTab()) {
      case 'upcoming': return this.upcomingReservations();
      case 'completed': return this.completedReservations();
      case 'cancelled': return this.cancelledReservations();
    }
  });

  ngOnInit(): void {
    this.loadReservations();
  }

  loadReservations(): void {
    const user = this.authService.currentUser();
    if (!user?.utilisateurId) return;

    this.isLoading.set(true);
    this.reservationService.getMyReservations(user.utilisateurId).subscribe({
      next: (res) => {
        this.reservations.set(res);
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
  }

  askCancel(id: string): void {
    this.targetCancelId.set(id);
    this.isCancelModalOpen.set(true);
  }

  executeCancel(): void {
    const id = this.targetCancelId();
    if (!id) return;

    this.reservationService.cancelReservation(id).subscribe({
      next: () => {
        this.notification.success('Votre réservation a été annulée conformément aux règles du club.');
        this.isCancelModalOpen.set(false);
        this.loadReservations();
      },
      error: (err) => {
        this.notification.error(err.error?.message || "Impossible d'annuler cette réservation.");
        this.isCancelModalOpen.set(false);
      }
    });
  }

  openReviewModal(res: Reservation): void {
    this.reviewTarget.set(res);
    this.isReviewModalOpen.set(true);
  }
}
