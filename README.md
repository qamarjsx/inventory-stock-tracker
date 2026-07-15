# Inventory & Stock Movements Tracker

A small ASP.NET Core MVC module for tracking products and their stock
levels. Current stock is never
stored or edited directly — it is always computed from an append-only log
of stock movements (`In` / `Out`), which is the core design constraint the
rest of the app is built around.

## Status

All three required screens (Product List, Product Detail + Record Movement,
Create/Edit Product) are complete and working, along with the
concurrency-safe zero-floor stock rule, soft deactivate/reactivate, and two
unit tests covering the stock calculation and the zero-floor rule. No
stretch goals were attempted — see "What I'd Improve" below.

## Tech Stack

- .NET 10, C# 13
- ASP.NET Core MVC (server-rendered Razor views)
- Entity Framework Core + SQLite
- xUnit for tests
- Hand-written CSS design system (no Tailwind/Bootstrap CSS; Bootstrap JS +
  jQuery + jquery-validation-unobtrusive kept via CDN for client-side
  validation only)

## How to Run

No manual database setup is required — the app is self-provisioning.

```bash
git clone <repo-url>
cd inventory-stock-tracker/src/InventoryStockTracker
dotnet run
```

On first run, this will:
1. Apply EF Core migrations automatically (creates `inventory.db`, a local
   SQLite file, in this folder), and
2. Seed four representative products with movement history — including one
   product exactly at its reorder level and one below it, so the low-stock
   badge is visible immediately.

Then open the URL shown in the console (typically `http://localhost:5053`).
The app redirects `/` straight to the product list.

Both steps above only happen in the Development environment and only if
the database is empty — safe to stop/restart `dotnet run` repeatedly
without duplicating seed data.

### Running the tests

From the solution root (one level up, where the `.sln` file is):

```bash
cd inventory-stock-tracker
dotnet test
```

## Assumptions

The assignment intentionally leaves some decisions open. These were made
deliberately and are documented here rather than left implicit:

1. **Deactivated products remain visible in the product list**, flagged
   with an "Inactive" badge, rather than being hidden — this keeps the
   "soft delete over hard delete" decision genuinely useful (history stays
   visible), rather than deactivation behaving like a disguised delete. A
   dedicated show/hide filter for inactive products was scoped originally
   but not implemented in the time available; see "What I'd Improve."

2. **Stock movements are immutable** once recorded — there is no edit or
   delete for a movement. Corrections happen via a compensating opposite
   movement. This preserves the core design goal of the system: current
   stock is always a trustworthy, derived number, never something that can
   be silently altered after the fact.

3. **SKU uniqueness is case-insensitive and whitespace-trimmed** (e.g.
   `ABC-1` and `abc-1 ` are treated as the same SKU), to avoid confusing
   near-duplicates. Enforced both as an application-level check and a
   database unique index, since the application-level check alone has the
   same check-then-act race condition as the stock rule.

4. **Reorder level of 0 is meaningful** — it means "flag as low stock only
   once stock reaches exactly 0," not "never flag this product."

5. **No authentication or user-identity model is in scope.** This is
   treated as a single-operator tool for the purposes of this exercise.
   Tracking *who* performed a movement is called out as a possible future
   improvement rather than attempted here.

6. **SKU is immutable after creation.** Once a product is created, its SKU
   can never be changed — there is no edit path for it, either in the UI or
   the domain model. This is a deliberate simplification over a more
   granular "lock only after the first movement" rule: that conditional
   approach is workable, but requires every code path that edits a product
   to remember to load its movement history before checking whether a
   change is still allowed — an easy rule to accidentally bypass if that
   collection isn't loaded. Making SKU permanently immutable removes that
   failure mode entirely, at the small cost of not being able to fix a
   typo'd SKU even in the first few seconds of a product's life (the
   correction path in that case is to deactivate the product and create a
   new one).

## The Zero-Floor Stock Rule (Concurrency)

An "Out" movement can never drop a product's stock below zero, including
under two near-simultaneous requests. This is enforced with a database
transaction plus an **in-transaction re-check**: current stock is
re-computed *inside* the open transaction, immediately before deciding
whether to allow the movement, not from a value read earlier or cached.

On SQLite, only one write transaction can be active at a time database-wide
(there's no true row-level lock, unlike SQL Server/PostgreSQL), so this
transaction-scoped re-check is sufficient to close the race: a second
concurrent request is naturally serialized behind the first, and its
re-check then sees the first request's already-committed result.

**Known, accepted gap:** SQLite opens a *deferred* transaction, which
doesn't take its write lock until the first `INSERT` — not at
`BeginTransactionAsync()`. This leaves a narrow theoretical window where two
transactions could both perform their read before either has the lock. The
fully rigorous fix is forcing an *immediate* transaction (`BEGIN IMMEDIATE`),
which requires dropping to raw ADO.NET underneath EF Core's SQLite provider
(no first-class API for it). Given this app's realistic concurrency level
and the assignment's time budget, this was judged not worth the additional
implementation time — documented here explicitly rather than silently
shipped. See "What I'd Improve."

## What I'd Improve With More Time

- **Force an immediate SQLite transaction** (`BEGIN IMMEDIATE` via raw
  ADO.NET) to close the narrow deferred-transaction window described above,
  or move to SQL Server/PostgreSQL in a real deployment, where a true
  row-level lock (`IsolationLevel.Serializable` or equivalent) closes it
  cleanly without the workaround.
- **Add a show/hide filter for inactive products** on the product list, per
  assumption #1 — currently they're visible with a badge but not
  filterable.
- **Paginate movement history** — `GetByIdAsync` currently loads a
  product's full, unfiltered movement history via `.Include`. Fine at this
  app's scale, but wouldn't scale to a product with years of history.
- **Avoid the extra query in `Details` GET** — it calls
  `GetCurrentStockAsync` separately even though `GetByIdAsync` already
  loaded the movement collection; summing the already-loaded collection in
  memory would save a round trip.
- **Audit trail** — track which operator performed each movement, once
  authentication is in scope.
- **Stretch goals**, none attempted given the time budget: pagination/
  sorting on the product list, CSV export of movement history, a small
  dashboard (total SKUs, count below reorder level), optimistic concurrency
  via `RowVersion` as an alternative/complement to the transaction-based
  approach above, and a JSON stock endpoint.

## AI Usage

See [`AI_NOTES.md`](./AI_NOTES.md) for details on how AI assistance was used
throughout this project.
