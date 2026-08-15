import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { SUPERHERO_API_CONFIG } from '../api-config';
import {
  PagedResponse,
  SuperheroDetail,
  SuperheroListItem,
  SuperheroQuery,
  SuperheroRequest,
} from '../../shared/models/superhero.models';

@Injectable({ providedIn: 'root' })
export class SuperheroService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${inject(SUPERHERO_API_CONFIG).baseUrl}/api/superheroes`;

  getPaged(query: SuperheroQuery): Observable<PagedResponse<SuperheroListItem>> {
    let params = new HttpParams();

    // Only send parameters that are actually set - an empty search= would still hit the DB filter.
    Object.entries(query).forEach(([key, value]) => {
      if (value !== undefined && value !== null && value !== '') {
        params = params.set(key, String(value));
      }
    });

    return this.http.get<PagedResponse<SuperheroListItem>>(this.baseUrl, { params });
  }

  getById(id: number): Observable<SuperheroDetail> {
    return this.http.get<SuperheroDetail>(`${this.baseUrl}/${id}`);
  }

  create(request: SuperheroRequest): Observable<SuperheroDetail> {
    return this.http.post<SuperheroDetail>(this.baseUrl, request);
  }

  update(id: number, request: SuperheroRequest): Observable<SuperheroDetail> {
    return this.http.put<SuperheroDetail>(`${this.baseUrl}/${id}`, request);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
