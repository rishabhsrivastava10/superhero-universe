import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { debounceTime, distinctUntilChanged } from 'rxjs';
import { AuthService } from '../../core/auth/auth.service';
import { SuperheroService } from '../../core/services/superhero.service';
import { NotificationService } from '../../core/services/notification.service';
import { ConfirmDialogComponent } from '../../shared/components/confirm-dialog.component';
import { HeroAvatarComponent } from '../../shared/components/hero-avatar.component';
import {
  ALIGNMENTS,
  PagedResponse,
  SuperheroListItem,
} from '../../shared/models/superhero.models';

const PAGE_SIZE = 8;

@Component({
  selector: 'hero-superhero-list',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [ReactiveFormsModule, RouterLink, ConfirmDialogComponent, HeroAvatarComponent],
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
   * The search text actually applied to the current result set, as opposed to whatever is
   * mid-typing in the box. Driven by the debounced stream in the constructor, so it stays in
   * step with the request that produced `result`.
   */
  private readonly searchTerm = signal('');

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
    () => !!this.searchTerm() || !!this.universe() || !!this.alignment() || this.minPowerLevel() > 0,
  );

  constructor() {
    /**
     * Debounced so typing "batman" fires ONE request instead of six.
     * distinctUntilChanged additionally skips a request when the text ends up unchanged
     * (e.g. type a character then immediately delete it).
     *
     * This is the only thing that applies a typed search - the template's (search) binding
     * fires solely on Enter and on the input's native clear "x", so the reload has to be
     * driven from here or typing would leave the grid unfiltered.
     */
    this.searchControl.valueChanges
      .pipe(debounceTime(350), distinctUntilChanged(), takeUntilDestroyed())
      .subscribe((term) => this.applySearch(term));

    this.load();
  }

  /**
   * Any filter change resets to page 1 - staying on page 4 of a now-2-page result set
   * would show a confusing empty grid.
   */
  private applySearch(term: string): void {
    this.searchTerm.set(term);
    this.page.set(1);
    this.load();
  }

  protected load(): void {
    this.loading.set(true);
    this.loadFailed.set(false);

    this.superheroes
      .getPaged({
        page: this.page(),
        pageSize: PAGE_SIZE,
        search: this.searchTerm() || undefined,
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
    // Enter (or the input's clear "x") should not wait out the debounce.
    this.applySearch(this.searchControl.value);
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
    // emitEvent: false - this method loads once at the end itself, and letting the control
    // emit would queue a second, identical request 350ms later.
    this.searchControl.setValue('', { emitEvent: false });
    this.searchTerm.set('');
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

}

