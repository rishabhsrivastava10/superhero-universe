import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { SUPERHERO_API_CONFIG } from '../../core/api-config';
import { PagedResponse, SuperheroListItem } from '../../shared/models/superhero.models';
import { SuperheroListComponent } from './superhero-list.component';

const BASE_URL = 'http://test-api';
const HEROES_URL = `${BASE_URL}/api/superheroes`;
const DEBOUNCE_MS = 350;

function pagedResponse(names: string[]): PagedResponse<SuperheroListItem> {
  return {
    items: names.map((name, i) => ({
      id: i + 1,
      name,
      realName: name,
      universe: 'DC',
      alignment: 'Hero',
      powerLevel: 90,
      imageUrl: null,
      powers: [],
    })) as SuperheroListItem[],
    page: 1,
    pageSize: 8,
    totalCount: names.length,
    totalPages: 1,
    hasPreviousPage: false,
    hasNextPage: false,
  };
}

describe('SuperheroListComponent', () => {
  let fixture: ComponentFixture<SuperheroListComponent>;
  let http: HttpTestingController;

  /** Answers the request the component fires on construction. */
  function flushInitialLoad(): void {
    fixture.detectChanges();
    http.expectOne((r) => r.url === HEROES_URL).flush(pagedResponse(['Thanos', 'Superman']));
    fixture.detectChanges();
  }

  function type(text: string): void {
    const input = fixture.nativeElement.querySelector('input[type=search]') as HTMLInputElement;
    input.value = text;
    input.dispatchEvent(new Event('input'));
  }

  beforeEach(() => {
    localStorage.clear();
    // The app is zoneless, so there is no zone.js/testing and fakeAsync/tick are unavailable.
    // debounceTime schedules on the RxJS async scheduler, which vitest's fake setTimeout drives.
    vi.useFakeTimers();

    TestBed.configureTestingModule({
      imports: [SuperheroListComponent],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([]),
        { provide: SUPERHERO_API_CONFIG, useValue: { baseUrl: BASE_URL } },
      ],
    });

    fixture = TestBed.createComponent(SuperheroListComponent);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    http.verify();
    vi.useRealTimers();
  });

  it('sends the typed text as search= once the debounce elapses', () => {
    flushInitialLoad();

    type('bat');
    vi.advanceTimersByTime(DEBOUNCE_MS);

    // Regression: the debounced stream used to feed only the "(filtered)" caption, so typing
    // flipped the label but never re-queried and the grid stayed unfiltered.
    http.expectOne((r) => r.url === HEROES_URL && r.params.get('search') === 'bat')
      .flush(pagedResponse(['Batman']));
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('Batman');
    expect(fixture.nativeElement.textContent).not.toContain('Thanos');
  });

  it('coalesces rapid keystrokes into a single request', () => {
    flushInitialLoad();

    for (const text of ['b', 'ba', 'bat']) {
      type(text);
      vi.advanceTimersByTime(100);        // shorter than the debounce, so nothing fires yet
    }
    http.expectNone((r) => r.url === HEROES_URL);

    vi.advanceTimersByTime(DEBOUNCE_MS);
    http.expectOne((r) => r.params.get('search') === 'bat').flush(pagedResponse(['Batman']));
  });

  it('omits search= entirely when the box is emptied', () => {
    flushInitialLoad();

    type('bat');
    vi.advanceTimersByTime(DEBOUNCE_MS);
    http.expectOne((r) => r.params.get('search') === 'bat').flush(pagedResponse(['Batman']));

    type('');
    vi.advanceTimersByTime(DEBOUNCE_MS);

    // An empty search= would still reach the DB filter, so the param must be dropped.
    const req = http.expectOne((r) => r.url === HEROES_URL);
    expect(req.request.params.has('search')).toBe(false);
    req.flush(pagedResponse(['Thanos', 'Superman']));
  });

  it('clearing filters issues exactly one reload', () => {
    flushInitialLoad();

    type('bat');
    vi.advanceTimersByTime(DEBOUNCE_MS);
    http.expectOne((r) => r.params.get('search') === 'bat').flush(pagedResponse(['Batman']));
    fixture.detectChanges();

    const clear = Array.from<HTMLButtonElement>(fixture.nativeElement.querySelectorAll('button'))
      .find((b) => b.textContent?.trim() === 'Clear');
    clear!.click();

    http.expectOne((r) => r.url === HEROES_URL).flush(pagedResponse(['Thanos', 'Superman']));

    // The control is reset with emitEvent: false; without it the debounce would fire a
    // second, identical request here and http.verify() would fail.
    vi.advanceTimersByTime(DEBOUNCE_MS);
  });
});
