export interface DashboardTotals {
  superheroes: number;
  marvelHeroes: number;
  dcHeroes: number;
  otherUniverseHeroes: number;
  powers: number;
  teams: number;
  battles: number;
  missions: number;
  missionsCompleted: number;
  missionsSucceeded: number;
}

export interface ChartSlice {
  label: string;
  value: number;
}

export interface DashboardHero {
  id: number;
  name: string;
  universe: string;
  imageUrl: string | null;
  powerLevel: number;
  battlesWon: number;
  totalBattles: number;
}

export interface DashboardTeam {
  id: number;
  name: string;
  universe: string;
  memberCount: number;
  totalPowerLevel: number;
}

export interface DashboardBattle {
  id: number;
  hero1Name: string;
  hero1ImageUrl: string | null;
  hero1Score: number;
  hero2Name: string;
  hero2ImageUrl: string | null;
  hero2Score: number;
  winnerName: string | null;
  battleDate: string;
}

export interface Dashboard {
  totals: DashboardTotals;
  heroesByUniverse: ChartSlice[];
  heroesByAlignment: ChartSlice[];
  missionsByStatus: ChartSlice[];
  topSuperheroes: DashboardHero[];
  mostVictorious: DashboardHero[];
  largestTeams: DashboardTeam[];
  recentBattles: DashboardBattle[];
}

export interface RankingEntry {
  rank: number;
  id: number;
  name: string;
  realName: string | null;
  universe: string;
  alignment: string;
  imageUrl: string | null;
  value: number;
  battlesWon: number;
  totalBattles: number;
}

export interface Rankings {
  sortedBy: string;
  overall: RankingEntry[];
  marvel: RankingEntry[];
  dc: RankingEntry[];
}

/** Attributes a ranking can be sorted by, matching the API's allow-list. */
export const RANKING_FIELDS = [
  { key: 'powerLevel', label: 'Power Level' },
  { key: 'strength', label: 'Strength' },
  { key: 'speed', label: 'Speed' },
  { key: 'intelligence', label: 'Intelligence' },
  { key: 'combat', label: 'Combat' },
  { key: 'durability', label: 'Durability' },
  { key: 'wins', label: 'Battles Won' },
] as const;
