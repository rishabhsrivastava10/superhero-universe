import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { SUPERHERO_API_CONFIG } from '../api-config';
import { Dashboard, Rankings } from '../../shared/models/dashboard.models';

@Injectable({ providedIn: 'root' })
export class DashboardService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(SUPERHERO_API_CONFIG).baseUrl;

  getDashboard(): Observable<Dashboard> {
    return this.http.get<Dashboard>(`${this.baseUrl}/api/dashboard`);
  }

  getRankings(sortBy: string, take = 10): Observable<Rankings> {
    const params = new HttpParams().set('sortBy', sortBy).set('take', take);
    return this.http.get<Rankings>(`${this.baseUrl}/api/rankings`, { params });
  }
}
