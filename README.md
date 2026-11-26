# Sonaria Software kill service

This branch holds the Cloudflare Worker behind the **kill leaderboard** in the Kosers of Sonaria Discord (#kill-leaderboard).

- The Sonaria Software Launcher (3.4.0+) watches for Creatures of Sonaria's **KILL CONFIRMED** banner and sends `POST /kill` with the player's Roblox user ID and the time.
- Kills are stored in a Cloudflare D1 database (binding `DB`).
- Every minute a cron trigger works out the weekly (Monday 00:00 UK time) and all-time top 10 and, when they change, edits the leaderboard message through a Discord webhook (secret `DISCORD_WEBHOOK`).
- `GET /leaderboard` returns the same boards as JSON.

Kills are taken on trust: the launcher's code is public, so the board can be faked by anyone determined to.
Turn the launcher side off for everyone with `"killLeaderboard": true` under `disable` in the `config` branch's `launcher.json`.

Tests: `node --experimental-sqlite test.mjs`
