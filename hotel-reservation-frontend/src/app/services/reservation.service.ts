import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { Reservation, CreerReservationRequest } from '../models/reservation.model';

@Injectable({
  providedIn: 'root'
})
export class ReservationService {
  private apiUrl = `${environment.apiUrl}/reservations`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<Reservation[]> {
    return this.http.get<Reservation[]>(this.apiUrl);
  }

  getByClient(clientId: string): Observable<Reservation[]> {
    return this.http.get<Reservation[]>(`${this.apiUrl}/client/${clientId}`);
  }

  create(request: CreerReservationRequest): Observable<Reservation> {
    return this.http.post<Reservation>(this.apiUrl, request);
  }

  confirmer(id: string): Observable<Reservation> {
    return this.http.post<Reservation>(`${this.apiUrl}/${id}/confirmer`, {});
  }

  annuler(id: string): Observable<Reservation> {
    return this.http.post<Reservation>(`${this.apiUrl}/${id}/annuler`, {});
  }
}
