export interface AttributeComparison {
  attribute: string;
  hero1Value: number;
  hero2Value: number;
  /** 1 = hero 1 wins this attribute, 2 = hero 2, 0 = tie. */
  wonBy: number;
  weightPercent: number;
}

export interface BattleCombatant {
  id: number;
  name: string;
  universe: string;
  alignment: string;
  imageUrl: string | null;
  powerLevel: number;
  score: number;
}

export interface BattleResult {
  battleId: number;
  hero1: BattleCombatant;
  hero2: BattleCombatant;
  winnerId: number | null;
  winnerName: string | null;
  loserId: number | null;
  loserName: string | null;
  isDraw: boolean;
  summary: string;
  breakdown: AttributeComparison[];
  battleDate: string;
}

export interface BattleListItem {
  id: number;
  hero1Id: number;
  hero1Name: string;
  hero1ImageUrl: string | null;
  hero1Score: number;
  hero2Id: number;
  hero2Name: string;
  hero2ImageUrl: string | null;
  hero2Score: number;
  winnerId: number | null;
  winnerName: string | null;
  battleDate: string;
}
