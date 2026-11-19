import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';
import { ToastContainerComponent } from '../../shared/components/toast-container.component';

@Component({
  selector: 'app-admin-layout',
  standalone: true,
  imports: [CommonModule, RouterModule, ToastContainerComponent],
  template: `
    <app-toast-container></app-toast-container>
    <div class="admin-wrapper" [class.sidebar-collapsed]="isSidebarCollapsed()">
      <!-- Sidebar Navigation -->
      <aside class="admin-sidebar">
        <div class="sidebar-header">
          <a routerLink="/admin/dashboard" class="sidebar-brand">
            <span class="brand-crest">✦</span>
            <div class="brand-info">
              <span class="brand-title">HOTELI</span>
              <span class="brand-mode">CONSOLE DIRECTION</span>
            </div>
          </a>
        </div>

        <nav class="sidebar-nav">
          <div class="nav-section-label">PILOTAGE GÉNÉRAL</div>
          <a routerLink="/admin/dashboard" routerLinkActive="active" class="side-link">
            <span class="side-icon">📊</span>
            <span class="side-text">Tableau de bord</span>
          </a>

          <div class="nav-section-label">EXPLOITATION HÔTELIÈRE</div>
          <a routerLink="/admin/hotels" routerLinkActive="active" class="side-link">
            <span class="side-icon">🏨</span>
            <span class="side-text">Gestion des Hôtels</span>
          </a>
          <a routerLink="/admin/rooms" routerLinkActive="active" class="side-link">
            <span class="side-icon">🛏️</span>
            <span class="side-text">Gestion des Chambres</span>
          </a>
          <a routerLink="/admin/reservations" routerLinkActive="active" class="side-link">
            <span class="side-icon">📅</span>
            <span class="side-text">Réservations</span>
          </a>
          <a routerLink="/admin/calendar" routerLinkActive="active" class="side-link">
            <span class="side-icon">🗓️</span>
            <span class="side-text">Calendrier Disponibilité</span>
          </a>

          <div class="nav-section-label">FINANCES & CONFIANCE</div>
          <a routerLink="/admin/payments" routerLinkActive="active" class="side-link">
            <span class="side-icon">💳</span>
            <span class="side-text">Gestion des Paiements</span>
          </a>

          @if (authService.userRole() === 'Administrateur') {
            <a routerLink="/admin/reviews" routerLinkActive="active" class="side-link">
              <span class="side-icon">⭐</span>
              <span class="side-text">Modération des Avis</span>
            </a>
            <div class="nav-section-label">DIRECTION & GOUVERNANCE</div>
            <a routerLink="/admin/statistics" routerLinkActive="active" class="side-link">
              <span class="side-icon">📈</span>
              <span class="side-text">Statistiques Avancées</span>
            </a>
            <a routerLink="/admin/agents" routerLinkActive="active" class="side-link">
              <span class="side-icon">👨‍💼</span>
              <span class="side-text">Équipe & Agents</span>
            </a>
            <a routerLink="/admin/clients" routerLinkActive="active" class="side-link">
              <span class="side-icon">👤</span>
              <span class="side-text">Voyageurs & Clients</span>
            </a>
            <a routerLink="/admin/users" routerLinkActive="active" class="side-link">
              <span class="side-icon">👥</span>
              <span class="side-text">Tous les Comptes & Rôles</span>
            </a>
            <a routerLink="/admin/audit" routerLinkActive="active" class="side-link">
              <span class="side-icon">📜</span>
              <span class="side-text">Journal d'Audit</span>
            </a>
          }
        </nav>

        <div class="sidebar-footer">
          <div class="admin-badge">
            <span class="dot"></span>
            <span>Connecté en {{ authService.userRole() }}</span>
          </div>
          <a routerLink="/" class="link-site">Visiter le site public ↗</a>
        </div>
      </aside>

      <!-- Main Panel -->
      <div class="admin-main">
        <!-- Top Navbar -->
        <header class="admin-topbar">
          <div class="topbar-left">
            <button class="btn-toggle" (click)="toggleSidebar()" aria-label="Menu">
              ☰
            </button>
            <div class="app-mode-tag">Système Hôtelier DDD v1.0</div>
          </div>

          <div class="topbar-right">
            <div class="operator-profile">
              <div class="operator-avatar">{{ authService.currentUser()?.nomComplet?.charAt(0) || 'A' }}</div>
              <div class="operator-details">
                <span class="operator-name">{{ authService.currentUser()?.nomComplet }}</span>
                <span class="operator-role">{{ authService.userRole() }}</span>
              </div>
            </div>
            <button class="btn btn-outline btn-sm" (click)="authService.logout()">Déconnexion</button>
          </div>
        </header>

        <!-- Dynamic Admin Content -->
        <main class="admin-body">
          <router-outlet></router-outlet>
        </main>
      </div>
    </div>
  `,
  styles: [`
    .admin-wrapper {
      display: flex;
      min-height: 100vh;
      background-color: var(--ivory);
    }

    /* Sidebar */
    .admin-sidebar {
      width: 270px;
      background-color: var(--forest-green);
      color: var(--white);
      display: flex;
      flex-direction: column;
      border-right: 1px solid rgba(214, 183, 122, 0.2);
      transition: width var(--transition-normal);
      flex-shrink: 0;
    }

    .sidebar-header {
      padding: 1.5rem;
      border-bottom: 1px solid rgba(255, 255, 255, 0.08);
    }

    .sidebar-brand {
      display: flex;
      align-items: center;
      gap: 0.75rem;
      color: var(--white);
      text-decoration: none;
    }

    .brand-crest {
      color: var(--soft-gold);
      font-size: 1.6rem;
    }

    .brand-title {
      font-family: var(--font-serif);
      font-size: 1.05rem;
      font-weight: 700;
      letter-spacing: 0.1em;
      color: var(--ivory);
      display: block;
    }

    .brand-mode {
      font-size: 0.65rem;
      letter-spacing: 0.15em;
      color: var(--soft-gold);
      display: block;
    }

    .sidebar-nav {
      flex: 1;
      padding: 1.25rem 0.75rem;
      overflow-y: auto;
    }

    .nav-section-label {
      font-size: 0.675rem;
      letter-spacing: 0.12em;
      color: rgba(214, 183, 122, 0.8);
      font-weight: 700;
      margin: 1.25rem 0.75rem 0.5rem;
    }

    .nav-section-label:first-child {
      margin-top: 0;
    }

    .side-link {
      display: flex;
      align-items: center;
      gap: 0.85rem;
      padding: 0.65rem 0.85rem;
      border-radius: var(--radius-sm);
      color: #d1ded7;
      font-size: 0.885rem;
      font-weight: 500;
      text-decoration: none;
      transition: all var(--transition-fast);
      margin-bottom: 2px;
    }

    .side-icon {
      font-size: 1.1rem;
      width: 22px;
      text-align: center;
    }

    .side-link:hover {
      background: rgba(255, 255, 255, 0.08);
      color: var(--white);
    }

    .side-link.active {
      background: var(--secondary-green);
      color: var(--soft-gold);
      font-weight: 600;
      border-left: 3px solid var(--soft-gold);
    }

    .sidebar-footer {
      padding: 1.25rem;
      border-top: 1px solid rgba(255, 255, 255, 0.08);
      background: var(--forest-green-dark);
    }

    .admin-badge {
      display: flex;
      align-items: center;
      gap: 0.5rem;
      font-size: 0.75rem;
      color: #b0c2b9;
      margin-bottom: 0.5rem;
    }

    .admin-badge .dot {
      width: 7px;
      height: 7px;
      background: #4ade80;
      border-radius: 50%;
    }

    .link-site {
      color: var(--soft-gold);
      font-size: 0.785rem;
      text-decoration: none;
    }

    .link-site:hover {
      text-decoration: underline;
    }

    /* Main Area */
    .admin-main {
      flex: 1;
      display: flex;
      flex-direction: column;
      overflow-x: hidden;
    }

    .admin-topbar {
      height: 68px;
      background: var(--white);
      border-bottom: 1px solid var(--border-subtle);
      display: flex;
      align-items: center;
      justify-content: space-between;
      padding: 0 2rem;
      position: sticky;
      top: 0;
      z-index: 99;
    }

    .topbar-left {
      display: flex;
      align-items: center;
      gap: 1.25rem;
    }

    .btn-toggle {
      background: none;
      border: none;
      font-size: 1.4rem;
      cursor: pointer;
      color: var(--forest-green);
      padding: 0.25rem;
    }

    .app-mode-tag {
      font-size: 0.8rem;
      font-weight: 600;
      color: var(--secondary-green);
      background: var(--secondary-green-subtle);
      padding: 0.25rem 0.65rem;
      border-radius: var(--radius-xs);
    }

    .topbar-right {
      display: flex;
      align-items: center;
      gap: 1.5rem;
    }

    .operator-profile {
      display: flex;
      align-items: center;
      gap: 0.75rem;
    }

    .operator-avatar {
      width: 36px;
      height: 36px;
      border-radius: 50%;
      background: var(--forest-green);
      color: var(--soft-gold);
      display: flex;
      align-items: center;
      justify-content: center;
      font-weight: 700;
      font-size: 0.95rem;
    }

    .operator-details {
      display: flex;
      flex-direction: column;
    }

    .operator-name {
      font-size: 0.875rem;
      font-weight: 600;
      color: var(--forest-green);
    }

    .operator-role {
      font-size: 0.725rem;
      color: var(--text-muted);
    }

    .admin-body {
      padding: 2rem;
      flex: 1;
    }

    /* Responsive Collapsed */
    .admin-wrapper.sidebar-collapsed .admin-sidebar {
      width: 0;
      overflow: hidden;
    }

    @media (max-width: 768px) {
      .admin-sidebar {
        position: fixed;
        top: 0;
        bottom: 0;
        left: 0;
        z-index: 1050;
      }
      .admin-wrapper.sidebar-collapsed .admin-sidebar {
        transform: translateX(-100%);
      }
    }
  `]
})
export class AdminLayoutComponent {
  authService = inject(AuthService);
  isSidebarCollapsed = signal<boolean>(false);

  toggleSidebar(): void {
    this.isSidebarCollapsed.update(v => !v);
  }
}
