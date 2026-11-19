import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-confirm-modal',
  standalone: true,
  imports: [CommonModule],
  template: `
    @if (isOpen) {
      <div class="modal-overlay" (click)="onCancel()">
        <div class="modal-dialog" (click)="$event.stopPropagation()">
          <div class="card-header">
            <h3 class="modal-title">{{ title }}</h3>
            <button class="btn-close" (click)="onCancel()">✕</button>
          </div>
          <div class="card-body">
            <p class="modal-message">{{ message }}</p>
          </div>
          <div class="card-footer modal-actions">
            <button class="btn btn-outline btn-sm" (click)="onCancel()">{{ cancelText }}</button>
            <button class="btn btn-sm" [ngClass]="isDanger ? 'btn-danger' : 'btn-primary'" (click)="onConfirm()">
              {{ confirmText }}
            </button>
          </div>
        </div>
      </div>
    }
  `,
  styles: [`
    .modal-title {
      font-size: 1.15rem;
      font-weight: 600;
      color: var(--forest-green);
    }
    .btn-close {
      background: none;
      border: none;
      font-size: 1.1rem;
      cursor: pointer;
      color: var(--text-muted);
    }
    .modal-message {
      color: var(--text-secondary);
      font-size: 0.95rem;
      line-height: 1.5;
    }
    .modal-actions {
      display: flex;
      justify-content: flex-end;
      gap: 0.75rem;
    }
  `]
})
export class ConfirmModalComponent {
  @Input() isOpen: boolean = false;
  @Input() title: string = 'Confirmation requise';
  @Input() message: string = 'Êtes-vous sûr de vouloir continuer cette action ?';
  @Input() confirmText: string = 'Confirmer';
  @Input() cancelText: string = 'Annuler';
  @Input() isDanger: boolean = false;

  @Output() confirmed = new EventEmitter<void>();
  @Output() cancelled = new EventEmitter<void>();

  onConfirm(): void {
    this.confirmed.emit();
  }

  onCancel(): void {
    this.cancelled.emit();
  }
}
