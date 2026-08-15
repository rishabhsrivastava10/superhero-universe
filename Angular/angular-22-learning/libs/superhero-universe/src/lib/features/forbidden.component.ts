import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'hero-forbidden',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterLink],
  template: `
    <div class="su-state">
      <h1>Access denied</h1>
      <p>This area is restricted to administrators.</p>
      <a class="su-btn" routerLink="/superheroes">Back to all heroes</a>
    </div>
  `,
})
export class ForbiddenComponent {}
