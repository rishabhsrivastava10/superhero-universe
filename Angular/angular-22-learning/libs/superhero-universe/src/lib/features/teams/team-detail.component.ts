import { ChangeDetectionStrategy, Component, OnInit, computed, inject, input, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';
import { NotificationService } from '../../core/services/notification.service';
import { SuperheroService } from '../../core/services/superhero.service';
import { TeamService } from '../../core/services/team.service';
import { ConfirmDialogComponent } from '../../shared/components/confirm-dialog.component';
import { HeroAvatarComponent } from '../../shared/components/hero-avatar.component';
import { TeamDetail, TeamMember } from '../../shared/models/power-team.models';
import { SuperheroListItem } from '../../shared/models/superhero.models';

@Component({
  selector: 'hero-team-detail',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterLink, ConfirmDialogComponent, HeroAvatarComponent],
  templateUrl: './team-detail.component.html',
  styleUrl: './team-detail.component.scss',
})
export class TeamDetailComponent implements OnInit {
  private readonly teams = inject(TeamService);
  private readonly superheroes = inject(SuperheroService);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly notifications = inject(NotificationService);

  readonly id = input.required<string>();

  protected readonly isAdmin = this.auth.isAdmin;

  protected readonly team = signal<TeamDetail | null>(null);
  protected readonly loading = signal(true);
  protected readonly notFound = signal(false);
  protected readonly confirmingDelete = signal(false);
  protected readonly addOpen = signal(false);
  protected readonly candidates = signal<SuperheroListItem[]>([]);

  /** Heroes not already on this team - prevents an add that would just 409. */
  protected readonly availableHeroes = computed(() => {
    const memberIds = new Set(this.team()?.members.map((m) => m.superheroId) ?? []);
    return this.candidates().filter((hero) => !memberIds.has(hero.id));
  });

  ngOnInit(): void {
    this.load();
  }

  protected load(): void {
    const teamId = Number(this.id());
    if (!Number.isFinite(teamId)) {
      this.notFound.set(true);
      this.loading.set(false);
      return;
    }

    this.loading.set(true);
    this.teams.getById(teamId).subscribe({
      next: (detail) => {
        this.team.set(detail);
        this.loading.set(false);
      },
      error: () => {
        this.notFound.set(true);
        this.loading.set(false);
      },
    });
  }

  protected openAdd(): void {
    this.addOpen.set(true);

    // pageSize 100 is the API's clamp - fine for this dataset, and it keeps the picker to a
    // single request. A larger roster would want a searchable/paged picker instead.
    this.superheroes.getPaged({ pageSize: 100, sortBy: 'name', sortDir: 'asc' }).subscribe({
      next: (response) => this.candidates.set(response.items),
    });
  }

  protected addMember(heroId: number): void {
    const current = this.team();
    if (!current) {
      return;
    }

    this.teams.addMember(current.id, heroId).subscribe({
      next: () => {
        this.notifications.success('Member added.');
        this.addOpen.set(false);
        this.load();
      },
    });
  }

  protected removeMember(member: TeamMember): void {
    const current = this.team();
    if (!current) {
      return;
    }

    this.teams.removeMember(current.id, member.superheroId).subscribe({
      next: () => {
        this.notifications.success(`${member.name} left the team.`);
        this.load();
      },
    });
  }

  protected confirmDelete(): void {
    const current = this.team();
    if (!current) {
      return;
    }

    this.teams.delete(current.id).subscribe({
      next: () => {
        this.notifications.success(`${current.name} was deleted.`);
        void this.router.navigate(['/teams']);
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

