import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AdministrationService } from '../../../core/services/administration.service';
import { NotificationService } from '../../../core/services/notification.service';
import { Avis, StatutAvis } from '../../../core/models';
import { StarRatingComponent } from '../../../shared/components/star-rating.component';
import { StatusBadgePipe } from '../../../shared/pipes/status-badge.pipe';
import { LoadingSpinnerComponent } from '../../../shared/components/loading-spinner.component';
import { EmptyStateComponent } from '../../../shared/components/empty-state.component';

@Component({
  selector: 'app-reviews-moderation',
  standalone: true,
  imports: [CommonModule, FormsModule, StarRatingComponent, StatusBadgePipe, LoadingSpinnerComponent, EmptyStateComponent],
  template: `
    <div class="reviews-moderation-page">
      <div class="page-header">
        <h1 class="page-title">Modération des Avis Clients</h1>
        <p class="page-subtitle">Validez ou rejetez les témoignages avant publication sur les fiches hôtels</p>
      </div>

      <!-- Filtres de modération -->
      <div class="card-luxury p-filters">
        <div class="tabs-row">
          <button 
            class="tab-btn" 
            [class.active]="filterStatut() === null" 
            (click)="filterStatut.set(null)">
            Tous les avis ({{ reviews().length }})
          </button>
          <button 
            class="tab-btn" 
            [class.active]="filterStatut() === 1" 
            (click)="filterStatut.set(1)">
            En attente ({{ pendingCount() }})
          </button>
          <button 
            class="tab-btn" 
            [class.active]="filterStatut() === 2" 
            (click)="filterStatut.set(2)">
            Validés
          </button>
          <button 
            class="tab-btn" 
            [class.active]="filterStatut() === 3" 
            (click)="filterStatut.set(3)">
            Rejetés
          </button>
        </div>
      </div>

      @if (isLoading()) {
        <app-loading-spinner message="Chargement des avis..."></app-loading-spinner>
      } @else if (filteredReviews().length === 0) {
        <app-empty-state
          icon="⭐"
          title="Aucun avis à modérer"
          description="Tous les témoignages soumis ont été traités.">
        </app-empty-state>
      } @else {
        <div class="reviews-list-grid">
          @for (rev of filteredReviews(); track rev.id) {
            <div class="review-mod-card card-luxury">
              <div class="rev-head">
                <div>
                  <h3 class="author-name">{{ rev.clientNom || 'Hôte Privilège' }}</h3>
                  <span class="hotel-mention">Pour : {{ rev.hotelNom || 'Établissement Haut de Gamme' }}</span>
                </div>
                <div class="rev-stat-badge">
                  @let b = (rev.statut | statusBadge:'avis');
                  <span class="badge" [ngClass]="b.cssClass">{{ b.label }}</span>
                </div>
              </div>

              <div class="rev-content">
                <div class="rating-bar">
                  <app-star-rating [rating]="rev.note"></app-star-rating>
                  <span class="date-tag">{{ rev.date | date:'dd/MM/yyyy à HH:mm' }}</span>
                </div>
                <p class="comment-text">"{{ rev.commentaire }}"</p>
              </div>

              @if (rev.statut === 1) {
                <div class="mod-actions">
                  <button class="btn btn-outline btn-sm" style="color: var(--color-error); border-color: var(--color-error);" (click)="reject(rev.id)">
                    ✕ Rejeter
                  </button>
                  <button class="btn btn-primary btn-sm" (click)="validate(rev.id)">
                    ✓ Valider & Publier
                  </button>
                </div>
              }
            </div>
          }
        </div>
      }
    </div>
  `,
  styles: [`
    .p-filters {
      padding: 1rem 1.5rem;
      margin-bottom: 2rem;
    }

    .tabs-row {
      display: flex;
      gap: 1rem;
    }

    .tab-btn {
      padding: 0.4rem 1rem;
      border: 1px solid var(--border-subtle);
      border-radius: var(--radius-sm);
      background: var(--white);
      color: var(--text-secondary);
      font-weight: 600;
      font-size: 0.85rem;
      cursor: pointer;
    }

    .tab-btn.active {
      background: var(--forest-green);
      color: var(--white);
      border-color: var(--forest-green);
    }

    .reviews-list-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(340px, 1fr));
      gap: 1.5rem;
    }

    .review-mod-card {
      padding: 1.5rem;
      display: flex;
      flex-direction: column;
      justify-content: space-between;
    }

    .rev-head {
      display: flex;
      justify-content: space-between;
      align-items: flex-start;
      margin-bottom: 1rem;
    }

    .author-name {
      font-size: 1.05rem;
      color: var(--forest-green);
      font-weight: 600;
    }

    .hotel-mention {
      font-size: 0.785rem;
      color: var(--text-muted);
    }

    .rating-bar {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 0.75rem;
    }

    .date-tag {
      font-size: 0.75rem;
      color: var(--text-muted);
    }

    .comment-text {
      font-size: 0.9rem;
      color: var(--text-secondary);
      line-height: 1.5;
      font-style: italic;
      margin-bottom: 1.5rem;
    }

    .mod-actions {
      display: flex;
      justify-content: flex-end;
      gap: 0.75rem;
      border-top: 1px solid var(--border-subtle);
      padding-top: 1rem;
    }
  `]
})
export class ReviewsModerationComponent implements OnInit {
  private adminService = inject(AdministrationService);
  private notification = inject(NotificationService);

  reviews = signal<Avis[]>([]);
  isLoading = signal<boolean>(true);
  filterStatut = signal<number | null>(null);

  pendingCount = () => this.reviews().filter(r => r.statut === StatutAvis.EnAttente).length;

  filteredReviews = () => {
    const s = this.filterStatut();
    if (s === null) return this.reviews();
    return this.reviews().filter(r => r.statut === s);
  };

  ngOnInit(): void {
    this.loadReviews();
  }

  loadReviews(): void {
    this.isLoading.set(true);
    this.adminService.getAvisEnAttente().subscribe({
      next: (res) => {
        this.reviews.set(res);
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
  }

  validate(id: string): void {
    this.adminService.validateReview(id).subscribe({
      next: () => {
        this.notification.success('Avis validé et rendu public.');
        this.loadReviews();
      },
      error: () => this.notification.error('Erreur lors de la validation.')
    });
  }

  reject(id: string): void {
    this.adminService.rejectReview(id).subscribe({
      next: () => {
        this.notification.warning('Avis rejeté.');
        this.loadReviews();
      },
      error: () => this.notification.error('Erreur lors du rejet.')
    });
  }
}
