import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { PaiementService } from '../../../core/services/paiement.service';
import { Paiement, ModePaiement } from '../../../core/models';
import { StatusBadgePipe } from '../../../shared/pipes/status-badge.pipe';
import { LoadingSpinnerComponent } from '../../../shared/components/loading-spinner.component';
import { EmptyStateComponent } from '../../../shared/components/empty-state.component';

@Component({
  selector: 'app-payment-history',
  standalone: true,
  imports: [CommonModule, RouterModule, StatusBadgePipe, LoadingSpinnerComponent, EmptyStateComponent],
  template: `
    <div class="payments-page">
      <div class="page-header">
        <h1 class="page-title">Paiements & Factures</h1>
        <p class="page-subtitle">Consultez l'ensemble de vos transactions financières et téléchargez vos justificatifs</p>
      </div>

      @if (isLoading()) {
        <app-loading-spinner message="Chargement des paiements..."></app-loading-spinner>
      } @else if (payments().length === 0) {
        <app-empty-state
          icon="💳"
          title="Aucun paiement enregistré"
          description="Vous n'avez effectué aucune transaction pour l'instant.">
        </app-empty-state>
      } @else {
        <div class="table-responsive">
          <table class="custom-table">
            <thead>
              <tr>
                <th>Référence</th>
                <th>Date</th>
                <th>Établissement</th>
                <th>Mode de paiement</th>
                <th>Montant</th>
                <th>Statut</th>
                <th style="text-align: right;">Facture</th>
              </tr>
            </thead>
            <tbody>
              @for (p of payments(); track p.id) {
                <tr>
                  <td>
                    <strong style="color: var(--forest-green); font-family: monospace;">{{ p.reference }}</strong>
                  </td>
                  <td>{{ p.date | date:'dd/MM/yyyy HH:mm' }}</td>
                  <td>{{ p.hotelNom || 'Établissement Prestige' }}</td>
                  <td>
                    @switch (p.modePaiement) {
                      @case (1) { <span>💳 Carte bancaire</span> }
                      @case (2) { <span>💵 Espèces</span> }
                      @case (3) { <span>🏦 Virement bancaire</span> }
                      @case (4) { <span>✉️ Chèque</span> }
                      @default { <span>Autre</span> }
                    }
                  </td>
                  <td>
                    <strong style="color: var(--forest-green);">{{ p.montant }} {{ p.devise }}</strong>
                  </td>
                  <td>
                    @let b = (p.statut | statusBadge:'paiement');
                    <span class="badge" [ngClass]="b.cssClass">{{ b.label }}</span>
                  </td>
                  <td style="text-align: right;">
                    <a [routerLink]="['/client/invoice', p.reservationId]" class="btn btn-outline btn-sm">
                      📄 Voir facture
                    </a>
                  </td>
                </tr>
              }
            </tbody>
          </table>
        </div>
      }
    </div>
  `,
  styles: [`
    .payments-page {
      max-width: 1000px;
      margin: 0 auto;
    }
  `]
})
export class PaymentHistoryComponent implements OnInit {
  private paiementService = inject(PaiementService);

  payments = signal<Paiement[]>([]);
  isLoading = signal<boolean>(true);

  ngOnInit(): void {
    this.paiementService.getMyPaiements().subscribe({
      next: (res) => {
        this.payments.set(res);
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
  }
}
