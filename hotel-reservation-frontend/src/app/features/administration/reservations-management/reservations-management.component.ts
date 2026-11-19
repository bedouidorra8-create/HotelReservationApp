import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { ReservationService } from '../../../core/services/reservation.service';
import { HotelService } from '../../../core/services/hotel.service';
import { NotificationService } from '../../../core/services/notification.service';
import { Reservation, Hotel, StatutReservation } from '../../../core/models';
import { StatusBadgePipe } from '../../../shared/pipes/status-badge.pipe';
import { LoadingSpinnerComponent } from '../../../shared/components/loading-spinner.component';
import { ConfirmModalComponent } from '../../../shared/components/confirm-modal.component';

@Component({
  selector: 'app-reservations-management',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, StatusBadgePipe, LoadingSpinnerComponent, ConfirmModalComponent],
  template: `
    <div class="reservations-mgmt-page">
      <div class="page-header header-with-action">
        <div>
          <h1 class="page-title">Gestion Globale des Réservations</h1>
          <p class="page-subtitle">Recherche multicritère, confirmation, annulation et gestion des statuts</p>
        </div>
        <button class="btn btn-outline-gold btn-sm" (click)="exportCsv()" [disabled]="reservations().length === 0">
          📊 Exporter CSV
        </button>
      </div>

      <!-- Filtres multicritères -->
      <div class="card-luxury p-filters">
        <div class="filters-grid">
          <div class="filter-col">
            <label class="form-label">Établissement</label>
            <select [(ngModel)]="filterHotelId" (ngModelChange)="applySearch()" class="form-select">
              <option value="">Tous les établissements</option>
              @for (h of hotels(); track h.id) {
                <option [value]="h.id">{{ h.nom }}</option>
              }
            </select>
          </div>

          <div class="filter-col">
            <label class="form-label">Statut</label>
            <select [(ngModel)]="filterStatut" (ngModelChange)="applySearch()" class="form-select">
              <option [ngValue]="null">Tous les statuts</option>
              <option [ngValue]="1">En Attente</option>
              <option [ngValue]="2">Confirmée</option>
              <option [ngValue]="3">Annulée</option>
              <option [ngValue]="4">Terminée</option>
            </select>
          </div>

          <div class="filter-col">
            <label class="form-label">Date Début</label>
            <input type="date" [(ngModel)]="filterDateDebut" (ngModelChange)="applySearch()" class="form-control" />
          </div>

          <div class="filter-col">
            <label class="form-label">Date Fin</label>
            <input type="date" [(ngModel)]="filterDateFin" (ngModelChange)="applySearch()" class="form-control" />
          </div>
        </div>
      </div>

      @if (isLoading()) {
        <app-loading-spinner message="Chargement des réservations..."></app-loading-spinner>
      } @else {
        <div class="table-responsive">
          <table class="custom-table">
            <thead>
              <tr>
                <th>Réf</th>
                <th>Établissement</th>
                <th>Chambre</th>
                <th>Arrivée → Départ</th>
                <th>Invités</th>
                <th>Total Figé</th>
                <th>Statut</th>
                <th style="text-align: right;">Actions</th>
              </tr>
            </thead>
            <tbody>
              @for (r of reservations(); track r.id) {
                <tr>
                  <td>
                    <strong style="font-family: monospace; color: var(--forest-green);">
                      #{{ r.id.substring(0, 8) }}
                    </strong>
                  </td>
                  <td>{{ r.hotelNom || 'Établissement Prestige' }}</td>
                  <td>Chambre {{ r.chambreNumero || '101' }}</td>
                  <td>
                    <div>{{ r.dateArrivee | date:'dd/MM/yyyy' }}</div>
                    <small style="color: var(--text-muted);">au {{ r.dateDepart | date:'dd/MM/yyyy' }} ({{ r.nbNuits }}n)</small>
                  </td>
                  <td>{{ r.nbPersonnes }} pers.</td>
                  <td>
                    <strong>{{ r.totalMontant || (r.nbNuits * 280) }} {{ r.totalDevise || 'DT' }}</strong>
                  </td>
                  <td>
                    @let b = (r.statut | statusBadge:'reservation');
                    <span class="badge" [ngClass]="b.cssClass">{{ b.label }}</span>
                  </td>
                  <td style="text-align: right;">
                    <div class="actions-group">
                      @if (r.statut === 1) {
                        <button class="btn btn-gold btn-sm" (click)="confirm(r.id)" title="Confirmer la réservation">
                          ✓ Confirmer
                        </button>
                      }

                      @if (r.statut === 1 || r.statut === 2) {
                        <button class="btn btn-danger btn-sm" (click)="askCancel(r.id)" title="Annuler">
                          Annuler
                        </button>
                      }

                      <a [routerLink]="['/client/invoice', r.id]" class="btn btn-outline btn-sm" title="Voir Facture">
                        📄
                      </a>
                    </div>
                  </td>
                </tr>
              }
            </tbody>
          </table>
        </div>
      }

      <!-- Modale de confirmation annulation -->
      <app-confirm-modal
        [isOpen]="isCancelModalOpen()"
        title="Annuler cette réservation ?"
        message="Êtes-vous sûr de vouloir annuler cette réservation ? La chambre sera remise en disponibilité."
        confirmText="Confirmer l'annulation"
        [isDanger]="true"
        (confirmed)="executeCancel()"
        (cancelled)="isCancelModalOpen.set(false)">
      </app-confirm-modal>
    </div>
  `,
  styles: [`
    .header-with-action {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 1.5rem;
      flex-wrap: wrap;
      gap: 1rem;
    }

    .p-filters {
      padding: 1.25rem 1.5rem;
      margin-bottom: 1.5rem;
    }

    .filters-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
      gap: 1rem;
    }

    .actions-group {
      display: inline-flex;
      gap: 0.35rem;
    }
  `]
})
export class ReservationsManagementComponent implements OnInit {
  private reservationService = inject(ReservationService);
  private hotelService = inject(HotelService);
  private notification = inject(NotificationService);

