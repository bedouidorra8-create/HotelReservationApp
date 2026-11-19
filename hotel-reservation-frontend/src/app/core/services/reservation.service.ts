import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, catchError, of } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  Reservation,
  StatutReservation,
  CreerReservationRequest,
  ModifierReservationRequest,
  CalendrierReservationsResponse
} from '../models';
import { DEMO_DASHBOARD } from '../mock-data/mock-data';

@Injectable({
  providedIn: 'root'
})
export class ReservationService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/reservations`;

  getMyReservations(clientId: string, statut?: StatutReservation): Observable<Reservation[]> {
    let params = new HttpParams();
    if (statut !== undefined && statut !== null) {
      params = params.set('statut', statut.toString());
    }
    return this.http.get<Reservation[]>(`${this.apiUrl}/client/${clientId}`, { params }).pipe(
      catchError(() => of(DEMO_DASHBOARD.dernieresReservations))
    );
  }

  getReservationById(id: string): Observable<Reservation> {
    return this.http.get<Reservation>(`${this.apiUrl}/${id}`);
  }

  searchReservations(filters: {
    clientId?: string;
    hotelId?: string;
    statut?: StatutReservation;
    dateDebut?: string;
    dateFin?: string;
  }): Observable<Reservation[]> {
    let params = new HttpParams();
    if (filters.clientId) params = params.set('clientId', filters.clientId);
    if (filters.hotelId) params = params.set('hotelId', filters.hotelId);
    if (filters.statut !== undefined && filters.statut !== null) {
      params = params.set('statut', filters.statut.toString());
    }
    if (filters.dateDebut) params = params.set('dateDebut', filters.dateDebut);
    if (filters.dateFin) params = params.set('dateFin', filters.dateFin);

    return this.http.get<Reservation[]>(`${this.apiUrl}/recherche`, { params }).pipe(
      catchError(() => of(DEMO_DASHBOARD.dernieresReservations))
    );
  }

  createReservation(data: CreerReservationRequest): Observable<Reservation> {
    return this.http.post<Reservation>(this.apiUrl, data);
  }

  confirmReservation(id: string): Observable<Reservation> {
    return this.http.post<Reservation>(`${this.apiUrl}/${id}/confirmer`, {});
  }

  updateReservation(id: string, data: ModifierReservationRequest): Observable<Reservation> {
    return this.http.put<Reservation>(`${this.apiUrl}/${id}`, data);
  }

  cancelReservation(id: string): Observable<Reservation> {
    return this.http.post<Reservation>(`${this.apiUrl}/${id}/annuler`, {});
  }

  changeStatus(id: string, statut: StatutReservation): Observable<Reservation> {
    return this.http.patch<Reservation>(`${this.apiUrl}/${id}/statut?statut=${statut}`, {});
  }

  getCalendrier(hotelId: string, dateDebut: string, dateFin: string): Observable<CalendrierReservationsResponse> {
    const params = new HttpParams()
      .set('hotelId', hotelId)
      .set('dateDebut', dateDebut)
      .set('dateFin', dateFin);
    return this.http.get<CalendrierReservationsResponse>(`${this.apiUrl}/calendrier`, { params }).pipe(
      catchError(() => of({
        hotelId,
        hotelNom: 'Hôtel Sélectionné',
        dateDebut,
        dateFin,
        reservations: []
      }))
    );
  }
}
