/**
 * Sonaria Software kill service (Cloudflare Worker).
 *
 * The Sonaria Software Launcher sends one request here for every KILL CONFIRMED it sees:
 *   POST /kill   {"userId": 123, "placeId": 5233782396, "at": <unix ms>, "version": "3.4.0"}
 *
 * Kills are kept in a D1 database (binding: DB). Every minute (cron trigger) the board is worked out
 * and, if anything changed, the one leaderboard message in #kill-leaderboard is edited through a
 * Discord webhook (secret: DISCORD_WEBHOOK). Roblox usernames are looked up from the user ID and
 * kept for a day.
 *
 *   GET /leaderboard   the weekly and all-time top 10 as JSON
 *   GET /              a one-line health check
 *
 * The week starts Monday 00:00 UK time.
 */

const TOP = 10;
const MIN_GAP_MS = 2000;            // two kills from one player can't be closer than this
const MAX_CLOCK_SKEW_MS = 10 * 60 * 1000;
const MAX_AGE_MS = 7 * 24 * 3600 * 1000;   // kills saved offline are still accepted for a week
const NAME_TTL_MS = 24 * 3600 * 1000;
const EXISTS_CHECK_MS = 3 * 60 * 1000;     // how often to check the board message still exists

const SCHEMA = [
  `CREATE TABLE IF NOT EXISTS kills (id INTEGER PRIMARY KEY AUTOINCREMENT, user_id INTEGER NOT NULL, at INTEGER NOT NULL, received INTEGER NOT NULL, place_id INTEGER, version TEXT)`,
  `CREATE INDEX IF NOT EXISTS kills_user_at ON kills (user_id, at)`,
  `CREATE INDEX IF NOT EXISTS kills_at ON kills (at)`,
  `CREATE TABLE IF NOT EXISTS users (user_id INTEGER PRIMARY KEY, name TEXT, checked INTEGER NOT NULL DEFAULT 0)`,
  `CREATE TABLE IF NOT EXISTS meta (key TEXT PRIMARY KEY, value TEXT)`,
];

let schemaReady = false;

async function ensureSchema(db) {
  if (schemaReady) return;
  for (const sql of SCHEMA) await db.prepare(sql).run();
  schemaReady = true;
}

function json(data, status = 200) {
  return new Response(JSON.stringify(data), {
    status,
    headers: { 'content-type': 'application/json', 'access-control-allow-origin': '*' },
  });
}

/** Monday 00:00 in the UK, as unix ms, for the week containing `now`. */
export function weekStart(now) {
  const parts = Object.fromEntries(
    new Intl.DateTimeFormat('en-GB', {
      timeZone: 'Europe/London', weekday: 'short', hour: '2-digit', minute: '2-digit', second: '2-digit', hourCycle: 'h23',
    }).formatToParts(new Date(now)).map(p => [p.type, p.value]));
  const days = { Mon: 0, Tue: 1, Wed: 2, Thu: 3, Fri: 4, Sat: 5, Sun: 6 }[parts.weekday];
  const sinceMidnight = (+parts.hour * 3600 + +parts.minute * 60 + +parts.second) * 1000 + (now % 1000);
  return now - days * 86400000 - sinceMidnight;
}

async function recordKill(request, env, ctx) {
  let body;
  try {
    body = await request.json();
  } catch {
    return json({ error: 'bad json' }, 400);
  }

  const userId = Number(body?.userId);
  const placeId = Number(body?.placeId) || null;
  const at = Number(body?.at);
  const version = typeof body?.version === 'string' ? body.version.slice(0, 20) : null;
  const now = Date.now();

  if (!Number.isSafeInteger(userId) || userId <= 0) return json({ error: 'bad userId' }, 400);
  if (!Number.isFinite(at) || at > now + MAX_CLOCK_SKEW_MS || at < now - MAX_AGE_MS) return json({ error: 'bad time' }, 400);

  const db = env.DB;
  await ensureSchema(db);

  const near = await db.prepare(`SELECT 1 FROM kills WHERE user_id = ? AND at > ? AND at < ? LIMIT 1`)
    .bind(userId, at - MIN_GAP_MS, at + MIN_GAP_MS).first();
  if (near) return json({ error: 'duplicate' }, 409);

  await db.prepare(`INSERT INTO kills (user_id, at, received, place_id, version) VALUES (?, ?, ?, ?, ?)`)
    .bind(userId, Math.round(at), now, placeId, version).run();

  ctx.waitUntil(refreshName(env, userId, false));
  return json({ ok: true });
}

/** Looks up the Roblox username for a user ID (kept for a day). */
async function refreshName(env, userId, force) {
  const db = env.DB;
  const row = await db.prepare(`SELECT name, checked FROM users WHERE user_id = ?`).bind(userId).first();
  if (row && !force && Date.now() - row.checked < NAME_TTL_MS) return row.name;
  try {
    const r = await fetch(`https://users.roblox.com/v1/users/${userId}`, { headers: { accept: 'application/json' } });
    if (r.ok) {
      const u = await r.json();
      const name = typeof u?.name === 'string' ? u.name : null;
      if (name) {
        await db.prepare(`INSERT INTO users (user_id, name, checked) VALUES (?, ?, ?) ON CONFLICT(user_id) DO UPDATE SET name = excluded.name, checked = excluded.checked`)
          .bind(userId, name, Date.now()).run();
        return name;
      }
    }
  } catch { /* try again next time */ }
  return row?.name ?? null;
}

