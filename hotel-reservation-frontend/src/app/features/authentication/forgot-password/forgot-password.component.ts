import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { AuthService } from '../../../core/auth/auth.service';
import { NotificationService } from '../../../core/services/notification.service';

@Component({
  selector: 'app-forgot-password',
  standalone: true,
  imports: [CommonModule, RouterModule, ReactiveFormsModule],
  template: `
    <div class="auth-page">
      <div class="auth-card card-luxury">
        <div class="auth-header">
          <span class="crest">✦</span>
          <h2 class="auth-title">Récupération d'Accès</h2>
          <p class="auth-subtitle">
            @if (step() === 1) {
              Saisissez votre email pour recevoir les instructions de réinitialisation
            } @else {
              Entrez le jeton de sécurité et votre nouveau mot de passe
            }
          </p>
        </div>

        @if (message()) {
          <div class="alert-info">
            <span>ℹ</span>
            <span>{{ message() }}</span>
          </div>
        }

        @if (step() === 1) {
          <form [formGroup]="requestForm" (ngSubmit)="onRequestReset()" class="auth-form">
            <div class="form-group">
              <label class="form-label">Adresse Email</label>
              <input type="email" formControlName="email" placeholder="votre.nom@exemple.com" class="form-control" />
            </div>

            <button type="submit" class="btn btn-gold btn-block" [disabled]="requestForm.invalid || isSubmitting()">
              @if (isSubmitting()) {
                <span>Envoi en cours...</span>
              } @else {
                <span>Envoyer le lien de réinitialisation</span>
              }
            </button>
          </form>
        } @else {
          <form [formGroup]="resetForm" (ngSubmit)="onResetPassword()" class="auth-form">
            <div class="form-group">
              <label class="form-label">Jeton de sécurité (Token)</label>
              <input type="text" formControlName="token" placeholder="Collez le jeton reçu" class="form-control" />
            </div>

            <div class="form-group">
              <label class="form-label">Nouveau mot de passe</label>
              <input type="password" formControlName="nouveauMotDePasse" placeholder="Minimum 6 caractères" class="form-control" />
            </div>

            <button type="submit" class="btn btn-gold btn-block" [disabled]="resetForm.invalid || isSubmitting()">
              @if (isSubmitting()) {
                <span>Mise à jour en cours...</span>
              } @else {
                <span>Définir le nouveau mot de passe</span>
              }
            </button>
          </form>
        }

        <div class="auth-footer">
          <a routerLink="/auth/login" class="link-back">← Revenir à la page de connexion</a>
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

    .auth-card {
      max-width: 460px;
      width: 100%;
      padding: 2.5rem;
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
      line-height: 1.5;
    }

    .alert-info {
      background: var(--color-info-bg);
      color: var(--color-info);
      border: 1px solid rgba(74, 101, 114, 0.2);
      padding: 0.75rem 1rem;
      border-radius: var(--radius-sm);
      font-size: 0.875rem;
      margin-bottom: 1.25rem;
      display: flex;
      gap: 0.5rem;
      align-items: center;
    }

    .btn-block {
      width: 100%;
      padding: 0.8rem;
      margin-top: 1rem;
    }

    .auth-footer {
      text-align: center;
      margin-top: 2rem;
      padding-top: 1.5rem;
      border-top: 1px solid var(--border-subtle);
    }

    .link-back {
      color: var(--secondary-green);
      font-size: 0.875rem;
      font-weight: 500;
    }

    .link-back:hover {
      text-decoration: underline;
    }
  `]
})
export class ForgotPasswordComponent {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private notification = inject(NotificationService);

  step = signal<1 | 2>(1);
  isSubmitting = signal<boolean>(false);
  message = signal<string | null>(null);

  requestForm: FormGroup = this.fb.group({
    email: ['', [Validators.required, Validators.email]]
  });

  resetForm: FormGroup = this.fb.group({
    token: ['', [Validators.required]],
    nouveauMotDePasse: ['', [Validators.required, Validators.minLength(6)]]
  });

  onRequestReset(): void {
    if (this.requestForm.invalid) return;

    this.isSubmitting.set(true);
    this.authService.forgotPassword(this.requestForm.value).subscribe({
      next: (res) => {
        this.isSubmitting.set(false);
        this.message.set(res.message || 'Si cet email correspond à un compte, un code de réinitialisation a été généré.');
        this.step.set(2);
      },
      error: () => {
        this.isSubmitting.set(false);
        this.message.set('Si cet email correspond à un compte, un code de réinitialisation a été généré.');
        this.step.set(2);
      }
    });
  }

  onResetPassword(): void {
    if (this.resetForm.invalid) return;

    this.isSubmitting.set(true);
    this.authService.resetPassword(this.resetForm.value).subscribe({
      next: (res) => {
        this.isSubmitting.set(false);
        this.notification.success(res.message || 'Votre mot de passe a été modifié avec succès.');
      },
      error: (err) => {
        this.isSubmitting.set(false);
        this.notification.error(err.error?.message || 'Jeton invalide ou expiré.');
      }
    });
  }
}
