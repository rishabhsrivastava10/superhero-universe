/** Mirrors ModelPagedResponse<T> from the API. */
export interface PagedResponse<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export interface SuperheroListItem {
  id: number;
  name: string;
  realName: string | null;
  universe: string;
  alignment: string;
  powerLevel: number;
  imageUrl: string | null;
  powers: string[];
}

export interface SuperheroDetail {
  id: number;
  name: string;
  realName: string | null;
  universe: string;
  alignment: string;
  description: string | null;
  powerLevel: number;
  intelligence: number;
  strength: number;
  speed: number;
  durability: number;
  combat: number;
  imageUrl: string | null;
  createdAt: string;
  updatedAt: string | null;
  powers: string[];
  teams: string[];
  totalBattles: number;
  battlesWon: number;
}

export interface SuperheroRequest {
  name: string;
  realName: string | null;
  universe: string;
  alignment: string;
  description: string | null;
  powerLevel: number;
  intelligence: number;
  strength: number;
  speed: number;
  durability: number;
  combat: number;
  imageUrl: string | null;
}

/** Query-string options for the superhero list endpoint. All optional. */
export interface SuperheroQuery {
  page?: number;
  pageSize?: number;
  search?: string;
  universe?: string;
  alignment?: string;
  minPowerLevel?: number;
  maxPowerLevel?: number;
  sortBy?: string;
  sortDir?: 'asc' | 'desc';
}

export const ALIGNMENTS = ['Hero', 'Villain', 'Anti-Hero'] as const;

/** The stat bars shown on hero cards and the detail view. */
export const STAT_KEYS = [
  'intelligence',
  'strength',
  'speed',
  'durability',
  'combat',
] as const;

export type StatKey = (typeof STAT_KEYS)[number];
