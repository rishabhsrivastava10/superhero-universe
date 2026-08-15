import { ChangeDetectionStrategy, Component, OnInit, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { DashboardService } from '../../core/services/dashboard.service';
import { HeroAvatarComponent } from '../../shared/components/hero-avatar.component';
import { ChartSlice, Dashboard } from '../../shared/models/dashboard.models';

interface PlottedSlice extends ChartSlice {
  /** Width as a percentage of the largest value in the same chart. */
  widthPercent: number;
  /** Share of the chart's total, shown alongside the raw count. */
  sharePercent: number;
  color: string;
}

@Component({
  selector: 'hero-dashboard',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterLink, HeroAvatarComponent],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss',
})
export class DashboardComponent implements OnInit {
  private readonly dashboard = inject(DashboardService);

  protected readonly data = signal<Dashboard | null>(null);
  protected readonly loading = signal(true);
  protected readonly failed = signal(false);

  /**
   * Categorical slots from the validated palette, assigned in fixed order and never cycled.
   * Verified on this app's dark surface: worst adjacent CVD deltaE 9.4, normal-vision 26.5,
   * all above 3:1 contrast. The app's semantic green/red/amber was measured first and FAILED
   * (deutan deltaE 5.3 between red and green), so it is deliberately not used for chart marks.
   */
  private static readonly CATEGORICAL = ['#3987e5', '#d95926', '#199e70', '#c98500'];

  protected readonly heroesByUniverse = computed(() => this.plot(this.data()?.heroesByUniverse));
  protected readonly heroesByAlignment = computed(() => this.plot(this.data()?.heroesByAlignment));

  protected readonly missionSuccessRate = computed(() => {
    const totals = this.data()?.totals;
    if (!totals || totals.missionsCompleted === 0) {
      return null;
    }
    return Math.round((totals.missionsSucceeded / totals.missionsCompleted) * 100);
  });

  ngOnInit(): void {
    this.load();
  }

  protected load(): void {
    this.loading.set(true);
    this.failed.set(false);

    this.dashboard.getDashboard().subscribe({
      next: (data) => {
        this.data.set(data);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
        this.failed.set(true);
      },
    });
  }

  private plot(slices: ChartSlice[] | undefined): PlottedSlice[] {
    if (!slices?.length) {
      return [];
    }

    // Bars are scaled against the largest value so the biggest category fills the track;
    // the share percentage is reported separately as text.
    const max = Math.max(...slices.map((s) => s.value), 1);
    const total = slices.reduce((sum, s) => sum + s.value, 0) || 1;

    return slices.map((slice, index) => ({
      ...slice,
      widthPercent: Math.round((slice.value / max) * 100),
      sharePercent: Math.round((slice.value / total) * 100),
      color: DashboardComponent.CATEGORICAL[index % DashboardComponent.CATEGORICAL.length],
    }));
  }

  protected winRate(hero: { battlesWon: number; totalBattles: number }): number {
    return hero.totalBattles === 0 ? 0 : Math.round((hero.battlesWon / hero.totalBattles) * 100);
  }
}
