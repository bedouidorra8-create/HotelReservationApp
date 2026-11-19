import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { PaiementService } from '../../../core/services/paiement.service';
import { NotificationService } from '../../../core/services/notification.service';
import { Paiement, StatutPaiement } from '../../../core/models';
import { StatusBadgePipe } from '../../../shared/pipes/status-badge.pipe';
import { LoadingSpinnerComponent } from '../../../shared/components/loading-spinner.component';
import { ConfirmModalComponent } from '../../../shared/components/confirm-modal.component';

@Component({
  selector: 'app-payments-management',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, StatusBadgePipe, LoadingSpinnerComponent, ConfirmModalComponent],
  template: `
    <div class="payments-mgmt-page">
      <div class="page-header header-with-action">
        <div>
          <h1 class="page-title">Gestion & Suivi des Transactions Financières</h1>
          <p class="page-subtitle">Contrôle des encaissements, modes de paiement et régularisations</p>
        </div>
        <button class="btn btn-outline-gold btn-sm" (click)="exportCsv()" [disabled]="payments().length === 0">
          📊 Exporter CSV
        </button>
      </div>

      <!-- Filtre rapide -->
      <div class="card-luxury p-filters">
        <input 
          type="text" 
          [(ngModel)]="searchRef" 
          placeholder="Rechercher par référence (ex: PAY-2026)..." 
          class="form-control" />
      </div>

      @if (isLoading()) {
        <app-loading-spinner message="Chargement des transactions..."></app-loading-spinner>
      } @else {
        <div class="table-responsive">
          <table class="custom-table">
            <thead>
              <tr>
                <th>Référence</th>
                <th>Date & Heure</th>
                <th>Réservation</th>
                <th>Mode</th>
                <th>Montant</th>
                <th>Statut</th>
                <th style="text-align: right;">Actions</th>
              </tr>
            </thead>
            <tbody>
              @for (p of filteredPayments(); track p.id) {
                <tr>
                  <td>
                    <strong style="color: var(--forest-green); font-family: monospace;">{{ p.reference }}</strong>
                  </td>
                  <td>{{ p.date | date:'dd/MM/yyyy HH:mm' }}</td>
                  <td>#{{ p.reservationId.substring(0, 8) }}</td>
                  <td>
                    @switch (p.modePaiement) {
                      @case (1) { <span>💳 Carte</span> }
                      @case (2) { <span>💵 Espèces</span> }
                      @case (3) { <span>🏦 Virement</span> }
                      @case (4) { <span>✉️ Chèque</span> }
                      @default { <span>Autre</span> }
                    }
                  </td>
                  <td>
                    <strong>{{ p.montant }} {{ p.devise }}</strong>
                  </td>
                  <td>
                    @let b = (p.statut | statusBadge:'paiement');
                    <span class="badge" [ngClass]="b.cssClass">{{ b.label }}</span>
                  </td>
                  <td style="text-align: right;">
                    <div class="actions-group">
                      <a [routerLink]="['/client/invoice', p.reservationId]" class="btn btn-outline btn-sm" title="Voir Facture">
                        📄 Facture
                      </a>
                      @if (p.statut === 2) {
                        <button class="btn btn-danger btn-sm" (click)="askCancel(p.id)" title="Annuler le paiement">
                          Annuler
                        </button>
                      }
                    </div>
                  </td>
                </tr>
              }
            </tbody>
          </table>
        </div>
      }

      <app-confirm-modal
        [isOpen]="isCancelModalOpen()"
        title="Annuler ce paiement ?"
        message="Êtes-vous sûr de vouloir annuler ce paiement ? Cette action mettra à jour le statut financier de la réservation."
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
    .actions-group {
      display: inline-flex;
      gap: 0.35rem;
    }
  `]
})
export class PaymentsManagementComponent implements OnInit {
  private paiementService = inject(PaiementService);
  private notification = inject(NotificationService);

  payments = signal<Paiement[]>([]);
  isLoading = signal<boolean>(true);
  searchRef: string = '';

  isCancelModalOpen = signal<boolean>(false);
  paymentToCancelId = signal<string | null>(null);

  filteredPayments = () => {
    const ref = this.searchRef.toLowerCase().trim();
    if (!ref) return this.payments();
    return this.payments().filter(p => p.reference.toLowerCase().includes(ref));
  };

  ngOnInit(): void {
    this.loadPayments();
  }

  loadPayments(): void {
    this.isLoading.set(true);
    this.paiementService.getAllPaiements().subscribe({
      next: (res) => {
        this.payments.set(res);
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
  }

  askCancel(id: string): void {
    this.paymentToCancelId.set(id);
    this.isCancelModalOpen.set(true);
  }

  executeCancel(): void {
    const id = this.paymentToCancelId();
    if (!id) return;

    this.paiementService.cancelPaiement(id).subscribe({
      next: () => {
        this.notification.success('Paiement annulé avec succès.');
        this.isCancelModalOpen.set(false);
        this.loadPayments();
      },
      error: (err) => {
        this.notification.error(err.error?.message || "Erreur lors de l'annulation du paiement.");
        this.isCancelModalOpen.set(false);
      }
    });
  }

  exportCsv(): void {
    const list = this.filteredPayments();
    if (list.length === 0) return;

    const headers = ['Reference', 'Date', 'ReservationId', 'ModePaiement', 'Montant', 'Devise', 'Statut'];
    const getModeLabel = (mode: number) => {
      switch (mode) {
        case 1: return 'Carte';
        case 2: return 'Especes';
        case 3: return 'Virement';
        case 4: return 'Cheque';
        default: return 'Autre';
      }
    };
    const getStatutLabel = (st: number) => {
      switch (st) {
        case 1: return 'EnAttente';
        case 2: return 'Paye';
        case 3: return 'Rembourse';
        case 4: return 'Echoue';
        case 5: return 'Annule';
        default: return 'Inconnu';
      }
    };

    const rows = list.map(p => [
      `"${p.reference}"`,
      `"${p.date ? p.date.substring(0, 19).replace('T', ' ') : ''}"`,
      `"${p.reservationId ? p.reservationId.substring(0, 8) : ''}"`,
      `"${getModeLabel(p.modePaiement)}"`,
      p.montant,
      `"${p.devise || 'DT'}"`,
      `"${getStatutLabel(p.statut)}"`
    ]);

    const csvContent = '\uFEFF' + [headers.join(';'), ...rows.map(e => e.join(';'))].join('\r\n');
    const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' });
    const link = document.createElement('a');
    link.href = URL.createObjectURL(blob);
    link.setAttribute('download', `paiements-hoteli-${new Date().toISOString().substring(0, 10)}.csv`);
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    this.notification.success('Export CSV des paiements téléchargé avec succès !');
  }
}
