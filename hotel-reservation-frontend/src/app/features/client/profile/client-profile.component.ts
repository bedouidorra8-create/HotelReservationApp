import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { AuthService } from '../../../core/auth/auth.service';
import { NotificationService } from '../../../core/services/notification.service';
import { ProfilUtilisateurDto, ModifierProfilRequest } from '../../../core/models';
import { LoadingSpinnerComponent } from '../../../shared/components/loading-spinner.component';

@Component({
  selector: 'app-client-profile',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, LoadingSpinnerComponent],
  template: `
    <div class="profile-page">
      <div class="page-header">
        <h1 class="page-title">Mon Profil Membre</h1>
        <p class="page-subtitle">Gérez vos coordonnées personnelles et la sécurité de votre compte</p>
      </div>

      @if (isLoading()) {
        <app-loading-spinner message="Chargement de votre profil..."></app-loading-spinner>
      } @else {
        <div class="profile-grid">
          <!-- Formulaire Informations Personnelles -->
          <div class="card-luxury p-card">
            <h2 class="section-title">Coordonnées Personnelles</h2>
            
            <form [formGroup]="profileForm" (ngSubmit)="onUpdateProfile()">
              <div class="form-row">
                <div class="form-group flex-1">
                  <label class="form-label">Prénom</label>
                  <input type="text" formControlName="prenom" class="form-control" />
                </div>
                <div class="form-group flex-1">
                  <label class="form-label">Nom</label>
                  <input type="text" formControlName="nom" class="form-control" />
                </div>
              </div>

              <div class="form-group">
                <label class="form-label">Email (identifiant)</label>
                <input type="email" [value]="profile()?.email" disabled class="form-control" style="background: #f7f7f7;" />
                <small style="color: var(--text-muted); font-size: 0.75rem;">L'adresse email ne peut être modifiée directement.</small>
              </div>

              <div class="form-row">
                <div class="form-group flex-1">
                  <label class="form-label">Téléphone</label>
                  <input type="tel" formControlName="telephone" class="form-control" />
                </div>
                <div class="form-group flex-1">
                  <label class="form-label">Date de naissance</label>
                  <input type="date" formControlName="dateNaissance" class="form-control" />
                </div>
              </div>

              <div class="form-group">
                <label class="form-label">Adresse de résidence</label>
                <input type="text" formControlName="adresse" class="form-control" />
              </div>

              <button type="submit" class="btn btn-gold" [disabled]="profileForm.invalid || isUpdating()">
                @if (isUpdating()) {
                  <span>Enregistrement...</span>
                } @else {
                  <span>Mettre à jour mes coordonnées</span>
                }
              </button>
            </form>
          </div>

          <!-- Sécurité & Mot de passe -->
          <div class="card-luxury p-card">
            <h2 class="section-title">Sécurité du Compte</h2>
            <p class="section-desc">Modifiez régulièrement votre mot de passe pour garantir la confidentialité de vos réservations.</p>

            <form [formGroup]="passwordForm" (ngSubmit)="onChangePassword()">
              <div class="form-group">
                <label class="form-label">Mot de passe actuel</label>
                <input type="password" formControlName="ancienMotDePasse" placeholder="••••••••••••" class="form-control" />
              </div>

              <div class="form-group">
                <label class="form-label">Nouveau mot de passe</label>
                <input type="password" formControlName="nouveauMotDePasse" placeholder="Minimum 6 caractères" class="form-control" />
              </div>

              <button type="submit" class="btn btn-primary" [disabled]="passwordForm.invalid || isChangingPassword()">
                @if (isChangingPassword()) {
                  <span>Mise à jour...</span>
                } @else {
                  <span>Changer le mot de passe</span>
                }
              </button>
            </form>

            <div class="danger-zone">
              <h3 class="danger-title">Fermeture de session</h3>
              <p class="danger-desc">Déconnectez votre session de cet appareil.</p>
              <button class="btn btn-outline btn-sm" (click)="authService.logout()">Déconnexion complète</button>
            </div>
          </div>
        </div>
      }
    </div>
  `,
  styles: [`
    .profile-page {
      max-width: 980px;
      margin: 0 auto;
    }

    .profile-grid {
      display: grid;
      grid-template-columns: 1.2fr 1fr;
      gap: 2rem;
      align-items: flex-start;
    }

    .p-card {
      padding: 2rem;
    }

    .section-title {
      font-family: var(--font-serif);
      font-size: 1.35rem;
      color: var(--forest-green);
      margin-bottom: 0.5rem;
    }

    .section-desc {
      color: var(--text-secondary);
      font-size: 0.885rem;
      margin-bottom: 1.5rem;
    }

    .form-row {
      display: flex;
      gap: 1rem;
    }

    .flex-1 {
      flex: 1;
    }

    .danger-zone {
      margin-top: 2.5rem;
      padding-top: 1.5rem;
      border-top: 1px solid var(--border-subtle);
    }

    .danger-title {
      font-size: 0.95rem;
      color: var(--forest-green);
      margin-bottom: 0.25rem;
    }

    .danger-desc {
      font-size: 0.825rem;
      color: var(--text-muted);
      margin-bottom: 1rem;
    }

    @media (max-width: 768px) {
      .profile-grid {
        grid-template-columns: 1fr;
      }
    }
  `]
})
export class ClientProfileComponent implements OnInit {
  authService = inject(AuthService);
  private notification = inject(NotificationService);
  private fb = inject(FormBuilder);

