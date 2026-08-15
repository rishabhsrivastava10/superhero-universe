import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { debounceTime, distinctUntilChanged, startWith } from 'rxjs';
import { AuthService } from '../../core/auth/auth.service';
import { SuperheroService } from '../../core/services/superhero.service';
import { NotificationService } from '../../core/services/notification.service';
import { ConfirmDialogComponent } from '../../shared/components/confirm-dialog.component';
import {
  ALIGNMENTS,
  PagedResponse,
  SuperheroListItem,
} from '../../shared/models/superhero.models';

const PAGE_SIZE = 8;

@Component({
  selector: 'hero-superhero-list',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [ReactiveFormsModule, RouterLink, ConfirmDialogComponent],
  templateUrl: './superhero-list.component.html',
  styleUrl: './superhero-list.component.scss',
})
export class SuperheroListComponent {
  private readonly superheroes = inject(SuperheroService);
  private readonly auth = inject(AuthService);
  private readonly notifications = inject(NotificationService);

  protected readonly isAdmin = this.auth.isAdmin;
  protected readonly alignments = ALIGNMENTS;

  protected readonly searchControl = new FormControl('', { nonNullable: true });

  /**
   * Debounced so typing "batman" fires ONE request instead of six.
   * distinctUntilChanged additionally skips a request when the text ends up unchanged
   * (e.g. type a character then immediately delete it).
   */
  private readonly debouncedSearch = toSignal(
    this.searchControl.valueChanges.pipe(
      debounceTime(350),
      distinctUntilChanged(),
      takeUntilDestroyed(),
      startWith(''),
    ),
    { initialValue: '' },
  );

  protected readonly page = signal(1);
  protected readonly universe = signal('');
  protected readonly alignment = signal('');
  protected readonly minPowerLevel = signal(0);
  protected readonly sortBy = signal('powerLevel');
  protected readonly sortDir = signal<'asc' | 'desc'>('desc');

  protected readonly loading = signal(false);
  protected readonly loadFailed = signal(false);
  protected readonly result = signal<PagedResponse<SuperheroListItem> | null>(null);
  protected readonly pendingDelete = signal<SuperheroListItem | null>(null);

  protected readonly heroes = computed(() => this.result()?.items ?? []);
  protected readonly totalCount = computed(() => this.result()?.totalCount ?? 0);
  protected readonly totalPages = computed(() => this.result()?.totalPages ?? 0);
  protected readonly hasFilters = computed(
    () => !!this.debouncedSearch() || !!this.universe() || !!this.alignment() || this.minPowerLevel() > 0,
  );

  constructor() {
    // Any filter change resets to page 1 - staying on page 4 of a now-2-page result set
    // would show a confusing empty grid.
    this.searchControl.valueChanges.pipe(takeUntilDestroyed()).subscribe(() => this.page.set(1));
    this.load();
  }

  protected load(): void {
    this.loading.set(true);
    this.loadFailed.set(false);

    this.superheroes
      .getPaged({
        page: this.page(),
        pageSize: PAGE_SIZE,
        search: this.debouncedSearch() || undefined,
        universe: this.universe() || undefined,
        alignment: this.alignment() || undefined,
        minPowerLevel: this.minPowerLevel() || undefined,
        sortBy: this.sortBy(),
        sortDir: this.sortDir(),
      })
      .subscribe({
        next: (response) => {
          this.result.set(response);
          this.loading.set(false);
        },
        error: () => {
          this.loading.set(false);
          this.loadFailed.set(true);
        },
      });
  }

  protected onFilterChange(): void {
    this.page.set(1);
    this.load();
  }

  protected onSearchApply(): void {
    this.load();
  }

  protected goToPage(page: number): void {
    if (page < 1 || page > this.totalPages() || page === this.page()) {
      return;
    }
    this.page.set(page);
    this.load();
  }

  protected setSort(value: string): void {
    const [field, dir] = value.split(':');
    this.sortBy.set(field);
    this.sortDir.set(dir as 'asc' | 'desc');
    this.page.set(1);
    this.load();
  }

  protected currentSort(): string {
    return `${this.sortBy()}:${this.sortDir()}`;
  }

  protected clearFilters(): void {
    this.searchControl.setValue('');
    this.universe.set('');
    this.alignment.set('');
    this.minPowerLevel.set(0);
    this.page.set(1);
    this.load();
  }

  protected requestDelete(hero: SuperheroListItem, event: Event): void {
    // The delete button sits inside the card's router link.
    event.preventDefault();
    event.stopPropagation();
    this.pendingDelete.set(hero);
  }

  protected confirmDelete(): void {
    const hero = this.pendingDelete();
    if (!hero) {
      return;
    }

    this.superheroes.delete(hero.id).subscribe({
      next: () => {
        this.notifications.success(`${hero.name} was deleted.`);
        this.pendingDelete.set(null);
        this.load();
      },
      // A 409 (hero has battle history) is already surfaced by the error interceptor.
      error: () => this.pendingDelete.set(null),
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

  protected pageNumbers(): number[] {
    return Array.from({ length: this.totalPages() }, (_, i) => i + 1);
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
