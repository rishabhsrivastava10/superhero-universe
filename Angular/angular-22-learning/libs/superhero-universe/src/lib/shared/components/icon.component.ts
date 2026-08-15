import { ChangeDetectionStrategy, Component, input } from '@angular/core';

export type IconName =
  | 'grid'
  | 'chart'
  | 'swords'
  | 'users'
  | 'target'
  | 'trophy'
  | 'plus'
  | 'eye'
  | 'eye-off';

/**
 * Inline SVG icons.
 *
 * Kept as hand-written paths rather than pulling in an icon package: the set is tiny, and it
 * keeps the library dependency-free. Every path uses currentColor so icons inherit the
 * surrounding text colour (including the sidebar's active/hover states).
 */
@Component({
  selector: 'hero-icon',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <svg
      [attr.width]="size()"
      [attr.height]="size()"
      viewBox="0 0 24 24"
      fill="none"
      stroke="currentColor"
      stroke-width="1.8"
      stroke-linecap="round"
      stroke-linejoin="round"
      aria-hidden="true"
      focusable="false"
    >
      @switch (name()) {
        @case ('grid') {
          <rect x="3" y="3" width="7" height="7" rx="1.5" />
          <rect x="14" y="3" width="7" height="7" rx="1.5" />
          <rect x="3" y="14" width="7" height="7" rx="1.5" />
          <rect x="14" y="14" width="7" height="7" rx="1.5" />
        }
        @case ('chart') {
          <path d="M3 3v18h18" />
          <path d="M7 15v3" />
          <path d="M12 10v8" />
          <path d="M17 6v12" />
        }
        @case ('swords') {
          <path d="M14.5 17.5 3 6V3h3l11.5 11.5" />
          <path d="m13 19 6-6" />
          <path d="m16 16 4 4" />
          <path d="M19 21h2v-2" />
          <path d="M9.5 6.5 21 18v3h-3L6.5 9.5" />
        }
        @case ('users') {
          <path d="M16 21v-2a4 4 0 0 0-4-4H6a4 4 0 0 0-4 4v2" />
          <circle cx="9" cy="7" r="4" />
          <path d="M22 21v-2a4 4 0 0 0-3-3.87" />
          <path d="M16 3.13a4 4 0 0 1 0 7.75" />
        }
        @case ('target') {
          <circle cx="12" cy="12" r="9" />
          <circle cx="12" cy="12" r="5" />
          <circle cx="12" cy="12" r="1.5" />
        }
        @case ('trophy') {
          <path d="M8 21h8" />
          <path d="M12 17v4" />
          <path d="M7 4h10v5a5 5 0 0 1-10 0V4z" />
          <path d="M7 6H4.5a2.5 2.5 0 0 0 2.5 4.5" />
          <path d="M17 6h2.5a2.5 2.5 0 0 1-2.5 4.5" />
        }
        @case ('plus') {
          <path d="M12 5v14" />
          <path d="M5 12h14" />
        }
        @case ('eye') {
          <path d="M2 12s3.6-7 10-7 10 7 10 7-3.6 7-10 7-10-7-10-7z" />
          <circle cx="12" cy="12" r="3" />
        }
        @case ('eye-off') {
          <path d="M10.6 5.2A9.9 9.9 0 0 1 12 5c6.4 0 10 7 10 7a17.6 17.6 0 0 1-3 3.9" />
          <path d="M6.6 6.6A17.3 17.3 0 0 0 2 12s3.6 7 10 7a9.7 9.7 0 0 0 4.5-1.1" />
          <path d="M9.9 9.9a3 3 0 0 0 4.2 4.2" />
          <path d="m3 3 18 18" />
        }
      }
    </svg>
  `,
  styles: `
    :host {
      display: inline-flex;
      flex-shrink: 0;
    }
  `,
})
export class IconComponent {
  readonly name = input.required<IconName>();
  readonly size = input(18);
}
