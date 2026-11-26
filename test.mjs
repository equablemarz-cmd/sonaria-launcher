import { DatabaseSync } from 'node:sqlite';
import worker, { weekStart, leaderboard } from './worker.js';
import assert from 'node:assert';

// D1-shaped wrapper over node:sqlite
const raw = new DatabaseSync(':memory:');
const DB = { prepare(sql) { const st = raw.prepare(sql); let args = [];
  const o = { bind(...a) { args = a; return o; }, async run() { st.run(...args); return {}; },
    async first() { return st.get(...args) ?? null; }, async all() { return { results: st.all(...args) }; } }; return o; } };

const discord = []; let msgDeleted = false;
globalThis.fetch = async (url, init = {}) => {
  url = String(url);
  if (url.startsWith('https://users.roblox.com/v1/users/')) {
    const id = url.split('/').pop();
    return new Response(JSON.stringify({ id: +id, name: id === '1' ? 'saberlish' : id === '2' ? 'Zeta3078' : `user_${id}` }));
  }
  if (url.startsWith('https://discord.com/api/webhooks/') && !init.method) {
    return new Response('{}', { status: 200 });   // the board message still exists
  }
  if (url.startsWith('https://discord.com/api/webhooks/')) {
    discord.push({ url, method: init.method, body: JSON.parse(init.body) });
    if (init.method === 'PATCH' && msgDeleted) { msgDeleted = false; return new Response('', { status: 404 }); }
    return new Response(JSON.stringify({ id: '999' }));
  }
  throw new Error('unexpected fetch ' + url);
};
const env = { DB, DISCORD_WEBHOOK: 'https://discord.com/api/webhooks/1/abc' };
const waits = []; const ctx = { waitUntil: p => waits.push(p) };
const post = (b) => worker.fetch(new Request('https://x/kill', { method: 'POST', body: JSON.stringify(b) }), env, ctx);

const now = Date.now();
// 1. good kills
assert.equal((await post({ userId: 1, placeId: 5233782396, at: now - 60000, version: '3.4.0' })).status, 200);
assert.equal((await post({ userId: 1, at: now - 30000 })).status, 200);
assert.equal((await post({ userId: 2, at: now - 20000 })).status, 200);
// 2. duplicate within 2s -> 409
assert.equal((await post({ userId: 1, at: now - 29000 })).status, 409);
// 3. bad input
assert.equal((await post({ userId: -5, at: now })).status, 400);
assert.equal((await post({ userId: 1, at: now + 3600000 })).status, 400);
assert.equal((await post({ userId: 1, at: now - 8 * 86400000 })).status, 400);
assert.equal((await worker.fetch(new Request('https://x/kill', { method: 'POST', body: 'nope' }), env, ctx)).status, 400);
// 4. old kill from last week counts all-time only
const lastWeek = weekStart(now) - 3600000;
assert.equal((await post({ userId: 2, at: lastWeek })).status, 200);
assert.equal((await post({ userId: 2, at: lastWeek - 10000 })).status, 200);
await Promise.all(waits);

const board = await leaderboard(env, now);
console.log('weekly', board.weekly.map(r => `${r.name}:${r.kills}`), 'alltime', board.allTime.map(r => `${r.name}:${r.kills}`));
assert.deepEqual(board.weekly.map(r => [r.name, r.kills]), [['saberlish', 2], ['Zeta3078', 1]]);
assert.deepEqual(board.allTime.map(r => [r.name, r.kills]), [['Zeta3078', 3], ['saberlish', 2]]);

// 5. cron posts once, then does nothing while unchanged, then edits after a new kill
await worker.scheduled({}, env, { waitUntil: p => p }); await new Promise(r => setTimeout(r, 50));
const before = discord.length;
await worker.scheduled({}, env, { waitUntil: p => p }); await new Promise(r => setTimeout(r, 50));
assert.equal(discord.length, before, 'no edit when unchanged');
await post({ userId: 3, at: now - 5000 }); await Promise.all(waits);
await worker.scheduled({}, env, { waitUntil: p => p }); await new Promise(r => setTimeout(r, 50));
console.log('discord calls', discord.map(d => d.method + ' ' + d.url.replace('https://discord.com/api/webhooks/1/abc', '')));
assert.equal(discord[0].method, 'POST'); assert.equal(discord.at(-1).method, 'PATCH');
// 6. deleted message gets reposted
msgDeleted = true; await post({ userId: 3, at: now - 1000 }); await Promise.all(waits);
await worker.scheduled({}, env, { waitUntil: p => p }); await new Promise(r => setTimeout(r, 50));
console.log('after delete', discord.slice(-2).map(d => d.method));
assert.equal(discord.at(-1).method, 'POST');
// 6b. board deleted while nothing changes -> reposted at the next existence check
msgDeleted = false;
await raw.prepare(`DELETE FROM meta WHERE key = 'checked_at'`).run();
const beforeCheck = discord.length;
let gets = 0; const realFetch = globalThis.fetch;
globalThis.fetch = async (url, init = {}) => { if (String(url).includes('/messages/') && !init.method) { gets++; return new Response('', { status: 404 }); } return realFetch(url, init); };
await worker.scheduled({}, env, { waitUntil: p => p }); await new Promise(r => setTimeout(r, 50));
console.log('existence check GETs', gets, 'then', discord.slice(beforeCheck).map(d => d.method));
assert.equal(gets, 1); assert.equal(discord.at(-1).method, 'POST');
// and the next run within 3 minutes doesn't check again
await worker.scheduled({}, env, { waitUntil: p => p }); await new Promise(r => setTimeout(r, 50));
assert.equal(gets, 1);
globalThis.fetch = realFetch;
// 7. week start is Monday 00:00 UK
const ws = new Date(weekStart(Date.UTC(2026, 10, 26, 0, 10)));
console.log('week start for Thu 26 Nov 00:10 UTC:', ws.toISOString());
assert.equal(ws.toISOString(), '2026-11-23T00:00:00.000Z');
const wsS = new Date(weekStart(Date.UTC(2026, 6, 15, 12, 0)));   // summer time
console.log('week start for Wed 15 Jul 2026 (BST):', wsS.toISOString());
assert.equal(wsS.toISOString(), '2026-07-12T23:00:00.000Z');
console.log('\nDISCORD MESSAGE:\n' + JSON.stringify(discord.at(-1).body.embeds[0], null, 1));
console.log('ALL TESTS PASSED');
