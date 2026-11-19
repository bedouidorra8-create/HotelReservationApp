import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { Avis, AjouterAvisRequest } from '../models/avis.model';

@Injectable({
  providedIn: 'root'
})
export class AvisService {
  private apiUrl = `${environment.apiUrl}/avis`;

  constructor(private http: HttpClient) {}

  getByHotel(hotelId: string): Observable<Avis[]> {
    return this.http.get<Avis[]>(`${this.apiUrl}/hotel/${hotelId}`);
  }

  create(request: AjouterAvisRequest): Observable<Avis> {
    return this.http.post<Avis>(this.apiUrl, request);
  }
}
