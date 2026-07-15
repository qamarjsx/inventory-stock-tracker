# Inventory Stock Tracker

A small inventory and stock-movement tracking module built as a take-home
technical assessment. Tracks products and their stock levels, where stock is
never edited directly — it is always derived from a log of stock movements
(goods received in, goods issued out).

## Status

🚧 Work in progress — solution scaffolding not yet created.

## Tech Stack

- .NET 10, C# (latest language version)
- ASP.NET Core MVC (server-rendered Razor views)
- Entity Framework Core
- Database: TBD (SQL Server LocalDB / SQLite / EF Core In-Memory — to be finalized in the design phase)
- xUnit for unit tests

## How to Run

_TODO — will be filled in once the solution and database setup exist._

## Assumptions

The assignment intentionally leaves some decisions open. The following
assumptions were made, and are documented here rather than left implicit:

1. **Deactivated products remain visible** in the product list, visually
   de-emphasized, with a filter to show/hide them — they are not silently
   hidden. This keeps the "soft delete over hard delete" decision genuinely
   useful (history stays visible), rather than deactivation behaving like a
   disguised delete.

2. **Stock movements are immutable** once recorded — there is no edit or
   delete for a movement. Corrections happen via a compensating opposite
   movement. This preserves the core design goal of the system: current
   stock is always a trustworthy, derived number, never something that can
   be silently altered after the fact.

3. **SKU uniqueness is case-insensitive and whitespace-trimmed** (e.g.
   `ABC-1` and `abc-1 ` are treated as the same SKU), to avoid confusing
   near-duplicates.

4. **Reorder level of 0 is meaningful** — it means "flag as low stock only
   once stock reaches exactly 0," not "never flag this product."

5. **No authentication or user-identity model is in scope.** This is treated
   as a single-operator tool for the purposes of this exercise. Tracking
   *who* performed a movement is called out as a possible future
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

## What I'd Improve With More Time

_TODO — will be filled in as the project progresses and time constraints
become clearer._

## AI Usage

See [`AI_NOTES.md`](./AI_NOTES.md) for details on how AI assistance was used
throughout this project.