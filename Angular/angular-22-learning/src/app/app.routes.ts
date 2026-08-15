import { Routes } from '@angular/router';
import { superheroUniverseRoutes } from 'superhero-universe';

// The shell app stays thin: it just mounts the feature library's routes.
export const routes: Routes = [...superheroUniverseRoutes];
