import { Injectable, inject, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, tap, catchError, throwError, of } from 'rxjs';
import { environment } from '../../../environments/environment';
import { TokenStorage } from './token.storage';
import {
  ConnexionRequest,
  ConnexionResponse,
  InscrireUtilisateurRequest,
  TokenResponse,
  ProfilUtilisateurDto,
  ModifierProfilRequest,
  ChangerMotDePasseRequest,
  DemanderResetPasswordRequest,
  ReinitialiserMotDePasseRequest
} from '../models';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private http = inject(HttpClient);
  private tokenStorage = inject(TokenStorage);
  private router = inject(Router);

  private apiUrl = `${environment.apiUrl}/auth`;

  currentUser = signal<Partial<ConnexionResponse> | null>(this.tokenStorage.getUser());
  isAuthenticated = computed(() => !!this.currentUser() && !!this.tokenStorage.getAccessToken());
  userRole = computed(() => this.currentUser()?.role || null);

  login(credentials: ConnexionRequest): Observable<ConnexionResponse> {
    return this.http.post<ConnexionResponse>(`${this.apiUrl}/connexion`, credentials).pipe(
      tap(res => {
        this.tokenStorage.saveTokens(res.jwt, res.refreshToken);
        this.tokenStorage.saveUser({
          utilisateurId: res.utilisateurId,
          nomComplet: res.nomComplet,
          email: res.email,
          role: res.role
        });
        this.currentUser.set({
          utilisateurId: res.utilisateurId,
          nomComplet: res.nomComplet,
          email: res.email,
          role: res.role
        });
      })
    );
  }

  register(data: InscrireUtilisateurRequest): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.apiUrl}/inscription`, data);
  }

  refreshToken(): Observable<TokenResponse> {
    const refreshToken = this.tokenStorage.getRefreshToken();
    if (!refreshToken) {
      return throwError(() => new Error('Aucun refresh token disponible'));
    }
    return this.http.post<TokenResponse>(`${this.apiUrl}/rafraichir-token`, { refreshToken }).pipe(
      tap(res => {
        this.tokenStorage.saveTokens(res.jwt, res.refreshToken);
      })
    );
  }

  logout(): void {
    const refreshToken = this.tokenStorage.getRefreshToken();
    if (refreshToken) {
      this.http.post(`${this.apiUrl}/logout`, { refreshToken }).subscribe({
        next: () => {},
        error: () => {}
      });
    }
    this.tokenStorage.clear();
    this.currentUser.set(null);
    this.router.navigate(['/auth/login']);
  }

  getProfile(): Observable<ProfilUtilisateurDto> {
    return this.http.get<ProfilUtilisateurDto>(`${this.apiUrl}/profil`);
  }

  updateProfile(data: ModifierProfilRequest): Observable<ProfilUtilisateurDto> {
    return this.http.put<ProfilUtilisateurDto>(`${this.apiUrl}/profil`, data).pipe(
      tap(profile => {
        const current = this.currentUser();
        if (current) {
          const updated = {
            ...current,
            nomComplet: `${profile.prenom} ${profile.nom}`
          };
          this.tokenStorage.saveUser(updated);
          this.currentUser.set(updated);
        }
      })
    );
  }

  changePassword(data: ChangerMotDePasseRequest): Observable<{ message: string }> {
    return this.http.put<{ message: string }>(`${this.apiUrl}/changer-mot-de-passe`, data);
  }

  forgotPassword(data: DemanderResetPasswordRequest): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.apiUrl}/mot-de-passe-oublie`, data);
  }

  resetPassword(data: ReinitialiserMotDePasseRequest): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.apiUrl}/reinitialiser-mot-de-passe`, data);
  }

  redirectAfterLogin(): void {
    const role = this.userRole();
    if (role === 'Administrateur' || role === 'Agent') {
      this.router.navigate(['/admin/dashboard']);
    } else {
      this.router.navigate(['/client/dashboard']);
    }
  }
}
