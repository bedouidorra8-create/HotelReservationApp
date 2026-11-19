import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, catchError, of } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuditLogDto } from '../models';
import { DEMO_AUDIT_LOGS } from '../mock-data/mock-data';

@Injectable({
  providedIn: 'root'
})
export class AuditService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/audit-logs`;

  getAuditLogs(dateDebut?: string, dateFin?: string): Observable<AuditLogDto[]> {
    let params = new HttpParams();
    if (dateDebut) params = params.set('dateDebut', dateDebut);
    if (dateFin) params = params.set('dateFin', dateFin);

    return this.http.get<AuditLogDto[]>(this.apiUrl, { params }).pipe(
      catchError(() => of(DEMO_AUDIT_LOGS))
    );
  }
}
