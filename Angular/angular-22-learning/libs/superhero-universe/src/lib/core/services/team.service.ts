import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { SUPERHERO_API_CONFIG } from '../api-config';
import { TeamDetail, TeamListItem, TeamRequest } from '../../shared/models/power-team.models';

@Injectable({ providedIn: 'root' })
export class TeamService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${inject(SUPERHERO_API_CONFIG).baseUrl}/api/teams`;

  getAll(universe?: string): Observable<TeamListItem[]> {
    const params = universe ? new HttpParams().set('universe', universe) : undefined;
    return this.http.get<TeamListItem[]>(this.baseUrl, { params });
  }

  getById(id: number): Observable<TeamDetail> {
    return this.http.get<TeamDetail>(`${this.baseUrl}/${id}`);
  }

  create(request: TeamRequest): Observable<TeamDetail> {
    return this.http.post<TeamDetail>(this.baseUrl, request);
  }

  update(id: number, request: TeamRequest): Observable<TeamDetail> {
    return this.http.put<TeamDetail>(`${this.baseUrl}/${id}`, request);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }

  addMember(teamId: number, superheroId: number): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/${teamId}/members`, { superheroId });
  }

  removeMember(teamId: number, superheroId: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${teamId}/members/${superheroId}`);
  }
}
