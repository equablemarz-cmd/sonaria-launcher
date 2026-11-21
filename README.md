# Module packages

This branch only holds the packaged tools ("modules") that the Sonaria Software Launcher downloads, and the workflow that publishes them as release assets. The launcher's code is on the other branches.

Every package is pinned in the launcher by its SHA-256 (see `Bloxstrap/Sonaria/ModuleCatalog.cs`), and the launcher refuses a download that doesn't match.

| Package | Version | SHA-256 |
|---|---|---|
| `TickTracker-4.0.0.zip` (in this branch's history) | 4.0.0 | `408aa52fb491357e250a1e84b6e75af29d33996fc46f6f60c6cdb9365f3b4b2b` |
| `TickTracker-4.0.1.zip` (in this branch's history) | 4.0.1 | `95cd322f93b81ce74f70b7858b37f0695700a95a36f3e519d50e6f93768cd027` |
| `TickTracker-4.1.0.zip` (in this branch's history, never used by a launcher) | 4.1.0 | `696122900472439d3d48056e1cbaecc2f969a307d5973144067598744f2324a8` |
| `TickTracker-4.1.1.zip` (in this branch's history) | 4.1.1 | `e80242e07247bb2a6e204e8fcbd9653b6ffeaee69e4668d95241cb9b547c8691` |
| `TickTracker-4.1.2.zip` (in this branch's history) | 4.1.2 | `8fd1bddf0dc59840e3689154879cca39d7a1793f4290467976512a9497114165` |
| `TickTracker-4.1.3.zip` (in this branch's history) | 4.1.3 | `09f3233ef8b04a0b216f7519e09a3ff5c5bf94cd2c3f06a852654fa73635c372` |
| `TickTracker-4.2.0.zip` (in this branch's history, withdrawn: launcher 3.1.9 went back to 4.1.3) | 4.2.0 | `a2ada02d80defbfeb05543b5d62441e4471938fc44d5b6c3ba352d5d8f313384` |
| `TickTracker-4.1.4.zip` (in this branch's history) | 4.1.4 | `ffe20bffb3f7cd327e7e7bdb94edfc56ac93e89c1add3ae477601bc21fb0860b` |
| `TickTracker-4.1.5.zip` | 4.1.5 | `47ec0b092060b9823928f2dceb9d7a8102b99c5866840fefc33ca05a24ee5722` |
| `DamageCounter-1.0.0.zip` (in this branch's history) | 1.0.0 | `a18eaaa239e51d9d912e35925f6141eba610fc45acbfca7a5087c3ac0713f6ba` |
| `DamageCounter-1.0.1.zip` (in this branch's history, withdrawn: made the game flash white on some PCs) | 1.0.1 | `d7ea5bc961576fdb91858435125afbce6f05b73d67c2e0ad33d6a20e454f243f` |
| `DamageCounter-1.0.2.zip` | 1.0.2 | `fcaf905242437dfe7ed9af209c461f10a3a73c5f256e5d8b22258c742ab00c78` |
