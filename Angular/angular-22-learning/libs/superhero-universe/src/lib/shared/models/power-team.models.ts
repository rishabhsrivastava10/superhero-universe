export interface PowerListItem {
  id: number;
  name: string;
  description: string | null;
  superheroCount: number;
}

export interface PowerRequest {
  name: string;
  description: string | null;
}

export interface TeamListItem {
  id: number;
  name: string;
  universe: string;
  description: string | null;
  foundedDate: string | null;
  memberCount: number;
}

export interface TeamMember {
  superheroId: number;
  name: string;
  realName: string | null;
  alignment: string;
  powerLevel: number;
  imageUrl: string | null;
  joinedDate: string;
}

export interface TeamStatistics {
  memberCount: number;
  totalPowerLevel: number;
  averagePowerLevel: number;
  strongestMemberPowerLevel: number;
  strongestMemberName: string | null;
  heroCount: number;
  villainCount: number;
  antiHeroCount: number;
}

export interface TeamDetail {
  id: number;
  name: string;
  universe: string;
  description: string | null;
  foundedDate: string | null;
  members: TeamMember[];
  statistics: TeamStatistics;
}

export interface TeamRequest {
  name: string;
  universe: string;
  description: string | null;
  foundedDate: string | null;
}
