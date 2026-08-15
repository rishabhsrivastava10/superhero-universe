import { ChangeDetectionStrategy, Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { AuthService } from '../../core/auth/auth.service';
import { MissionService } from '../../core/services/mission.service';
import { NotificationService } from '../../core/services/notification.service';
import { SuperheroService } from '../../core/services/superhero.service';
import { ConfirmDialogComponent } from '../../shared/components/confirm-dialog.component';
import { HeroAvatarComponent } from '../../shared/components/hero-avatar.component';
import {
  DIFFICULTIES,
  MissionListItem,
  MissionResult,
} from '../../shared/models/mission.models';
import { SuperheroListItem } from '../../shared/models/superhero.models';

@Component({
  selector: 'hero-mission-list',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [ReactiveFormsModule, ConfirmDialogComponent, HeroAvatarComponent],
  templateUrl: './mission-list.component.html',
  styleUrl: './mission-list.component.scss',
})
export class MissionListComponent implements OnInit {
  private readonly missions = inject(MissionService);
  private readonly superheroes = inject(SuperheroService);
  private readonly auth = inject(AuthService);
  private readonly fb = inject(FormBuilder);
  private readonly notifications = inject(NotificationService);

  protected readonly isAdmin = this.auth.isAdmin;
  protected readonly difficulties = DIFFICULTIES;

  protected readonly items = signal<MissionListItem[]>([]);
  protected readonly roster = signal<SuperheroListItem[]>([]);
  protected readonly loading = signal(true);
  protected readonly statusFilter = signal('');

  // Deploy flow
  protected readonly deploying = signal<MissionListItem | null>(null);
  protected readonly selectedIds = signal<Set<number>>(new Set());
  protected readonly running = signal(false);
  protected readonly result = signal<MissionResult | null>(null);

  // Admin
  protected readonly formOpen = signal(false);
  protected readonly saving = signal(false);
  protected readonly pendingDelete = signal<MissionListItem | null>(null);

  protected readonly selectedHeroes = computed(() =>
    this.roster().filter((hero) => this.selectedIds().has(hero.id)),
  );

  /** Mirrors the server's rule so the button disables before a pointless request. */
  protected readonly canDeploy = computed(() => this.selectedIds().size > 0);

  protected readonly averagePower = computed(() => {
    const squad = this.selectedHeroes();
    if (squad.length === 0) {
      return 0;
    }
    return Math.round(squad.reduce((sum, h) => sum + h.powerLevel, 0) / squad.length);
  });

  protected readonly form = this.fb.nonNullable.group({
    title: ['', [Validators.required, Validators.maxLength(200)]],
    description: [''],
    location: [''],
    difficulty: ['Medium', [Validators.required]],
    requiredHeroCount: [3, [Validators.required, Validators.min(1), Validators.max(20)]],
  });

  ngOnInit(): void {
    this.load();
    this.superheroes.getPaged({ pageSize: 100, sortBy: 'name', sortDir: 'asc' }).subscribe({
      next: (response) => this.roster.set(response.items),
    });
  }

  protected load(): void {
    this.loading.set(true);
    this.missions.getAll(this.statusFilter() || undefined).subscribe({
      next: (items) => {
        this.items.set(items);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  protected onStatusChange(value: string): void {
    this.statusFilter.set(value);
    this.load();
  }

  // ---- Deploy ------------------------------------------------------------------------------
  protected openDeploy(mission: MissionListItem): void {
    this.deploying.set(mission);
    this.selectedIds.set(new Set());
    this.result.set(null);
  }

  protected toggleHero(id: number): void {
    this.selectedIds.update((selected) => {
      // Copy, don't mutate: signals compare by reference.
      const next = new Set(selected);
      next.has(id) ? next.delete(id) : next.add(id);
      return next;
    });
  }

  protected isSelected(id: number): boolean {
    return this.selectedIds().has(id);
  }

  /** Convenience: pick the strongest heroes up to the mission's requirement. */
  protected autoPick(): void {
    const mission = this.deploying();
    if (!mission) {
      return;
    }

    const best = [...this.roster()]
      .sort((a, b) => b.powerLevel - a.powerLevel)
      .slice(0, mission.requiredHeroCount)
      .map((h) => h.id);

    this.selectedIds.set(new Set(best));
  }

  protected deploy(): void {
    const mission = this.deploying();
    if (!mission || !this.canDeploy() || this.running()) {
      return;
    }

    this.running.set(true);

    this.missions.start(mission.id, [...this.selectedIds()]).subscribe({
      next: (result) => {
        this.result.set(result);
        this.running.set(false);
        this.load();
      },
      error: () => this.running.set(false),
    });
  }

  protected closeDeploy(): void {
    this.deploying.set(null);
    this.result.set(null);
  }

  protected reset(mission: MissionListItem): void {
    this.missions.reset(mission.id).subscribe({
      next: () => {
        this.notifications.success(`"${mission.title}" is ready to attempt again.`);
        this.load();
      },
    });
  }

  // ---- Admin CRUD --------------------------------------------------------------------------
  protected openCreate(): void {
    this.form.reset({
      title: '',
      description: '',
      location: '',
      difficulty: 'Medium',
      requiredHeroCount: 3,
    });
    this.formOpen.set(true);
  }

  protected save(): void {
    if (this.form.invalid || this.saving()) {
      this.form.markAllAsTouched();
      return;
    }

    this.saving.set(true);
    const raw = this.form.getRawValue();

    this.missions
      .create({
        title: raw.title.trim(),
        description: raw.description.trim() || null,
        location: raw.location.trim() || null,
        difficulty: raw.difficulty,
        requiredHeroCount: raw.requiredHeroCount,
      })
      .subscribe({
        next: () => {
          this.notifications.success('Mission created.');
          this.saving.set(false);
          this.formOpen.set(false);
          this.load();
        },
        error: () => this.saving.set(false),
      });
  }

  protected confirmDelete(): void {
    const mission = this.pendingDelete();
    if (!mission) {
      return;
    }

    this.missions.delete(mission.id).subscribe({
      next: () => {
        this.notifications.success(`"${mission.title}" was deleted.`);
        this.pendingDelete.set(null);
        this.load();
      },
      error: () => this.pendingDelete.set(null),
    });
  }

  protected invalidTitle(): boolean {
    const field = this.form.controls.title;
    return field.invalid && (field.dirty || field.touched);
  }

  // ---- Display helpers ---------------------------------------------------------------------
  protected difficultyClass(difficulty: string): string {
    switch (difficulty) {
      case 'Easy':
        return 'is-easy';
      case 'Hard':
        return 'is-hard';
      default:
        return 'is-medium';
    }
  }

  protected statusClass(status: string): string {
    switch (status) {
      case 'Success':
        return 'is-success';
      case 'Failed':
        return 'is-failed';
      default:
        return 'is-pending';
    }
  }

  protected isResolved(status: string): boolean {
    return status === 'Success' || status === 'Failed';
  }
}
