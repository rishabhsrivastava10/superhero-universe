# Continuous Integration

`ci.yml` runs on every push and pull request to `main`, and can be triggered by hand from the
Actions tab. A newer push to the same branch cancels an in-flight run, so results nobody is
waiting for don't hold up the queue.

## What runs

Two jobs, in parallel:

| Job | Steps | Covers |
|-----|-------|--------|
| **Backend (.NET)** | restore → build → test | 111 xUnit tests |
| **Frontend (Angular)** | `npm ci` → build → test | 20 Vitest tests |

Both fail the run on any test failure, so a red check means something is genuinely broken.

### Why these steps in this order

- **`dotnet build --no-restore`** — deliberately does *not* re-restore. If the restore step were
  skipped or the cache were broken, this fails loudly rather than silently restoring again and
  hiding the problem.
- **`dotnet test --no-build`** — tests exactly the binaries the build step produced, rather than
  quietly rebuilding something different.
- **`npm ci`, never `npm install`** — installs exactly what `package-lock.json` specifies, and
  fails if the lockfile and `package.json` have drifted apart. `npm install` would happily
  "fix" the drift and mask it.
- **Test results are uploaded with `if: always()`** — they're most worth reading when the run
  failed, which is exactly when a plain step would be skipped.

### SDK pinning

`global.json` at the repository root pins the .NET SDK, and the workflow reads it via
`global-json-file`. Local and CI therefore build with the same SDK band, so "works on my machine"
means something. `rollForward: latestFeature` allows patch/feature updates without a hard break.

## What is deliberately NOT in CI

### API integration tests (234 assertions)

`IntegrationTests/` drives the real HTTP API against a seeded SQL Server database. That covers the
whole stack — routing, model binding, authorization, EF Core translation, and the database's own
constraints — which is genuinely valuable, but it needs infrastructure this workflow does not
provide: a SQL Server instance, the schema and seed scripts applied, a `Jwt:Key`, and the API
process running.

Run them locally instead:

```powershell
# 1. Start the API (needs SQL Server with SuperheroUniverseDb seeded)
.\Dot Net\DotNetCoreAPI\WebAPISuperheroUniverse\start-api.ps1

# 2. In another terminal
.\Dot Net\DotNetCoreAPI\WebAPISuperheroUniverse\IntegrationTests\Run-All.ps1
```

They are already CI-ready in every other respect: each suite honours a `SHU_BASE_URL` override,
emits a machine-readable `SHU_SUMMARY` line, and sets a non-zero exit code on failure. Adding them
to a workflow would need a SQL Server service container plus steps to apply
`SQL/DB/Tables/.../SuperheroUniverseDb_Schema.sql`, then the seed scripts, then start the API with
`JWT__KEY` set. That belongs with the Docker work rather than bolted on here.

### Docker image build

The original pipeline design ended with "build Docker images". That step is absent because the
Docker phase has not been done — there are no Dockerfiles in the repository yet. It is left out
rather than stubbed, so the pipeline reflects what actually exists.

### Deployment

Nothing is deployed. No deployment step is faked or claimed.

## Making the check actually block merges

A failing run is only advisory until branch protection is enabled — that is a repository setting,
not something a workflow file can configure. On GitHub:

**Settings → Branches → Add branch ruleset** for `main`, then enable *Require status checks to
pass* and select **Backend (.NET)** and **Frontend (Angular)**.

Without it, a red pipeline can still be merged.

## Running the same checks locally

```powershell
# Backend
dotnet restore "Dot Net/DotNetCoreAPI/WebAPISuperheroUniverse/WebAPISuperheroUniverse.sln"
dotnet build   "Dot Net/DotNetCoreAPI/WebAPISuperheroUniverse/WebAPISuperheroUniverse.sln" -c Release --no-restore
dotnet test    "Dot Net/DotNetCoreAPI/WebAPISuperheroUniverse/WebAPISuperheroUniverse.sln" -c Release --no-build

# Frontend
cd Angular/angular-22-learning
npm ci
npx ng build --configuration production
npx ng test superhero-universe --watch=false
```

Note the quoting: the `Dot Net` folder name contains a space.
