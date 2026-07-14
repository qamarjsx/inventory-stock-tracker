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

6. **SKU is editable only until a product's first stock movement is
   recorded.** Once at least one `StockMovement` exists for a product, its
   SKU becomes immutable — enforced both in the UI (field disabled) and
   independently on the server (not just trusted from the UI state) —
   because at that point the SKU is functioning as a real business key
   referenced by transactional history.

## What I'd Improve With More Time

_TODO — will be filled in as the project progresses and time constraints
become clearer._

## AI Usage

See [`AI_NOTES.md`](./AI_NOTES.md) for details on how AI assistance was used
throughout this project.