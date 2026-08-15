import { ChangeDetectionStrategy, Component, OnInit, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { DashboardService } from '../../core/services/dashboard.service';
import { HeroAvatarComponent } from '../../shared/components/hero-avatar.component';
import { RANKING_FIELDS, RankingEntry, Rankings } from '../../shared/models/dashboard.models';

type Scope = 'overall' | 'marvel' | 'dc';

@Component({
  selector: 'hero-rankings',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterLink, HeroAvatarComponent],
  templateUrl: './rankings.component.html',
  styleUrl: './rankings.component.scss',
})
export class RankingsComponent implements OnInit {
  private readonly dashboard = inject(DashboardService);

  protected readonly fields = RANKING_FIELDS;
  protected readonly scopes: { key: Scope; label: string }[] = [
    { key: 'overall', label: 'Overall' },
    { key: 'marvel', label: 'Marvel' },
    { key: 'dc', label: 'DC' },
  ];

  protected readonly sortBy = signal<string>('powerLevel');
  protected readonly scope = signal<Scope>('overall');
  protected readonly data = signal<Rankings | null>(null);
  protected readonly loading = signal(true);

  protected readonly entries = computed<RankingEntry[]>(() => {
    const current = this.data();
    if (!current) {
      return [];
    }
    return current[this.scope()];
  });

  /** Bar width relative to the top entry, so the leader fills the track. */
  protected readonly maxValue = computed(() => Math.max(...this.entries().map((e) => e.value), 1));

  protected readonly fieldLabel = computed(
    () => this.fields.find((f) => f.key === this.sortBy())?.label ?? 'Power Level',
  );

  ngOnInit(): void {
    this.load();
  }

  protected load(): void {
    this.loading.set(true);
    this.dashboard.getRankings(this.sortBy(), 10).subscribe({
      next: (data) => {
        this.data.set(data);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  protected setSort(field: string): void {
    this.sortBy.set(field);
    // Only the sort field needs a refetch - the API returns all three scopes each time,
    // so switching scope is instant and requires no request.
    this.load();
  }

  protected barWidth(value: number): number {
    return Math.round((value / this.maxValue()) * 100);
  }

  protected winRate(entry: RankingEntry): number {
    return entry.totalBattles === 0 ? 0 : Math.round((entry.battlesWon / entry.totalBattles) * 100);
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
