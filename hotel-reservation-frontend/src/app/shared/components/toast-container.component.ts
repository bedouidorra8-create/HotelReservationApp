import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NotificationService } from '../../core/services/notification.service';

@Component({
  selector: 'app-toast-container',
  standalone: true,
  imports: [CommonModule],
  template: `
    @if (notificationService.toasts().length > 0) {
      <div class="toast-container">
        @for (toast of notificationService.toasts(); track toast.id) {
          <div class="toast-item" [ngClass]="'toast-' + toast.type">
            <div class="toast-icon">
              @switch (toast.type) {
                @case ('success') { <span>✓</span> }
                @case ('error') { <span>✕</span> }
                @case ('warning') { <span>⚠</span> }
                @default { <span>ℹ</span> }
              }
            </div>
            <div class="toast-content">{{ toast.message }}</div>
            <button class="toast-close" (click)="notificationService.remove(toast.id)" aria-label="Fermer">✕</button>
          </div>
        }
      </div>
    }
  `,
  styles: [`
    .toast-icon {
      font-weight: bold;
      font-size: 1.1rem;
    }
    .toast-content {
      flex: 1;
      font-size: 0.885rem;
      line-height: 1.4;
      color: var(--text-primary);
    }
    .toast-close {
      background: none;
      border: none;
      color: var(--text-muted);
      cursor: pointer;
      font-size: 0.9rem;
      padding: 0 0.2rem;
    }
    .toast-close:hover {
      color: var(--text-primary);
    }
  `]
})
export class ToastContainerComponent {
  notificationService = inject(NotificationService);
}
