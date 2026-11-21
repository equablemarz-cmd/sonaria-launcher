Damage Counter 1.0.2, packaged as a module for Sonaria Software Launcher. The launcher downloads this file itself and checks it against a SHA-256 built into the launcher, so there is nothing to install by hand.

Fixes the game flashing white. 1.0.1 asked the Roblox window to draw a copy of itself 20 times a second (the way OBS's window capture works), and on some PCs that made the game flash white. The counter now reads the game area straight off the screen while Roblox is the window in front, like a screenshot, and pauses while you're tabbed out. It never touches the game window. It also works properly with Windows display scaling (125%, 150%) switched on.

Everything else is as in 1.0.1: 20 looks a second (10 on a PC that can't keep up), hits count after two clear sightings, quick roll-up.

SHA-256 of `DamageCounter-1.0.2.zip`: `fcaf905242437dfe7ed9af209c461f10a3a73c5f256e5d8b22258c742ab00c78`
