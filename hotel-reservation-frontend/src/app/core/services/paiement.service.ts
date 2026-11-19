import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, catchError, of } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Paiement, EnregistrerPaiementRequest, FactureDto } from '../models';
import { DEMO_PAIEMENTS } from '../mock-data/mock-data';

@Injectable({
  providedIn: 'root'
})
export class PaiementService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/paiements`;

  createPaiement(data: EnregistrerPaiementRequest): Observable<Paiement> {
    return this.http.post<Paiement>(this.apiUrl, data);
  }

  getMyPaiements(): Observable<Paiement[]> {
    return this.http.get<Paiement[]>(`${this.apiUrl}/me`).pipe(
      catchError(() => of(DEMO_PAIEMENTS))
    );
  }

  getByReservation(reservationId: string): Observable<Paiement[]> {
    return this.http.get<Paiement[]>(`${this.apiUrl}/reservation/${reservationId}`).pipe(
      catchError(() => of(DEMO_PAIEMENTS.filter(p => p.reservationId === reservationId)))
    );
  }

  cancelPaiement(id: string): Observable<Paiement> {
    return this.http.post<Paiement>(`${this.apiUrl}/${id}/annuler`, {});
  }

  getFacture(reservationId: string): Observable<FactureDto> {
    return this.http.get<FactureDto>(`${this.apiUrl}/${reservationId}/facture`).pipe(
      catchError(() => of({
        reservationId,
        dateEmission: new Date().toISOString(),
        total: 1280.00,
        devise: 'EUR',
        paiements: DEMO_PAIEMENTS,
        client: { id: 'u111', nom: 'Alexandre', prenom: 'Dumas', email: 'alexandre.dumas@example.com' },
        hotel: { id: 'h111', nom: 'Le Grand Palais Royal', adresse: '15 Avenue des Champs-Élysées, Paris' },
        chambre: { id: 'c111', numero: '101' }
      }))
    );
  }

  /**
   * Contrat BG-01 : Téléchargement PDF de facture.
   * Si l'endpoint binaire n'est pas encore présent, l'interface déclenche l'impression/sauvegarde locale.
   */
  telechargerFacturePdf(reservationId: string): Observable<Blob | null> {
    return this.http.get(`${this.apiUrl}/${reservationId}/facture/pdf`, { responseType: 'blob' }).pipe(
      catchError(() => of(null))
    );
  }

  /**
   * Contrat BG-03 : Consultation globale des transactions pour le personnel.
   */
  getAllPaiements(): Observable<Paiement[]> {
    return this.http.get<Paiement[]>(this.apiUrl).pipe(
      catchError(() => of(DEMO_PAIEMENTS))
    );
  }
}
