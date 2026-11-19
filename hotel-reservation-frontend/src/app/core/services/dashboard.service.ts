import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, catchError, of } from 'rxjs';
import { environment } from '../../../environments/environment';
import { DashboardDto, StatistiquesDto } from '../models';
import { DEMO_DASHBOARD, DEMO_STATISTIQUES } from '../mock-data/mock-data';

@Injectable({
  providedIn: 'root'
})
export class DashboardService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/dashboard`;

  getDashboard(): Observable<DashboardDto> {
    return this.http.get<DashboardDto>(this.apiUrl).pipe(
      catchError(() => of(DEMO_DASHBOARD))
    );
  }

  getStatistiques(): Observable<StatistiquesDto> {
    return this.http.get<StatistiquesDto>(`${this.apiUrl}/statistiques`).pipe(
      catchError(() => of(DEMO_STATISTIQUES))
    );
  }
}
