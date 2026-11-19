import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, catchError, of } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Hotel, HotelResultatRechercheDto, CreerHotelRequest, ModifierHotelRequest } from '../models';
import { DEMO_HOTELS } from '../mock-data/mock-data';

@Injectable({
  providedIn: 'root'
})
export class HotelService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/hotels`;

  getHotels(): Observable<Hotel[]> {
    return this.http.get<Hotel[]>(this.apiUrl).pipe(
      catchError(() => of(DEMO_HOTELS))
    );
  }

  getHotelById(id: string): Observable<Hotel> {
    return this.http.get<Hotel>(`${this.apiUrl}/${id}`).pipe(
      catchError(() => {
        const found = DEMO_HOTELS.find(h => h.id === id) || DEMO_HOTELS[0];
        return of(found);
      })
    );
  }

  searchHotels(filters: {
    ville?: string;
    prixMin?: number;
    prixMax?: number;
    etoilesMin?: number;
    dateDebut?: string;
    dateFin?: string;
    disponibleUniquement?: boolean;
    tri?: string;
  }): Observable<HotelResultatRechercheDto[]> {
    let params = new HttpParams();
    if (filters.ville) params = params.set('ville', filters.ville);
    if (filters.prixMin) params = params.set('prixMin', filters.prixMin.toString());
    if (filters.prixMax) params = params.set('prixMax', filters.prixMax.toString());
    if (filters.etoilesMin) params = params.set('etoilesMin', filters.etoilesMin.toString());
    if (filters.dateDebut) params = params.set('dateDebut', filters.dateDebut);
    if (filters.dateFin) params = params.set('dateFin', filters.dateFin);
    if (filters.disponibleUniquement !== undefined) {
      params = params.set('disponibleUniquement', filters.disponibleUniquement.toString());
    }
    if (filters.tri) params = params.set('tri', filters.tri);

    return this.http.get<HotelResultatRechercheDto[]>(`${this.apiUrl}/recherche`, { params }).pipe(
      catchError(() => {
        // Fallback de démonstration si le backend n'est pas encore démarré
        const results: HotelResultatRechercheDto[] = DEMO_HOTELS.map(h => ({
          id: h.id,
          nom: h.nom,
          ville: h.ville,
          adresse: h.adresse,
          nbEtoiles: h.nbEtoiles,
          description: h.description,
          images: h.images,
          prixLePlusBas: h.prixMin || 250,
          chambresDisponibles: h.nbChambresDisponibles
        }));
        return of(results);
      })
    );
  }

  createHotel(data: CreerHotelRequest): Observable<Hotel> {
    return this.http.post<Hotel>(this.apiUrl, data);
  }

  updateHotel(id: string, data: ModifierHotelRequest): Observable<Hotel> {
    return this.http.put<Hotel>(`${this.apiUrl}/${id}`, data);
  }

  toggleActive(id: string, actif: boolean): Observable<Hotel> {
    return this.http.patch<Hotel>(`${this.apiUrl}/${id}/actif?actif=${actif}`, {});
  }

  deleteHotel(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
