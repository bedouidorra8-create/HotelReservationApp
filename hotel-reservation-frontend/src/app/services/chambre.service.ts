import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { Chambre, CreerChambreRequest } from '../models/hotel.model';

@Injectable({
  providedIn: 'root'
})
export class ChambreService {
  private apiUrl = `${environment.apiUrl}/hotels`;

  constructor(private http: HttpClient) {}

  addChambre(hotelId: string, request: CreerChambreRequest): Observable<Chambre> {
    return this.http.post<Chambre>(`${this.apiUrl}/${hotelId}/chambres`, request);
  }
}
