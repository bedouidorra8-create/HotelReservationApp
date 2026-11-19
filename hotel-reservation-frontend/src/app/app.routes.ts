import { Routes } from '@angular/router';
import { authGuard, roleGuard } from './core/guards/auth.guard';

// Layouts
import { PublicLayoutComponent } from './layout/public-layout/public-layout.component';
import { ClientLayoutComponent } from './layout/client-layout/client-layout.component';
import { AdminLayoutComponent } from './layout/admin-layout/admin-layout.component';

// Features Publiques & Auth
import { LandingPageComponent } from './features/public/landing-page/landing-page.component';
import { HotelSearchComponent } from './features/hotels/hotel-search/hotel-search.component';
import { HotelDetailPageComponent } from './features/hotels/hotel-detail/hotel-detail-page.component';
import { LoginComponent } from './features/authentication/login/login.component';
import { RegisterComponent } from './features/authentication/register/register.component';
import { ForgotPasswordComponent } from './features/authentication/forgot-password/forgot-password.component';
import { ReservationWizardComponent } from './features/reservations/reservation-wizard/reservation-wizard.component';

// Features Client
import { ClientDashboardComponent } from './features/client/dashboard/client-dashboard.component';
import { MyReservationsComponent } from './features/reservations/my-reservations/my-reservations.component';
import { PaymentHistoryComponent } from './features/payments/payment-history/payment-history.component';
import { InvoiceViewComponent } from './features/payments/invoice-view/invoice-view.component';
import { ClientFavoritesComponent } from './features/client/favorites/client-favorites.component';
import { ClientProfileComponent } from './features/client/profile/client-profile.component';

// Features Administration & Agent
import { AdminDashboardComponent } from './features/administration/admin-dashboard/admin-dashboard.component';
import { HotelsManagementComponent } from './features/administration/hotels-management/hotels-management.component';
import { RoomsManagementComponent } from './features/administration/rooms-management/rooms-management.component';
import { ReservationsManagementComponent } from './features/administration/reservations-management/reservations-management.component';
import { CalendarComponent } from './features/administration/calendar/calendar.component';
import { PaymentsManagementComponent } from './features/administration/payments-management/payments-management.component';
import { ReviewsModerationComponent } from './features/administration/reviews-moderation/reviews-moderation.component';
import { StatisticsComponent } from './features/administration/statistics/statistics.component';
import { UsersManagementComponent } from './features/administration/users-management/users-management.component';
import { AuditLogsComponent } from './features/administration/audit-logs/audit-logs.component';

export const routes: Routes = [
  // 1. ESPACE PUBLIC & AUTHENTIFICATION
  {
    path: '',
    component: PublicLayoutComponent,
    children: [
      { path: '', component: LandingPageComponent },
      { path: 'hotels', component: HotelSearchComponent },
      { path: 'hotels/:id', component: HotelDetailPageComponent },
      {
        path: 'reservations/wizard',
        component: ReservationWizardComponent,
        canActivate: [authGuard]
      },
      { path: 'auth/login', component: LoginComponent },
      { path: 'auth/register', component: RegisterComponent },
      { path: 'auth/forgot-password', component: ForgotPasswordComponent }
    ]
  },

  // 2. ESPACE CLIENT PRIVILÈGE
  {
    path: 'client',
    component: ClientLayoutComponent,
    canActivate: [authGuard, roleGuard],
    data: { roles: ['Client'] },
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      { path: 'dashboard', component: ClientDashboardComponent },
      { path: 'reservations', component: MyReservationsComponent },
      { path: 'payments', component: PaymentHistoryComponent },
      { path: 'invoice/:reservationId', component: InvoiceViewComponent },
      { path: 'favorites', component: ClientFavoritesComponent },
      { path: 'profile', component: ClientProfileComponent }
    ]
  },

  // 3. ESPACE ADMINISTRATION & PERSONNEL (AGENT / ADMIN)
  {
    path: 'admin',
    component: AdminLayoutComponent,
    canActivate: [authGuard, roleGuard],
    data: { roles: ['Administrateur', 'Agent'] },
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      { path: 'dashboard', component: AdminDashboardComponent },
      { path: 'hotels', component: HotelsManagementComponent },
      { path: 'rooms', component: RoomsManagementComponent },
      { path: 'reservations', component: ReservationsManagementComponent },
      { path: 'calendar', component: CalendarComponent },
      { path: 'payments', component: PaymentsManagementComponent },
      {
        path: 'reviews',
        component: ReviewsModerationComponent,
        canActivate: [roleGuard],
        data: { roles: ['Administrateur'] }
      },
      {
        path: 'statistics',
        component: StatisticsComponent,
        canActivate: [roleGuard],
        data: { roles: ['Administrateur'] }
      },
      {
        path: 'agents',
        component: UsersManagementComponent,
        canActivate: [roleGuard],
        data: { roles: ['Administrateur'], defaultTab: 'agents' }
      },
      {
        path: 'clients',
        component: UsersManagementComponent,
        canActivate: [roleGuard],
        data: { roles: ['Administrateur'], defaultTab: 'clients' }
      },
      {
        path: 'users',
        component: UsersManagementComponent,
        canActivate: [roleGuard],
        data: { roles: ['Administrateur'], defaultTab: 'all' }
      },
      {
        path: 'audit',
        component: AuditLogsComponent,
        canActivate: [roleGuard],
        data: { roles: ['Administrateur'] }
      }
    ]
  },

  // Redirection par défaut vers l'accueil
  { path: '**', redirectTo: '' }
];
