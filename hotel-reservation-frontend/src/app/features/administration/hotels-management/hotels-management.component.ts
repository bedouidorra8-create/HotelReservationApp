import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { HotelService } from '../../../core/services/hotel.service';
import { AuthService } from '../../../core/auth/auth.service';
import { NotificationService } from '../../../core/services/notification.service';
import { Hotel, CreerHotelRequest } from '../../../core/models';
import { LoadingSpinnerComponent } from '../../../shared/components/loading-spinner.component';
import { ConfirmModalComponent } from '../../../shared/components/confirm-modal.component';

@Component({
  selector: 'app-hotels-management',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, LoadingSpinnerComponent, ConfirmModalComponent],
  template: `
    <div class="hotels-management-page">
      <div class="page-header flex-header">
        <div>
          <h1 class="page-title">Gestion des Établissements Hôteliers</h1>
          <p class="page-subtitle">Configurez, modifiez et pilotez le parc hôtelier de prestige</p>
        </div>
        <button class="btn btn-gold" (click)="openAddModal()">+ Ajouter un Établissement</button>
      </div>

      <!-- Filtres rapides -->
      <div class="card-luxury p-filters">
        <div class="filters-row">
          <input 
            type="text" 
            [(ngModel)]="searchTerm" 
            placeholder="Rechercher par nom d'hôtel ou par ville..." 
            class="form-control" />
        </div>
      </div>

      @if (isLoading()) {
        <app-loading-spinner message="Chargement du parc hôtelier..."></app-loading-spinner>
      } @else {
        <div class="table-responsive">
          <table class="custom-table">
            <thead>
              <tr>
                <th>Établissement</th>
                <th>Ville & Adresse</th>
                <th>Standing</th>
                <th>Coordonnées</th>
                <th>Chambres</th>
                <th>Statut</th>
                <th style="text-align: right;">Actions</th>
              </tr>
            </thead>
            <tbody>
              @for (h of filteredHotels(); track h.id) {
                <tr>
                  <td>
                    <strong class="hotel-table-name">{{ h.nom }}</strong>
                  </td>
                  <td>
                    <div style="font-weight: 500;">{{ h.ville }}</div>
                    <small style="color: var(--text-muted);">{{ h.adresse }}</small>
                  </td>
                  <td>
                    <span style="color: var(--soft-gold); font-size: 1.05rem;">
                      {{ '★'.repeat(h.nbEtoiles) }}
                    </span>
                  </td>
                  <td>
                    <div>{{ h.telephone }}</div>
                    <small style="color: var(--text-muted);">{{ h.email }}</small>
                  </td>
                  <td>
                    <span class="badge badge-info">{{ h.chambres?.length || h.nbChambresDisponibles || 0 }} chambres</span>
                  </td>
                  <td>
                    <span class="badge" [ngClass]="h.actif !== false ? 'badge-success' : 'badge-danger'">
                      {{ h.actif !== false ? 'En exploitation' : 'Désactivé' }}
                    </span>
                  </td>
                  <td style="text-align: right;">
                    <div class="actions-group">
                      <button class="btn btn-ghost btn-sm" (click)="openEditModal(h)" title="Modifier">✏️</button>
                      <button 
                        class="btn btn-sm" 
                        [ngClass]="h.actif !== false ? 'btn-outline' : 'btn-secondary'"
                        (click)="toggleActive(h)" 
                        title="Activer/Désactiver">
                        {{ h.actif !== false ? 'Désactiver' : 'Activer' }}
                      </button>
                      @if (authService.userRole() === 'Administrateur') {
                        <button class="btn btn-ghost btn-sm" style="color: var(--color-error);" (click)="askDelete(h)" title="Supprimer">🗑️</button>
                      }
                    </div>
                  </td>
                </tr>
              }
            </tbody>
          </table>
        </div>
      }

      <!-- Modale Ajout / Édition -->
      @if (isFormModalOpen()) {
        <div class="modal-overlay" (click)="isFormModalOpen.set(false)">
          <div class="modal-dialog modal-lg" (click)="$event.stopPropagation()">
            <div class="card-header">
              <h3 class="modal-title">{{ editingHotelId() ? "Modifier l'Établissement" : "Créer un Nouvel Établissement" }}</h3>
              <button class="btn-close" (click)="isFormModalOpen.set(false)">✕</button>
            </div>

            <form [formGroup]="hotelForm" (ngSubmit)="saveHotel()">
              <div class="card-body">
                <div class="form-row">
                  <div class="form-group flex-2">
                    <label class="form-label">Nom de l'hôtel *</label>
                    <input type="text" formControlName="nom" placeholder="Ex: Le Grand Palais Royal" class="form-control" />
                  </div>
                  <div class="form-group flex-1">
                    <label class="form-label">Étoiles (1 à 5) *</label>
                    <select formControlName="nbEtoiles" class="form-select">
                      <option [ngValue]="5">5 étoiles (Palace)</option>
                      <option [ngValue]="4">4 étoiles</option>
                      <option [ngValue]="3">3 étoiles</option>
                    </select>
                  </div>
                </div>

                <div class="form-row">
                  <div class="form-group flex-1">
                    <label class="form-label">Ville *</label>
                    <input type="text" formControlName="ville" placeholder="Ex: Paris" class="form-control" />
                  </div>
                  <div class="form-group flex-2">
                    <label class="form-label">Adresse complète *</label>
                    <input type="text" formControlName="adresse" placeholder="15 Avenue des Champs-Élysées" class="form-control" />
                  </div>
                </div>

                <div class="form-row">
                  <div class="form-group flex-1">
                    <label class="form-label">Téléphone *</label>
                    <input type="tel" formControlName="telephone" placeholder="+33 1 42 68 00 00" class="form-control" />
                  </div>
                  <div class="form-group flex-1">
                    <label class="form-label">Email de contact *</label>
                    <input type="email" formControlName="email" placeholder="contact@palais.com" class="form-control" />
                  </div>
                </div>

                <div class="form-group">
                  <label class="form-label">Description détaillée *</label>
                  <textarea formControlName="description" rows="3" class="form-control"></textarea>
                </div>

                <div class="form-group">
                  <label class="form-label">URL de l'image de couverture</label>
                  <input type="url" formControlName="imageUrl" placeholder="https://..." class="form-control" />
                </div>
              </div>

              <div class="card-footer modal-actions">
                <button type="button" class="btn btn-outline btn-sm" (click)="isFormModalOpen.set(false)">Annuler</button>
                <button type="submit" class="btn btn-gold btn-sm" [disabled]="hotelForm.invalid || isSubmitting()">
                  {{ editingHotelId() ? 'Mettre à jour' : 'Enregistrer' }}
                </button>
              </div>
            </form>
          </div>
        </div>
      }

      <!-- Modale Confirmation Suppression -->
      <app-confirm-modal
        [isOpen]="isDeleteModalOpen()"
        title="Supprimer définitivement cet établissement ?"
        message="Cette action est irréversible. L'hôtel ne doit avoir aucune réservation active."
        confirmText="Supprimer"
        [isDanger]="true"
        (confirmed)="executeDelete()"
        (cancelled)="isDeleteModalOpen.set(false)">
      </app-confirm-modal>
    </div>
  `,
  styles: [`
    .flex-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 1.5rem;
    }

    .p-filters {
      padding: 1rem 1.5rem;
      margin-bottom: 1.5rem;
    }

    .hotel-table-name {
      color: var(--forest-green);
      font-size: 0.95rem;
    }

    .actions-group {
      display: inline-flex;
      align-items: center;
      gap: 0.35rem;
    }

    .form-row {
      display: flex;
      gap: 1rem;
    }

    .flex-1 { flex: 1; }
    .flex-2 { flex: 2; }

    .modal-title {
      font-family: var(--font-serif);
      font-size: 1.25rem;
      color: var(--forest-green);
    }

    .btn-close {
      background: none;
      border: none;
      font-size: 1.1rem;
      cursor: pointer;
      color: var(--text-muted);
    }

    .modal-actions {
      display: flex;
      justify-content: flex-end;
      gap: 0.75rem;
    }
  `]
})
export class HotelsManagementComponent implements OnInit {
  private hotelService = inject(HotelService);
  private notification = inject(NotificationService);
  private fb = inject(FormBuilder);
  authService = inject(AuthService);

