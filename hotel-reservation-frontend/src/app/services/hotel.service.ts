import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { Hotel, CreerHotelRequest, DisponibilitesHotelResponse } from '../models/hotel.model';

@Injectable({
  providedIn: 'root'
})
export class HotelService {
  private apiUrl = `${environment.apiUrl}/hotels`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<Hotel[]> {
    return this.http.get<Hotel[]>(this.apiUrl);
  }

  getById(id: string): Observable<Hotel> {
    return this.http.get<Hotel>(`${this.apiUrl}/${id}`);
  }

  create(request: CreerHotelRequest): Observable<Hotel> {
    return this.http.post<Hotel>(this.apiUrl, request);
  }

  getDisponibilites(hotelId: string, dateDebut: string, dateFin: string): Observable<DisponibilitesHotelResponse> {
    return this.http.get<DisponibilitesHotelResponse>(`${this.apiUrl}/${hotelId}/disponibilites`, {
      params: { dateDebut, dateFin }
    });
  }
}
