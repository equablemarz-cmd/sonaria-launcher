# Sonaria Software Launcher: web pages

This branch is published with GitHub Pages at `https://equablemarz-cmd.github.io/sonaria-launcher/`.

- `join/` turns a copied server link (`join/?s=<server ID>`) into a Roblox join for that Creatures of Sonaria server. Discord won't make `roblox://` links clickable, and Roblox's own https game links ignore the server ID, so the launcher's "Copy current server link" button points here. The page only ever opens Creatures of Sonaria and only accepts a server ID shaped like one.
- `index.html` forwards to the launcher's repository.

Nothing on these pages loads anything from elsewhere or records visits.
