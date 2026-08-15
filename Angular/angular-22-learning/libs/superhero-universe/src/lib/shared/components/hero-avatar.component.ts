import { ChangeDetectionStrategy, Component, computed, input, signal } from '@angular/core';

/**
 * Character portrait with a graceful fallback.
 *
 * Renders the image when there is one, and falls back to the character's initials both when no
 * ImageUrl is set AND when the image fails to load - a remote avatar service being unreachable
 * should degrade to initials, not to a broken-image icon.
 */
@Component({
  selector: 'hero-avatar',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    @if (imageUrl() && !failed()) {
      <img
        class="avatar__img"
        [src]="imageUrl()"
        [alt]="name()"
        loading="lazy"
        decoding="async"
        (error)="failed.set(true)"
      />
    } @else {
      <span class="avatar__initials">{{ initials() }}</span>
    }
  `,
  styles: `
    :host {
      display: grid;
      place-items: center;
      overflow: hidden;
      flex-shrink: 0;
      /* Gradient shows through while a transparent avatar loads, and is the initials backdrop */
      background: var(--accent-gradient);
      color: #fff;
      font-weight: 700;
      line-height: 1;
      width: var(--avatar-size, 54px);
      height: var(--avatar-size, 54px);
      border-radius: var(--avatar-radius, 14px);
    }

    .avatar__img {
      width: 100%;
      height: 100%;
      object-fit: cover;
      display: block;
    }

    .avatar__initials {
      font-size: var(--avatar-font-size, 1.05rem);
    }
  `,
})
export class HeroAvatarComponent {
  readonly name = input.required<string>();
  readonly imageUrl = input<string | null>(null);

  protected readonly failed = signal(false);

  protected readonly initials = computed(() =>
    this.name()
      .split(' ')
      .filter(Boolean)
      .map((part) => part[0])
      .join('')
      .slice(0, 2)
      .toUpperCase(),
  );
}
