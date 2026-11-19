import { Component, Input, Output, EventEmitter, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AvisService } from '../../../core/services/avis.service';
import { NotificationService } from '../../../core/services/notification.service';
import { StarRatingComponent } from '../../../shared/components/star-rating.component';

@Component({
  selector: 'app-review-modal',
  standalone: true,
  imports: [CommonModule, FormsModule, StarRatingComponent],
  template: `
    @if (isOpen) {
      <div class="modal-overlay" (click)="onClose()">
        <div class="modal-dialog" (click)="$event.stopPropagation()">
          <div class="card-header">
            <h3 class="modal-title">Partager votre Expérience</h3>
            <button class="btn-close" (click)="onClose()">✕</button>
          </div>

          <div class="card-body">
            <p class="modal-subtitle">
              Votre avis compte énormément pour maintenir nos standards d'excellence.
            </p>

            <div class="rating-select-group">
              <label class="form-label">Note générale du séjour (1 à 5 étoiles) :</label>
              <app-star-rating [rating]="note" [interactive]="true" (ratingChange)="note = $event"></app-star-rating>
            </div>

            <div class="form-group">
              <label class="form-label">Votre commentaire & impressions :</label>
              <textarea 
                [(ngModel)]="commentaire" 
                rows="4" 
                placeholder="Qualité de l'accueil, confort de la chambre, gastronomie, spa..." 
                class="form-control">
              </textarea>
            </div>
          </div>

          <div class="card-footer modal-actions">
            <button class="btn btn-outline btn-sm" (click)="onClose()">Annuler</button>
            <button 
              class="btn btn-gold btn-sm" 
              [disabled]="isSubmitting() || !commentaire.trim()" 
              (click)="submitReview()">
              @if (isSubmitting()) {
                <span>Envoi en cours...</span>
              } @else {
                <span>Publier mon avis</span>
              }
            </button>
          </div>
        </div>
      </div>
    }
  `,
  styles: [`
    .modal-title {
      font-family: var(--font-serif);
      font-size: 1.25rem;
      color: var(--forest-green);
    }
    .modal-subtitle {
      color: var(--text-secondary);
      font-size: 0.885rem;
      margin-bottom: 1.25rem;
    }
    .btn-close {
      background: none;
      border: none;
      font-size: 1.1rem;
      cursor: pointer;
      color: var(--text-muted);
    }
    .rating-select-group {
      margin-bottom: 1.25rem;
    }
    .modal-actions {
      display: flex;
      justify-content: flex-end;
      gap: 0.75rem;
    }
  `]
})
export class ReviewModalComponent {
  @Input() isOpen: boolean = false;
  @Input() hotelId!: string;
  @Input() reservationId!: string;

  @Output() closed = new EventEmitter<void>();
  @Output() submitted = new EventEmitter<void>();

  private avisService = inject(AvisService);
  private notification = inject(NotificationService);

  note: number = 5;
  commentaire: string = '';
  isSubmitting = signal<boolean>(false);

  onClose(): void {
    this.closed.emit();
  }

  submitReview(): void {
    if (!this.hotelId || !this.reservationId || !this.commentaire.trim()) return;

    this.isSubmitting.set(true);
    this.avisService.createReview({
      hotelId: this.hotelId,
      reservationId: this.reservationId,
      note: this.note,
      commentaire: this.commentaire
    }).subscribe({
      next: () => {
        this.isSubmitting.set(false);
        this.notification.success('Votre avis a été soumis avec succès ! Il sera publié après validation.');
        this.submitted.emit();
        this.onClose();
      },
      error: (err) => {
        this.isSubmitting.set(false);
        this.notification.error(err.error?.message || 'Impossible de soumettre votre avis.');
      }
    });
  }
}