async function topSince(db, since) {
  const { results } = await db.prepare(
    `SELECT k.user_id AS userId, COUNT(*) AS kills, MAX(k.at) AS last, u.name AS name
       FROM kills k LEFT JOIN users u ON u.user_id = k.user_id
      WHERE k.at >= ?
      GROUP BY k.user_id
      ORDER BY kills DESC, last ASC
      LIMIT ?`).bind(since, TOP).all();
  return results;
}

export async function leaderboard(env, now = Date.now()) {
  await ensureSchema(env.DB);
  const start = weekStart(now);
  const [weekly, allTime] = await Promise.all([topSince(env.DB, start), topSince(env.DB, 0)]);
  return { weekStart: start, weekly, allTime };
}

const MEDALS = ['🥇', '🥈', '🥉'];

function escapeName(name) {
  return String(name).replace(/([\\*_~`|>#\-\[\]()])/g, '\\$1');
}

function lines(rows) {
  if (!rows.length) return '*No kills yet. Get the first one!*';
  return rows.map((r, i) => {
    const place = MEDALS[i] ?? `\`${String(i + 1).padStart(2, ' ')}.\``;
    const name = r.name ? escapeName(r.name) : `Player ${r.userId}`;
    return `${place} **${name}** — ${r.kills} kill${r.kills === 1 ? '' : 's'}`;
  }).join('\n');
}

export function buildMessage(board) {
  return {
    username: 'Kill Leaderboard',
    content: '',
    allowed_mentions: { parse: [] },
    embeds: [{
      title: '⚔️ Kill Leaderboard',
      description: 'Every **KILL CONFIRMED** in Creatures of Sonaria counts, tracked by the Sonaria Software Launcher.',
      color: 0x1fb6c9,
      fields: [
        { name: '📅 This week', value: lines(board.weekly), inline: false },
        { name: '🏆 All time', value: lines(board.allTime), inline: false },
      ],
      footer: { text: 'Weekly board resets every Monday 00:00 UK time' },
    }],
  };
}

export async function updateDiscord(env) {
  if (!env.DISCORD_WEBHOOK) return 'no webhook set';
  const board = await leaderboard(env);
  const message = buildMessage(board);
  // the reset line only changes once a week, so it's safe to show as a live Discord timestamp
  message.embeds[0].fields.push({ name: '​', value: `Weekly reset <t:${Math.floor((board.weekStart + 7 * 86400000) / 1000)}:R>`, inline: false });

  const body = JSON.stringify(message);
  const db = env.DB;
  const last = await db.prepare(`SELECT value FROM meta WHERE key = 'last_body'`).first();
  const msg = await db.prepare(`SELECT value FROM meta WHERE key = 'message_id'`).first();

  if (msg && last?.value === body) {
    // nothing changed, but check now and then that the board message is still there (someone may
    // have deleted it) and post it again if not
    const checked = await db.prepare(`SELECT value FROM meta WHERE key = 'checked_at'`).first();
    if (checked && Date.now() - Number(checked.value) < EXISTS_CHECK_MS) return 'unchanged';
    await db.prepare(`INSERT INTO meta (key, value) VALUES ('checked_at', ?) ON CONFLICT(key) DO UPDATE SET value = excluded.value`).bind(String(Date.now())).run();
    const hook = env.DISCORD_WEBHOOK.replace(/\/$/, '');
    const r = await fetch(`${hook}/messages/${msg.value}`);
    if (r.status !== 404) return 'unchanged';
    await db.prepare(`DELETE FROM meta WHERE key = 'message_id'`).run();
    return updateDiscord(env);
  }

  const hook = env.DISCORD_WEBHOOK.replace(/\/$/, '');
  let r;
  if (msg) {
    r = await fetch(`${hook}/messages/${msg.value}`, { method: 'PATCH', headers: { 'content-type': 'application/json' }, body });
    if (r.status === 404) {
      // the message was deleted: post a new one below
      await db.prepare(`DELETE FROM meta WHERE key = 'message_id'`).run();
      return updateDiscord(env);
    }
  } else {
    r = await fetch(`${hook}?wait=true`, { method: 'POST', headers: { 'content-type': 'application/json' }, body });
    if (r.ok) {
      const posted = await r.json();
      await db.prepare(`INSERT INTO meta (key, value) VALUES ('message_id', ?) ON CONFLICT(key) DO UPDATE SET value = excluded.value`).bind(posted.id).run();
    }
  }
  if (!r.ok) return `discord said ${r.status}`;
  await db.prepare(`INSERT INTO meta (key, value) VALUES ('last_body', ?) ON CONFLICT(key) DO UPDATE SET value = excluded.value`).bind(body).run();
  return 'updated';
}

export default {
  async fetch(request, env, ctx) {
    const url = new URL(request.url);
    try {
      if (request.method === 'POST' && url.pathname === '/kill') return await recordKill(request, env, ctx);
      if (request.method === 'GET' && url.pathname === '/leaderboard') return json(await leaderboard(env));
      if (request.method === 'GET' && url.pathname === '/') return new Response('Sonaria Software kill service is running.\n');
      return json({ error: 'not found' }, 404);
    } catch (e) {
      return json({ error: 'server error' }, 500);
    }
  },

  async scheduled(event, env, ctx) {
    await ensureSchema(env.DB);
    // keep names fresh for everyone on the board
    const board = await leaderboard(env);
    const ids = [...new Set([...board.weekly, ...board.allTime].filter(r => !r.name).map(r => r.userId))];
    for (const id of ids.slice(0, 10)) await refreshName(env, id, true);
    ctx.waitUntil(updateDiscord(env));
  },
};
