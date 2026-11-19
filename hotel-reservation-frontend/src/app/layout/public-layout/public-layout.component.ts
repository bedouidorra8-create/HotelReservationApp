import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';
import { ToastContainerComponent } from '../../shared/components/toast-container.component';

@Component({
  selector: 'app-public-layout',
  standalone: true,
  imports: [CommonModule, RouterModule, ToastContainerComponent],
  template: `
    <app-toast-container></app-toast-container>
    <div class="public-layout">
      <!-- Navbar Haute Gamme -->
      <header class="public-header">
        <div class="container header-container">
          <a routerLink="/" class="brand">
            <span class="brand-crest">✦</span>
            <div class="brand-text">
              <span class="brand-title">HOTELI</span>
              <span class="brand-tagline">DISCOVER TUNISIA</span>
            </div>
          </a>

          <nav class="public-nav">
            <a routerLink="/" routerLinkActive="active" [routerLinkActiveOptions]="{exact: true}" class="nav-item">Accueil</a>
            <a routerLink="/hotels" routerLinkActive="active" class="nav-item">Nos Hôtels</a>
          </nav>

          <div class="header-actions">
            @if (authService.isAuthenticated()) {
              <div class="user-greeting">
                <span class="user-name">{{ authService.currentUser()?.nomComplet }}</span>
                <span class="user-badge">{{ authService.userRole() }}</span>
              </div>
              @if (authService.userRole() === 'Client') {
                <a routerLink="/client/dashboard" class="btn btn-gold btn-sm">Mon Espace</a>
              } @else {
                <a routerLink="/admin/dashboard" class="btn btn-gold btn-sm">Administration</a>
              }
              <button class="btn btn-ghost btn-sm" (click)="authService.logout()">Déconnexion</button>
            } @else {
              <a routerLink="/auth/login" class="btn btn-ghost btn-sm">Connexion</a>
              <a routerLink="/auth/register" class="btn btn-gold btn-sm">Rejoindre le Club</a>
            }
          </div>
        </div>
      </header>

      <!-- Main Content Outlet -->
      <main class="public-main">
        <router-outlet></router-outlet>
      </main>

      <!-- Footer Prestige -->
      <footer class="public-footer">
        <div class="container footer-grid">
          <div class="footer-col brand-col">
            <div class="brand footer-brand">
              <span class="brand-crest">✦</span>
              <span class="brand-title">HOTELI</span>
            </div>
            <p class="footer-desc">
              HOTELI sublime l'hospitalité tunisienne. Des demeures d'artistes de Sidi Bou Saïd aux menzels secrets de Djerba et aux oasis de Tozeur, réservez des séjours d'exception.
            </p>
          </div>

          <div class="footer-col">
            <h4 class="footer-heading">Destinations Clés</h4>
            <ul class="footer-links">
              <li><a routerLink="/hotels">Toutes les adresses</a></li>
              <li><a routerLink="/hotels" [queryParams]="{ville: 'Sidi Bou Saïd'}">Sidi Bou Saïd</a></li>
              <li><a routerLink="/hotels" [queryParams]="{ville: 'Djerba'}">Djerba la Douce</a></li>
              <li><a routerLink="/hotels" [queryParams]="{ville: 'Tozeur'}">Tozeur & Le Sahara</a></li>
              <li><a routerLink="/hotels" [queryParams]="{ville: 'Hammamet'}">Hammamet & Cap Bon</a></li>
            </ul>
          </div>

          <div class="footer-col">
            <h4 class="footer-heading">Espace Membres</h4>
            <ul class="footer-links">
              <li><a routerLink="/client/reservations">Mes Réservations</a></li>
              <li><a routerLink="/client/favorites">Mes Favoris</a></li>
              <li><a routerLink="/client/profile">Mon Profil</a></li>
              <li><a routerLink="/auth/login">Espace Professionnel</a></li>
            </ul>
          </div>

          <div class="footer-col">
            <h4 class="footer-heading">Conciergerie HOTELI</h4>
            <p class="concierge-text">Service dédié 7j/7 pour vos séjours sur mesure en Tunisie :</p>
            <p class="concierge-contact">contact&#64;hoteli.tn</p>
            <p class="concierge-contact">+216 71 000 100</p>
          </div>
        </div>

        <div class="container footer-bottom">
          <p>© 2026 HOTELI — Plateforme d'hospitalité & réservation en Tunisie. Tous droits réservés.</p>
          <div class="footer-bottom-links">
            <span>Confidentialité</span>
            <span>Conditions Générales</span>
            <span>Mentions Légales</span>
          </div>
        </div>
      </footer>
    </div>
  `,
  styles: [`
    .public-layout {
      min-height: 100vh;
      display: flex;
      flex-direction: column;
    }

    .public-header {
      background-color: var(--forest-green);
      color: var(--white);
      border-bottom: 1px solid rgba(214, 183, 122, 0.25);
      position: sticky;
      top: 0;
      z-index: 1000;
    }

    .header-container {
      display: flex;
      align-items: center;
      justify-content: space-between;
      height: 78px;
    }

    .brand {
      display: flex;
      align-items: center;
      gap: 0.75rem;
      text-decoration: none;
      color: var(--white);
    }

    .brand-crest {
      color: var(--soft-gold);
      font-size: 1.6rem;
    }

    .brand-title {
      font-family: var(--font-serif);
      font-size: 1.15rem;
      letter-spacing: 0.12em;
      font-weight: 700;
      color: var(--ivory);
      display: block;
    }

    .brand-tagline {
      font-size: 0.65rem;
      letter-spacing: 0.2em;
      color: var(--soft-gold);
      display: block;
    }

    .public-nav {
      display: flex;
      align-items: center;
      gap: 2rem;
    }

    .nav-item {
      color: var(--ivory);
      font-size: 0.95rem;
      font-weight: 500;
      letter-spacing: 0.03em;
      padding: 0.5rem 0;
      position: relative;
    }

    .nav-item.active, .nav-item:hover {
      color: var(--soft-gold);
    }

    .nav-item.active::after {
      content: '';
      position: absolute;
      bottom: -2px;
      left: 0;
      right: 0;
      height: 2px;
      background-color: var(--soft-gold);
    }

    .header-actions {
      display: flex;
      align-items: center;
      gap: 1rem;
    }

    .user-greeting {
      display: flex;
      flex-direction: column;
      align-items: flex-end;
    }

    .user-name {
      font-size: 0.875rem;
      color: var(--ivory);
      font-weight: 500;
    }

    .user-badge {
      font-size: 0.7rem;
      color: var(--soft-gold);
    }

    .public-main {
      flex: 1;
    }

    .public-footer {
      background-color: var(--forest-green-dark);
      color: var(--ivory);
      padding: 4.5rem 0 2rem;
      border-top: 1px solid rgba(214, 183, 122, 0.2);
    }

    .footer-grid {
      display: grid;
      grid-template-columns: 1.8fr 1fr 1fr 1.2fr;
      gap: 3rem;
      margin-bottom: 3.5rem;
    }

    .footer-desc {
      color: #a4b3ac;
      font-size: 0.885rem;
      line-height: 1.6;
      margin-top: 1rem;
      max-width: 320px;
    }

    .footer-heading {
      color: var(--soft-gold);
      font-size: 0.95rem;
      letter-spacing: 0.08em;
      text-transform: uppercase;
      margin-bottom: 1.25rem;
    }

    .footer-links {
      list-style: none;
      display: flex;
      flex-direction: column;
      gap: 0.75rem;
    }

    .footer-links a {
      color: #c7d2cc;
      font-size: 0.885rem;
    }

    .footer-links a:hover {
      color: var(--soft-gold);
      padding-left: 4px;
    }

    .concierge-text {
      color: #c7d2cc;
      font-size: 0.885rem;
      margin-bottom: 0.5rem;
    }

    .concierge-contact {
      color: var(--ivory);
      font-size: 0.9rem;
      font-weight: 600;
      margin-bottom: 0.35rem;
    }

    .footer-bottom {
      border-top: 1px solid rgba(255, 255, 255, 0.08);
      padding-top: 2rem;
      display: flex;
      justify-content: space-between;
      align-items: center;
      font-size: 0.825rem;
      color: #8c9e95;
    }

    .footer-bottom-links {
      display: flex;
      gap: 1.5rem;
      cursor: pointer;
    }

    @media (max-width: 900px) {
      .footer-grid {
        grid-template-columns: 1fr 1fr;
      }
      .public-nav {
        display: none;
      }
    }

    @media (max-width: 600px) {
      .footer-grid {
        grid-template-columns: 1fr;
        gap: 2rem;
      }
      .footer-bottom {
        flex-direction: column;
        gap: 1rem;
        text-align: center;
      }
    }
  `]
})
export class PublicLayoutComponent {
  authService = inject(AuthService);
}
