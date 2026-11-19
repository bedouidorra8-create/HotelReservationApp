import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { HotelService } from '../../../core/services/hotel.service';
import { ChambreService } from '../../../core/services/chambre.service';
import { NotificationService } from '../../../core/services/notification.service';
import { Hotel, Chambre, TypeChambre, StatutChambre, CreerChambreRequest, ModifierChambreRequest } from '../../../core/models';
import { RoomTypePipe } from '../../../shared/pipes/room-type.pipe';
import { StatusBadgePipe } from '../../../shared/pipes/status-badge.pipe';
import { LoadingSpinnerComponent } from '../../../shared/components/loading-spinner.component';
import { ConfirmModalComponent } from '../../../shared/components/confirm-modal.component';

@Component({
  selector: 'app-rooms-management',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, RoomTypePipe, LoadingSpinnerComponent, ConfirmModalComponent],
  template: `
    <div class="rooms-management-page">
      <div class="page-header flex-header">
        <div>
          <h1 class="page-title">Gestion des Chambres & Suites</h1>
          <p class="page-subtitle">Inventaire, tarification et statuts opérationnels des hébergements</p>
        </div>
        <button class="btn btn-gold" [disabled]="!selectedHotelId()" (click)="openAddModal()">
          + Ajouter une Chambre
        </button>
      </div>

      <!-- Sélecteur d'hôtel -->
      <div class="card-luxury p-selector">
        <label class="form-label">Sélectionner un Établissement Hôtelier :</label>
        <select class="form-select" [ngModel]="selectedHotelId()" (ngModelChange)="onHotelChange($event)">
          @for (h of hotels(); track h.id) {
            <option [value]="h.id">{{ h.nom }} ({{ h.ville }})</option>
          }
        </select>
      </div>

      @if (isLoading()) {
        <app-loading-spinner message="Chargement des chambres..."></app-loading-spinner>
      } @else {
        <div class="table-responsive">
          <table class="custom-table">
            <thead>
              <tr>
                <th>Numéro</th>
                <th>Catégorie / Type</th>
                <th>Étage</th>
                <th>Tarif / nuitée</th>
                <th>Statut Opérationnel</th>
                <th style="text-align: right;">Actions</th>
              </tr>
            </thead>
            <tbody>
              @for (r of rooms(); track r.id) {
                <tr>
                  <td>
                    <strong style="color: var(--forest-green); font-size: 1.05rem;">{{ r.numero }}</strong>
                  </td>
                  <td>
                    <strong>{{ r.type | roomType }}</strong>
                  </td>
                  <td>Étage {{ r.etage }}</td>
                  <td>
                    <strong style="color: var(--forest-green);">{{ r.prixMontant }} {{ r.prixDevise || 'EUR' }}</strong>
                  </td>
                  <td>
                    <select 
                      class="form-select form-select-sm status-dropdown" 
                      [ngModel]="r.statut" 
                      (ngModelChange)="onStatusChange(r, $event)">
                      <option [ngValue]="1">Libre (Disponible)</option>
                      <option [ngValue]="2">Occupée</option>
                      <option [ngValue]="3">Maintenance</option>
                    </select>
                  </td>
                  <td style="text-align: right;">
                    <div class="actions-group">
                      <button class="btn btn-ghost btn-sm" (click)="openEditModal(r)" title="Modifier">✏️</button>
                      <button class="btn btn-ghost btn-sm" style="color: var(--color-error);" (click)="askDelete(r)" title="Supprimer">🗑️</button>
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
          <div class="modal-dialog" (click)="$event.stopPropagation()">
            <div class="card-header">
              <h3 class="modal-title">{{ editingRoomId() ? 'Modifier la Chambre' : 'Ajouter une Chambre' }}</h3>
              <button class="btn-close" (click)="isFormModalOpen.set(false)">✕</button>
            </div>

            <form [formGroup]="roomForm" (ngSubmit)="saveRoom()">
              <div class="card-body">
                @if (!editingRoomId()) {
                  <div class="form-group">
                    <label class="form-label">Numéro de chambre * (ex: 101)</label>
                    <input type="text" formControlName="numero" class="form-control" />
                  </div>
                }

                <div class="form-group">
                  <label class="form-label">Type d'hébergement *</label>
                  <select formControlName="type" class="form-select">
                    <option [ngValue]="1">Chambre Simple</option>
                    <option [ngValue]="2">Chambre Double</option>
                    <option [ngValue]="3">Suite Prestige</option>
                    <option [ngValue]="4">Chambre Familiale</option>
                  </select>
                </div>

                <div class="form-row">
                  <div class="form-group flex-1">
                    <label class="form-label">Étage *</label>
                    <input type="number" formControlName="etage" class="form-control" min="0" max="50" />
                  </div>
                  <div class="form-group flex-1">
                    <label class="form-label">Tarif par nuit (DT) *</label>
                    <input type="number" formControlName="prixMontant" class="form-control" min="1" />
                  </div>
                </div>
              </div>

              <div class="card-footer modal-actions">
                <button type="button" class="btn btn-outline btn-sm" (click)="isFormModalOpen.set(false)">Annuler</button>
                <button type="submit" class="btn btn-gold btn-sm" [disabled]="roomForm.invalid || isSubmitting()">
                  {{ editingRoomId() ? 'Mettre à jour' : 'Créer la chambre' }}
                </button>
              </div>
            </form>
          </div>
        </div>
      }

      <!-- Modale Confirmation Suppression -->
      <app-confirm-modal
        [isOpen]="isDeleteModalOpen()"
        title="Supprimer cette chambre ?"
        message="Êtes-vous certain de vouloir supprimer cette chambre ? Elle ne doit pas comporter de réservations en cours."
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

    .p-selector {
      padding: 1.25rem 1.5rem;
      margin-bottom: 1.5rem;
      max-width: 500px;
    }

    .status-dropdown {
      max-width: 170px;
      padding: 0.35rem 0.65rem;
      font-size: 0.85rem;
    }

    .actions-group {
      display: inline-flex;
      gap: 0.35rem;
    }

    .form-row {
      display: flex;
      gap: 1rem;
    }

    .flex-1 { flex: 1; }

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
export class RoomsManagementComponent implements OnInit {
  private hotelService = inject(HotelService);
  private chambreService = inject(ChambreService);
  private notification = inject(NotificationService);
  private fb = inject(FormBuilder);

  hotels = signal<Hotel[]>([]);
  selectedHotelId = signal<string | null>(null);
  rooms = signal<Chambre[]>([]);
  isLoading = signal<boolean>(true);
  isSubmitting = signal<boolean>(false);

  isFormModalOpen = signal<boolean>(false);
  editingRoomId = signal<string | null>(null);

  isDeleteModalOpen = signal<boolean>(false);
  roomToDelete = signal<Chambre | null>(null);

  roomForm: FormGroup = this.fb.group({
    numero: ['', Validators.required],
    type: [2, Validators.required],
    etage: [1, [Validators.required, Validators.min(0)]],
    prixMontant: [250, [Validators.required, Validators.min(1)]]
  });

  ngOnInit(): void {
    this.hotelService.getHotels().subscribe({
      next: (hList) => {
        this.hotels.set(hList);
        if (hList.length > 0) {
          this.onHotelChange(hList[0].id);
        } else {
          this.isLoading.set(false);
        }
      },
      error: () => this.isLoading.set(false)
    });
  }

  onHotelChange(hotelId: string): void {
    this.selectedHotelId.set(hotelId);
    this.loadRooms(hotelId);
  }

  loadRooms(hotelId: string): void {
    this.isLoading.set(true);
    this.chambreService.getChambresByHotel(hotelId).subscribe({
      next: (rList) => {
        this.rooms.set(rList);
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
  }

  openAddModal(): void {
    this.editingRoomId.set(null);
    this.roomForm.reset({ type: 2, etage: 1, prixMontant: 280 });
    this.roomForm.get('numero')?.enable();
    this.isFormModalOpen.set(true);
  }

  openEditModal(room: Chambre): void {
    this.editingRoomId.set(room.id);
    this.roomForm.patchValue({
      numero: room.numero,
      type: room.type,
      etage: room.etage,
      prixMontant: room.prixMontant
    });
    this.roomForm.get('numero')?.disable();
    this.isFormModalOpen.set(true);
  }

  saveRoom(): void {
    if (this.roomForm.invalid) return;

    const hotelId = this.selectedHotelId();
    if (!hotelId) return;

    this.isSubmitting.set(true);
    const formVal = this.roomForm.getRawValue();

    const id = this.editingRoomId();
    if (id) {
      const updatePayload: ModifierChambreRequest = {
        type: Number(formVal.type),
        etage: Number(formVal.etage),
        prixMontant: Number(formVal.prixMontant)
      };
      this.chambreService.updateChambre(id, updatePayload).subscribe({
        next: () => {
          this.isSubmitting.set(false);
          this.isFormModalOpen.set(false);
          this.notification.success('Chambre mise à jour avec succès.');
          this.loadRooms(hotelId);
        },
        error: (err) => {
          this.isSubmitting.set(false);
          this.notification.error(err.error?.message || 'Erreur lors de la mise à jour.');
        }
      });
    } else {
      const createPayload: CreerChambreRequest = {
        hotelId,
        numero: formVal.numero,
        type: Number(formVal.type),
        etage: Number(formVal.etage),
        prixMontant: Number(formVal.prixMontant)
      };
      this.chambreService.createChambre(hotelId, createPayload).subscribe({
        next: () => {
          this.isSubmitting.set(false);
          this.isFormModalOpen.set(false);
          this.notification.success('Nouvelle chambre créée avec succès.');
          this.loadRooms(hotelId);
        },
        error: (err) => {
          this.isSubmitting.set(false);
          this.notification.error(err.error?.message || 'Erreur lors de la création (le numéro existe peut-être déjà).');
        }
      });
    }
  }

  onStatusChange(room: Chambre, newStatus: number): void {
    this.chambreService.changeStatus(room.id, newStatus).subscribe({
      next: () => {
        this.notification.success(`Statut de la chambre ${room.numero} modifié.`);
      },
      error: () => this.notification.error('Erreur lors du changement de statut.')
    });
  }

  askDelete(room: Chambre): void {
    this.roomToDelete.set(room);
    this.isDeleteModalOpen.set(true);
  }

  executeDelete(): void {
    const r = this.roomToDelete();
    const hotelId = this.selectedHotelId();
    if (!r || !hotelId) return;

    this.chambreService.deleteChambre(r.id).subscribe({
      next: () => {
        this.notification.success('Chambre supprimée avec succès.');
        this.isDeleteModalOpen.set(false);
        this.loadRooms(hotelId);
      },
      error: (err) => {
        this.notification.error(err.error?.message || 'Impossible de supprimer cette chambre.');
        this.isDeleteModalOpen.set(false);
      }
    });
  }
}
