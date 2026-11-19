import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { AdministrationService } from '../../../core/services/administration.service';
import { NotificationService } from '../../../core/services/notification.service';
import { UtilisateurDto, RoleDto, AjouterUtilisateurRequest, StatutUtilisateur } from '../../../core/models';
import { LoadingSpinnerComponent } from '../../../shared/components/loading-spinner.component';
import { ConfirmModalComponent } from '../../../shared/components/confirm-modal.component';

@Component({
  selector: 'app-users-management',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, LoadingSpinnerComponent, ConfirmModalComponent],
  template: `
    <div class="users-mgmt-page">
      <div class="page-header flex-header">
        <div>
          <h1 class="page-title">
            @switch (activeTab()) {
              @case ('agents') { Gestion de l'Équipe & Agents Hôteliers }
              @case ('clients') { Gestion des Voyageurs & Clients }
              @case ('admins') { Gouvernance des Administrateurs }
              @case ('roles') { Configuration des Rôles & Privilèges }
              @default { Gestion des Utilisateurs & Rôles }
            }
          </h1>
          <p class="page-subtitle">Administration des comptes, privilèges d'accès et attribution des rôles</p>
        </div>
        <div class="header-actions">
          <button class="btn btn-gold" (click)="openAddModal()">+ Nouveau Compte Personnel</button>
        </div>
      </div>

      <!-- Onglets de sélection rapides -->
      <div class="tabs-nav">
        <button 
          class="tab-btn" 
          [class.active]="activeTab() === 'all'" 
          (click)="selectTab('all')">
          👥 Tous les Utilisateurs ({{ users().length }})
        </button>
        <button 
          class="tab-btn" 
          [class.active]="activeTab() === 'agents'" 
          (click)="selectTab('agents')">
          👨‍💼 Agents Hôteliers ({{ getCount('Agent') }})
        </button>
        <button 
          class="tab-btn" 
          [class.active]="activeTab() === 'clients'" 
          (click)="selectTab('clients')">
          👤 Clients Voyageurs ({{ getCount('Client') }})
        </button>
        <button 
          class="tab-btn" 
          [class.active]="activeTab() === 'admins'" 
          (click)="selectTab('admins')">
          🛡️ Administrateurs ({{ getCount('Administrateur') }})
        </button>
        <button 
          class="tab-btn" 
          [class.active]="activeTab() === 'roles'" 
          (click)="selectTab('roles')">
          🔐 Rôles & Matrice
        </button>
      </div>

      <!-- Si onglet roles -->
      @if (activeTab() === 'roles') {
        <div class="roles-matrix-card card-luxury">
          <h2 class="sub-title">Configuration des Rôles & Permissions Système</h2>
          <p style="color: var(--text-muted); font-size: 0.9rem; margin-bottom: 1.5rem;">
            Matrice des droits et responsabilités au sein de la plateforme HOTELI
          </p>
          <div class="roles-grid">
            @for (r of roles(); track r.id) {
              <div class="role-box">
                <div class="role-header">
                  <span class="role-badge">{{ r.nom }}</span>
                </div>
                <ul class="perm-list">
                  @for (perm of r.permissions; track perm) {
                    <li><span class="check-icon">✓</span> {{ perm }}</li>
                  }
                </ul>
              </div>
            }
          </div>
        </div>
      } @else {
        <!-- Filtres et Recherche -->
        <div class="card-luxury p-filters">
          <div class="filters-row">
            <input 
              type="text" 
              [(ngModel)]="searchTerm" 
              (ngModelChange)="applySearch()" 
              placeholder="Rechercher par nom, prénom, email ou téléphone..." 
              class="form-control" />
            
            @if (activeTab() === 'all') {
              <select [(ngModel)]="filterRole" (ngModelChange)="applySearch()" class="form-select" style="max-width: 220px;">
                <option value="">Tous les rôles</option>
                <option value="Administrateur">Administrateur</option>
                <option value="Agent">Agent</option>
                <option value="Client">Client</option>
              </select>
            }
          </div>
        </div>

        @if (isLoading()) {
          <app-loading-spinner message="Chargement des comptes..."></app-loading-spinner>
        } @else {
          <div class="table-responsive">
            <table class="custom-table">
              <thead>
                <tr>
                  <th>Utilisateur</th>
                  <th>Email</th>
                  <th>Téléphone</th>
                  <th>Rôle</th>
                  <th>Statut</th>
                  <th>Date d'adhésion</th>
                  <th style="text-align: right;">Actions</th>
                </tr>
              </thead>
              <tbody>
                @for (u of filteredUsers(); track u.id) {
                  <tr>
                    <td>
                      <div class="user-cell">
                        <div class="user-avatar">{{ u.prenom.charAt(0) }}{{ u.nom.charAt(0) }}</div>
                        <div>
                          <strong>{{ u.prenom }} {{ u.nom }}</strong>
                          @if (u.role === 'Agent') {
                            <div class="user-sub">Collaborateur Hôtelier</div>
                          } @else if (u.role === 'Client') {
                            <div class="user-sub">Voyageur Membre</div>
                          } @else {
                            <div class="user-sub">Direction Générale</div>
                          }
                        </div>
                      </div>
                    </td>
                    <td>{{ u.email }}</td>
                    <td>{{ u.telephone || '—' }}</td>
                    <td>
                      <span class="role-pill" 
                        [class.admin]="u.role === 'Administrateur'" 
                        [class.agent]="u.role === 'Agent'"
                        [class.client]="u.role === 'Client'">
                        {{ u.role }}
                      </span>
                    </td>
                    <td>
                      <span class="badge" [ngClass]="u.statut === 1 ? 'badge-success' : 'badge-danger'">
                        {{ u.statut === 1 ? 'Actif' : 'Désactivé' }}
                      </span>
                    </td>
                    <td>{{ u.dateCreation | date:'dd/MM/yyyy' }}</td>
                    <td style="text-align: right;">
                      <div class="actions-group">
                        <!-- Bouton Modifier -->
                        <button 
                          class="btn btn-outline btn-sm" 
                          (click)="openEditModal(u)" 
                          title="Modifier les coordonnées">
                          ✏️ Modifier
                        </button>

                        <!-- Bouton Activer / Désactiver -->
                        <button 
                          class="btn btn-sm" 
                          [ngClass]="u.statut === 1 ? 'btn-ghost' : 'btn-secondary'"
                          (click)="toggleActive(u)"
                          title="{{ u.statut === 1 ? 'Désactiver le compte' : 'Activer le compte' }}">
                          {{ u.statut === 1 ? 'Désactiver' : 'Activer' }}
                        </button>

                        <!-- Bouton Supprimer -->
                        <button 
                          class="btn btn-ghost btn-sm" 
                          style="color: var(--color-error);" 
                          (click)="askDelete(u)"
                          title="Supprimer définitivement">
                          🗑️
                        </button>
                      </div>
                    </td>
                  </tr>
                } @empty {
                  <tr>
                    <td colspan="7" style="text-align: center; padding: 2.5rem; color: var(--text-muted);">
                      Aucun compte correspondant aux critères sélectionnés.
                    </td>
                  </tr>
                }
              </tbody>
            </table>
          </div>
        }
      }

      <!-- Modale de Modification Utilisateur / Agent / Client -->
      @if (isEditModalOpen() && editingUser()) {
        <div class="modal-overlay" (click)="isEditModalOpen.set(false)">
          <div class="modal-dialog" (click)="$event.stopPropagation()">
            <div class="card-header">
              <h3 class="modal-title">
                Modifier {{ editingUser()!.role }} : {{ editingUser()!.prenom }} {{ editingUser()!.nom }}
              </h3>
              <button class="btn-close" (click)="isEditModalOpen.set(false)">✕</button>
            </div>

            <form [formGroup]="editForm" (ngSubmit)="saveEdit()">
              <div class="card-body">
                <div class="form-row">
                  <div class="form-group flex-1">
                    <label class="form-label">Prénom *</label>
                    <input type="text" formControlName="prenom" class="form-control" />
                  </div>
                  <div class="form-group flex-1">
                    <label class="form-label">Nom *</label>
                    <input type="text" formControlName="nom" class="form-control" />
                  </div>
                </div>

                <div class="form-row">
                  <div class="form-group flex-1">
                    <label class="form-label">Email (Identifiant)</label>
                    <input type="email" [value]="editingUser()!.email" disabled class="form-control disabled-input" />
                    <small style="color: var(--text-muted); font-size: 0.75rem;">L'adresse email constitue l'identifiant unique de connexion.</small>
                  </div>
                  <div class="form-group flex-1">
                    <label class="form-label">Téléphone *</label>
                    <input type="tel" formControlName="telephone" class="form-control" />
                  </div>
                </div>

                <div class="form-group">
                  <label class="form-label">Adresse / Résidence</label>
                  <input type="text" formControlName="adresse" class="form-control" placeholder="ex: Tunis, Sidi Bou Saïd, Hammamet..." />
                </div>
              </div>

              <div class="card-footer modal-actions">
                <button type="button" class="btn btn-outline btn-sm" (click)="isEditModalOpen.set(false)">Annuler</button>
                <button type="submit" class="btn btn-gold btn-sm" [disabled]="editForm.invalid || isSubmitting()">
                  Enregistrer les modifications
                </button>
              </div>
            </form>
          </div>
        </div>
      }

      <!-- Modale Création Compte Personnel (Collaborateur) -->
      @if (isAddModalOpen()) {
        <div class="modal-overlay" (click)="isAddModalOpen.set(false)">
          <div class="modal-dialog" (click)="$event.stopPropagation()">
            <div class="card-header">
              <h3 class="modal-title">Créer un Compte Collaborateur</h3>
              <button class="btn-close" (click)="isAddModalOpen.set(false)">✕</button>
            </div>

            <form [formGroup]="userForm" (ngSubmit)="saveUser()">
              <div class="card-body">
                <div class="form-row">
                  <div class="form-group flex-1">
                    <label class="form-label">Prénom *</label>
                    <input type="text" formControlName="prenom" class="form-control" />
                  </div>
                  <div class="form-group flex-1">
                    <label class="form-label">Nom *</label>
                    <input type="text" formControlName="nom" class="form-control" />
                  </div>
                </div>

                <div class="form-row">
                  <div class="form-group flex-1">
                    <label class="form-label">Email *</label>
                    <input type="email" formControlName="email" class="form-control" />
                  </div>
                  <div class="form-group flex-1">
                    <label class="form-label">Téléphone *</label>
                    <input type="tel" formControlName="telephone" class="form-control" />
                  </div>
                </div>

                <div class="form-row">
                  <div class="form-group flex-1">
                    <label class="form-label">Mot de passe temporaire *</label>
                    <input type="password" formControlName="motDePasse" class="form-control" />
                  </div>
                  <div class="form-group flex-1">
                    <label class="form-label">Rôle attribué *</label>
                    <select formControlName="role" class="form-select">
                      <option value="Agent">Agent Hôtelier</option>
                      <option value="Administrateur">Administrateur</option>
                    </select>
                  </div>
                </div>

                <div class="form-group">
                  <label class="form-label">Adresse professionnelle</label>
                  <input type="text" formControlName="adresse" class="form-control" placeholder="Bureau / Établissement de rattachement" />
                </div>
              </div>

              <div class="card-footer modal-actions">
                <button type="button" class="btn btn-outline btn-sm" (click)="isAddModalOpen.set(false)">Annuler</button>
                <button type="submit" class="btn btn-gold btn-sm" [disabled]="userForm.invalid || isSubmitting()">
                  Créer le compte
                </button>
              </div>
            </form>
          </div>
        </div>
      }

      <!-- Modale de Confirmation de Suppression -->
      <app-confirm-modal
        [isOpen]="isDeleteModalOpen()"
        title="Supprimer ce compte ?"
        [message]="getDeleteMessage()"
        confirmText="Confirmer la suppression"
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

    .tabs-nav {
      display: flex;
      gap: 0.5rem;
      border-bottom: 2px solid var(--border-subtle);
      margin-bottom: 1.5rem;
      overflow-x: auto;
    }

    .tab-btn {
      padding: 0.75rem 1.25rem;
      background: transparent;
      border: none;
      border-bottom: 3px solid transparent;
      color: var(--text-secondary);
      font-weight: 600;
      cursor: pointer;
      font-size: 0.95rem;
      transition: all var(--transition-fast);
      white-space: nowrap;
    }

    .tab-btn:hover {
      color: var(--forest-green);
    }

    .tab-btn.active {
      color: var(--forest-green);
      border-bottom-color: var(--soft-gold);
    }

    .p-filters {
      padding: 1.25rem 1.5rem;
      margin-bottom: 1.5rem;
    }

    .filters-row {
      display: flex;
      gap: 1rem;
    }

    .user-cell {
      display: flex;
      align-items: center;
      gap: 0.75rem;
    }

    .user-avatar {
      width: 36px;
      height: 36px;
      border-radius: 50%;
      background: var(--forest-green);
      color: var(--ivory);
      display: flex;
      align-items: center;
      justify-content: center;
      font-size: 0.8rem;
      font-weight: 700;
      letter-spacing: 0.05em;
    }

    .user-sub {
      font-size: 0.75rem;
      color: var(--text-muted);
    }

    .role-pill {
      font-size: 0.75rem;
      font-weight: 700;
      padding: 0.2rem 0.65rem;
      border-radius: var(--radius-xs);
      background: var(--ivory-light);
      color: var(--text-secondary);
      border: 1px solid var(--border-subtle);
    }

    .role-pill.admin {
      background: rgba(44, 95, 45, 0.1);
      color: var(--forest-green);
      border-color: rgba(44, 95, 45, 0.3);
    }

    .role-pill.agent {
      background: rgba(200, 155, 60, 0.15);
      color: #946914;
      border-color: rgba(200, 155, 60, 0.4);
    }

    .role-pill.client {
      background: rgba(33, 150, 243, 0.1);
      color: #1565c0;
      border-color: rgba(33, 150, 243, 0.3);
    }

    .actions-group {
      display: flex;
      gap: 0.5rem;
      justify-content: flex-end;
    }

    .roles-matrix-card {
      padding: 2rem;
      margin-bottom: 2rem;
    }

    .sub-title {
      font-family: var(--font-heading);
      color: var(--forest-green);
      margin-bottom: 0.5rem;
      font-size: 1.25rem;
    }

    .roles-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(280px, 1fr));
      gap: 1.5rem;
    }

    .role-box {
      border: 1px solid var(--border-subtle);
      border-radius: var(--radius-sm);
      padding: 1.25rem;
      background: var(--card-bg, #fff);
      box-shadow: var(--shadow-sm);
    }

    .role-header {
      margin-bottom: 1rem;
    }

    .role-badge {
      display: inline-block;
      font-weight: 700;
      font-size: 0.95rem;
      color: var(--forest-green);
    }

    .perm-list {
      list-style: none;
      padding: 0;
      margin: 0;
      display: flex;
      flex-direction: column;
      gap: 0.5rem;
    }

    .perm-list li {
      font-size: 0.85rem;
      color: var(--text-primary);
      display: flex;
      align-items: center;
      gap: 0.5rem;
    }

    .check-icon {
      color: var(--forest-green);
      font-weight: bold;
    }

    .disabled-input {
      background-color: #f1f3f5;
      color: var(--text-muted);
      cursor: not-allowed;
    }

    .modal-overlay {
      position: fixed;
      inset: 0;
      background: rgba(0, 0, 0, 0.5);
      backdrop-filter: blur(4px);
      display: flex;
      align-items: center;
      justify-content: center;
      z-index: 1000;
    }

    .modal-dialog {
      background: #fff;
      border-radius: var(--radius-md);
      box-shadow: var(--shadow-lg);
      width: 100%;
      max-width: 580px;
      overflow: hidden;
    }

    .modal-actions {
      display: flex;
      justify-content: flex-end;
      gap: 0.75rem;
    }
  `]
})
export class UsersManagementComponent implements OnInit {
  private adminService = inject(AdministrationService);
  private notification = inject(NotificationService);
  private fb = inject(FormBuilder);
  private route = inject(ActivatedRoute);