  profile = signal<ProfilUtilisateurDto | null>(null);
  isLoading = signal<boolean>(true);
  isUpdating = signal<boolean>(false);
  isChangingPassword = signal<boolean>(false);

  profileForm: FormGroup = this.fb.group({
    prenom: ['', Validators.required],
    nom: ['', Validators.required],
    telephone: ['', Validators.required],
    adresse: ['', Validators.required],
    dateNaissance: [''],
    sexe: [null]
  });

  passwordForm: FormGroup = this.fb.group({
    ancienMotDePasse: ['', Validators.required],
    nouveauMotDePasse: ['', [Validators.required, Validators.minLength(6)]]
  });

  ngOnInit(): void {
    this.authService.getProfile().subscribe({
      next: (p) => {
        this.profile.set(p);
        this.profileForm.patchValue({
          prenom: p.prenom,
          nom: p.nom,
          telephone: p.telephone,
          adresse: p.adresse,
          dateNaissance: p.dateNaissance ? p.dateNaissance.substring(0, 10) : '',
          sexe: p.sexe
        });
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
  }

  onUpdateProfile(): void {
    if (this.profileForm.invalid) return;

    this.isUpdating.set(true);
    const formVal = this.profileForm.value;
    const req: ModifierProfilRequest = {
      prenom: formVal.prenom,
      nom: formVal.nom,
      telephone: formVal.telephone,
      adresse: formVal.adresse,
      dateNaissance: formVal.dateNaissance ? new Date(formVal.dateNaissance).toISOString() : null,
      sexe: formVal.sexe
    };

    this.authService.updateProfile(req).subscribe({
      next: (updated) => {
        this.profile.set(updated);
        this.isUpdating.set(false);
        this.notification.success('Vos coordonnées ont été mises à jour avec succès.');
      },
      error: (err) => {
        this.isUpdating.set(false);
        this.notification.error(err.error?.message || 'Erreur lors de la mise à jour.');
      }
    });
  }

  onChangePassword(): void {
    if (this.passwordForm.invalid) return;

    this.isChangingPassword.set(true);
    this.authService.changePassword(this.passwordForm.value).subscribe({
      next: (res) => {
        this.isChangingPassword.set(false);
        this.passwordForm.reset();
        this.notification.success(res.message || 'Votre mot de passe a été modifié.');
      },
      error: (err) => {
        this.isChangingPassword.set(false);
        this.notification.error(err.error?.message || 'Erreur lors du changement de mot de passe.');
      }
    });
  }
}
