import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { SUPERHERO_API_CONFIG } from '../api-config';
import {
  MissionDetail,
  MissionListItem,
  MissionRequest,
  MissionResult,
} from '../../shared/models/mission.models';

@Injectable({ providedIn: 'root' })
export class MissionService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${inject(SUPERHERO_API_CONFIG).baseUrl}/api/missions`;

  getAll(status?: string): Observable<MissionListItem[]> {
    const params = status ? new HttpParams().set('status', status) : undefined;
    return this.http.get<MissionListItem[]>(this.baseUrl, { params });
  }

  getById(id: number): Observable<MissionDetail> {
    return this.http.get<MissionDetail>(`${this.baseUrl}/${id}`);
  }

  /** Runs the attempt. Success is decided entirely by the server. */
  start(id: number, superheroIds: number[]): Observable<MissionResult> {
    return this.http.post<MissionResult>(`${this.baseUrl}/${id}/start`, { superheroIds });
  }

  reset(id: number): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/${id}/reset`, {});
  }

  create(request: MissionRequest): Observable<MissionDetail> {
    return this.http.post<MissionDetail>(this.baseUrl, request);
  }

  update(id: number, request: MissionRequest): Observable<MissionDetail> {
    return this.http.put<MissionDetail>(`${this.baseUrl}/${id}`, request);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
