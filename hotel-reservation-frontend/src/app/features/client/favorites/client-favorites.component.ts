import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FavorisService } from '../../../core/services/favoris.service';
import { AuthService } from '../../../core/auth/auth.service';
import { Favori } from '../../../core/models';
import { LoadingSpinnerComponent } from '../../../shared/components/loading-spinner.component';
import { EmptyStateComponent } from '../../../shared/components/empty-state.component';

@Component({
  selector: 'app-client-favorites',
  standalone: true,
  imports: [CommonModule, RouterModule, LoadingSpinnerComponent, EmptyStateComponent],
  template: `
    <div class="favorites-page">
      <div class="page-header">
        <h1 class="page-title">Mes Établissements Favoris</h1>
        <p class="page-subtitle">Accédez en un clic à vos adresses d'exception enregistrées</p>
      </div>

      @if (isLoading()) {
        <app-loading-spinner message="Chargement de vos favoris..."></app-loading-spinner>
      } @else if (favorisService.favorites().length === 0) {
        <app-empty-state
          icon="♥"
          title="Vous n'avez aucun favori"
          description="Enregistrez vos établissements coups de cœur en cliquant sur le cœur lors de votre navigation.">
          <a routerLink="/hotels" class="btn btn-gold btn-sm">Découvrir les hôtels</a>
        </app-empty-state>
      } @else {
        <div class="favorites-grid">
          @for (fav of favorisService.favorites(); track fav.id) {
            <div class="fav-card card-luxury">
              <div class="fav-thumb">
                <img [src]="fav.hotelImage || 'https://images.unsplash.com/photo-1566073771259-6a8506099945?auto=format&fit=crop&w=600&q=80'" alt="Hôtel" class="fav-img" />
                <button class="remove-btn" (click)="remove(fav.id)" title="Retirer des favoris">✕</button>
              </div>

              <div class="fav-info">
                <span class="fav-city">📍 {{ fav.hotelVille || 'Destination Prestige' }}</span>
                <h3 class="fav-title">{{ fav.hotelNom || 'Hôtel Palace & Spa' }}</h3>
                <p class="fav-price">Dès {{ fav.prixMin || 280 }} DT / nuit</p>

                <div class="fav-actions">
                  <a [routerLink]="['/hotels', fav.hotelId]" class="btn btn-primary btn-sm">Voir l'établissement</a>
                </div>
              </div>
            </div>
          }
        </div>
      }
    </div>
  `,
  styles: [`
    .favorites-page {
      max-width: 980px;
      margin: 0 auto;
    }

    .favorites-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
      gap: 1.75rem;
    }

    .fav-card {
      overflow: hidden;
      display: flex;
      flex-direction: column;
    }

    .fav-thumb {
      position: relative;
      height: 180px;
    }

    .fav-img {
      width: 100%;
      height: 100%;
      object-fit: cover;
    }

    .remove-btn {
      position: absolute;
      top: 0.65rem;
      right: 0.65rem;
      width: 28px;
      height: 28px;
      border-radius: 50%;
      background: rgba(255, 255, 255, 0.9);
      border: none;
      font-size: 0.8rem;
      cursor: pointer;
      display: flex;
      align-items: center;
      justify-content: center;
      color: var(--text-primary);
      transition: all var(--transition-fast);
    }

    .remove-btn:hover {
      background: var(--color-error);
      color: var(--white);
    }

    .fav-info {
      padding: 1.25rem;
      display: flex;
      flex-direction: column;
      flex: 1;
    }

    .fav-city {
      font-size: 0.75rem;
      color: var(--secondary-green);
      font-weight: 600;
    }

    .fav-title {
      font-family: var(--font-serif);
      font-size: 1.15rem;
      color: var(--forest-green);
      margin: 0.25rem 0 0.5rem;
    }

    .fav-price {
      font-size: 0.885rem;
      font-weight: 600;
      color: var(--forest-green);
      margin-bottom: 1rem;
    }

    .fav-actions {
      margin-top: auto;
    }
  `]
})
export class ClientFavoritesComponent implements OnInit {
  favorisService = inject(FavorisService);
  private authService = inject(AuthService);

  isLoading = signal<boolean>(true);

  ngOnInit(): void {
    const user = this.authService.currentUser();
    if (user?.utilisateurId) {
      this.favorisService.getFavorites(user.utilisateurId).subscribe({
        next: () => this.isLoading.set(false),
        error: () => this.isLoading.set(false)
      });
    } else {
      this.isLoading.set(false);
    }
  }

  remove(id: string): void {
    this.favorisService.removeFavorite(id).subscribe();
  }
}
