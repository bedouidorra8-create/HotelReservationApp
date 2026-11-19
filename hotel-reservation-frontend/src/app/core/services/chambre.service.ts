import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, catchError, of } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  Chambre,
  TypeChambre,
  StatutChambre,
  CreerChambreRequest,
  ModifierChambreRequest,
  DisponibilitesHotelResponse
} from '../models';
import { DEMO_HOTELS } from '../mock-data/mock-data';

@Injectable({
  providedIn: 'root'
})
export class ChambreService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/chambres`;

  getChambresByHotel(hotelId: string): Observable<Chambre[]> {
    return this.http.get<Chambre[]>(`${environment.apiUrl}/hotels/${hotelId}/chambres`).pipe(
      catchError(() => {
        const hotel = DEMO_HOTELS.find(h => h.id === hotelId);
        return of(hotel?.chambres || []);
      })
    );
  }

  getChambreById(id: string): Observable<Chambre> {
    return this.http.get<Chambre>(`${this.apiUrl}/${id}`);
  }

  searchChambres(criteria: {
    type?: TypeChambre;
    prixMin?: number;
    prixMax?: number;
    statut?: StatutChambre;
  }): Observable<Chambre[]> {
    let params = new HttpParams();
    if (criteria.type) params = params.set('type', criteria.type.toString());
    if (criteria.prixMin) params = params.set('prixMin', criteria.prixMin.toString());
    if (criteria.prixMax) params = params.set('prixMax', criteria.prixMax.toString());
    if (criteria.statut) params = params.set('statut', criteria.statut.toString());

    return this.http.get<Chambre[]>(`${this.apiUrl}/recherche`, { params }).pipe(
      catchError(() => {
        const all = DEMO_HOTELS.flatMap(h => h.chambres || []);
        return of(all);
      })
    );
  }

  createChambre(hotelId: string, data: CreerChambreRequest): Observable<Chambre> {
    return this.http.post<Chambre>(`${environment.apiUrl}/hotels/${hotelId}/chambres`, data);
  }

  updateChambre(id: string, data: ModifierChambreRequest): Observable<Chambre> {
    return this.http.put<Chambre>(`${this.apiUrl}/${id}`, data);
  }

  deleteChambre(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  changeStatus(id: string, statut: StatutChambre): Observable<Chambre> {
    return this.http.patch<Chambre>(`${this.apiUrl}/${id}/statut?statut=${statut}`, {});
  }

  getDisponibilites(hotelId: string, dateDebut: string, dateFin: string): Observable<DisponibilitesHotelResponse> {
    const params = new HttpParams()
      .set('dateDebut', dateDebut)
      .set('dateFin', dateFin);
    return this.http.get<DisponibilitesHotelResponse>(
      `${environment.apiUrl}/hotels/${hotelId}/chambres/disponibilites`,
      { params }
    );
  }
}
