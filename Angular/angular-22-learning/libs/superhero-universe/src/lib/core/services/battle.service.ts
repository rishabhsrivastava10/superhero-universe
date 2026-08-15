import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { SUPERHERO_API_CONFIG } from '../api-config';
import { BattleListItem, BattleResult } from '../../shared/models/battle.models';
import { PagedResponse } from '../../shared/models/superhero.models';

@Injectable({ providedIn: 'root' })
export class BattleService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${inject(SUPERHERO_API_CONFIG).baseUrl}/api/battles`;

  /**
   * Runs a battle. The outcome is calculated entirely on the server - the client only
   * submits two ids and renders whatever comes back.
   */
  simulate(hero1Id: number, hero2Id: number): Observable<BattleResult> {
    return this.http.post<BattleResult>(`${this.baseUrl}/simulate`, { hero1Id, hero2Id });
  }

  getHistory(page = 1, pageSize = 10, superheroId?: number): Observable<PagedResponse<BattleListItem>> {
    let params = new HttpParams().set('page', page).set('pageSize', pageSize);
    if (superheroId) {
      params = params.set('superheroId', superheroId);
    }
    return this.http.get<PagedResponse<BattleListItem>>(this.baseUrl, { params });
  }

  getById(id: number): Observable<BattleResult> {
    return this.http.get<BattleResult>(`${this.baseUrl}/${id}`);
  }
}