  users = signal<UtilisateurDto[]>([]);
  roles = signal<RoleDto[]>([]);
  isLoading = signal<boolean>(true);
  isSubmitting = signal<boolean>(false);

  activeTab = signal<'all' | 'agents' | 'clients' | 'admins' | 'roles'>('all');
  searchTerm: string = '';
  filterRole: string = '';

  isAddModalOpen = signal<boolean>(false);
  isEditModalOpen = signal<boolean>(false);
  isDeleteModalOpen = signal<boolean>(false);

  editingUser = signal<UtilisateurDto | null>(null);
  userToDelete = signal<UtilisateurDto | null>(null);

  // Formulaire d'ajout
  userForm: FormGroup = this.fb.group({
    prenom: ['', Validators.required],
    nom: ['', Validators.required],
    email: ['', [Validators.required, Validators.email]],
    telephone: ['', Validators.required],
    motDePasse: ['', [Validators.required, Validators.minLength(6)]],
    role: ['Agent', Validators.required],
    adresse: ['']
  });

  // Formulaire de modification
  editForm: FormGroup = this.fb.group({
    prenom: ['', Validators.required],
    nom: ['', Validators.required],
    telephone: ['', Validators.required],
    adresse: ['']
  });

  ngOnInit(): void {
    // Vérifier si un onglet est demandé par l'URL
    const defaultTab = this.route.snapshot.data['defaultTab'];
    if (defaultTab && ['all', 'agents', 'clients', 'admins', 'roles'].includes(defaultTab)) {
      this.activeTab.set(defaultTab);
    }

    this.adminService.getRoles().subscribe(r => this.roles.set(r));
    this.applySearch();
  }

