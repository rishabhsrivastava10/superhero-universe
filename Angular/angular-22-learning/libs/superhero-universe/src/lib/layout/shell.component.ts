import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { NavbarComponent } from './navbar/navbar.component';
import { SidebarComponent } from './sidebar/sidebar.component';

/** Authenticated layout: sidebar + navbar wrapping the routed feature. */
@Component({
  selector: 'hero-shell',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterOutlet, SidebarComponent, NavbarComponent],
  template: `
    <div class="shell">
      <hero-sidebar />
      <div class="shell__main">
        <hero-navbar />
        <main class="shell__content">
          <router-outlet />
        </main>
      </div>
    </div>
  `,
  styles: `
    .shell {
      display: flex;
      height: 100vh;
      overflow: hidden;
    }
    .shell__main {
      display: flex;
      flex-direction: column;
      flex: 1;
      min-width: 0;
    }
    .shell__content {
      flex: 1;
      overflow-y: auto;
      padding: 1.75rem;
    }
    @media (max-width: 900px) {
      .shell {
        flex-direction: column;
      }
      .shell__content {
        padding: 1rem;
      }
    }
  `,
})
export class ShellComponent {}
