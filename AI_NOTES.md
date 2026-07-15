# AI Usage Notes

## Tools used

Claude (claude.ai) was used throughout this project, in a mentoring/
pair-programming mode rather than a code-generation mode — I designed and
wrote the domain model, service layer, controllers, and views myself, and
used Claude to:

- Talk through architecture and technology tradeoffs before committing to
  them (MVC vs. Razor Pages vs. Blazor; EF Core vs. Dapper; SQLite vs.
  LocalDB; the concurrency strategy for the zero-floor stock rule)
- Review my code PR-style before each commit (spotting a missing namespace
  on `StockMovementType`, a mismatched README/entities commit, an
  auto-generated NuGet vulnerability warning caught by
  `TreatWarningsAsErrors`)
- Explain unfamiliar EF Core/C# mechanics in depth once I asked for it —
  transactions and isolation levels, `await using` disposal semantics,
  SQLite's file-level locking vs. row-level locking, tag helpers
- Help scaffold the CSS design system and Razor view markup, which I then
  reviewed and adjusted
- Write the two unit tests covering the stock calculation and the
  zero-floor rejection rule

## Something the AI got wrong that I had to catch and correct

While explaining `StockService.RecordMovementAsync`, Claude initially
justified duplicating the current-stock summing query directly inside
`RecordMovementAsync` — instead of calling the existing
`GetCurrentStockAsync` method — by claiming it was necessary for the
re-check to run inside the open transaction. I pushed back and asked why
we couldn't just call `GetCurrentStockAsync` from inside the same method,
since it's on the same service. Claude then acknowledged the reasoning
didn't actually hold: `RecordMovementAsync` and `GetCurrentStockAsync` are
both methods on the same `StockService` instance, sharing the same
`AppDbContext` field — so a query issued via `GetCurrentStockAsync` from
inside `RecordMovementAsync`, after `BeginTransactionAsync()` has run, still
participates in that same open transaction. The "must duplicate the query"
justification was wrong; the duplication was an unnecessary simplification
that should have been refactored to call the existing method instead. This
was a useful reminder to actually question the reasoning behind AI-provided
explanations rather than accept them because the code itself compiled and
worked.