  hotels = signal<Hotel[]>([]);
  reservations = signal<Reservation[]>([]);
  isLoading = signal<boolean>(true);

  filterHotelId: string = '';
  filterStatut: number | null = null;
  filterDateDebut: string = '';
  filterDateFin: string = '';

  isCancelModalOpen = signal<boolean>(false);
  targetCancelId = signal<string | null>(null);

  ngOnInit(): void {
    this.hotelService.getHotels().subscribe(h => this.hotels.set(h));
    this.applySearch();
  }

  applySearch(): void {
    this.isLoading.set(true);
    this.reservationService.searchReservations({
      hotelId: this.filterHotelId || undefined,
      statut: this.filterStatut !== null ? this.filterStatut : undefined,
      dateDebut: this.filterDateDebut || undefined,
      dateFin: this.filterDateFin || undefined
    }).subscribe({
      next: (res) => {
        this.reservations.set(res);
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
  }

  confirm(id: string): void {
    this.reservationService.confirmReservation(id).subscribe({
      next: () => {
        this.notification.success('Réservation confirmée avec succès !');
        this.applySearch();
      },
      error: (err) => this.notification.error(err.error?.message || 'Erreur lors de la confirmation.')
    });
  }

  askCancel(id: string): void {
    this.targetCancelId.set(id);
    this.isCancelModalOpen.set(true);
  }

  executeCancel(): void {
    const id = this.targetCancelId();
    if (!id) return;

    this.reservationService.cancelReservation(id).subscribe({
      next: () => {
        this.notification.success('Réservation annulée.');
        this.isCancelModalOpen.set(false);
        this.applySearch();
      },
      error: (err) => {
        this.notification.error(err.error?.message || "Impossible d'annuler.");
        this.isCancelModalOpen.set(false);
      }
    });
  }

  exportCsv(): void {
    const list = this.reservations();
    if (list.length === 0) return;

    const headers = ['Reference', 'Hotel', 'Chambre', 'Arrivee', 'Depart', 'Nuits', 'Personnes', 'Total', 'Devise', 'Statut'];
    const rows = list.map(r => [
      `"${r.id.substring(0, 8)}"`,
      `"${(r.hotelNom || '').replace(/"/g, '""')}"`,
      `"${r.chambreNumero || ''}"`,
      `"${r.dateArrivee ? r.dateArrivee.substring(0, 10) : ''}"`,
      `"${r.dateDepart ? r.dateDepart.substring(0, 10) : ''}"`,
      r.nbNuits || 0,
      r.nbPersonnes || 1,
      r.totalMontant || 0,
      `"${r.totalDevise || 'DT'}"`,
      `"${r.statut === 1 ? 'En Attente' : r.statut === 2 ? 'Confirmee' : r.statut === 3 ? 'Annulee' : 'Terminee'}"`
    ]);

    const csvContent = '\uFEFF' + [headers.join(';'), ...rows.map(e => e.join(';'))].join('\r\n');
    const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' });
    const link = document.createElement('a');
    link.href = URL.createObjectURL(blob);
    link.setAttribute('download', `reservations-hoteli-${new Date().toISOString().substring(0, 10)}.csv`);
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    this.notification.success('Export CSV des réservations téléchargé avec succès !');
  }
}
