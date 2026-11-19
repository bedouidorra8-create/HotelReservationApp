import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { AuthService } from '../../../core/auth/auth.service';
import { NotificationService } from '../../../core/services/notification.service';
import { Genre } from '../../../core/models';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, RouterModule, ReactiveFormsModule],
  template: `
    <div class="auth-page">
      <div class="register-card card-luxury">
        <div class="auth-header">
          <span class="crest">✦</span>
          <h2 class="auth-title">Adhésion au Club Privilège</h2>
          <p class="auth-subtitle">Créez votre profil voyageur pour réserver nos établissements</p>
        </div>

        @if (errorMessage()) {
          <div class="alert-error">
            <span>⚠</span>
            <span>{{ errorMessage() }}</span>
          </div>
        }

        <form [formGroup]="registerForm" (ngSubmit)="onSubmit()" class="register-form">
          <div class="form-row">
            <div class="form-group">
              <label class="form-label">Prénom *</label>
              <input type="text" formControlName="prenom" placeholder="Ex: Alexandre" class="form-control" />
            </div>
            <div class="form-group">
              <label class="form-label">Nom *</label>
              <input type="text" formControlName="nom" placeholder="Ex: Dumas" class="form-control" />
            </div>
          </div>

          <div class="form-row">
            <div class="form-group">
              <label class="form-label">Adresse Email *</label>
              <input type="email" formControlName="email" placeholder="alexandre.dumas@exemple.com" class="form-control" />
            </div>
            <div class="form-group">
              <label class="form-label">Téléphone *</label>
              <input type="tel" formControlName="telephone" placeholder="+33 6 12 34 56 78" class="form-control" />
            </div>
          </div>

          <div class="form-row">
            <div class="form-group">
              <label class="form-label">Mot de passe *</label>
              <input type="password" formControlName="motDePasse" placeholder="Minimum 6 caractères" class="form-control" />
            </div>
            <div class="form-group">
              <label class="form-label">Date de naissance *</label>
              <input type="date" formControlName="dateNaissance" class="form-control" />
            </div>
          </div>

          <div class="form-row">
            <div class="form-group">
              <label class="form-label">Civilité</label>
              <select formControlName="sexe" class="form-select">
                <option [ngValue]="null">Non précisé</option>
                <option [ngValue]="1">Monsieur (Homme)</option>
                <option [ngValue]="2">Madame (Femme)</option>
              </select>
            </div>
            <div class="form-group flex-2">
              <label class="form-label">Adresse de résidence *</label>
              <input type="text" formControlName="adresse" placeholder="12 Rue de la Paix, Paris" class="form-control" />
            </div>
          </div>

          <button type="submit" class="btn btn-gold btn-block" [disabled]="registerForm.invalid || isSubmitting()">
            @if (isSubmitting()) {
              <span>Création de votre compte...</span>
            } @else {
              <span>Valider mon inscription</span>
            }
          </button>
        </form>

        <div class="auth-footer">
          <p>Vous possédez déjà un compte membre ?</p>
          <a routerLink="/auth/login" class="link-login">Se connecter</a>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .auth-page {
      min-height: calc(100vh - 78px - 250px);
      display: flex;
      align-items: center;
      justify-content: center;
      padding: 3rem 1.5rem;
      background: radial-gradient(circle at center, #fbf9f4 0%, var(--ivory) 100%);
    }

    .register-card {
      max-width: 680px;
      width: 100%;
      padding: 2.75rem;
      background: var(--white);
      border: 1px solid var(--soft-gold-border);
      border-radius: var(--radius-lg);
      box-shadow: var(--shadow-lg);
    }

    .auth-header {
      text-align: center;
      margin-bottom: 2rem;
    }

    .crest {
      color: var(--soft-gold);
      font-size: 2rem;
      display: block;
      margin-bottom: 0.5rem;
    }

    .auth-title {
      font-family: var(--font-serif);
      font-size: 1.85rem;
      color: var(--forest-green);
      margin-bottom: 0.35rem;
    }

    .auth-subtitle {
      color: var(--text-secondary);
      font-size: 0.885rem;
    }

    .alert-error {
      background: var(--color-error-bg);
      color: var(--color-error);
      border: 1px solid rgba(197, 48, 48, 0.2);
      padding: 0.75rem 1rem;
      border-radius: var(--radius-sm);
      font-size: 0.875rem;
      margin-bottom: 1.25rem;
      display: flex;
      gap: 0.5rem;
      align-items: center;
    }

    .form-row {
      display: flex;
      gap: 1.25rem;
    }

    .form-row .form-group {
      flex: 1;
    }

    .form-row .flex-2 {
      flex: 2;
    }

    .btn-block {
      width: 100%;
      padding: 0.85rem;
      margin-top: 1rem;
      font-size: 0.95rem;
    }

    .auth-footer {
      text-align: center;
      margin-top: 2rem;
      padding-top: 1.5rem;
      border-top: 1px solid var(--border-subtle);
      font-size: 0.875rem;
      color: var(--text-secondary);
    }

    .link-login {
      display: inline-block;
      margin-top: 0.35rem;
      color: var(--forest-green);
      font-weight: 600;
    }

    .link-login:hover {
      text-decoration: underline;
    }

    @media (max-width: 600px) {
      .form-row {
        flex-direction: column;
        gap: 0;
      }
    }
  `]
})
export class RegisterComponent {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private notification = inject(NotificationService);
  private router = inject(Router);

  isSubmitting = signal<boolean>(false);
  errorMessage = signal<string | null>(null);

  registerForm: FormGroup = this.fb.group({
    prenom: ['', [Validators.required]],
    nom: ['', [Validators.required]],
    email: ['', [Validators.required, Validators.email]],
    telephone: ['', [Validators.required]],
    motDePasse: ['', [Validators.required, Validators.minLength(6)]],
    dateNaissance: ['', [Validators.required]],
    sexe: [null],
    adresse: ['', [Validators.required]],
    role: ['Client']
  });

  onSubmit(): void {
    if (this.registerForm.invalid) return;

    this.isSubmitting.set(true);
    this.errorMessage.set(null);

    const payload = {
      ...this.registerForm.value,
      sexe: this.registerForm.value.sexe ? Number(this.registerForm.value.sexe) : null
    };

    this.authService.register(payload).subscribe({
      next: () => {
        this.isSubmitting.set(false);
        this.notification.success('Votre compte a été créé avec succès ! Connectez-vous dès maintenant.');
        this.router.navigate(['/auth/login']);
      },
      error: (err) => {
        this.isSubmitting.set(false);
        this.errorMessage.set(err.error?.message || "Une erreur est survenue lors de l'inscription.");
      }
    });
  }
}
