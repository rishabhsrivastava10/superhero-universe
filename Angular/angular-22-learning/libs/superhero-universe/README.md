# superhero-universe (Angular library)

The entire Superhero Universe front end. The shell application in `src/app` is deliberately thin —
it provides the API base URL and HTTP interceptors, then mounts the routes exported from here.

## Layout

```
src/lib/
  core/          singletons: auth, guards, interceptors, HTTP services
  shared/        reusable components and the TypeScript models mirroring the API DTOs
  features/      one folder per feature area, each lazy-loaded
  layout/        shell, sidebar, navbar
  superhero-universe.routes.ts
```

## Consuming it

`tsconfig.json` maps `superhero-universe` to this library's **source**, not to `dist/`, so a change
here is picked up without rebuilding the library first. Switch the path mapping to `./dist/*` if
this is ever published to a registry.

```ts
import { superheroUniverseRoutes, SUPERHERO_API_CONFIG } from 'superhero-universe';
```

## Tests

```bash
npx ng test superhero-universe --watch=false
```
