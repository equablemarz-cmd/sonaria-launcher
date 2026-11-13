# Module packages

This branch only holds the packaged tools ("modules") that the Sonaria Software Launcher downloads, and the workflow that publishes them as release assets. The launcher's code is on the other branches.

Every package is pinned in the launcher by its SHA-256 (see `Bloxstrap/Sonaria/ModuleCatalog.cs`), and the launcher refuses a download that doesn't match.

| Package | Version | SHA-256 |
|---|---|---|
| `TickTracker-4.0.0.zip` (in this branch's history) | 4.0.0 | `408aa52fb491357e250a1e84b6e75af29d33996fc46f6f60c6cdb9365f3b4b2b` |
| `TickTracker-4.0.1.zip` (in this branch's history) | 4.0.1 | `95cd322f93b81ce74f70b7858b37f0695700a95a36f3e519d50e6f93768cd027` |
| `TickTracker-4.1.0.zip` (in this branch's history, never used by a launcher) | 4.1.0 | `696122900472439d3d48056e1cbaecc2f969a307d5973144067598744f2324a8` |
| `TickTracker-4.1.1.zip` (in this branch's history) | 4.1.1 | `e80242e07247bb2a6e204e8fcbd9653b6ffeaee69e4668d95241cb9b547c8691` |
| `TickTracker-4.1.2.zip` | 4.1.2 | `8fd1bddf0dc59840e3689154879cca39d7a1793f4290467976512a9497114165` |
