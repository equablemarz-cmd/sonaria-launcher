# Launcher notices and off switches

Sonaria Software Launcher reads `launcher.json` from this branch each time it opens. Editing the file changes what every copy of the launcher does on its next launch; no new version is needed.

## Notice

`notice.text` is shown in a box at the top of the launch menu. Leave it empty for no notice. It is shown as one line of plain text, up to 240 characters.

`notice.link` is optional and adds a "Read more" link. Only links starting with `https://github.com/equablemarz-cmd/sonaria-launcher` or `https://discord.gg/` are accepted.

## Off switches

Set one of these to `true` to switch that feature off for everyone, and back to `false` to restore it.

| Switch | What it does |
| --- | --- |
| `tickTracker` | TickTracker is not downloaded or started |
| `fpsAbove240` | The frame rate limit is held at 240, Roblox's supported maximum |
| `engineSettings` | Engine settings, FPS boost included, are not given to Roblox |
| `mods` | The font and cursor mods are not given to Roblox |
| `discordServerButton` | The "Join Kosers of Sonaria" button is taken off the Discord status (launcher 3.1.6 and later). Use it if the discord.gg/koser link ever stops working |

The launch menu names anything that is switched off, so it is never done silently. The file can only switch features off or show text. It cannot turn anything on, and reading it sends nothing about the user.

If the launcher can't fetch the file, it uses the last copy it fetched.
