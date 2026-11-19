import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap, catchError, of } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Favori, AjouterFavoriRequest } from '../models';

@Injectable({
  providedIn: 'root'
})
export class FavorisService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/favoris`;

  favorites = signal<Favori[]>([]);

  getFavorites(clientId: string): Observable<Favori[]> {
    return this.http.get<Favori[]>(`${this.apiUrl}/client/${clientId}`).pipe(
      tap(favs => this.favorites.set(favs || [])),
      catchError(() => {
        // Fallback local
        return of(this.favorites());
      })
    );
  }

  addFavorite(hotelId: string): Observable<Favori> {
    const req: AjouterFavoriRequest = { hotelId };
    return this.http.post<Favori>(this.apiUrl, req).pipe(
      tap(newFav => {
        this.favorites.update(current => [...current, newFav]);
      }),
      catchError(() => {
        const simulated: Favori = {
          id: Math.random().toString(36).substring(2, 9),
          clientId: 'current',
          hotelId,
          dateAjout: new Date().toISOString()
        };
        this.favorites.update(current => [...current, simulated]);
        return of(simulated);
      })
    );
  }

  removeFavorite(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`).pipe(
      tap(() => {
        this.favorites.update(current => current.filter(f => f.id !== id && f.hotelId !== id));
      }),
      catchError(() => {
        this.favorites.update(current => current.filter(f => f.id !== id && f.hotelId !== id));
        return of(void 0);
      })
    );
  }

  isFavorite(hotelId: string): boolean {
    return this.favorites().some(f => f.hotelId === hotelId);
  }
}
