import { Routes } from '@angular/router';
import { adminGuard, authGuard, guestGuard } from './core/guards/auth.guard';

/**
 * Every feature is lazy-loaded with loadComponent, so the initial bundle only contains the
 * shell plus whichever route the user actually landed on.
 */
export const superheroUniverseRoutes: Routes = [
  {
    path: 'login',
    canActivate: [guestGuard],
    loadComponent: () => import('./features/auth/login.component').then((m) => m.LoginComponent),
    title: 'Sign in | Superhero Universe',
  },
  {
    path: 'register',
    canActivate: [guestGuard],
    loadComponent: () =>
      import('./features/auth/register.component').then((m) => m.RegisterComponent),
    title: 'Create account | Superhero Universe',
  },
  {
    path: '',
    loadComponent: () => import('./layout/shell.component').then((m) => m.ShellComponent),
    canActivate: [authGuard],
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'superheroes' },
      {
        path: 'superheroes',
        loadComponent: () =>
          import('./features/superheroes/superhero-list.component').then(
            (m) => m.SuperheroListComponent,
          ),
        title: 'Heroes | Superhero Universe',
      },
      {
        // Declared BEFORE :id so "new" isn't swallowed by the id route.
        path: 'superheroes/new',
        canActivate: [adminGuard],
        loadComponent: () =>
          import('./features/superheroes/superhero-form.component').then(
            (m) => m.SuperheroFormComponent,
          ),
        title: 'New hero | Superhero Universe',
      },
      {
        path: 'superheroes/:id/edit',
        canActivate: [adminGuard],
        loadComponent: () =>
          import('./features/superheroes/superhero-form.component').then(
            (m) => m.SuperheroFormComponent,
          ),
        title: 'Edit hero | Superhero Universe',
      },
      {
        path: 'superheroes/:id',
        loadComponent: () =>
          import('./features/superheroes/superhero-detail.component').then(
            (m) => m.SuperheroDetailComponent,
          ),
        title: 'Hero | Superhero Universe',
      },
      {
        path: 'forbidden',
        loadComponent: () =>
          import('./features/forbidden.component').then((m) => m.ForbiddenComponent),
        title: 'Access denied | Superhero Universe',
      },
    ],
  },
  { path: '**', redirectTo: '' },
];
