import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';

/** A 0-100 attribute rendered as a labelled progress bar. */
@Component({
  selector: 'hero-stat-bar',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="stat">
      <div class="stat__head">
        <span class="stat__label">{{ label() }}</span>
        <span class="stat__value">{{ value() }}</span>
      </div>
      <div
        class="stat__track"
        role="progressbar"
        [attr.aria-label]="label()"
        [attr.aria-valuenow]="value()"
        aria-valuemin="0"
        aria-valuemax="100"
      >
        <div class="stat__fill" [style.width.%]="clamped()"></div>
      </div>
    </div>
  `,
  styles: `
    .stat {
      margin-bottom: 0.85rem;
    }
    .stat__head {
      display: flex;
      justify-content: space-between;
      margin-bottom: 0.3rem;
      font-size: 0.82rem;
    }
    .stat__label {
      color: var(--text-secondary);
      text-transform: capitalize;
    }
    .stat__value {
      color: var(--text-primary);
      font-weight: 600;
      font-variant-numeric: tabular-nums;
    }
    .stat__track {
      height: 7px;
      background: var(--bg-app);
      border-radius: 99px;
      overflow: hidden;
    }
    .stat__fill {
      height: 100%;
      background: var(--accent-gradient);
      border-radius: 99px;
      transition: width 0.35s ease;
    }
  `,
})
export class StatBarComponent {
  readonly label = input.required<string>();
  readonly value = input.required<number>();

  // Guards against a bad API value blowing the bar past its track.
  protected readonly clamped = computed(() => Math.max(0, Math.min(100, this.value())));
}
