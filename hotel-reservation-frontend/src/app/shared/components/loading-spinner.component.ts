import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-loading-spinner',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="spinner-container" [class.overlay]="overlay">
      <div class="spinner-ring"></div>
      @if (message) {
        <p class="spinner-message">{{ message }}</p>
      }
    </div>
  `,
  styles: [`
    .spinner-container {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      padding: 2.5rem;
      gap: 0.75rem;
    }
    .spinner-container.overlay {
      position: absolute;
      inset: 0;
      background: rgba(255, 255, 255, 0.75);
      backdrop-filter: blur(2px);
      z-index: 100;
    }
    .spinner-message {
      color: var(--secondary-green);
      font-size: 0.9rem;
      font-weight: 500;
    }
  `]
})
export class LoadingSpinnerComponent {
  @Input() message: string = 'Chargement en cours...';
  @Input() overlay: boolean = false;
}
