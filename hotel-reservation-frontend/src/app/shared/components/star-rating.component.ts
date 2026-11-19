import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-star-rating',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="stars-container" [class.interactive]="interactive">
      @for (star of [1, 2, 3, 4, 5]; track star) {
        <span 
          class="star" 
          [class.filled]="star <= rating"
          (click)="onSelect(star)">
          ★
        </span>
      }
    </div>
  `,
  styles: [`
    .stars-container {
      display: inline-flex;
      gap: 2px;
      color: #d1c7b7;
      font-size: 1.25rem;
      user-select: none;
    }
    .star.filled {
      color: var(--soft-gold);
    }
    .stars-container.interactive .star {
      cursor: pointer;
      transition: transform 150ms ease, color 150ms ease;
    }
    .stars-container.interactive .star:hover {
      transform: scale(1.2);
      color: var(--soft-gold-hover);
    }
  `]
})
export class StarRatingComponent {
  @Input() rating: number = 5;
  @Input() interactive: boolean = false;
  @Output() ratingChange = new EventEmitter<number>();

  onSelect(value: number): void {
    if (this.interactive) {
      this.rating = value;
      this.ratingChange.emit(value);
    }
  }
}
