# SuperheroUniverse — Database Layout & Schema Ownership

## Who owns the schema

**EF Core migrations are authoritative.** The C# model in
`Dot Net\DotNetCoreAPI\WebAPISuperheroUniverse\WebAPISuperheroUniverse\DBContext\EntityFramework\AppDbContext.cs`
is the single source of truth for the schema. Change the model → add a migration → the SQL here
is regenerated from it.

The application breaks if the EF model and the physical database disagree, so the model has to
win. Having a second, hand-maintained copy of the schema would guarantee eventual drift — and a
stale schema document is worse than no document, because people trust it.

This isn't hypothetical: the original hand-written `xtBattles` script was missing an index on
`WinnerId` (EF indexes every FK column by default; hand-written SQL doesn't). It was only caught
by diffing an EF-built database against the real one. That's exactly the class of bug that
dual-maintenance produces.

## Folder layout

```
DB\
  Tables\SuperheroUniverse\
    00_CreateDatabase.sql … 12_RunAllTables.sql   Original hand-written schema (historical)
    _GeneratedFromEfMigrations\
      SuperheroUniverseDb_Schema.sql              GENERATED - do not hand-edit
  StoredProcedures\SuperheroUniverse\             Hand-written (xsp prefix) - genuinely owned here
  DataScripts\SuperheroUniverse\                  Hand-written seed / one-off scripts
```

### `Tables\*.sql` (numbered scripts)
The original hand-written schema that first created the database. Kept as a design record of the
intended constraints, indexes and naming. **Do not add new schema changes here** — they will not
reach the EF model, and the two will diverge.

### `Tables\_GeneratedFromEfMigrations\SuperheroUniverseDb_Schema.sql`
Generated, idempotent, safe to re-run. Builds the schema from scratch on an empty database, and
is a no-op against an up-to-date one (verified both ways). This is the script to hand to a DBA or
run in a deployment pipeline — enterprise environments generally deploy reviewed SQL rather than
running `dotnet ef database update` against production.

Regenerate after **every** new migration:

```powershell
cd "Dot Net\DotNetCoreAPI\WebAPISuperheroUniverse\WebAPISuperheroUniverse\DBContext"
dotnet ef migrations script --idempotent --startup-project ..\API --output "C:\Projects\SQL\DB\Tables\SuperheroUniverse\_GeneratedFromEfMigrations\SuperheroUniverseDb_Schema.sql"
```

### `StoredProcedures\SuperheroUniverse\`
Genuinely hand-written and owned here — EF migrations don't generate stored procedures. Uses the
`xsp` prefix.

### `DataScripts\SuperheroUniverse\`
Hand-written seed data and one-off scripts. Every seed script is idempotent (`IF NOT EXISTS`
guards), so re-running inserts nothing twice.

`11_BaselineEfMigrationHistory.sql` is a special case: it marks the initial migration as already
applied on a database that was created by the hand-written scripts *before* EF migrations existed.
Only needed for such a database — a database created from the generated script or from
`dotnet ef database update` records that row itself.

## Making a schema change from here

Run these from `WebAPISuperheroUniverse\WebAPISuperheroUniverse\DBContext`. The `--startup-project`
flag is required because the `DbContext` lives in a class library, while the connection string
lives in the API project's `appsettings.json`.

1. Edit the entity in `Entities\Tables\` + the `AppDbContext` Fluent API configuration.
2. `dotnet ef migrations add <DescriptiveName> --startup-project ..\API --output-dir EntityFramework\Migrations`
3. **Read the generated migration** before applying it — auto-generated `Down()` methods aren't
   always safe for destructive changes.
4. `dotnet ef database update --startup-project ..\API`
5. Regenerate the deployment script (command above) and commit it with the migration.
