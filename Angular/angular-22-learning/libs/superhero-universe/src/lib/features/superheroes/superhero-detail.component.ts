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
import { SuperheroService } from '../../core/services/superhero.service';
import { ConfirmDialogComponent } from '../../shared/components/confirm-dialog.component';
import { StatBarComponent } from '../../shared/components/stat-bar.component';
import { STAT_KEYS, SuperheroDetail } from '../../shared/models/superhero.models';

@Component({
  selector: 'hero-superhero-detail',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterLink, StatBarComponent, ConfirmDialogComponent],
  templateUrl: './superhero-detail.component.html',
  styleUrl: './superhero-detail.component.scss',
})
export class SuperheroDetailComponent implements OnInit {
  private readonly superheroes = inject(SuperheroService);
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

  protected initials(name: string): string {
    return name
      .split(' ')
      .map((part) => part[0])
      .join('')
      .slice(0, 2)
      .toUpperCase();
  }
}