  selectTab(tab: 'all' | 'agents' | 'clients' | 'admins' | 'roles'): void {
    this.activeTab.set(tab);
  }

  getCount(role: string): number {
    return this.users().filter(u => u.role === role).length;
  }

  filteredUsers(): UtilisateurDto[] {
    const list = this.users();
    const tab = this.activeTab();
    if (tab === 'agents') {
      return list.filter(u => u.role === 'Agent');
    }
    if (tab === 'clients') {
      return list.filter(u => u.role === 'Client');
    }
    if (tab === 'admins') {
      return list.filter(u => u.role === 'Administrateur');
    }
    return list;
  }

  applySearch(): void {
    this.isLoading.set(true);
    const roleParam = this.activeTab() === 'all' ? (this.filterRole || undefined) : undefined;
    this.adminService.searchUsers(this.searchTerm || undefined, roleParam).subscribe({
      next: (res) => {
        this.users.set(res);
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
  }

  openAddModal(): void {
    this.userForm.reset({ role: 'Agent' });
    this.isAddModalOpen.set(true);
  }

  saveUser(): void {
    if (this.userForm.invalid) return;

    this.isSubmitting.set(true);
    const val = this.userForm.value;
    const req: AjouterUtilisateurRequest = {
      prenom: val.prenom,
      nom: val.nom,
      email: val.email,
      telephone: val.telephone,
      motDePasse: val.motDePasse,
      adresse: val.adresse || 'Siège Social',
      dateNaissance: new Date('1990-01-01').toISOString(),
      role: val.role
    };

    this.adminService.addUser(req).subscribe({
      next: () => {
        this.isSubmitting.set(false);
        this.isAddModalOpen.set(false);
        this.notification.success('Compte collaborateur créé avec succès.');
        this.applySearch();
      },
      error: (err) => {
        this.isSubmitting.set(false);
        this.notification.error(err.error?.message || 'Erreur lors de la création du compte.');
      }
    });
  }

  openEditModal(user: UtilisateurDto): void {
    this.editingUser.set(user);
    this.editForm.patchValue({
      prenom: user.prenom,
      nom: user.nom,
      telephone: user.telephone || '',
      adresse: ''
    });
    this.isEditModalOpen.set(true);
  }

  saveEdit(): void {
    const user = this.editingUser();
    if (!user || this.editForm.invalid) return;

    this.isSubmitting.set(true);
    const val = this.editForm.value;

    let op$;
    if (user.role === 'Agent') {
      op$ = this.adminService.updateAgent(user.id, {
        prenom: val.prenom,
        nom: val.nom,
        telephone: val.telephone,
        adresse: val.adresse || ''
      });
    } else if (user.role === 'Client') {
      op$ = this.adminService.updateClient(user.id, {
        prenom: val.prenom,
        nom: val.nom,
        telephone: val.telephone,
        adresse: val.adresse || ''
      });
    } else {
      op$ = this.adminService.updateAdmin(user.id, {
        prenom: val.prenom,
        nom: val.nom,
        telephone: val.telephone,
        adresse: val.adresse || ''
      });
    }

    op$.subscribe({
      next: () => {
        this.isSubmitting.set(false);
        this.isEditModalOpen.set(false);
        this.notification.success(`Coordonnées de ${user.prenom} ${user.nom} mises à jour avec succès.`);
        this.applySearch();
      },
      error: (err) => {
        this.isSubmitting.set(false);
        this.notification.error(err.error?.message || 'Impossible de mettre à jour ce profil.');
      }
    });
  }

  toggleActive(user: UtilisateurDto): void {
    const nextActive = user.statut !== StatutUtilisateur.Actif;
    this.adminService.toggleUserStatus(user.id, nextActive).subscribe({
      next: () => {
        this.notification.success(`Compte ${nextActive ? 'activé' : 'désactivé'}.`);
        this.applySearch();
      },
      error: () => this.notification.error('Erreur lors du changement de statut.')
    });
  }

  askDelete(user: UtilisateurDto): void {
    this.userToDelete.set(user);
    this.isDeleteModalOpen.set(true);
  }

  getDeleteMessage(): string {
    const u = this.userToDelete();
    if (!u) return 'Confirmez-vous la suppression ?';
    if (u.role === 'Agent') {
      return `Attention : supprimer l'agent hôtelier ${u.prenom} ${u.nom} révoquera immédiatement tous ses accès à la gestion des réservations et des chambres.`;
    }
    if (u.role === 'Client') {
      return `Attention : supprimer le client voyageur ${u.prenom} ${u.nom} clôturera son compte.`;
    }
    return `Êtes-vous sûr de vouloir supprimer définitivement le compte administrateur de ${u.prenom} ${u.nom} ?`;
  }

  executeDelete(): void {
    const u = this.userToDelete();
    if (!u) return;

    let op$;
    if (u.role === 'Administrateur') {
      op$ = this.adminService.deleteAdmin(u.id);
    } else if (u.role === 'Client') {
      op$ = this.adminService.deleteClient(u.id);
    } else {
      op$ = this.adminService.deleteAgent(u.id);
    }

    op$.subscribe({
      next: () => {
        this.notification.success('Compte supprimé avec succès.');
        this.isDeleteModalOpen.set(false);
        this.applySearch();
      },
      error: (err) => {
        this.notification.error(err.error?.message || 'Impossible de supprimer ce compte.');
        this.isDeleteModalOpen.set(false);
      }
    });
  }
}
