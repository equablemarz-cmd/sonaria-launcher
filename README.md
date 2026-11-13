# Module packages

This branch only holds the packaged tools ("modules") that the Sonaria Software Launcher downloads, and the workflow that publishes them as release assets. The launcher's code is on the other branches.

Every package is pinned in the launcher by its SHA-256 (see `Bloxstrap/Sonaria/ModuleCatalog.cs`), and the launcher refuses a download that doesn't match.

| Package | Version | SHA-256 |
|---|---|---|
| `TickTracker-4.0.0.zip` | 4.0.0 | `408aa52fb491357e250a1e84b6e75af29d33996fc46f6f60c6cdb9365f3b4b2b` |
