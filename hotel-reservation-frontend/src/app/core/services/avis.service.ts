import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, catchError, of } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Avis, AjouterAvisRequest, ModifierAvisRequest } from '../models';
import { DEMO_AVIS } from '../mock-data/mock-data';

@Injectable({
  providedIn: 'root'
})
export class AvisService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/avis`;

  getHotelReviews(hotelId: string): Observable<Avis[]> {
    return this.http.get<Avis[]>(`${this.apiUrl}/hotel/${hotelId}`).pipe(
      catchError(() => of(DEMO_AVIS.filter(a => a.hotelId === hotelId)))
    );
  }

  createReview(data: AjouterAvisRequest): Observable<Avis> {
    return this.http.post<Avis>(this.apiUrl, data);
  }

  updateReview(id: string, data: ModifierAvisRequest): Observable<Avis> {
    return this.http.put<Avis>(`${this.apiUrl}/${id}`, data);
  }

  deleteReview(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
