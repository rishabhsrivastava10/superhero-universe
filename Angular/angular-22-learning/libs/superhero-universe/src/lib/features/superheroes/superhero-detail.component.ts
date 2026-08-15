import {
  ChangeDetectionStrategy,
  Component,
  OnInit,
  computed,
  inject,
  input,
  signal,
} from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';
import { NotificationService } from '../../core/services/notification.service';
import { PowerService } from '../../core/services/power.service';
import { SuperheroService } from '../../core/services/superhero.service';
import { ConfirmDialogComponent } from '../../shared/components/confirm-dialog.component';
import { HeroAvatarComponent } from '../../shared/components/hero-avatar.component';
import { StatBarComponent } from '../../shared/components/stat-bar.component';
import { PowerListItem } from '../../shared/models/power-team.models';
import { STAT_KEYS, SuperheroDetail } from '../../shared/models/superhero.models';

@Component({
  selector: 'hero-superhero-detail',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterLink, StatBarComponent, ConfirmDialogComponent, HeroAvatarComponent],
  templateUrl: './superhero-detail.component.html',
  styleUrl: './superhero-detail.component.scss',
})
export class SuperheroDetailComponent implements OnInit {
  private readonly superheroes = inject(SuperheroService);
  private readonly powers = inject(PowerService);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly notifications = inject(NotificationService);

  /** Bound from the route via withComponentInputBinding(). */
  readonly id = input.required<string>();

  protected readonly isAdmin = this.auth.isAdmin;
  protected readonly statKeys = STAT_KEYS;

  protected readonly hero = signal<SuperheroDetail | null>(null);
  protected readonly loading = signal(true);
  protected readonly notFound = signal(false);
  protected readonly confirmingDelete = signal(false);

  // ---- Power assignment (admin only) --------------------------------------------------------
  protected readonly powersOpen = signal(false);
  protected readonly savingPowers = signal(false);
  protected readonly allPowers = signal<PowerListItem[]>([]);
  /** Ids ticked in the picker. A Set keeps the toggle O(1) and the template checks cheap. */
  protected readonly selectedPowerIds = signal<Set<number>>(new Set());

  protected readonly winRate = computed(() => {
    const current = this.hero();
    if (!current || current.totalBattles === 0) {
      return 0;
    }
    return Math.round((current.battlesWon / current.totalBattles) * 100);
  });

  /** Circumference of the r=52 donut, used to drive the stroke-dasharray. */
  protected readonly ringCircumference = 2 * Math.PI * 52;

  protected readonly ringOffset = computed(
    () => this.ringCircumference - (this.winRate() / 100) * this.ringCircumference,
  );

  ngOnInit(): void {
    // Route inputs are populated before ngOnInit, so this.id() is safe to read here.
    this.load();
  }

  protected load(): void {
    const heroId = Number(this.id());
    if (!Number.isFinite(heroId)) {
      this.notFound.set(true);
      this.loading.set(false);
      return;
    }

    this.loading.set(true);
    this.superheroes.getById(heroId).subscribe({
      next: (detail) => {
        this.hero.set(detail);
        this.loading.set(false);
      },
      error: () => {
        this.notFound.set(true);
        this.loading.set(false);
      },
    });
  }

  protected statValue(hero: SuperheroDetail, key: string): number {
    return hero[key as keyof SuperheroDetail] as number;
  }

  protected openPowers(): void {
    this.powersOpen.set(true);

    this.powers.getAll().subscribe({
      next: (list) => {
        this.allPowers.set(list);
        // Pre-tick what the hero already has. The API matches on id, but the detail DTO only
        // carries names, so map names back to ids here.
        const currentNames = new Set(this.hero()?.powers ?? []);
        this.selectedPowerIds.set(
          new Set(list.filter((p) => currentNames.has(p.name)).map((p) => p.id)),
        );
      },
    });
  }

  protected togglePower(powerId: number): void {
    this.selectedPowerIds.update((selected) => {
      // Signals compare by reference, so mutate a COPY - mutating in place would not notify.
      const next = new Set(selected);
      if (next.has(powerId)) {
        next.delete(powerId);
      } else {
        next.add(powerId);
      }
      return next;
    });
  }

  protected isPowerSelected(powerId: number): boolean {
    return this.selectedPowerIds().has(powerId);
  }

  protected savePowers(): void {
    const current = this.hero();
    if (!current || this.savingPowers()) {
      return;
    }

    this.savingPowers.set(true);

    this.powers.assignToSuperhero(current.id, [...this.selectedPowerIds()]).subscribe({
      next: (names) => {
        this.notifications.success('Powers updated.');
        // Patch the local copy rather than refetching the whole hero for one changed field.
        this.hero.set({ ...current, powers: names });
        this.savingPowers.set(false);
        this.powersOpen.set(false);
      },
      error: () => this.savingPowers.set(false),
    });
  }

  protected confirmDelete(): void {
    const current = this.hero();
    if (!current) {
      return;
    }

    this.superheroes.delete(current.id).subscribe({
      next: () => {
        this.notifications.success(`${current.name} was deleted.`);
        void this.router.navigate(['/superheroes']);
      },
      error: () => this.confirmingDelete.set(false),
    });
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

