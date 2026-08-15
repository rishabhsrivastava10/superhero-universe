import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { NotificationService } from '../../core/services/notification.service';

/** Renders every queued toast. Mounted once, in the app shell. */
@Component({
  selector: 'hero-toast-host',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="toasts" aria-live="polite" aria-atomic="true">
      @for (toast of notifications.toasts(); track toast.id) {
        <div class="toast" [class]="'is-' + toast.kind">
          <span class="toast__msg">{{ toast.message }}</span>
          <button type="button" class="toast__close" (click)="notifications.dismiss(toast.id)" aria-label="Dismiss">
            &times;
          </button>
        </div>
      }
    </div>
  `,
  styles: `
    .toasts {
      position: fixed;
      right: 1.25rem;
      bottom: 1.25rem;
      z-index: 1000;
      display: flex;
      flex-direction: column;
      gap: 0.6rem;
      max-width: min(380px, calc(100vw - 2.5rem));
    }
    .toast {
      display: flex;
      align-items: flex-start;
      gap: 0.75rem;
      padding: 0.8rem 1rem;
      border-radius: var(--radius-sm);
      border-left: 3px solid var(--info);
      background: var(--bg-elevated);
      box-shadow: var(--shadow);
      animation: toast-in 0.2s ease;
    }
    .toast.is-success {
      border-left-color: var(--success);
    }
    .toast.is-error {
      border-left-color: var(--danger);
    }
    .toast__msg {
      flex: 1;
      font-size: 0.88rem;
      line-height: 1.4;
    }
    .toast__close {
      background: none;
      border: none;
      color: var(--text-muted);
      font-size: 1.15rem;
      line-height: 1;
      cursor: pointer;
      padding: 0;
    }
    .toast__close:hover {
      color: var(--text-primary);
    }
    @keyframes toast-in {
      from {
        opacity: 0;
        transform: translateY(8px);
      }
    }
  `,
})
export class ToastHostComponent {
  protected readonly notifications = inject(NotificationService);
}
