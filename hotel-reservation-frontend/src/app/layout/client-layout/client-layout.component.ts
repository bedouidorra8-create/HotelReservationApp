import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';
import { ToastContainerComponent } from '../../shared/components/toast-container.component';

@Component({
  selector: 'app-client-layout',
  standalone: true,
  imports: [CommonModule, RouterModule, ToastContainerComponent],
  template: `
    <app-toast-container></app-toast-container>
    <div class="client-layout">
      <!-- Header Client Dédié -->
      <header class="client-header">
        <div class="container client-nav-wrapper">
          <a routerLink="/client/dashboard" class="client-brand">
            <span class="crest">✦</span>
            <div>
              <span class="brand-name">HOTELI</span>
              <span class="brand-sub">CLUB VOYAGEURS</span>
            </div>
          </a>

          <nav class="client-menu">
            <a routerLink="/client/dashboard" routerLinkActive="active" class="client-link">
              <span>Tableau de bord</span>
            </a>
            <a routerLink="/hotels" class="client-link">
              <span>Réserver un séjour</span>
            </a>
            <a routerLink="/client/reservations" routerLinkActive="active" class="client-link">
              <span>Mes réservations</span>
            </a>
            <a routerLink="/client/payments" routerLinkActive="active" class="client-link">
              <span>Paiements & Factures</span>
            </a>
            <a routerLink="/client/favorites" routerLinkActive="active" class="client-link">
              <span>Mes favoris</span>
            </a>
            <a routerLink="/client/profile" routerLinkActive="active" class="client-link">
              <span>Mon profil</span>
            </a>
          </nav>

          <div class="client-user-actions">
            <div class="user-chip">
              <span class="user-avatar">👤</span>
              <span class="user-display">{{ authService.currentUser()?.nomComplet || 'Voyageur' }}</span>
            </div>
            <button class="btn btn-ghost btn-sm" (click)="authService.logout()">Quitter</button>
          </div>
        </div>
      </header>

      <!-- Contenu Principal -->
      <main class="client-content">
        <div class="container py-content">
          <router-outlet></router-outlet>
        </div>
      </main>

      <footer class="client-footer">
        <div class="container footer-content">
          <p>© 2026 L'Écrin Hôtelier — Espace Membre Exclusif</p>
          <a routerLink="/" class="back-home">Retour au site public ↗</a>
        </div>
      </footer>
    </div>
  `,
  styles: [`
    .client-layout {
      min-height: 100vh;
      display: flex;
      flex-direction: column;
      background-color: var(--ivory);
    }

    .client-header {
      background-color: var(--forest-green);
      color: var(--white);
      border-bottom: 2px solid var(--soft-gold);
      position: sticky;
      top: 0;
      z-index: 100;
    }

    .client-nav-wrapper {
      display: flex;
      align-items: center;
      justify-content: space-between;
      height: 72px;
      gap: 1rem;
    }

    .client-brand {
      display: flex;
      align-items: center;
      gap: 0.75rem;
      color: var(--white);
      text-decoration: none;
    }

    .crest {
      color: var(--soft-gold);
      font-size: 1.5rem;
    }

    .brand-name {
      font-family: var(--font-serif);
      font-size: 1.05rem;
      letter-spacing: 0.1em;
      font-weight: 700;
      color: var(--ivory);
      display: block;
    }

    .brand-sub {
      font-size: 0.625rem;
      letter-spacing: 0.15em;
      color: var(--soft-gold);
      display: block;
    }

    .client-menu {
      display: flex;
      align-items: center;
      gap: 1.5rem;
    }

    .client-link {
      color: var(--ivory);
      font-size: 0.9rem;
      font-weight: 500;
      padding: 0.5rem 0.25rem;
      position: relative;
      transition: color var(--transition-fast);
    }

    .client-link:hover, .client-link.active {
      color: var(--soft-gold);
    }

    .client-link.active::after {
      content: '';
      position: absolute;
      bottom: -2px;
      left: 0;
      right: 0;
      height: 2px;
      background-color: var(--soft-gold);
    }

    .client-user-actions {
      display: flex;
      align-items: center;
      gap: 0.75rem;
    }

    .user-chip {
      display: flex;
      align-items: center;
      gap: 0.5rem;
      background: rgba(255, 255, 255, 0.1);
      padding: 0.35rem 0.85rem;
      border-radius: var(--radius-full);
      border: 1px solid rgba(214, 183, 122, 0.3);
    }

    .user-avatar {
      font-size: 0.9rem;
    }

    .user-display {
      font-size: 0.825rem;
      color: var(--ivory);
      font-weight: 500;
    }

    .client-content {
      flex: 1;
    }

    .py-content {
      padding-top: 2.5rem;
      padding-bottom: 3.5rem;
    }

    .client-footer {
      background-color: var(--forest-green-dark);
      color: #9ab0a6;
      font-size: 0.825rem;
      padding: 1.5rem 0;
      border-top: 1px solid rgba(214, 183, 122, 0.2);
    }

    .footer-content {
      display: flex;
      justify-content: space-between;
      align-items: center;
    }

    .back-home {
      color: var(--soft-gold);
      font-weight: 500;
    }

    @media (max-width: 992px) {
      .client-menu {
        overflow-x: auto;
        padding-bottom: 0.25rem;
      }
      .client-nav-wrapper {
        flex-wrap: wrap;
        height: auto;
        padding: 0.75rem 1rem;
      }
    }
  `]
})
export class ClientLayoutComponent {
  authService = inject(AuthService);
}
