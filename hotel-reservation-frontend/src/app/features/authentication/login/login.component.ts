import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router, ActivatedRoute } from '@angular/router';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { AuthService } from '../../../core/auth/auth.service';
import { NotificationService } from '../../../core/services/notification.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, RouterModule, ReactiveFormsModule],
  template: `
    <div class="auth-page">
      <div class="auth-card card-luxury">
        <div class="auth-header">
          <span class="crest">✦</span>
          <h2 class="auth-title">Connexion Privilège</h2>
          <p class="auth-subtitle">Accédez à votre espace personnel ou de gestion</p>
        </div>

        @if (errorMessage()) {
          <div class="alert-error">
            <span>⚠</span>
            <span>{{ errorMessage() }}</span>
          </div>
        }

        <form [formGroup]="loginForm" (ngSubmit)="onSubmit()" class="auth-form">
          <div class="form-group">
            <label class="form-label" for="email">Adresse Email</label>
            <input
              id="email"
              type="email"
              formControlName="email"
              placeholder="votre.nom@exemple.com"
              class="form-control"
              [class.is-invalid]="loginForm.get('email')?.invalid && loginForm.get('email')?.touched"
            />
            @if (loginForm.get('email')?.invalid && loginForm.get('email')?.touched) {
              <div class="form-error">Veuillez renseigner un email valide.</div>
            }
          </div>

          <div class="form-group">
            <div class="password-label-wrap">
              <label class="form-label" for="password">Mot de passe</label>
              <a routerLink="/auth/forgot-password" class="forgot-link">Mot de passe oublié ?</a>
            </div>
            <input
              id="password"
              type="password"
              formControlName="motDePasse"
              placeholder="••••••••••••"
              class="form-control"
              [class.is-invalid]="loginForm.get('motDePasse')?.invalid && loginForm.get('motDePasse')?.touched"
            />
            @if (loginForm.get('motDePasse')?.invalid && loginForm.get('motDePasse')?.touched) {
              <div class="form-error">Le mot de passe est obligatoire.</div>
            }
          </div>

          <button type="submit" class="btn btn-gold btn-block" [disabled]="loginForm.invalid || isSubmitting()">
            @if (isSubmitting()) {
              <span>Connexion en cours...</span>
            } @else {
              <span>Se Connecter</span>
            }
          </button>
        </form>

        <div class="auth-footer">
          <p>Vous n'avez pas encore de compte ?</p>
          <a routerLink="/auth/register" class="link-register">Créer un compte Client Privilège</a>
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
      max-width: 450px;
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

    .password-label-wrap {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 0.4rem;
    }

    .forgot-link {
      font-size: 0.785rem;
      color: var(--secondary-green);
    }

    .forgot-link:hover {
      text-decoration: underline;
    }

    .btn-block {
      width: 100%;
      padding: 0.8rem;
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

    .link-register {
      display: inline-block;
      margin-top: 0.35rem;
      color: var(--forest-green);
      font-weight: 600;
    }

    .link-register:hover {
      text-decoration: underline;
    }
  `]
})
export class LoginComponent {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private notification = inject(NotificationService);
  private router = inject(Router);
  private route = inject(ActivatedRoute);

  isSubmitting = signal<boolean>(false);
  errorMessage = signal<string | null>(null);

  loginForm: FormGroup = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
    motDePasse: ['', [Validators.required]]
  });

  onSubmit(): void {
    if (this.loginForm.invalid) return;

    this.isSubmitting.set(true);
    this.errorMessage.set(null);

    this.authService.login(this.loginForm.value).subscribe({
      next: (res) => {
        this.isSubmitting.set(false);
        this.notification.success(`Bienvenue, ${res.nomComplet} !`);
        const returnUrl = this.route.snapshot.queryParams['returnUrl'];
        if (returnUrl) {
          this.router.navigateByUrl(returnUrl);
        } else {
          this.authService.redirectAfterLogin();
        }
      },
      error: (err) => {
        this.isSubmitting.set(false);
        this.errorMessage.set(err.error?.message || 'Identifiants invalides ou compte inactif.');
      }
    });
  }
}
