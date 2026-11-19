import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AuditService } from '../../../core/services/audit.service';
import { AuditLogDto } from '../../../core/models';
import { LoadingSpinnerComponent } from '../../../shared/components/loading-spinner.component';
import { EmptyStateComponent } from '../../../shared/components/empty-state.component';

@Component({
  selector: 'app-audit-logs',
  standalone: true,
  imports: [CommonModule, FormsModule, LoadingSpinnerComponent, EmptyStateComponent],
  template: `
    <div class="audit-logs-page">
      <div class="page-header">
        <h1 class="page-title">Journal d'Audit & Sécurité Système</h1>
        <p class="page-subtitle">Traçabilité complète des opérations administratives et modifications de données</p>
      </div>

      <!-- Filtres temporels -->
      <div class="card-luxury p-filters">
        <div class="filters-row">
          <div class="filter-field">
            <label class="form-label">Date Début</label>
            <input type="date" [(ngModel)]="dateDebut" (ngModelChange)="loadLogs()" class="form-control" />
          </div>
          <div class="filter-field">
            <label class="form-label">Date Fin</label>
            <input type="date" [(ngModel)]="dateFin" (ngModelChange)="loadLogs()" class="form-control" />
          </div>
        </div>
      </div>

      @if (isLoading()) {
        <app-loading-spinner message="Chargement des journaux de sécurité..."></app-loading-spinner>
      } @else if (logs().length === 0) {
        <app-empty-state
          icon="📜"
          title="Aucun journal pour cette période"
          description="Aucune activité administrative n'a été enregistrée sur l'intervalle sélectionné.">
        </app-empty-state>
      } @else {
        <div class="table-responsive">
          <table class="custom-table">
            <thead>
              <tr>
                <th>Date & Heure</th>
                <th>Opération / Action</th>
                <th>Entité Cible</th>
                <th>Détails & Contexte</th>
                <th>Adresse IP</th>
              </tr>
            </thead>
            <tbody>
              @for (log of logs(); track log.id) {
                <tr>
                  <td>
                    <strong>{{ log.dateAction | date:'dd/MM/yyyy HH:mm:ss' }}</strong>
                  </td>
                  <td>
                    <span class="action-badge">{{ log.action }}</span>
                  </td>
                  <td>
                    <span class="entity-pill">{{ log.entite }}</span>
                    @if (log.entiteId) {
                      <small style="display: block; color: var(--text-muted); font-family: monospace;">
                        #{{ log.entiteId.substring(0, 8) }}
                      </small>
                    }
                  </td>
                  <td>{{ log.details || '-' }}</td>
                  <td>
                    <code class="ip-code">{{ log.adresseIp || '127.0.0.1' }}</code>
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
    .p-filters {
      padding: 1.25rem 1.5rem;
      margin-bottom: 1.5rem;
    }

    .filters-row {
      display: flex;
      gap: 1.5rem;
    }

    .filter-field {
      max-width: 240px;
    }

    .action-badge {
      font-size: 0.785rem;
      font-weight: 700;
      color: var(--forest-green);
      background: var(--ivory-light);
      padding: 0.25rem 0.65rem;
      border-radius: var(--radius-xs);
      border: 1px solid var(--border-subtle);
    }

    .entity-pill {
      font-size: 0.8rem;
      font-weight: 600;
      color: var(--secondary-green);
    }

    .ip-code {
      font-size: 0.8rem;
      background: #f1f1f1;
      padding: 0.2rem 0.4rem;
      border-radius: var(--radius-xs);
    }
  `]
})
export class AuditLogsComponent implements OnInit {
  private auditService = inject(AuditService);

  logs = signal<AuditLogDto[]>([]);
  isLoading = signal<boolean>(true);

  dateDebut: string = '';
  dateFin: string = '';

  ngOnInit(): void {
    this.loadLogs();
  }

  loadLogs(): void {
    this.isLoading.set(true);
    this.auditService.getAuditLogs(this.dateDebut || undefined, this.dateFin || undefined).subscribe({
      next: (res) => {
        this.logs.set(res);
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
  }
}
