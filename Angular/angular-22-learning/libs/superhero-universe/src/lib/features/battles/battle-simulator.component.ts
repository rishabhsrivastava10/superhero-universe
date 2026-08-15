import { ChangeDetectionStrategy, Component, OnInit, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { BattleService } from '../../core/services/battle.service';
import { SuperheroService } from '../../core/services/superhero.service';
import { HeroAvatarComponent } from '../../shared/components/hero-avatar.component';
import { BattleListItem, BattleResult } from '../../shared/models/battle.models';
import { SuperheroListItem } from '../../shared/models/superhero.models';

type Side = 1 | 2;

@Component({
  selector: 'hero-battle-simulator',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterLink, HeroAvatarComponent],
  templateUrl: './battle-simulator.component.html',
  styleUrl: './battle-simulator.component.scss',
})
export class BattleSimulatorComponent implements OnInit {
  private readonly superheroes = inject(SuperheroService);
  private readonly battles = inject(BattleService);

  protected readonly roster = signal<SuperheroListItem[]>([]);
  protected readonly hero1 = signal<SuperheroListItem | null>(null);
  protected readonly hero2 = signal<SuperheroListItem | null>(null);

  protected readonly simulating = signal(false);
  protected readonly result = signal<BattleResult | null>(null);
  protected readonly history = signal<BattleListItem[]>([]);

  /** Which side the picker is currently choosing for, or null when closed. */
  protected readonly picking = signal<Side | null>(null);
  protected readonly pickerSearch = signal('');

  protected readonly canFight = computed(
    () => !!this.hero1() && !!this.hero2() && this.hero1()!.id !== this.hero2()!.id,
  );

  /** Excludes whoever is already selected on the other side, so a self-battle can't be picked. */
  protected readonly pickerOptions = computed(() => {
    const side = this.picking();
    if (!side) {
      return [];
    }

    const otherId = side === 1 ? this.hero2()?.id : this.hero1()?.id;
    const term = this.pickerSearch().trim().toLowerCase();

    return this.roster()
      .filter((hero) => hero.id !== otherId)
      .filter((hero) => !term || hero.name.toLowerCase().includes(term));
  });

  ngOnInit(): void {
    // pageSize 100 covers the whole roster in one request; the picker filters client-side,
    // which is instant and avoids a request per keystroke.
    this.superheroes.getPaged({ pageSize: 100, sortBy: 'name', sortDir: 'asc' }).subscribe({
      next: (response) => this.roster.set(response.items),
    });

    this.loadHistory();
  }

  protected loadHistory(): void {
    this.battles.getHistory(1, 8).subscribe({
      next: (response) => this.history.set(response.items),
    });
  }

  protected openPicker(side: Side): void {
    this.pickerSearch.set('');
    this.picking.set(side);
  }

  protected select(hero: SuperheroListItem): void {
    if (this.picking() === 1) {
      this.hero1.set(hero);
    } else {
      this.hero2.set(hero);
    }
    this.picking.set(null);
    // Selecting a new combatant invalidates the previous result.
    this.result.set(null);
  }

  protected randomise(): void {
    const pool = this.roster();
    if (pool.length < 2) {
      return;
    }

    const first = Math.floor(Math.random() * pool.length);
    let second = Math.floor(Math.random() * pool.length);
    while (second === first) {
      second = Math.floor(Math.random() * pool.length);
    }

    this.hero1.set(pool[first]);
    this.hero2.set(pool[second]);
    this.result.set(null);
  }

  protected swap(): void {
    const a = this.hero1();
    this.hero1.set(this.hero2());
    this.hero2.set(a);
    this.result.set(null);
  }

  protected clear(): void {
    this.hero1.set(null);
    this.hero2.set(null);
    this.result.set(null);
  }

  protected fight(): void {
    if (!this.canFight() || this.simulating()) {
      return;
    }

    this.simulating.set(true);
    this.result.set(null);

    this.battles.simulate(this.hero1()!.id, this.hero2()!.id).subscribe({
      next: (result) => {
        this.result.set(result);
        this.simulating.set(false);
        this.loadHistory();
      },
      error: () => this.simulating.set(false),
    });
  }

  /** Bar width for a score, relative to the higher of the two, so the gap reads visually. */
  protected scoreWidth(score: number): number {
    const current = this.result();
    if (!current) {
      return 0;
    }
    const max = Math.max(current.hero1.score, current.hero2.score, 1);
    return Math.round((score / max) * 100);
  }

  protected alignmentClass(alignment: string): string {
    switch (alignment) {
      case 'Hero':
        return 'is-hero';
      case 'Villain':
        return 'is-villain';
      default:
        return 'is-antihero';
    }
  }
}
