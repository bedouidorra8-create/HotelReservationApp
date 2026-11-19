import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-empty-state',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="empty-state">
      <div class="empty-state-icon">{{ icon }}</div>
      <h3 class="empty-state-title">{{ title }}</h3>
      <p class="empty-state-desc">{{ description }}</p>
      <div class="empty-state-actions">
        <ng-content></ng-content>
      </div>
    </div>
  `,
  styles: [`
    .empty-state {
      text-align: center;
      padding: 3.5rem 1.5rem;
      background: var(--white);
      border-radius: var(--radius-md);
      border: 1px dashed var(--border-subtle);
      margin: 1.5rem 0;
    }
    .empty-state-icon {
      font-size: 2.8rem;
      margin-bottom: 0.75rem;
      color: var(--soft-gold);
    }
    .empty-state-title {
      font-size: 1.25rem;
      color: var(--forest-green);
      margin-bottom: 0.5rem;
      font-weight: 600;
    }
    .empty-state-desc {
      color: var(--text-secondary);
      max-width: 450px;
      margin: 0 auto 1.5rem;
      font-size: 0.925rem;
    }
    .empty-state-actions {
      display: flex;
      justify-content: center;
      gap: 0.75rem;
    }
  `]
})
export class EmptyStateComponent {
  @Input() icon: string = '🏨';
  @Input() title: string = 'Aucun élément trouvé';
  @Input() description: string = 'Aucun résultat ne correspond aux critères sélectionnés.';
}
