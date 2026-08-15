import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';

/** Modal confirmation, used before destructive actions such as deleting a hero. */
@Component({
  selector: 'hero-confirm-dialog',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="backdrop" (click)="cancelled.emit()">
      <!-- stopPropagation so clicking inside the panel doesn't dismiss it -->
      <div class="panel" role="dialog" aria-modal="true" (click)="$event.stopPropagation()">
        <h3 class="panel__title">{{ title() }}</h3>
        <p class="panel__body">{{ message() }}</p>
        <div class="panel__actions">
          <button type="button" class="su-btn is-ghost" (click)="cancelled.emit()">Cancel</button>
          <button type="button" class="su-btn is-danger" (click)="confirmed.emit()">
            {{ confirmLabel() }}
          </button>
        </div>
      </div>
    </div>
  `,
  styles: `
    .backdrop {
      position: fixed;
      inset: 0;
      z-index: 900;
      display: grid;
      place-items: center;
      padding: 1rem;
      background: rgb(4 6 20 / 70%);
      backdrop-filter: blur(2px);
    }
    .panel {
      width: min(420px, 100%);
      background: var(--bg-surface);
      border: 1px solid var(--border);
      border-radius: var(--radius);
      padding: 1.5rem;
      box-shadow: var(--shadow);
    }
    .panel__title {
      font-size: 1.1rem;
      margin-bottom: 0.6rem;
    }
    .panel__body {
      margin: 0 0 1.4rem;
      color: var(--text-secondary);
      line-height: 1.5;
    }
    .panel__actions {
      display: flex;
      justify-content: flex-end;
      gap: 0.6rem;
    }
  `,
})
export class ConfirmDialogComponent {
  readonly title = input('Are you sure?');
  readonly message = input('This action cannot be undone.');
  readonly confirmLabel = input('Delete');

  readonly confirmed = output<void>();
  readonly cancelled = output<void>();
}
