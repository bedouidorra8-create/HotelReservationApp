import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { PaiementService } from '../../../core/services/paiement.service';
import { FactureDto } from '../../../core/models';
import { LoadingSpinnerComponent } from '../../../shared/components/loading-spinner.component';
import { NotificationService } from '../../../core/services/notification.service';

@Component({
  selector: 'app-invoice-view',
  standalone: true,
  imports: [CommonModule, RouterModule, LoadingSpinnerComponent],
  template: `
    <div class="invoice-container">
      <div class="invoice-actions no-print">
        <a routerLink="/client/payments" class="btn btn-outline btn-sm">← Revenir aux paiements</a>
        <div class="action-buttons">
          <button class="btn btn-secondary btn-sm" (click)="printInvoice()">🖨️ Imprimer la facture</button>
          <button class="btn btn-gold btn-sm" (click)="downloadPdf()">⬇ Télécharger PDF</button>
        </div>
      </div>

      @if (isLoading()) {
        <app-loading-spinner message="Édition de votre facture en cours..."></app-loading-spinner>
      } @else if (facture()) {
        <div class="invoice-card card-luxury" id="printable-invoice">
          <!-- En-tête Facture -->
          <div class="inv-header">
            <div class="inv-brand">
              <span class="crest">✦</span>
              <div>
                <h2 class="brand-title">L'ÉCRIN HÔTELIER</h2>
                <span class="brand-tagline">COLLECTION PRESTIGE DDD</span>
              </div>
            </div>

            <div class="inv-meta">
              <span class="inv-badge">FACTURE OFFICIELLE</span>
              <p class="inv-ref">N° FACT-{{ facture()!.reservationId.substring(0, 8).toUpperCase() }}</p>
              <p class="inv-date">Date : {{ facture()!.dateEmission | date:'dd MMMM yyyy' }}</p>
            </div>
          </div>

          <!-- Émetteur & Destinataire -->
          <div class="inv-parties-grid">
            <div class="party-col">
              <span class="party-label">ÉMETTEUR</span>
              <h3 class="party-name">{{ facture()!.hotel?.nom || "L'Écrin Hôtelier Palais" }}</h3>
              <p class="party-info">{{ facture()!.hotel?.adresse || '15 Avenue des Champs-Élysées, 75008 Paris' }}</p>
              <p class="party-info">Siret : 842 190 281 00019 • TVA : FR 48 842190281</p>
            </div>

            <div class="party-col text-right">
              <span class="party-label">DESTINATAIRE (CLIENT)</span>
              <h3 class="party-name">{{ facture()!.client?.prenom }} {{ facture()!.client?.nom }}</h3>
              <p class="party-info">{{ facture()!.client?.email }}</p>
              <p class="party-info">Réservation : #{{ facture()!.reservationId.substring(0, 8) }}</p>
            </div>
          </div>

          <!-- Lignes de Facturation -->
          <div class="inv-table-wrap">
            <table class="inv-table">
              <thead>
                <tr>
                  <th>Description de la prestation</th>
                  <th style="text-align: center;">Chambre</th>
                  <th style="text-align: right;">Total HT</th>
                  <th style="text-align: right;">TVA (10%)</th>
                  <th style="text-align: right;">Total TTC</th>
                </tr>
              </thead>
              <tbody>
                <tr>
                  <td>
                    <strong>Séjour Hôtelier Prestige & Prestations associées</strong>
                    <div style="font-size: 0.8rem; color: #666;">Inclus : hébergement, petit-déjeuner gastronomique & accès spa</div>
                  </td>
                  <td style="text-align: center;">n° {{ facture()!.chambre?.numero || '101' }}</td>
                  <td style="text-align: right;">{{ (facture()!.total / 1.10) | number:'1.2-2' }} {{ facture()!.devise }}</td>
                  <td style="text-align: right;">{{ (facture()!.total - (facture()!.total / 1.10)) | number:'1.2-2' }} {{ facture()!.devise }}</td>
                  <td style="text-align: right;">
                    <strong>{{ facture()!.total | number:'1.2-2' }} {{ facture()!.devise }}</strong>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>

          <!-- Totaux & Règlements -->
          <div class="inv-summary-grid">
            <div class="payments-recorded">
              <h4 class="sub-head">Règlements reçus</h4>
              @for (pay of facture()!.paiements; track pay.id) {
                <div class="pay-line">
                  <span>{{ pay.date | date:'dd/MM/yyyy' }} ({{ pay.reference }})</span>
                  <strong>{{ pay.montant }} {{ pay.devise }} - Encaissé ✓</strong>
                </div>
              }
            </div>

            <div class="totals-block">
              <div class="tot-row">
                <span>Total Net HT</span>
                <span>{{ (facture()!.total / 1.10) | number:'1.2-2' }} {{ facture()!.devise }}</span>
              </div>
              <div class="tot-row">
                <span>TVA (10%)</span>
                <span>{{ (facture()!.total - (facture()!.total / 1.10)) | number:'1.2-2' }} {{ facture()!.devise }}</span>
              </div>
              <div class="tot-row final-total">
                <span>Total TTC Acquis</span>
                <span>{{ facture()!.total | number:'1.2-2' }} {{ facture()!.devise }}</span>
              </div>
            </div>
          </div>

          <!-- Pied de Facture -->
          <div class="inv-footer">
            <p>Facture acquittée en totalité. Nous vous remercions de votre confiance et espérons vous revoir très prochainement dans nos maisons.</p>
          </div>
        </div>
      }
    </div>
  `,
  styles: [`
    .invoice-container {
      max-width: 860px;
      margin: 0 auto;
      padding-bottom: 4rem;
    }

    .invoice-actions {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 1.5rem;
    }

    .action-buttons {
      display: flex;
      gap: 0.75rem;
    }

    .invoice-card {
      background: var(--white);
      padding: 3.5rem;
      border: 1px solid var(--border-subtle);
    }

    .inv-header {
      display: flex;
      justify-content: space-between;
      align-items: flex-start;
      border-bottom: 2px solid var(--soft-gold);
      padding-bottom: 2rem;
      margin-bottom: 2.5rem;
    }

    .inv-brand {
      display: flex;
      align-items: center;
      gap: 0.75rem;
    }

    .crest {
      color: var(--soft-gold);
      font-size: 2.2rem;
    }

    .brand-title {
      font-family: var(--font-serif);
      font-size: 1.4rem;
      color: var(--forest-green);
    }

    .brand-tagline {
      font-size: 0.65rem;
      letter-spacing: 0.15em;
      color: var(--text-muted);
    }

    .inv-meta {
      text-align: right;
    }

    .inv-badge {
      background: var(--forest-green);
      color: var(--soft-gold);
      font-size: 0.7rem;
      font-weight: 700;
      letter-spacing: 0.1em;
      padding: 0.25rem 0.65rem;
      border-radius: var(--radius-xs);
    }

    .inv-ref {
      font-weight: 700;
      color: var(--forest-green);
      font-family: monospace;
      font-size: 1.05rem;
      margin: 0.5rem 0 0.2rem;
    }

    .inv-date {
      font-size: 0.85rem;
      color: var(--text-muted);
    }

    .inv-parties-grid {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 3rem;
      margin-bottom: 3rem;
    }

    .text-right {
      text-align: right;
    }

    .party-label {
      font-size: 0.7rem;
      letter-spacing: 0.1em;
      color: var(--text-muted);
      font-weight: 700;
      display: block;
      margin-bottom: 0.4rem;
    }

    .party-name {
      font-family: var(--font-serif);
      font-size: 1.15rem;
      color: var(--forest-green);
      margin-bottom: 0.35rem;
    }

    .party-info {
      font-size: 0.85rem;
      color: var(--text-secondary);
      line-height: 1.4;
    }

    .inv-table-wrap {
      margin-bottom: 2.5rem;
    }

    .inv-table {
      width: 100%;
      border-collapse: collapse;
      font-size: 0.9rem;
    }

    .inv-table th {
      background: var(--ivory-light);
      padding: 0.85rem 1rem;
      border-bottom: 1px solid var(--border-subtle);
      font-size: 0.785rem;
      color: var(--text-secondary);
      text-transform: uppercase;
      letter-spacing: 0.05em;
    }

    .inv-table td {
      padding: 1.25rem 1rem;
      border-bottom: 1px solid var(--border-subtle);
      color: var(--text-primary);
    }

    .inv-summary-grid {
      display: grid;
      grid-template-columns: 1.2fr 1fr;
      gap: 2.5rem;
      margin-bottom: 3rem;
    }

    .sub-head {
      font-size: 0.85rem;
      color: var(--forest-green);
      margin-bottom: 0.75rem;
      text-transform: uppercase;
      letter-spacing: 0.05em;
    }

    .pay-line {
      display: flex;
      justify-content: space-between;
      font-size: 0.825rem;
      color: var(--text-secondary);
      padding: 0.35rem 0;
      border-bottom: 1px dashed var(--border-subtle);
    }

    .totals-block {
      background: var(--ivory-light);
      padding: 1.25rem;
      border-radius: var(--radius-sm);
    }

    .tot-row {
      display: flex;
      justify-content: space-between;
      font-size: 0.885rem;
      color: var(--text-secondary);
      margin-bottom: 0.4rem;
    }

    .final-total {
      margin-top: 0.75rem;
      padding-top: 0.75rem;
      border-top: 2px solid var(--soft-gold);
      font-size: 1.15rem;
      font-weight: 700;
      color: var(--forest-green);
    }

    .inv-footer {
      border-top: 1px solid var(--border-subtle);
      padding-top: 1.5rem;
      text-align: center;
      font-size: 0.8rem;
      color: var(--text-muted);
      line-height: 1.5;
    }

    @media print {
      .no-print {
        display: none !important;
      }
      body {
        background: white !important;
      }
      .invoice-card {
        border: none !important;
        box-shadow: none !important;
        padding: 0 !important;
      }
    }
  `]
})
export class InvoiceViewComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private paiementService = inject(PaiementService);
  private notification = inject(NotificationService);

  facture = signal<FactureDto | null>(null);
  isLoading = signal<boolean>(true);

  ngOnInit(): void {
    const reservationId = this.route.snapshot.params['reservationId'];
    if (reservationId) {
      this.paiementService.getFacture(reservationId).subscribe({
        next: (f) => {
          this.facture.set(f);
          this.isLoading.set(false);
        },
        error: () => this.isLoading.set(false)
      });
    }
  }

  printInvoice(): void {
    window.print();
  }

  downloadPdf(): void {
    const f = this.facture();
    if (!f) return;

    // Déclenchement de l'endpoint PDF binaire (Contrat BG-01) avec fallback impression
    this.paiementService.telechargerFacturePdf(f.reservationId).subscribe({
      next: (blob) => {
        if (blob) {
          const url = window.URL.createObjectURL(blob);
          const a = document.createElement('a');
          a.href = url;
          a.download = `Facture_${f.reservationId.substring(0, 8)}.pdf`;
          a.click();
          window.URL.revokeObjectURL(url);
          this.notification.success('Téléchargement du PDF en cours...');
        } else {
          this.notification.info("La génération PDF serveur est en cours d'intégration (BG-01). Impression système activée.");
          window.print();
        }
      }
    });
  }
}
