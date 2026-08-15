import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';
import { NotificationService } from '../../core/services/notification.service';

@Component({
  selector: 'hero-navbar',
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './navbar.component.html',
  styleUrl: './navbar.component.scss',
})
export class NavbarComponent {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly notifications = inject(NotificationService);

  protected readonly user = this.auth.user;
  protected readonly isAdmin = this.auth.isAdmin;
  protected readonly menuOpen = signal(false);

  protected toggleMenu(): void {
    this.menuOpen.update((open) => !open);
  }

  protected logout(): void {
    this.menuOpen.set(false);

    // The session is already cleared locally by the service, so navigate immediately and
    // treat the server call as best-effort - a failed logout shouldn't strand the user.
    this.auth.logout().subscribe({
      next: () => this.notifications.success('Signed out.'),
      error: () => this.notifications.info('Signed out locally.'),
    });

    void this.router.navigate(['/login']);
  }

  protected initials(username: string): string {
    return username.slice(0, 2).toUpperCase();
  }
}
