import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, catchError, of } from 'rxjs';
import { environment } from '../../../environments/environment';
import { UtilisateurDto, AjouterUtilisateurRequest, RoleDto, Avis } from '../models';
import { DEMO_AVIS } from '../mock-data/mock-data';

@Injectable({
  providedIn: 'root'
})
export class AdministrationService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/administration`;

  getUsers(): Observable<UtilisateurDto[]> {
    return this.http.get<UtilisateurDto[]>(`${this.apiUrl}/utilisateurs`).pipe(
      catchError(() => of([]))
    );
  }

  searchUsers(terme?: string, role?: string): Observable<UtilisateurDto[]> {
    let params = new HttpParams();
    if (terme) params = params.set('terme', terme);
    if (role) params = params.set('role', role);

    return this.http.get<UtilisateurDto[]>(`${this.apiUrl}/utilisateurs/recherche`, { params });
  }

  addUser(data: AjouterUtilisateurRequest): Observable<UtilisateurDto> {
    return this.http.post<UtilisateurDto>(`${this.apiUrl}/utilisateurs`, data);
  }

  updateAgent(id: string, data: Partial<AjouterUtilisateurRequest>): Observable<UtilisateurDto> {
    return this.http.put<UtilisateurDto>(`${this.apiUrl}/agents/${id}`, data);
  }

  deleteAgent(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/agents/${id}`);
  }

  updateAdmin(id: string, data: Partial<AjouterUtilisateurRequest>): Observable<UtilisateurDto> {
    return this.http.put<UtilisateurDto>(`${this.apiUrl}/administrateurs/${id}`, data);
  }

  deleteAdmin(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/administrateurs/${id}`);
  }

  updateClient(id: string, data: { nom: string; prenom: string; telephone: string; adresse: string; photo?: string }): Observable<any> {
    return this.http.put(`${environment.apiUrl}/clients/${id}`, data);
  }

  deleteClient(id: string): Observable<void> {
    return this.http.delete<void>(`${environment.apiUrl}/clients/${id}`);
  }

  toggleUserStatus(id: string, actif: boolean): Observable<UtilisateurDto> {
    return this.http.patch<UtilisateurDto>(`${this.apiUrl}/utilisateurs/${id}/actif`, { actif });
  }

  getRoles(): Observable<RoleDto[]> {
    return this.http.get<RoleDto[]>(`${this.apiUrl}/roles`).pipe(
      catchError(() => of([
        { id: '1', nom: 'Administrateur', permissions: ['ALL_PERMISSIONS'] },
        { id: '2', nom: 'Agent', permissions: ['MANAGE_RESERVATIONS', 'MANAGE_ROOMS', 'VIEW_DASHBOARD'] },
        { id: '3', nom: 'Client', permissions: ['BOOK_ROOM', 'VIEW_OWN_PROFILE', 'SUBMIT_REVIEW'] }
      ]))
    );
  }

  manageRole(data: { roleId?: string; nom: string; permissions: string[] }): Observable<RoleDto> {
    return this.http.post<RoleDto>(`${this.apiUrl}/roles`, data);
  }

  /**
   * Contrat BG-02 : Récupération des avis en attente de modération.
   */
  getAvisEnAttente(): Observable<Avis[]> {
    return this.http.get<Avis[]>(`${this.apiUrl}/avis`).pipe(
      catchError(() => of(DEMO_AVIS))
    );
  }

  validateReview(id: string): Observable<Avis> {
    return this.http.post<Avis>(`${this.apiUrl}/avis/${id}/valider`, {});
  }

  rejectReview(id: string): Observable<Avis> {
    return this.http.post<Avis>(`${this.apiUrl}/avis/${id}/rejeter`, {});
  }
}