  hotels = signal<Hotel[]>([]);
  isLoading = signal<boolean>(true);
  isSubmitting = signal<boolean>(false);
  searchTerm: string = '';

  isFormModalOpen = signal<boolean>(false);
  editingHotelId = signal<string | null>(null);

  isDeleteModalOpen = signal<boolean>(false);
  hotelToDelete = signal<Hotel | null>(null);

  hotelForm: FormGroup = this.fb.group({
    nom: ['', Validators.required],
    ville: ['', Validators.required],
    adresse: ['', Validators.required],
    nbEtoiles: [5, Validators.required],
    telephone: ['', Validators.required],
    email: ['', [Validators.required, Validators.email]],
    description: ['', Validators.required],
    imageUrl: ['']
  });

  filteredHotels = () => {
    const term = this.searchTerm.toLowerCase().trim();
    if (!term) return this.hotels();
    return this.hotels().filter(h => 
      h.nom.toLowerCase().includes(term) || h.ville.toLowerCase().includes(term)
    );
  };

  ngOnInit(): void {
    this.loadHotels();
  }

  loadHotels(): void {
    this.isLoading.set(true);
    this.hotelService.getHotels().subscribe({
      next: (res) => {
        this.hotels.set(res);
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
  }

  openAddModal(): void {
    this.editingHotelId.set(null);
    this.hotelForm.reset({ nbEtoiles: 5 });
    this.isFormModalOpen.set(true);
  }

  openEditModal(hotel: Hotel): void {
    this.editingHotelId.set(hotel.id);
    this.hotelForm.patchValue({
      nom: hotel.nom,
      ville: hotel.ville,
      adresse: hotel.adresse,
      nbEtoiles: hotel.nbEtoiles,
      telephone: hotel.telephone,
      email: hotel.email,
      description: hotel.description,
      imageUrl: hotel.images?.[0] || ''
    });
    this.isFormModalOpen.set(true);
  }

  saveHotel(): void {
    if (this.hotelForm.invalid) return;

    this.isSubmitting.set(true);
    const formVal = this.hotelForm.value;
    const payload: CreerHotelRequest = {
      nom: formVal.nom,
      ville: formVal.ville,
      adresse: formVal.adresse,
      nbEtoiles: Number(formVal.nbEtoiles),
      telephone: formVal.telephone,
      email: formVal.email,
      description: formVal.description,
      images: formVal.imageUrl ? [formVal.imageUrl] : []
    };

    const id = this.editingHotelId();
    if (id) {
      this.hotelService.updateHotel(id, payload).subscribe({
        next: () => {
          this.isSubmitting.set(false);
          this.isFormModalOpen.set(false);
          this.notification.success('Hôtel mis à jour avec succès.');
          this.loadHotels();
        },
        error: (err) => {
          this.isSubmitting.set(false);
          this.notification.error(err.error?.message || 'Erreur lors de la mise à jour.');
        }
      });
    } else {
      this.hotelService.createHotel(payload).subscribe({
        next: () => {
          this.isSubmitting.set(false);
          this.isFormModalOpen.set(false);
          this.notification.success('Nouvel hôtel créé avec succès.');
          this.loadHotels();
        },
        error: (err) => {
          this.isSubmitting.set(false);
          this.notification.error(err.error?.message || 'Erreur lors de la création.');
        }
      });
    }
  }

  toggleActive(hotel: Hotel): void {
    const nextState = !(hotel.actif !== false);
    this.hotelService.toggleActive(hotel.id, nextState).subscribe({
      next: () => {
        this.notification.success(`Hôtel ${nextState ? 'activé' : 'désactivé'} avec succès.`);
        this.loadHotels();
      },
      error: () => this.notification.error('Erreur lors du changement de statut.')
    });
  }

  askDelete(hotel: Hotel): void {
    this.hotelToDelete.set(hotel);
    this.isDeleteModalOpen.set(true);
  }

  executeDelete(): void {
    const h = this.hotelToDelete();
    if (!h) return;

    this.hotelService.deleteHotel(h.id).subscribe({
      next: () => {
        this.notification.success('Hôtel supprimé avec succès.');
        this.isDeleteModalOpen.set(false);
        this.loadHotels();
      },
      error: (err) => {
        this.notification.error(err.error?.message || 'Impossible de supprimer cet hôtel (réservations rattachées).');
        this.isDeleteModalOpen.set(false);
      }
    });
  }
}
