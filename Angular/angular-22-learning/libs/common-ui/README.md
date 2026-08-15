# common-ui (Angular library)

**Currently empty, and that is intentional.**

This is the front-end counterpart to the backend's `CommonAPI` project: a home for components that
are genuinely generic — a data table, a date picker, a confirm dialog with no domain knowledge —
so they can be shared by a second application in this workspace later.

Nothing lives here yet because everything built so far is superhero-specific and belongs in
`libs/superhero-universe/src/lib/shared/`. Putting domain components here just because they are
reused *within one app* would defeat the point.

Move a component here only when a second application in this workspace actually needs it.
