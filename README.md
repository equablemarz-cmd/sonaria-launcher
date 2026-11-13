# Sonaria Software Launcher

A PvP-focused Roblox launcher for Creatures of Sonaria, by Sonaria Software.

- **Launch Creatures of Sonaria** in one click, or rejoin the server you were last in.
- **TickTracker built in.** The natural heal tick countdown starts when you join CoS and closes when you leave. Nothing extra to install.
- **Frame rate limit.** Sets Roblox's maximum frame rate for you before it starts.
- **FPS boost.** One switch for the speed-ups Roblox still allows launchers to set.
- **The usual launcher extras:** mods (cursor, sounds, fonts), Discord Rich Presence, server details, custom loading styles.

Windows 10 and 11 only.

## Download and install

1. Get the latest `SonariaSoftwareLauncher-….exe` from the [Releases page](https://github.com/equablemarz-cmd/sonaria-launcher/releases). That page is the only official download.
2. Run it. Windows SmartScreen may warn that the file is unknown because it is new and not yet code-signed: choose **More info**, then **Run anyway**.
3. Follow the installer. It needs Microsoft's .NET 6 Desktop Runtime and will offer to get it if it is missing.

Installing makes this your Roblox launcher, so pressing Play on the Roblox website opens it too. To go back to normal Roblox, uninstall it from Windows Settings.

Each release lists the SHA-256 of its executable, and includes it as a `.sha256` file, so you can check your download.

## Is it safe?

The launcher only starts Roblox and works with Roblox's own local files: its settings, its logs and its mod folder. It never reads, writes or injects into the Roblox game process.

- **TickTracker** only looks at the screen, like you do.
- **Engine settings** can only be the ones on Roblox's official allowlist. Roblox ignores everything else.
- **Nothing about you is collected.** There is no analytics or crash reporting. The only things that use the internet are downloading Roblox, checking this repository for updates, tools and notices, Discord status (which goes only to the Discord app on your own PC and can be switched off), and server details if you turn that on.

## Updates

When a new version is released the launcher asks before installing it. If you say no, it won't ask again until the next version. An update is only run if it matches the SHA-256 published with the release.

## Troubleshooting

**Roblox will not download or install.** Check your internet connection, then check that your antivirus or firewall is not blocking the launcher. Try again after a restart. If it still fails, uninstall the launcher, reinstall it and launch again.

**Roblox fails to launch or closes straight away.** Turn FPS boost off and set the frame rate limit to 240 or lower under Settings > Engine settings, then try again. Roblox does not officially support frame rates above 240. If that does not help, remove any mods under Settings > Mods and try again.

**TickTracker does not appear.** It is downloaded the first time you join Creatures of Sonaria, which needs an internet connection and can take a moment. Check that it is switched on under Settings > Integrations. If its badge shows a heart instead of a number while your health bar is on screen, right-click the badge and choose Save debug report.

**Discord is not showing what you are playing.** Rich Presence needs activity tracking switched on, and the Discord desktop app installed and running. In Discord, check that sharing your activity is enabled in its privacy settings.

**Something else.** Send the newest file from `%LOCALAPPDATA%\Sonaria Software Launcher\Logs` to the bug-reports channel in the Kosers of Sonaria Discord.

## Settings notes

### Engine settings

- **Frame rate limit** writes Roblox's own Maximum Frame Rate setting each time Roblox starts. Roblox's menu goes up to 240. The 320 option is above what Roblox supports, and Roblox says the game can become unstable there.
- **FPS boost** turns anti-aliasing off and sets textures to their lowest quality. It is the same as choosing those two presets yourself further down the page.
- **Exclusive fullscreen.** Pressing Alt+Enter puts Roblox into Direct3D exclusive fullscreen, which can lower input delay. Other windows, including the TickTracker badge, may not show on top of it. Use Roblox's normal fullscreen (F11) if the badge disappears.

### Integrations

- **TickTracker** can be switched off, or kept open whenever Roblox is running instead of only in Creatures of Sonaria.
- **Activity tracking** lets the launcher know which game you are in by reading Roblox's log file. TickTracker's "only in CoS" option, Rejoin last server and Discord Rich Presence depend on it.
- **Don't exit to desktop app** closes Roblox completely when you leave a game. Anything that relies on going back to the Roblox app, such as picking another game from inside Roblox, stops working while it is on.
- **Discord status** is on by default: your Discord profile shows "Playing Sonaria Software" with the game you are in. Turn off "Show game activity" to stop it. Letting friends join you and showing your Roblox account stay off unless you turn them on.

### Shortcut icons

Turning on "Extract Roblox icons to folder" under Settings > Shortcuts saves the launcher's set of Roblox icons into an `Icons` folder inside `%LOCALAPPDATA%\Sonaria Software Launcher`. To use one, right-click a shortcut, choose Properties, then Change Icon, then Browse, and pick a file from that folder.

### Mods

The mods folder mirrors Roblox's own folder layout. Any file you put there replaces the matching Roblox file each time Roblox starts, and survives Roblox updates. Use "Open mods folder" in Settings > Mods to get to it. Stick to cosmetic changes such as cursors, sounds and fonts.

Roblox's pointer is the Sonaria cursor by default; set "Mouse cursor" under Settings > Mods to Default to get Roblox's own back. A game that sets its own pointer still shows its own.

Roblox's text uses the Creatures of Sonaria font (Fredoka One) by default. "Remove custom font" under Settings > Mods puts Roblox's own fonts back, and "Use the Creatures of Sonaria font" brings it back.

### Notices and off switches

Each time it opens, the launcher reads one small file from the [`config` branch](https://github.com/equablemarz-cmd/sonaria-launcher/tree/config) of this repository. Sonaria Software can use it to show a notice on the launch menu, or to switch off TickTracker, frame rates above 240, engine settings, or the font and cursor mods for everyone, for example if a game update breaks one of them. It cannot turn anything on, and the launch menu always names what has been switched off.

## Building

The launcher is a C# / WPF application. Every push is built by GitHub Actions (see `.github/workflows/ci.yml`), and pushing a tag starting with `v` publishes a release. The packaged tools the launcher downloads live on the `modules` branch.

## Licence and credit

Sonaria Software Launcher is based on [Bloxstrap](https://github.com/bloxstraplabs/bloxstrap) and is released under the same MIT licence (see [LICENSE](LICENSE)). It is not affiliated with Bloxstrap or with Roblox.
