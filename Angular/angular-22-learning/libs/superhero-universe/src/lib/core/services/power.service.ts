import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { SUPERHERO_API_CONFIG } from '../api-config';
import { PowerListItem, PowerRequest } from '../../shared/models/power-team.models';

@Injectable({ providedIn: 'root' })
export class PowerService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(SUPERHERO_API_CONFIG).baseUrl;

  getAll(): Observable<PowerListItem[]> {
    return this.http.get<PowerListItem[]>(`${this.baseUrl}/api/powers`);
  }

  create(request: PowerRequest): Observable<PowerListItem> {
    return this.http.post<PowerListItem>(`${this.baseUrl}/api/powers`, request);
  }

  update(id: number, request: PowerRequest): Observable<PowerListItem> {
    return this.http.put<PowerListItem>(`${this.baseUrl}/api/powers/${id}`, request);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/api/powers/${id}`);
  }

  /** Replaces the hero's entire power set; returns the resulting power names. */
  assignToSuperhero(superheroId: number, powerIds: number[]): Observable<string[]> {
    return this.http.put<string[]>(`${this.baseUrl}/api/superheroes/${superheroId}/powers`, {
      powerIds,
    });
  }
}
