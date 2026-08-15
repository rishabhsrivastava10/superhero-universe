export interface MissionListItem {
  id: number;
  title: string;
  description: string | null;
  location: string | null;
  difficulty: string;
  requiredHeroCount: number;
  status: string;
  assignedHeroCount: number;
  createdAt: string;
}

export interface MissionHeroSummary {
  superheroId: number;
  name: string;
  alignment: string;
  powerLevel: number;
  imageUrl: string | null;
}

export interface MissionDetail {
  id: number;
  title: string;
  description: string | null;
  location: string | null;
  difficulty: string;
  requiredHeroCount: number;
  status: string;
  createdAt: string;
  assignedHeroes: MissionHeroSummary[];
}

export interface MissionRequest {
  title: string;
  description: string | null;
  location: string | null;
  difficulty: string;
  requiredHeroCount: number;
}

export interface MissionFactor {
  label: string;
  detail: string;
  contributionPercent: number;
}

export interface MissionResult {
  missionId: number;
  title: string;
  difficulty: string;
  status: string;
  succeeded: boolean;
  successChancePercent: number;
  rollPercent: number;
  summary: string;
  squad: MissionHeroSummary[];
  factors: MissionFactor[];
}

export const DIFFICULTIES = ['Easy', 'Medium', 'Hard'] as const;
export const MISSION_STATUSES = ['Pending', 'InProgress', 'Success', 'Failed'] as const;
